# Gestion multi-disque (module Duplicate Files) — Plan d'implémentation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implémenter le snapshot courant multi-disque du spec `docs/superpowers/specs/2026-07-16-multi-disque-design.md` : table `scans` (schéma v2), identité de volume observée, état courant = dernier scan par volume appliqué à la source de Ω, volume porté par chaque exemplaire du rapport.

**Architecture:** Le producteur (scan) écrit un schéma v2 (table `scans` + colonne `scan_id`) ; le lecteur Ω (`LecteurDObservationsSqlite`) devient bi-version — v1 lu intégralement (oracle de conformité intact), v2 filtré sur le dernier scan de chaque volume — de sorte que `identity`, `duplicates` et `plan` consomment le même état courant sans changer. Un petit lecteur séparé joint `scan_id → scans` pour enrichir le rapport du volume de chaque exemplaire.

**Tech Stack:** .NET 10 / C#, Microsoft.Data.Sqlite, P/Invoke (kernel32 `GetVolumeInformationW`, mpr `WNetGetConnectionW`), xUnit.

## Global Constraints

- **Windows-only assumé** (ADR-001) ; P/Invoke direct autorisé, style ADR-006.
- **Append-only strict** (ADR-002) : jamais d'UPDATE/DELETE ; « remplacer » = sortir de l'état courant à la lecture, jamais effacer.
- **NULL = absence** (ADR-004) : aucune valeur sentinelle, aucune interprétation dans le pipeline.
- **`user_version`** : le producteur écrit **2** exclusivement ; le lecteur Ω accepte **1 et 2** (amendement bi-version du spec — l'oracle `tests/oracle/corpus1-postA1.db` est une base v1 de la conformité v3 et doit rester lisible tel quel).
- **Ne jamais toucher `src/InstallChecker.Identity`** (le moteur pur) : seuls `InstallChecker.Core`, `InstallChecker.Identity.Access`, `InstallChecker.DuplicateFiles` et l'exécutable `InstallChecker` évoluent.
- Nommage : types/méthodes en anglais côté `Core` et exécutable (`FileHeaderExtractor.Read`), en français côté `Identity.Access` et module (`LecteurDObservationsSqlite`, `GenerateurDeRapport`) — suivre le fichier voisin.
- Commentaires en français, expliquant la contrainte, jamais la ligne suivante.
- Tests : xUnit, exécutés par projet — `dotnet test tests/InstallChecker.Tests`, `dotnet test tests/InstallChecker.Identity.Tests`, `dotnet test tests/InstallChecker.DuplicateFiles.Tests`. La suite complète : `dotnet test InstallChecker.slnx`.
- **Ordre des tâches obligatoire** : le lecteur bi-version (tâche 2) doit précéder le producteur v2 (tâche 3), sinon `FrontiereDeDonneesTests` (scan réel + lecture) casse entre les deux commits.

---

### Task 1: VolumeIdentityExtractor (identité observée du volume)

**Files:**
- Create: `src/InstallChecker.Core/VolumeIdentityExtractor.cs`
- Test: `tests/InstallChecker.Tests/VolumeIdentityExtractorTests.cs`

**Interfaces:**
- Consumes: rien (feuille).
- Produces: `public sealed record VolumeIdentity(string VolumeId, string? VolumeLabel)` et `public static class VolumeIdentityExtractor { public static VolumeIdentity Resolve(string root); }` — namespace `InstallChecker` (comme les autres extracteurs Core). `Resolve` lève `InvalidOperationException` (message commençant par `Erreur : identité de volume irrésoluble`) si l'identité ne peut pas être établie. La tâche 3 appelle `Resolve` depuis `ScanCommand`.

- [ ] **Step 1: Écrire les tests qui échouent**

Créer `tests/InstallChecker.Tests/VolumeIdentityExtractorTests.cs` :

```csharp
namespace InstallChecker.Tests;

public class VolumeIdentityExtractorTests
{
    // La branche UNC est du pur calcul de chaîne : aucune I/O réseau, testable partout.
    [Theory]
    [InlineData(@"\\SERVEUR\Partage\sous\dossier", @"\\serveur\partage")]
    [InlineData(@"\\nas\Archives", @"\\nas\archives")]
    [InlineData(@"\\nas\Archives\", @"\\nas\archives")]
    public void Une_racine_UNC_est_normalisee_en_minuscules_sur_serveur_partage(string chemin, string attendu)
    {
        var identite = VolumeIdentityExtractor.Resolve(chemin);

        Assert.Equal(attendu, identite.VolumeId);
        Assert.Null(identite.VolumeLabel);
    }

    [Fact]
    public void Un_chemin_UNC_sans_partage_est_refuse_explicitement()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => VolumeIdentityExtractor.Resolve(@"\\serveur"));
        Assert.StartsWith("Erreur : identité de volume irrésoluble", ex.Message);
    }

    [Fact]
    public void Un_chemin_local_produit_une_serie_hexadecimale_de_8_caracteres()
    {
        var identite = VolumeIdentityExtractor.Resolve(Path.GetTempPath());

        Assert.Matches("^[0-9a-f]{8}$", identite.VolumeId);
    }
}
```

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/InstallChecker.Tests --filter "FullyQualifiedName~VolumeIdentityExtractorTests"`
Expected: échec de compilation — `VolumeIdentityExtractor` n'existe pas.

- [ ] **Step 3: Implémenter l'extracteur**

Créer `src/InstallChecker.Core/VolumeIdentityExtractor.cs` :

```csharp
using System.Runtime.InteropServices;
using System.Text;

namespace InstallChecker;

/// <summary>
/// L'identité observée du volume portant une racine de scan (spec multi-disque D3) : ce qui permet
/// au « dernier scan par volume » de reconnaître le même disque physique quand la lettre change.
/// Métadonnée du système de fichiers uniquement (garde A1) — jamais de contenu.
/// </summary>
public sealed record VolumeIdentity(string VolumeId, string? VolumeLabel);

public static class VolumeIdentityExtractor
{
    /// <summary>
    /// Résout l'identité du volume de <paramref name="root"/> : racine UNC normalisée en minuscules
    /// pour le réseau (lettre mappée résolue en UNC — le même partage via Z: ou via UNC est le même
    /// volume), numéro de série hexadécimal pour un disque local. Identité irrésoluble = erreur
    /// explicite : un volume mal identifié corromprait le remplacement de l'état courant.
    /// </summary>
    public static VolumeIdentity Resolve(string root)
    {
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(root);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {root} : {ex.Message}");
        }

        if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
            return new VolumeIdentity(NormalizeUncRoot(fullPath), null);

        var driveRoot = Path.GetPathRoot(fullPath);
        if (driveRoot is null || driveRoot.Length < 3 || fullPath[1] != ':')
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {root}");

        if (new DriveInfo(driveRoot).DriveType == DriveType.Network)
            return new VolumeIdentity(NormalizeUncRoot(ResolveMappedDrive(driveRoot[..2])), null);

        return ResolveLocal(driveRoot);
    }

    private static string NormalizeUncRoot(string uncPath)
    {
        var parts = uncPath.TrimStart('\\').Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {uncPath} (racine UNC sans partage)");
        return $@"\\{parts[0]}\{parts[1]}".ToLowerInvariant();
    }

    private static string ResolveMappedDrive(string driveLetter)
    {
        var length = 1024;
        var remoteName = new StringBuilder(length);
        var result = WNetGetConnectionW(driveLetter, remoteName, ref length);
        if (result != 0)
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {driveLetter} (WNetGetConnection={result})");
        return remoteName.ToString();
    }

    private static VolumeIdentity ResolveLocal(string driveRoot)
    {
        var label = new StringBuilder(261); // MAX_PATH + 1, taille documentée pour lpVolumeNameBuffer
        if (!GetVolumeInformationW(driveRoot, label, label.Capacity, out var serial, out _, out _, null, 0))
            throw new InvalidOperationException($"Erreur : identité de volume irrésoluble : {driveRoot} (Win32={Marshal.GetLastWin32Error()})");
        return new VolumeIdentity(serial.ToString("x8"), label.Length == 0 ? null : label.ToString());
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetVolumeInformationW(
        string lpRootPathName, StringBuilder lpVolumeNameBuffer, int nVolumeNameSize,
        out uint lpVolumeSerialNumber, out uint lpMaximumComponentLength, out uint lpFileSystemFlags,
        StringBuilder? lpFileSystemNameBuffer, int nFileSystemNameSize);

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnectionW(string lpLocalName, StringBuilder lpRemoteName, ref int lpnLength);
}
```

Note : la branche « lettre mappée réseau » n'est pas couverte automatiquement (aucun lecteur mappé garanti sur la machine de test) — elle est exercée manuellement ; les deux autres branches sont testées.

- [ ] **Step 4: Vérifier le passage**

Run: `dotnet test tests/InstallChecker.Tests --filter "FullyQualifiedName~VolumeIdentityExtractorTests"`
Expected: PASS (5 tests).

- [ ] **Step 5: Commit**

```bash
git add src/InstallChecker.Core/VolumeIdentityExtractor.cs tests/InstallChecker.Tests/VolumeIdentityExtractorTests.cs
git commit -m "feat(multi-disque): identite de volume observee (serie locale, racine UNC normalisee)"
```

---

### Task 2: Lecteur Ω bi-version — état courant sur les bases v2

**Files:**
- Modify: `src/InstallChecker.Identity.Access/Observations/LecteurDObservationsSqlite.cs`
- Modify: `tests/InstallChecker.Identity.Tests/MiniBaseDObservations.cs`
- Test: `tests/InstallChecker.Identity.Tests/LecteurDObservationsSqliteTests.cs`

**Interfaces:**
- Consumes: rien de nouveau (schéma v2 défini ici côté fixture ; le producteur réel arrive en tâche 3 avec le même DDL).
- Produces: `LecteurDObservationsSqlite` accepte `user_version` 1 **ou** 2. Sur une base v2, `ProjeterModele`/`ProjeterContexte` ne retournent que les observations des scans courants (dernier `scans.id` par `volume_id`) ; `ProjeterIdentite` déclare la version réelle de la base. `MiniBaseDObservations.CreerConformeV2(string chemin)` fabrique le schéma v2 pour les fixtures.

- [ ] **Step 1: Étendre la fabrique de fixtures**

Dans `tests/InstallChecker.Identity.Tests/MiniBaseDObservations.cs`, ajouter après `CreerConforme` :

```csharp
    /// <summary>
    /// Fabrique du contrat v2 (spec multi-disque D2) : mêmes tables de capacité, plus la table
    /// <c>scans</c> et la colonne <c>scan_id</c> — le schéma que le producteur écrit à partir de
    /// la gestion multi-disque, matérialisé côté fixtures comme <see cref="CreerConforme"/> pour v1.
    /// </summary>
    public static void CreerConformeV2(string chemin)
    {
        using var connection = new SqliteConnection($"Data Source={chemin}");
        connection.Open();
        using var commande = connection.CreateCommand();
        commande.CommandText = """
            CREATE TABLE scans (
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                volume_id    TEXT NOT NULL,
                volume_label TEXT,
                root_path    TEXT NOT NULL,
                started_at   TEXT NOT NULL,
                extensions   TEXT
            );
            CREATE TABLE scan_observations (
                id         INTEGER PRIMARY KEY AUTOINCREMENT,
                scan_id    INTEGER NOT NULL,
                path       TEXT NOT NULL,
                size       INTEGER NOT NULL,
                sha256     TEXT NOT NULL,
                scanned_at TEXT NOT NULL
            );
            CREATE TABLE version_info (
                observation_id  INTEGER NOT NULL,
                product_name    TEXT,
                company_name    TEXT,
                product_version TEXT,
                file_version    TEXT
            );
            CREATE TABLE file_headers (
                observation_id INTEGER NOT NULL,
                magic_hex      TEXT NOT NULL,
                container      TEXT
            );
            CREATE TABLE pe_info (
                observation_id        INTEGER NOT NULL,
                machine               TEXT,
                subsystem             TEXT,
                characteristics       INTEGER,
                timestamp             INTEGER,
                optional_header_magic TEXT
            );
            CREATE TABLE authenticode (
                observation_id INTEGER NOT NULL,
                subject        TEXT,
                issuer         TEXT,
                serial_number  TEXT,
                thumbprint     TEXT,
                not_before     TEXT,
                not_after      TEXT
            );
            CREATE TABLE msi_properties (
                observation_id   INTEGER NOT NULL,
                product_name     TEXT,
                product_version  TEXT,
                manufacturer     TEXT,
                product_code     TEXT,
                upgrade_code     TEXT,
                product_language TEXT
            );
            CREATE TABLE appx_manifest (
                observation_id         INTEGER NOT NULL,
                name                   TEXT,
                publisher              TEXT,
                version                TEXT,
                processor_architecture TEXT
            );
            PRAGMA user_version = 2;
            """;
        commande.ExecuteNonQuery();
    }
```

- [ ] **Step 2: Écrire les tests qui échouent**

Dans `tests/InstallChecker.Identity.Tests/LecteurDObservationsSqliteTests.cs`, ajouter une section après le test `Lordre_de_stockage_SQL_nimporte_pas_a_lordre_logique_des_actes` :

```csharp
    // --- Contrat v2 : l'état courant = le dernier scan de chaque volume (spec multi-disque D1/D4) ---

    /// <summary>Trois scans : deux sur vol-a (le second remplace le premier), un sur vol-b.
    /// Les capacités portent une ligne par observation, courante ou non — comme une vraie base.</summary>
    private string BaseV2DeuxVolumes()
    {
        var chemin = NouveauCheminDeBase();
        MiniBaseDObservations.CreerConformeV2(chemin);

        using var connection = new SqliteConnection($"Data Source={chemin}");
        connection.Open();
        using var commande = connection.CreateCommand();
        commande.CommandText = """
            INSERT INTO scans (id, volume_id, volume_label, root_path, started_at) VALUES
                (1, 'vol-a', 'Data',  'D:\',            '2026-01-01T00:00:00Z'),
                (2, 'vol-a', 'Data',  'D:\',            '2026-02-01T00:00:00Z'),
                (3, 'vol-b', NULL,    '\\nas\partage',  '2026-01-15T00:00:00Z');
            INSERT INTO scan_observations (id, scan_id, path, size, sha256, scanned_at) VALUES
                (1, 1, 'D:\a.exe',            1, 'sha-a', '2026-01-01T00:00:00Z'),
                (2, 1, 'D:\b.exe',            2, 'sha-b', '2026-01-01T00:00:00Z'),
                (3, 2, 'D:\a.exe',            1, 'sha-a', '2026-02-01T00:00:00Z'),
                (4, 3, '\\nas\partage\c.exe', 3, 'sha-c', '2026-01-15T00:00:00Z');
            INSERT INTO version_info (observation_id) VALUES (1), (2), (3), (4);
            INSERT INTO file_headers (observation_id, magic_hex) VALUES (1, 'aa'), (2, 'aa'), (3, 'aa'), (4, 'aa');
            INSERT INTO pe_info (observation_id) VALUES (1), (2), (3), (4);
            INSERT INTO authenticode (observation_id) VALUES (1), (2), (3), (4);
            INSERT INTO msi_properties (observation_id) VALUES (1), (2), (3), (4);
            INSERT INTO appx_manifest (observation_id) VALUES (1), (2), (3), (4);
            """;
        commande.ExecuteNonQuery();
        return chemin;
    }

    [Fact]
    public void Base_v2_ne_lit_que_le_dernier_scan_de_chaque_volume()
    {
        var modele = new LecteurDObservationsSqlite(BaseV2DeuxVolumes()).ProjeterModele();

        // L'observation 3 (rescan de vol-a) et la 4 (vol-b) sont courantes ; 1 et 2 sont remplacées.
        Assert.Equal([3L, 4L], modele.Actes.Select(a => a.Identifiant));
    }

    [Fact]
    public void Base_v2_le_contexte_est_filtre_comme_les_actes()
    {
        var contexte = new LecteurDObservationsSqlite(BaseV2DeuxVolumes()).ProjeterContexte();

        Assert.Equal([3L, 4L], contexte.Select(c => c.Identifiant));
    }

    [Fact]
    public void Base_v2_lidentite_declare_la_version_2()
    {
        var identite = new LecteurDObservationsSqlite(BaseV2DeuxVolumes()).ProjeterIdentite();

        Assert.Equal(2, identite.Version);
        Assert.Equal(2, identite.NombreActes);
    }

    [Fact]
    public void Base_v2_sans_scan_produit_un_omega_vide()
    {
        var chemin = NouveauCheminDeBase();
        MiniBaseDObservations.CreerConformeV2(chemin);

        var modele = new LecteurDObservationsSqlite(chemin).ProjeterModele();

        Assert.Empty(modele.Actes);
    }
```

- [ ] **Step 3: Vérifier l'échec**

Run: `dotnet test tests/InstallChecker.Identity.Tests --filter "FullyQualifiedName~LecteurDObservationsSqliteTests"`
Expected: les 4 nouveaux tests échouent avec `OmegaIncompatibleException` (« user_version=2, attendu 1 ») ; les anciens passent.

- [ ] **Step 4: Implémenter le lecteur bi-version**

Dans `src/InstallChecker.Identity.Access/Observations/LecteurDObservationsSqlite.cs` :

1. Remplacer la constante de version et ajouter les fragments de filtre :

```csharp
    // Versions de contrat acceptées à la lecture : v1 (lecture intégrale — l'oracle de conformité
    // reste lisible tel quel) et v2 (état courant multi-disque). Le producteur, lui, n'écrit que v2.
    private static readonly long[] VersionsSupportees = [1, 2];

    // v2 : l'état courant = le dernier scan de chaque volume (spec multi-disque D1/D4).
    private const string ScansCourants = "(SELECT MAX(id) FROM scans GROUP BY volume_id)";
    private const string ObservationsCourantes =
        "(SELECT id FROM scan_observations WHERE scan_id IN " + ScansCourants + ")";
```

2. `Ouvrir` expose la version lue :

```csharp
    private SqliteConnection Ouvrir(out long version)
    {
        if (!File.Exists(cheminBase))
        {
            throw new OmegaAbsentException($"support d'observations introuvable : {cheminBase}");
        }

        var connection = new SqliteConnection($"Data Source={cheminBase};Mode=ReadOnly");
        try
        {
            connection.Open();

            using var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA user_version;";
            version = (long)pragma.ExecuteScalar()!;

            if (!VersionsSupportees.Contains(version))
            {
                throw new OmegaIncompatibleException(
                    $"version de contrat non supportée : {cheminBase} : user_version={version}, attendu 1 ou 2");
            }
        }
        catch (SqliteException ex)
        {
            connection.Dispose();
            throw new OmegaAbsentException($"support d'observations illisible : {cheminBase} : {ex.Message}");
        }
        catch
        {
            connection.Dispose();
            throw;
        }

        return connection;
    }
```

3. `ProjeterModele` et `ProjeterContexte` propagent la version aux lectures :

```csharp
    public ModeleObservations ProjeterModele()
    {
        using var connection = Ouvrir(out var version);

        var actesBruts = LireActesBruts(connection, version);
        var attributsParActe = actesBruts.Keys.ToDictionary(id => id, _ => new Dictionary<Attribut, ValeurObservee>());

        foreach (var table in TablesDeCapacite)
        {
            LireCapacite(connection, table, version, actesBruts.Keys, attributsParActe);
        }

        var actes = actesBruts
            .OrderBy(kv => kv.Key)
            .Select(kv => new ActeObservation(kv.Key, kv.Value.Taille, kv.Value.Empreinte, attributsParActe[kv.Key]))
            .ToList();

        return new ModeleObservations(actes);
    }
```

Dans `LireActesBruts(SqliteConnection connection, long version)`, la requête devient :

```csharp
            commande.CommandText = version == 2
                ? "SELECT id, size, sha256 FROM scan_observations WHERE scan_id IN " + ScansCourants + ";"
                : "SELECT id, size, sha256 FROM scan_observations;";
```

Dans `LireCapacite(SqliteConnection connection, string table, long version, ...)` :

```csharp
            commande.CommandText = version == 2
                ? $"SELECT * FROM {table} WHERE observation_id IN {ObservationsCourantes};"
                : $"SELECT * FROM {table};";
```

(les contrôles existants — orphelin, doublon, complétude 1:1 — restent inchangés : ils s'appliquent désormais à l'ensemble courant).

Dans `ProjeterContexte`, même bascule :

```csharp
            commande.CommandText = version == 2
                ? "SELECT id, path, scanned_at FROM scan_observations WHERE scan_id IN " + ScansCourants + ";"
                : "SELECT id, path, scanned_at FROM scan_observations;";
```

et l'ouverture devient `using var connection = Ouvrir(out _);` si la version n'y sert pas ailleurs.

4. `ProjeterIdentite` déclare la version réelle du support (025 : la version est celle que le support déclare, jamais une présomption) :

```csharp
    /// <summary>L'identité de l'état (025 §§ 3–4) : produite par le support — sa fonction déclarée (SHA-256) et la version de contrat qu'il déclare (1 ou 2) sur l'encodage des couples (identifiant, empreinte).</summary>
    public IndexOmega ProjeterIdentite()
    {
        long version;
        using (Ouvrir(out version)) { } // lit et valide la version déclarée par le support
        return IdentiteDeLEtatOmega.Calculer(ProjeterModele(), version);
    }
```

5. Mettre à jour le commentaire de classe : « schéma `user_version = 1` » devient « schéma `user_version` 1 ou 2 (v2 : état courant multi-disque, filtré au dernier scan par volume) ».

- [ ] **Step 5: Mettre à jour les tests qui simulaient « version non supportée » avec 2**

Trois tests existants estampillent `user_version = 2` pour simuler une version *non supportée* — 2 devient supportée, la valeur de fixture passe à 3 (l'intention des tests — « une version inconnue est refusée » — est inchangée) :

1. `tests/InstallChecker.Identity.Tests/LecteurDObservationsSqliteTests.cs`, test `User_version_incorrect_est_refuse_comme_incompatible` (~ligne 123) : `"PRAGMA user_version = 2;"` → `"PRAGMA user_version = 3;"`.
2. `tests/InstallChecker.Identity.Tests/ConformiteV2Tests.cs`, fabrique `BaseVersionNonSupportee` (~ligne 68) : `"PRAGMA user_version = 2;"` → `"PRAGMA user_version = 3;"`. **Suite de conformité** : seule la valeur de fixture change, jamais l'assertion — si un test de cette suite affirme le contenu du message (`user_version=2`), adapter la valeur affirmée en `user_version=3`.
3. `tests/InstallChecker.Tests/IdentityCommandTests.cs`, fabrique `BaseVersionNonSupportee` (~ligne 52) : `"PRAGMA user_version = 2;"` → `"PRAGMA user_version = 3;"` (même règle pour une éventuelle assertion de message).

(Le pendant **producteur** — `ScanCommandTests.Scan_UnknownUserVersion_ReturnsOneWithExplicitError` — casse seulement quand `SchemaVersion` passera à 2 : il est traité en tâche 3.)

- [ ] **Step 6: Vérifier le passage — y compris l'oracle v1 intact**

Run: `dotnet test tests/InstallChecker.Identity.Tests && dotnet test tests/InstallChecker.Tests`
Expected: PASS complet — les tests d'or v1 (`Lit_loracle_complet_497_actes`, etc.) inchangés et verts, les 4 nouveaux tests v2 verts, les 3 tests « version non supportée » verts avec la valeur 3.

- [ ] **Step 7: Commit**

```bash
git add src/InstallChecker.Identity.Access/Observations/LecteurDObservationsSqlite.cs tests/InstallChecker.Identity.Tests/MiniBaseDObservations.cs tests/InstallChecker.Identity.Tests/LecteurDObservationsSqliteTests.cs tests/InstallChecker.Identity.Tests/ConformiteV2Tests.cs tests/InstallChecker.Tests/IdentityCommandTests.cs
git commit -m "feat(multi-disque): lecteur Omega bi-version - etat courant (dernier scan par volume) sur les bases v2"
```

---

### Task 3: Producteur v2 — table `scans`, `scan_id`, identité de volume au scan

**Files:**
- Modify: `src/InstallChecker/ObservationStore.cs`
- Modify: `src/InstallChecker/ScanCommand.cs`
- Test: `tests/InstallChecker.Tests/ScanCommandTests.cs`

**Interfaces:**
- Consumes: `VolumeIdentityExtractor.Resolve(string)` → `VolumeIdentity(string VolumeId, string? VolumeLabel)` (tâche 1).
- Produces: `public sealed record ScanDeclaration(string VolumeId, string? VolumeLabel, string RootPath, string StartedAt, string? Extensions)` (dans `ObservationStore.cs`) ; `ObservationStore` a pour constructeur `ObservationStore(string dbPath, ScanDeclaration scan)` et écrit `user_version = 2`. La signature publique `ScanCommand.Run(root, dbPath, jsonOutput, output, errors, extensions)` ne change pas.

- [ ] **Step 1: Écrire les tests qui échouent**

Dans `tests/InstallChecker.Tests/ScanCommandTests.cs`, ajouter :

```csharp
    [Fact]
    public void Scan_enregistre_le_scan_avec_volume_racine_et_extensions()
    {
        File.WriteAllText(Path.Combine(_root, "a.exe"), "x");

        var exitCode = ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null, ["exe"]);
        Assert.Equal(0, exitCode);

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        using var version = connection.CreateCommand();
        version.CommandText = "PRAGMA user_version;";
        Assert.Equal(2L, version.ExecuteScalar());

        using var scans = connection.CreateCommand();
        scans.CommandText = "SELECT id, volume_id, volume_label, root_path, started_at, extensions FROM scans;";
        using var lecteur = scans.ExecuteReader();
        Assert.True(lecteur.Read());
        var scanId = lecteur.GetInt64(0);
        Assert.Matches("^[0-9a-f]{8}$", lecteur.GetString(1)); // série du volume local des dossiers temporaires
        Assert.Equal(Path.GetFullPath(_root), lecteur.GetString(3));
        Assert.False(lecteur.IsDBNull(4));
        Assert.Equal("exe", lecteur.GetString(5));
        Assert.False(lecteur.Read()); // un scan = exactement une ligne

        using var observations = connection.CreateCommand();
        observations.CommandText = "SELECT DISTINCT scan_id FROM scan_observations;";
        Assert.Equal(scanId, observations.ExecuteScalar());
    }

    [Fact]
    public void Deux_scans_dans_la_meme_base_ajoutent_deux_lignes_scans_sans_rien_effacer()
    {
        File.WriteAllText(Path.Combine(_root, "a.txt"), "x");
        Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));
        Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();
        using var scans = connection.CreateCommand();
        scans.CommandText = "SELECT COUNT(*) FROM scans;";
        Assert.Equal(2L, scans.ExecuteScalar());
        using var observations = connection.CreateCommand();
        observations.CommandText = "SELECT COUNT(*) FROM scan_observations;";
        Assert.Equal(2L, observations.ExecuteScalar()); // append-only : les deux observations coexistent
    }
```

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/InstallChecker.Tests --filter "FullyQualifiedName~ScanCommandTests"`
Expected: les 2 nouveaux tests échouent (`user_version` = 1, table `scans` absente) ; les anciens passent.

- [ ] **Step 3: Implémenter ObservationStore v2**

Dans `src/InstallChecker/ObservationStore.cs` :

1. Ajouter le record de déclaration (après `FileObservation`) :

```csharp
/// <summary>
/// La déclaration du scan en cours (spec multi-disque D2/D3) : l'identité observée du volume,
/// la racine et le filtre d'extensions tels que passés — conservés pour que l'éviction de l'état
/// précédent du volume reste explicable. Simple objet de transmission — aucune logique.
/// </summary>
public sealed record ScanDeclaration(string VolumeId, string? VolumeLabel, string RootPath, string StartedAt, string? Extensions);
```

2. `SchemaVersion` passe à 2 :

```csharp
    /// <summary>Version de schéma écrite et exigée dans PRAGMA user_version. Aucune migration : version inconnue = erreur.</summary>
    public const long SchemaVersion = 2;
```

3. Le DDL gagne la table `scans` (avant `scan_observations`) et la colonne `scan_id` :

```sql
                CREATE TABLE IF NOT EXISTS scans (
                    id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    volume_id    TEXT NOT NULL,
                    volume_label TEXT,
                    root_path    TEXT NOT NULL,
                    started_at   TEXT NOT NULL,
                    extensions   TEXT
                );
                CREATE TABLE IF NOT EXISTS scan_observations (
                    id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    scan_id    INTEGER NOT NULL,
                    path       TEXT NOT NULL,
                    size       INTEGER NOT NULL,
                    sha256     TEXT NOT NULL,
                    scanned_at TEXT NOT NULL
                );
```

4. Le constructeur devient `public ObservationStore(string dbPath, ScanDeclaration scan)` ; après `_transaction = _connection.BeginTransaction();`, insérer la ligne du scan (dans la transaction unique : un scan interrompu ne laisse ni scan ni observations) :

```csharp
        _transaction = _connection.BeginTransaction();

        using (var insertScan = _connection.CreateCommand())
        {
            insertScan.Transaction = _transaction;
            insertScan.CommandText = """
                INSERT INTO scans (volume_id, volume_label, root_path, started_at, extensions)
                VALUES ($volumeId, $volumeLabel, $rootPath, $startedAt, $extensions);
                SELECT last_insert_rowid();
                """;
            insertScan.Parameters.AddWithValue("$volumeId", scan.VolumeId);
            insertScan.Parameters.AddWithValue("$volumeLabel", (object?)scan.VolumeLabel ?? DBNull.Value);
            insertScan.Parameters.AddWithValue("$rootPath", scan.RootPath);
            insertScan.Parameters.AddWithValue("$startedAt", scan.StartedAt);
            insertScan.Parameters.AddWithValue("$extensions", (object?)scan.Extensions ?? DBNull.Value);
            _scanId = (long)insertScan.ExecuteScalar()!;
        }
```

avec le champ `private readonly long _scanId;` déclaré près de `_transaction`.

5. L'INSERT d'observation porte `scan_id` :

```csharp
        _insert.CommandText = """
            INSERT INTO scan_observations (scan_id, path, size, sha256, scanned_at) VALUES ($scanId, $path, $size, $sha256, $scannedAt);
            SELECT last_insert_rowid();
            """;
        _insert.Parameters.AddWithValue("$scanId", _scanId);
```

(les quatre paramètres existants `_pPath`/`_pSize`/`_pSha256`/`_pScannedAt` sont inchangés ; `$scanId` est fixé une fois, jamais réassigné dans `Persist`).

- [ ] **Step 4: Brancher ScanCommand sur l'identité de volume**

Dans `src/InstallChecker/ScanCommand.cs`, entre le contrôle `Directory.Exists` et la construction du store :

```csharp
        // Identité du volume résolue avant toute écriture : un volume irrésoluble corromprait le
        // remplacement de l'état courant (spec multi-disque D3) — on refuse de démarrer.
        VolumeIdentity volume;
        try
        {
            volume = VolumeIdentityExtractor.Resolve(root);
        }
        catch (InvalidOperationException ex)
        {
            errors.WriteLine(ex.Message);
            return 1;
        }

        var declaration = new ScanDeclaration(
            volume.VolumeId,
            volume.VolumeLabel,
            Path.GetFullPath(root),
            DateTime.UtcNow.ToString("O"),
            extensions is { Count: > 0 } ? string.Join(",", extensions) : null);

        ObservationStore store;
        try
        {
            store = new ObservationStore(dbPath, declaration);
        }
```

(le reste du corps — gestion `SqliteException`/`InvalidDataException`, boucle, résumé — est inchangé ; le message « base incompatible … attendu 2 » suit automatiquement `SchemaVersion`).

- [ ] **Step 4bis: Mettre à jour les deux tests côté producteur touchés par le bump**

1. `tests/InstallChecker.Tests/ScanCommandTests.cs`, test `Scan_UnknownUserVersion_ReturnsOneWithExplicitError` (~ligne 132) : la valeur 2 devient la version supportée — estampiller 3 et adapter l'assertion :

```csharp
            stamp.CommandText = "PRAGMA user_version = 3;";
```

et

```csharp
        Assert.Contains("user_version=3", errors.ToString());
```

2. `tests/InstallChecker.Tests/IdentityCommandTests.cs`, fabrique `BaseVersionNonSupportee` (~ligne 48) : le constructeur d'`ObservationStore` exige désormais une déclaration de scan :

```csharp
        using (var store = new ObservationStore(chemin,
            new ScanDeclaration("vol-test", null, @"C:\", "2026-01-01T00:00:00Z", null))) store.Commit();
```

- [ ] **Step 5: Vérifier le passage — projets InstallChecker.Tests et Identity.Tests**

Run: `dotnet test tests/InstallChecker.Tests && dotnet test tests/InstallChecker.Identity.Tests`
Expected: PASS complet — dont `FrontiereDeDonneesTests` (scan réel désormais v2, lu par le lecteur bi-version de la tâche 2) et les anciens `ScanCommandTests`.

- [ ] **Step 6: Commit**

```bash
git add src/InstallChecker/ObservationStore.cs src/InstallChecker/ScanCommand.cs tests/InstallChecker.Tests/ScanCommandTests.cs tests/InstallChecker.Tests/IdentityCommandTests.cs
git commit -m "feat(multi-disque): schema v2 - table scans, scan_id, identite de volume declaree au scan"
```

---

### Task 4: Le volume de chaque exemplaire dans le rapport `duplicates`

**Files:**
- Create: `src/InstallChecker.DuplicateFiles/VolumeDuFichier.cs`
- Create: `src/InstallChecker/LecteurDeVolumes.cs`
- Modify: `src/InstallChecker.DuplicateFiles/FichierEnrichi.cs`
- Modify: `src/InstallChecker.DuplicateFiles/EnrichisseurDeGroupe.cs`
- Modify: `src/InstallChecker.DuplicateFiles/GenerateurDeRapport.cs`
- Modify: `src/InstallChecker/DuplicatesCommand.cs`
- Test: `tests/InstallChecker.DuplicateFiles.Tests/GenerateurDeRapportTests.cs`, `tests/InstallChecker.Tests/LecteurDeVolumesTests.cs`

**Interfaces:**
- Consumes: le schéma v2 (tâche 3) ; `GenerateurDeRapport.Generer(W, IObservationsSource)` existant.
- Produces: `public sealed record VolumeDuFichier(string VolumeId, string? VolumeLabel)` (namespace `InstallChecker.DuplicateFiles`) ; `GenerateurDeRapport.Generer(W w, IObservationsSource omega, IReadOnlyDictionary<long, VolumeDuFichier>? volumes = null)` ; `FichierEnrichi` gagne `string? VolumeId = null, string? VolumeLabel = null` en fin de paramètres ; `public static class LecteurDeVolumes { public static IReadOnlyDictionary<long, VolumeDuFichier> Lire(string cheminBase); }` (namespace `InstallChecker`, dictionnaire vide sur une base v1).

- [ ] **Step 1: Écrire le test module qui échoue**

Dans `tests/InstallChecker.DuplicateFiles.Tests/GenerateurDeRapportTests.cs`, ajouter :

```csharp
    [Fact]
    public void Les_volumes_fournis_apparaissent_sur_les_exemplaires_et_leur_absence_donne_null()
    {
        var w = new W(IndexDeTest, [Election(1, 2)]);
        var volumes = new Dictionary<long, VolumeDuFichier> { [1] = new("vol-a", "Data") };

        var rapport = GenerateurDeRapport.Generer(w, OmegaDeuxFichiers(), volumes);

        var exemplaires = Assert.Single(rapport.Groupes).Exemplaires;
        var fichier1 = exemplaires.Single(e => e.Fichier.ActeId == 1).Fichier;
        var fichier2 = exemplaires.Single(e => e.Fichier.ActeId == 2).Fichier;
        Assert.Equal("vol-a", fichier1.VolumeId);
        Assert.Equal("Data", fichier1.VolumeLabel);
        Assert.Null(fichier2.VolumeId);   // pas d'entrée volume pour l'acte 2 : absence, jamais une erreur
        Assert.Null(fichier2.VolumeLabel);
    }

    [Fact]
    public void Sans_volumes_le_rapport_reste_celui_daujourdhui()
    {
        var w = new W(IndexDeTest, [Election(1, 2)]);

        var rapport = GenerateurDeRapport.Generer(w, OmegaDeuxFichiers());

        var exemplaires = Assert.Single(rapport.Groupes).Exemplaires;
        Assert.All(exemplaires, e => Assert.Null(e.Fichier.VolumeId));
    }
```

- [ ] **Step 2: Vérifier l'échec**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests --filter "FullyQualifiedName~GenerateurDeRapportTests"`
Expected: échec de compilation — `VolumeDuFichier` et le paramètre `volumes` n'existent pas.

- [ ] **Step 3: Implémenter les DTO et l'enrichissement**

Créer `src/InstallChecker.DuplicateFiles/VolumeDuFichier.cs` :

```csharp
namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Le volume porteur d'un exemplaire, tel qu'observé au scan (spec multi-disque D5) : avec des
/// lettres de lecteur changeantes, c'est lui — pas le chemin — qui dit sur quel disque physique
/// se trouve chaque copie. DTO du module — aucun type moteur.
/// </summary>
public sealed record VolumeDuFichier(string VolumeId, string? VolumeLabel);
```

Dans `src/InstallChecker.DuplicateFiles/FichierEnrichi.cs`, étendre le record (paramètres optionnels : les fabricants existants — tests du classement notamment — restent valides, un fichier sans volume est une absence légitime) :

```csharp
public sealed record FichierEnrichi(
    long ActeId,
    string Chemin,
    long Taille,
    bool SignatureAuthenticodePresente,
    bool EstUnPeLisible,
    bool PresenceMetadonneesMsi,
    string DateDObservation,
    string? VolumeId = null,
    string? VolumeLabel = null);
```

et compléter le commentaire XML : « `VolumeId`/`VolumeLabel` — le volume observé au scan (spec multi-disque D5), `null` sur une base v1 ».

Dans `src/InstallChecker.DuplicateFiles/EnrichisseurDeGroupe.cs` :

```csharp
    public static IReadOnlyList<FichierEnrichi> Enrichir(
        IReadOnlyList<long> domaine,
        IReadOnlyDictionary<long, ActeObservation> actes,
        IReadOnlyDictionary<long, ContexteObservation> contextes,
        IReadOnlyDictionary<long, VolumeDuFichier>? volumes = null) =>
        domaine.Select(id =>
        {
            var acte = actes[id];
            var contexte = contextes[id];
            var volume = volumes is not null && volumes.TryGetValue(id, out var v) ? v : null;
            return new FichierEnrichi(
                id,
                contexte.Chemin,
                acte.Taille,
                ValeurPresente(acte, new Attribut("authenticode", "subject")),
                ValeurPresente(acte, new Attribut("pe_info", "machine")),
                ValeurPresente(acte, new Attribut("msi_properties", "product_name")),
                contexte.DateDeScan,
                volume?.VolumeId,
                volume?.VolumeLabel);
        }).ToList();
```

(seuls le paramètre `volumes`, la variable `volume` et les deux derniers arguments changent — le reste du corps est l'existant, dont `contexte.DateDeScan`).

Dans `src/InstallChecker.DuplicateFiles/GenerateurDeRapport.cs` :

```csharp
    public static RapportDeDoublons Generer(
        W w, IObservationsSource omega, IReadOnlyDictionary<long, VolumeDuFichier>? volumes = null)
```

et dans la boucle : `var fichiers = EnrichisseurDeGroupe.Enrichir(groupeActeW.Domaine, actes, contextes, volumes);`

- [ ] **Step 4: Vérifier le passage du test module**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests`
Expected: PASS complet (nouveaux tests + tests de classement existants, qui construisent `FichierEnrichi` sans les nouveaux paramètres).

- [ ] **Step 5: Écrire le test du lecteur de volumes qui échoue**

Créer `tests/InstallChecker.Tests/LecteurDeVolumesTests.cs` :

```csharp
using InstallChecker.DuplicateFiles;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

public class LecteurDeVolumesTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("installchecker-volumes-").FullName;
    private readonly string _dbDir = Directory.CreateTempSubdirectory("installchecker-volumes-db-").FullName;
    private string DbPath => Path.Combine(_dbDir, "test.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
        Directory.Delete(_dbDir, recursive: true);
    }

    [Fact]
    public void Une_base_v2_donne_le_volume_du_scan_pour_chaque_observation_courante()
    {
        File.WriteAllText(Path.Combine(_root, "a.exe"), "x");
        Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));

        var volumes = LecteurDeVolumes.Lire(DbPath);

        var volume = Assert.Single(volumes).Value;
        Assert.Matches("^[0-9a-f]{8}$", volume.VolumeId);
    }

    [Fact]
    public void Une_base_v1_donne_un_dictionnaire_vide()
    {
        using (var connection = new SqliteConnection($"Data Source={DbPath}"))
        {
            connection.Open();
            using var commande = connection.CreateCommand();
            commande.CommandText = "PRAGMA user_version = 1;";
            commande.ExecuteNonQuery();
        }

        Assert.Empty(LecteurDeVolumes.Lire(DbPath));
    }
}
```

- [ ] **Step 6: Vérifier l'échec**

Run: `dotnet test tests/InstallChecker.Tests --filter "FullyQualifiedName~LecteurDeVolumesTests"`
Expected: échec de compilation — `LecteurDeVolumes` n'existe pas.

- [ ] **Step 7: Implémenter LecteurDeVolumes et le câblage de la commande**

Créer `src/InstallChecker/LecteurDeVolumes.cs` :

```csharp
using InstallChecker.DuplicateFiles;
using Microsoft.Data.Sqlite;

namespace InstallChecker;

/// <summary>
/// Lit, pour chaque observation de l'état courant d'une base v2, le volume observé au scan
/// (spec multi-disque D5) — jointure scan_id → scans, aucune valeur recalculée. Sur une base v1
/// (aucune table scans) : dictionnaire vide, les champs volume du rapport restent absents.
/// Le filtre « dernier scan par volume » est le même que celui du lecteur Ω : les deux lectures
/// décrivent le même état courant.
/// </summary>
public static class LecteurDeVolumes
{
    public static IReadOnlyDictionary<long, VolumeDuFichier> Lire(string cheminBase)
    {
        using var connection = new SqliteConnection($"Data Source={cheminBase};Mode=ReadOnly");
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA user_version;";
        if ((long)pragma.ExecuteScalar()! != 2)
            return new Dictionary<long, VolumeDuFichier>();

        using var commande = connection.CreateCommand();
        commande.CommandText = """
            SELECT o.id, s.volume_id, s.volume_label
            FROM scan_observations o
            JOIN scans s ON s.id = o.scan_id
            WHERE o.scan_id IN (SELECT MAX(id) FROM scans GROUP BY volume_id);
            """;

        var volumes = new Dictionary<long, VolumeDuFichier>();
        using var lecteur = commande.ExecuteReader();
        while (lecteur.Read())
        {
            volumes[lecteur.GetInt64(0)] = new VolumeDuFichier(
                lecteur.GetString(1),
                lecteur.IsDBNull(2) ? null : lecteur.GetString(2));
        }

        return volumes;
    }
}
```

Dans `src/InstallChecker/DuplicatesCommand.cs`, remplacer la génération du rapport :

```csharp
            var omega = new LecteurDObservationsSqlite(cheminBase);
            var w = Porteur.Deriver(omega, new LecteurDeRegistreMarkdown(cheminRegistre));
            // Volumes lus après la dérivation : une base absente/incompatible est déjà refusée par
            // le moteur avec son erreur contractuelle, jamais par ce lecteur annexe.
            var rapport = GenerateurDeRapport.Generer(w, omega, LecteurDeVolumes.Lire(cheminBase));
```

(`PlanCommand` est volontairement inchangé — spec D5 : le plan ne porte que des chemins).

- [ ] **Step 8: Vérifier le passage**

Run: `dotnet test tests/InstallChecker.Tests`
Expected: PASS complet — dont `DuplicatesCommandTests` sur l'oracle v1 (champs volume `null`, aucune assertion existante brisée).

- [ ] **Step 9: Commit**

```bash
git add src/InstallChecker.DuplicateFiles/VolumeDuFichier.cs src/InstallChecker.DuplicateFiles/FichierEnrichi.cs src/InstallChecker.DuplicateFiles/EnrichisseurDeGroupe.cs src/InstallChecker.DuplicateFiles/GenerateurDeRapport.cs src/InstallChecker/LecteurDeVolumes.cs src/InstallChecker/DuplicatesCommand.cs tests/InstallChecker.DuplicateFiles.Tests/GenerateurDeRapportTests.cs tests/InstallChecker.Tests/LecteurDeVolumesTests.cs
git commit -m "feat(multi-disque): volume de chaque exemplaire dans le rapport duplicates"
```

---

### Task 5: Bout en bout — un rescan ne fabrique aucun faux doublon

**Files:**
- Test: `tests/InstallChecker.Tests/MultiDisqueTests.cs`

**Interfaces:**
- Consumes: `ScanCommand.Run`, `DuplicatesCommand.Deriver` (tâches 3–4) ; le registre réel du dépôt (`registre/`), motif repris de `DuplicatesCommandTests.RacineDuDepot`.
- Produces: rien — test de la chaîne complète uniquement. Le scénario deux-volumes est déjà couvert au niveau lecteur (tâche 2) : une seule machine de test n'a qu'un volume garanti, on ne le re-teste pas ici.

- [ ] **Step 1: Écrire le test de bout en bout**

Créer `tests/InstallChecker.Tests/MultiDisqueTests.cs` :

```csharp
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

/// <summary>
/// La chaîne complète du chantier multi-disque (spec 2026-07-16, A4) : scan v2 → lecteur Ω filtré
/// à l'état courant → rapport porteur des volumes. Le scénario à deux volumes est prouvé au niveau
/// du lecteur (LecteurDObservationsSqliteTests) — ici, la propriété de bout en bout : un rescan
/// n'introduit aucun faux doublon.
/// </summary>
public class MultiDisqueTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("installchecker-multidisque-").FullName;
    private readonly string _dbDir = Directory.CreateTempSubdirectory("installchecker-multidisque-db-").FullName;
    private string DbPath => Path.Combine(_dbDir, "test.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
        Directory.Delete(_dbDir, recursive: true);
    }

    private static string CheminRegistreReel()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        var racine = repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
        return Path.Combine(racine, "registre");
    }

    [Fact]
    public void Un_rescan_du_meme_volume_ne_fabrique_aucun_faux_doublon_et_le_rapport_porte_le_volume()
    {
        File.WriteAllText(Path.Combine(_root, "original.exe"), "même contenu");
        File.WriteAllText(Path.Combine(_root, "copie.exe"), "même contenu");

        // Deux scans successifs du même dossier : sans état courant, 4 observations de même
        // empreinte donneraient un groupe de 4 « doublons » dont 2 fantômes.
        Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));
        Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));

        var sortie = new StringWriter();
        var erreurs = new StringWriter();
        Assert.Equal(0, DuplicatesCommand.Deriver(DbPath, CheminRegistreReel(), sortie, erreurs));
        Assert.Empty(erreurs.ToString());

        using var json = JsonDocument.Parse(sortie.ToString());
        var groupe = Assert.Single(json.RootElement.GetProperty("Groupes").EnumerateArray());
        var exemplaires = groupe.GetProperty("Exemplaires");
        Assert.Equal(2, exemplaires.GetArrayLength()); // les 2 fichiers du dernier scan, jamais 4

        foreach (var exemplaire in exemplaires.EnumerateArray())
        {
            var fichier = exemplaire.GetProperty("Fichier");
            Assert.Matches("^[0-9a-f]{8}$", fichier.GetProperty("VolumeId").GetString());
        }
    }
}
```

- [ ] **Step 2: Exécuter le test**

Run: `dotnet test tests/InstallChecker.Tests --filter "FullyQualifiedName~MultiDisqueTests"`
Expected: PASS — si `Exemplaires` vaut 4, le filtre d'état courant du lecteur n'est pas appliqué ; si `VolumeId` est absent du JSON, le câblage de la tâche 4 manque.

- [ ] **Step 3: Suite complète**

Run: `dotnet test InstallChecker.slnx`
Expected: PASS complet (les 4 projets de tests).

- [ ] **Step 4: Commit**

```bash
git add tests/InstallChecker.Tests/MultiDisqueTests.cs
git commit -m "test(multi-disque): bout en bout - un rescan ne fabrique aucun faux doublon"
```

---

### Task 6: Documentation — A4 livré, README des commandes, ADR-011, roadmap

**Files:**
- Modify: `docs/projet/ameliorations-duplicate-files-v1.md` (section A4)
- Modify: `modules/duplicate-files/README.md`
- Modify: `CLAUDE.md` (racine du dépôt `installChecker` : nouvel ADR-011 après ADR-010 ; ligne Phase 6 de la roadmap)

**Interfaces:**
- Consumes: l'état livré des tâches 1–5.
- Produces: documentation à jour ; aucun code.

- [ ] **Step 1: Mettre à jour A4**

Dans `docs/projet/ameliorations-duplicate-files-v1.md`, remplacer le paragraphe « **Chantier ouvert.** … » de la section A4 par :

```markdown
**Livré (2026-07-16).** Le snapshot courant est implémenté selon
`docs/superpowers/specs/2026-07-16-multi-disque-design.md` : table `scans` (schéma v2),
identité de volume observée au scan (numéro de série local, racine UNC normalisée),
état courant = dernier scan par volume, appliqué par le lecteur Ω **avant toute dérivation** —
tous les consommateurs (`identity`, `duplicates`, `plan`) partagent le même état courant,
Ω restant append-only. Le lecteur reste bi-version (v1 : lecture intégrale, l'oracle de
conformité est intact ; v2 : état courant). Le rapport `duplicates` porte le volume de
chaque exemplaire.
```

- [ ] **Step 2: Compléter le README des commandes**

Dans `modules/duplicate-files/README.md`, lire le document puis insérer une section « Multi-disque » cohérente avec sa structure existante (après la présentation des commandes) :

```markdown
## Multi-disque

La base est **unique pour tous les disques** (A4) : un doublon entre deux volumes (interne,
USB, NAS) est détecté comme n'importe quel autre. Chaque `scan` enregistre le volume de sa
racine — numéro de série pour un disque local, racine UNC normalisée (`\\serveur\partage`)
pour le réseau, lettre mappée résolue en UNC — sans option nouvelle.

**État courant.** Les rapports (`duplicates`, `plan`, `identity`) ne voient, pour chaque
volume, que son **dernier scan** : rescanner un disque remplace son état précédent sans
toucher aux autres, et sans fabriquer de faux doublons entre deux scans. Rien n'est effacé
(append-only) : « remplacer » signifie sortir de l'état courant.

**Conséquence à connaître** : un scan partiel (sous-dossier, ou `--ext` étroit) remplace
tout l'état courant du volume — les fichiers non re-scannés en sortent. La ligne `scans`
de la base conserve la racine et le filtre utilisés pour que ce soit explicable.

Chaque exemplaire du rapport `duplicates` porte `VolumeId` et `VolumeLabel` : avec des
lettres de lecteur changeantes (USB), c'est le volume qui dit sur quel disque physique se
trouve chaque copie.

Les bases v1 antérieures sont refusées par `scan` (aucune migration, ADR-008) : rescanner.
```

- [ ] **Step 3: Ajouter ADR-011 et mettre à jour la roadmap**

Dans `CLAUDE.md` (racine du dépôt), après ADR-010 :

```markdown
## ADR-011 — Schéma v2 : scans, volumes et état courant multi-disque

- **Contexte** : A4 — base unique pour tous les disques ; avec des rescans, l'append-only (ADR-002) empile les observations d'un même fichier et fabrique de faux doublons entre deux scans.
- **Décision** : schéma `user_version = 2` — table `scans` (identité de volume observée : numéro de série local ou racine UNC normalisée ; racine et filtre `--ext` conservés pour l'explicabilité) et colonne `scan_id` sur les observations. L'**état courant** = le dernier scan de chaque volume, appliqué par le lecteur Ω avant toute dérivation : tous les consommateurs partagent le même état, Ω reste append-only. Le lecteur accepte v1 (lecture intégrale — l'oracle de conformité v3 reste lisible tel quel) et v2 ; le producteur n'écrit que v2 (025 : le support déclare sa version de contrat).
- **Alternatives** : filtrage par consommateur (rejeté — trois implémentations divergentes du même « courant ») ; flag « courant » maintenu à l'écriture (rejeté — UPDATE interdit par ADR-002) ; lecteur v2 seul (rejeté — casserait le test d'or v1 et rouvrirait la conformité v3).
- **Conséquences** : bases v1 rescannées (ADR-008, aucune migration) ; un scan partiel remplace tout l'état courant de son volume ; rejouer un état passé reste possible (l'historique est conservé) mais n'est pas exposé.
```

Dans la roadmap (§ 18), compléter la ligne Phase 6 :

```markdown
- Phase 6 : premier module métier « Duplicate Files » — **en cours** (rapport de doublons `duplicates`, plan de suppression `plan`, politique de rétention versionnée, sélection de corpus `--ext`, gestion multi-disque : schéma v2, état courant par volume, volumes dans le rapport)
```

- [ ] **Step 4: Commit**

```bash
git add docs/projet/ameliorations-duplicate-files-v1.md modules/duplicate-files/README.md CLAUDE.md
git commit -m "docs(duplicate-files): A4 livre - multi-disque (ADR-011, README, roadmap)"
```

---

## Notes de gouvernance (hors plan, à l'attention du relecteur)

- Le document `docs/identity/013` mentionne « le schéma documenté (`user_version = 1`) » comme frontière de données. Le lecteur bi-version est conforme au 025 (« le support déclare sa version de contrat ») et ne touche pas au moteur pur ; si une mise à jour de la série identity est souhaitée (note ou erratum au 013), c'est un acte de gouvernance documentaire séparé, hors de ce plan.
- La tâche 2 modifie une **valeur de fixture** dans `ConformiteV2Tests` (2 → 3 pour « version non supportée ») : l'invariant testé est inchangé, mais la suite de conformité est touchée — à signaler dans le message de commit et à la relecture.
- La branche « lettre mappée réseau » de `VolumeIdentityExtractor` (WNetGetConnection) n'est pas couverte automatiquement — à exercer manuellement avec un lecteur mappé réel.
