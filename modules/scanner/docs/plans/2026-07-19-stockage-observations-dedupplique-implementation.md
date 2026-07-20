# Stockage dédupliqué des observations Scanner - Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stocker une seule fois les observations brutes identiques tout en conservant une occurrence distincte par chemin et par scan, puis exposer le même état Omega aux consommateurs existants.

**Architecture:** Le schéma SQLite v3 sépare les snapshots immuables des entrées de scan. Un lecteur v3 appartenant au module Scanner implémente `IObservationsSource`, tandis qu'une fabrique délègue les bases v1/v2 au lecteur Identity.Access gelé. Les commandes consomment la fabrique sans modifier Identity ni Identity.Access.

**Tech Stack:** .NET 10, C# 14, Microsoft.Data.Sqlite 10.0.9, xUnit 2.9.3, SHA-256 BCL.

## Global Constraints

- Ne modifier aucun fichier sous `src/InstallChecker.Identity`, `src/InstallChecker.Identity.Access`, `tests/InstallChecker.Identity.Tests`, `tests/oracle`, `docs/identity`, `docs/conformite` ou `registre`.
- Tout nouveau code de stockage ou de lecture v3 vit sous `modules/scanner`.
- Les observations restent brutes : aucune normalisation métier dans le snapshot.
- Le chemin et la date ne participent jamais à la clé du snapshot.
- Une occurrence distincte est conservée pour chaque chemin d'un scan.
- Le dernier scan de chaque volume définit toujours l'état courant.
- Les bases v1/v2 restent lisibles ; Scanner écrit uniquement v3 et ne migre aucune base.
- Le JSON n'est émis qu'après commit ; `--json-file` remplace le fichier et n'utilise jamais append.
- Chaque changement suit un cycle test rouge, implémentation minimale, test vert.
- Les étapes Git sont proposées comme checkpoints mais ne sont exécutées que sur demande explicite dans l'arbre de travail actuel.

---

## File Structure

### Nouveaux fichiers

- `modules/scanner/src/InstallChecker.Scanner/CleSnapshotObservation.cs` : encodage canonique et clé SHA-256 du snapshot.
- `modules/scanner/src/InstallChecker.Scanner.Observations/InstallChecker.Scanner.Observations.csproj` : adaptateur de lecture du stockage Scanner vers le port public Identity.
- `modules/scanner/src/InstallChecker.Scanner.Observations/SourceObservationsSqlite.cs` : fabrique v1/v2/v3.
- `modules/scanner/src/InstallChecker.Scanner.Observations/LecteurObservationsSqliteV3.cs` : projection v3 vers `IObservationsSource`.
- `modules/scanner/tests/InstallChecker.Scanner.Tests/CleSnapshotObservationTests.cs` : contrat canonique de clé.
- `modules/scanner/tests/InstallChecker.Scanner.Tests/ObservationStoreV3Tests.cs` : schéma, déduplication et collision.
- `modules/scanner/tests/InstallChecker.Scanner.Observations.Tests/InstallChecker.Scanner.Observations.Tests.csproj` : tests du lecteur et de la fabrique.
- `modules/scanner/tests/InstallChecker.Scanner.Observations.Tests/LecteurObservationsSqliteV3Tests.cs` : projections modèle, contexte et identité.
- `modules/scanner/tests/InstallChecker.Scanner.Observations.Tests/SourceObservationsSqliteTests.cs` : délégation v1/v2 et sélection v3.

### Fichiers modifiés

- `modules/scanner/src/InstallChecker.Scanner/ObservationStore.cs` : propriétaire du schéma v3 et des écritures normalisées.
- `modules/scanner/src/InstallChecker.Scanner/ScanCommand.cs` : émission JSON post-commit et fichier JSON remplacé.
- `modules/scanner/tests/InstallChecker.Scanner.Tests/ScanCommandTests.cs` : attentes v3 et requêtes physiques.
- `modules/scanner/src/InstallChecker.Scanner/InstallChecker.Scanner.csproj` : visibilité interne de la clé aux tests, sans dépendance Identity.
- `modules/duplicate-files/src/InstallChecker.DuplicateFiles/*.cs` : utilisation de la fabrique Scanner.
- `modules/duplicate-files/src/InstallChecker.DuplicateFiles/LecteurDeVolumes.cs` : lecture des volumes v2 et v3.
- `modules/duplicate-files/src/InstallChecker.DuplicateFiles/InstallChecker.DuplicateFiles.csproj` : référence vers l'adaptateur Scanner.
- `apps/cli/InstallChecker.Cli/IdentityCommand.cs` : utilisation de la fabrique Scanner.
- `apps/cli/InstallChecker.Cli/Program.cs` : option `--json-file`.
- `apps/cli/InstallChecker.Cli/InstallChecker.Cli.csproj` : référence vers l'adaptateur Scanner.
- `apps/cli/tests/InstallChecker.Cli.Tests/FrontiereDeDonneesTests.cs` : lecture des bases Scanner v3 via la fabrique.
- `InstallChecker.slnx` : ajout du projet et de ses tests.
- `modules/scanner/README.md`, spécification, ADR et `AGENTS.md` : documentation du contrat livré.

---

### Task 1: Clé canonique de snapshot

**Files:**
- Create: `modules/scanner/src/InstallChecker.Scanner/CleSnapshotObservation.cs`
- Create: `modules/scanner/tests/InstallChecker.Scanner.Tests/CleSnapshotObservationTests.cs`

**Interfaces:**
- Consumes: `FileObservation` existant.
- Produces: `SnapshotCalcule CleSnapshotObservation.Calculer(FileObservation observation)`.

- [ ] **Step 1: Écrire les tests rouges de stabilité et de séparation**

Créer `CleSnapshotObservationTests.cs` avec un constructeur d'observation complet et les assertions suivantes :

```csharp
[Fact]
public void Le_chemin_et_la_date_ne_modifient_pas_la_cle()
{
    var premiere = Observation(path: @"C:\a\outil.exe", scannedAt: "2026-07-19T10:00:00Z");
    var seconde = Observation(path: @"D:\b\copie.exe", scannedAt: "2026-07-20T11:00:00Z");

    var a = CleSnapshotObservation.Calculer(premiere);
    var b = CleSnapshotObservation.Calculer(seconde);

    Assert.Equal(a.Cle, b.Cle);
    Assert.Equal(a.ChargeCanonique, b.ChargeCanonique);
    Assert.Matches("^snapshot:sha256:[0-9a-f]{64}$", a.Cle);
}

[Fact]
public void Une_valeur_brute_differente_modifie_la_cle()
{
    var reference = CleSnapshotObservation.Calculer(Observation(productVersion: "1.0"));
    var modifiee = CleSnapshotObservation.Calculer(Observation(productVersion: "2.0"));

    Assert.NotEqual(reference.Cle, modifiee.Cle);
}

[Fact]
public void Une_absence_est_distincte_dune_chaine_vide()
{
    var absente = CleSnapshotObservation.Calculer(Observation(companyName: null));
    var vide = CleSnapshotObservation.Calculer(Observation(companyName: ""));

    Assert.NotEqual(absente.Cle, vide.Cle);
}
```

Ajouter un test paramétré qui change successivement taille, hash, `MagicHex`, conteneur et chaque
champ VersionInfo, PE, Authenticode, MSI et Appx ; chaque changement doit produire une clé différente.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/scanner/tests/InstallChecker.Scanner.Tests/InstallChecker.Scanner.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~CleSnapshotObservationTests
```

Expected: échec de compilation car `CleSnapshotObservation` et `SnapshotCalcule` n'existent pas.

- [ ] **Step 3: Implémenter l'encodage canonique**

Créer les types :

```csharp
public sealed record SnapshotCalcule(string Cle, byte[] ChargeCanonique);

public static class CleSnapshotObservation
{
    public const string VersionContrat = "scanner-observation/v1";

    public static SnapshotCalcule Calculer(FileObservation observation)
    {
        ArgumentNullException.ThrowIfNull(observation);
        using var flux = new MemoryStream();
        Ecrire(flux, VersionContrat);
        Ecrire(flux, observation.Size);
        Ecrire(flux, observation.Sha256);
        Ecrire(flux, observation.MagicHex);
        Ecrire(flux, observation.Container);
        // Écrire ensuite tous les champs dans l'ordre déclaré par FileObservation.
        var charge = flux.ToArray();
        var hash = Convert.ToHexString(SHA256.HashData(charge)).ToLowerInvariant();
        return new SnapshotCalcule($"snapshot:sha256:{hash}", charge);
    }
}
```

Utiliser un préfixe de quatre octets big-endian pour les textes UTF-8, `uint.MaxValue` pour `null`,
et un entier signé big-endian pour les nombres. Ne jamais sérialiser `Path` ou `ScannedAt`.

- [ ] **Step 4: Vérifier les tests verts**

Run: commande de l'étape 2.

Expected: tous les tests `CleSnapshotObservationTests` passent.

- [ ] **Step 5: Checkpoint Git proposé**

```powershell
git add modules/scanner/src/InstallChecker.Scanner/CleSnapshotObservation.cs modules/scanner/tests/InstallChecker.Scanner.Tests/CleSnapshotObservationTests.cs
git commit -m "feat(scanner): define canonical observation snapshots"
```

---

### Task 2: Schéma SQLite v3 et déduplication physique

**Files:**
- Modify: `modules/scanner/src/InstallChecker.Scanner/ObservationStore.cs`
- Create: `modules/scanner/tests/InstallChecker.Scanner.Tests/ObservationStoreV3Tests.cs`
- Modify: `modules/scanner/tests/InstallChecker.Scanner.Tests/ScanCommandTests.cs`

**Interfaces:**
- Consumes: `CleSnapshotObservation.Calculer`.
- Produces: `ObservationStore.SchemaVersion == 3`, `Persist(FileObservation) -> long scanEntryId`.

- [ ] **Step 1: Écrire le test rouge du même fichier rescanné**

```csharp
[Fact]
public void Deux_scans_identiques_stockent_un_snapshot_et_deux_occurrences()
{
    File.WriteAllText(Path.Combine(_root, "a.txt"), "identique");

    Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));
    Assert.Equal(0, ScanCommand.Run(_root, DbPath, false, TextWriter.Null, TextWriter.Null));

    using var db = Ouvrir();
    Assert.Equal(1L, Scalaire(db, "SELECT COUNT(*) FROM observation_snapshots"));
    Assert.Equal(2L, Scalaire(db, "SELECT COUNT(*) FROM scan_entries"));
    Assert.Equal(2L, Scalaire(db, "SELECT COUNT(*) FROM scans"));
}
```

Ajouter les tests : deux chemins identiques en contenu partagent un snapshot ; un contenu modifié
crée un second snapshot ; six tables de capacités contiennent exactement une ligne par snapshot.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/scanner/tests/InstallChecker.Scanner.Tests/InstallChecker.Scanner.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~ObservationStoreV3Tests|FullyQualifiedName~Deux_scans"
```

Expected: échec SQL sur les tables v3 absentes ou attente `user_version` incorrecte.

- [ ] **Step 3: Remplacer le DDL v2 par le DDL v3**

Définir `SchemaVersion = 3`, activer `PRAGMA foreign_keys = ON` et créer :

```sql
CREATE TABLE scans (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    volume_id TEXT NOT NULL,
    volume_label TEXT,
    root_path TEXT NOT NULL,
    started_at TEXT NOT NULL,
    extensions TEXT
);
CREATE TABLE observation_snapshots (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    snapshot_key TEXT NOT NULL UNIQUE,
    extraction_contract TEXT NOT NULL,
    canonical_payload BLOB NOT NULL,
    size INTEGER NOT NULL,
    sha256 TEXT NOT NULL
);
CREATE TABLE scan_entries (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    scan_id INTEGER NOT NULL REFERENCES scans(id),
    snapshot_id INTEGER NOT NULL REFERENCES observation_snapshots(id),
    path TEXT NOT NULL,
    path_key TEXT NOT NULL,
    scanned_at TEXT NOT NULL,
    UNIQUE(scan_id, path_key)
);
```

Créer les six tables `snapshot_*` avec `snapshot_id INTEGER PRIMARY KEY REFERENCES
observation_snapshots(id)` et les mêmes colonnes brutes que les tables v2 correspondantes. Ajouter
les index `scan_entries(scan_id)` et `scan_entries(snapshot_id)`.

- [ ] **Step 4: Implémenter get-or-create et la vérification de collision**

Dans `Persist` :

```csharp
var calcule = CleSnapshotObservation.Calculer(o);
var snapshotId = TrouverSnapshot(calcule.Cle);
if (snapshotId is null)
    snapshotId = InsererSnapshotEtCapacites(o, calcule);
else
    VerifierChargeCanonique(snapshotId.Value, calcule.ChargeCanonique);

return InsererEntreeScan(snapshotId.Value, o.Path, o.ScannedAt);
```

`VerifierChargeCanonique` compare les octets avec `SequenceEqual` et lève
`InvalidDataException("collision de clé de snapshot")` en cas de différence. `path_key` utilise le
chemin complet, séparateurs Windows et `ToUpperInvariant`.

- [ ] **Step 5: Adapter les requêtes physiques des tests Scanner**

Remplacer les lectures directes v2 par des jointures explicites :

```sql
SELECT e.path, s.size, s.sha256, e.scanned_at
FROM scan_entries e
JOIN observation_snapshots s ON s.id = e.snapshot_id
ORDER BY e.id;
```

Pour les capacités, joindre `scan_entries e` à la table `snapshot_*` sur `snapshot_id` et filtrer
sur `e.path`. Mettre l'attente `PRAGMA user_version` à `3`. Le test de version inconnue utilise `4`.

- [ ] **Step 6: Vérifier les tests verts du stockage**

Run: commande de l'étape 2, puis :

```powershell
dotnet test modules/scanner/tests/InstallChecker.Scanner.Tests/InstallChecker.Scanner.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~ScanCommandTests"
```

Expected: tous les tests ciblés passent.

- [ ] **Step 7: Checkpoint Git proposé**

```powershell
git add modules/scanner/src/InstallChecker.Scanner/ObservationStore.cs modules/scanner/tests/InstallChecker.Scanner.Tests/ObservationStoreV3Tests.cs modules/scanner/tests/InstallChecker.Scanner.Tests/ScanCommandTests.cs
git commit -m "feat(scanner): deduplicate stored observations in schema v3"
```

---

### Task 3: JSON post-commit et export fichier sans append

**Files:**
- Modify: `modules/scanner/src/InstallChecker.Scanner/ObservationStore.cs`
- Modify: `modules/scanner/src/InstallChecker.Scanner/ScanCommand.cs`
- Modify: `modules/scanner/tests/InstallChecker.Scanner.Tests/ScanCommandTests.cs`

**Interfaces:**
- Produces: `IEnumerable<string> ObservationStore.ProjeterJsonDuScan()` après commit.
- Extends: `ScanCommand.Run(..., IReadOnlyCollection<string>? extensions = null, string? jsonFilePath = null)`.

- [ ] **Step 1: Écrire les tests rouges**

Ajouter :

```csharp
[Fact]
public void Le_json_ne_peut_pas_etre_projete_avant_commit()
{
    using var store = NouveauStore();
    store.Persist(ObservationSimple());
    Assert.Throws<InvalidOperationException>(() => store.ProjeterJsonDuScan().ToList());
}

[Fact]
public void JsonFile_remplace_le_fichier_existant()
{
    var jsonPath = Path.Combine(_dbDir, "scan.jsonl");
    File.WriteAllText(jsonPath, "ancienne ligne");
    File.WriteAllText(Path.Combine(_root, "a.txt"), "x");

    var code = ScanCommand.Run(
        _root, DbPath, false, TextWriter.Null, TextWriter.Null,
        extensions: null, jsonFilePath: jsonPath);

    Assert.Equal(0, code);
    var ligne = Assert.Single(File.ReadAllLines(jsonPath));
    Assert.DoesNotContain("ancienne ligne", ligne);
    using var _ = JsonDocument.Parse(ligne);
}
```

Ajouter un test qui lance deux scans vers le même `jsonFilePath` et vérifie que le nombre de lignes
égale le nombre de fichiers du second scan, jamais la somme des deux scans.

- [ ] **Step 2: Vérifier les échecs ciblés**

Run:

```powershell
dotnet test modules/scanner/tests/InstallChecker.Scanner.Tests/InstallChecker.Scanner.Tests.csproj -c Release --no-restore --filter "FullyQualifiedName~Json|FullyQualifiedName~json"
```

Expected: échec de compilation sur la nouvelle signature ou la projection post-commit.

- [ ] **Step 3: Projeter le JSON depuis les tables validées**

Ajouter un booléen `_commitEffectue`. `Commit` valide la transaction puis passe ce booléen à vrai.
`ProjeterJsonDuScan` refuse l'appel avant commit et exécute une jointure unique entre `scan_entries`,
`observation_snapshots` et les six tables de capacités pour `_scanId`. Sérialiser exactement le
contrat JSONL existant avec `observation_id = scan_entries.id`.

- [ ] **Step 4: Déplacer l'émission JSON après `store.Commit()`**

Dans `ScanCommand.Run`, ne plus appeler `ProjectJson` dans la boucle. Après le commit :

```csharp
if (jsonOutput || jsonFilePath is not null)
{
    TextWriter cible = output;
    StreamWriter? fichier = null;
    try
    {
        if (jsonFilePath is not null)
        {
            fichier = new StreamWriter(jsonFilePath, append: false, new UTF8Encoding(false));
            cible = fichier;
        }
        foreach (var ligne in store.ProjeterJsonDuScan())
            cible.WriteLine(ligne);
    }
    finally
    {
        fichier?.Dispose();
    }
}
```

TSV reste émis pendant la boucle. Refuser `jsonOutput && jsonFilePath is not null` avec code `2` au
niveau CLI ; l'API `ScanCommand` lève `ArgumentException` pour cette combinaison impossible.

- [ ] **Step 5: Vérifier les tests verts**

Run: commande de l'étape 2.

Expected: tous les tests JSON passent et le contrat champ à champ reste identique.

- [ ] **Step 6: Checkpoint Git proposé**

```powershell
git add modules/scanner/src/InstallChecker.Scanner/ObservationStore.cs modules/scanner/src/InstallChecker.Scanner/ScanCommand.cs modules/scanner/tests/InstallChecker.Scanner.Tests/ScanCommandTests.cs
git commit -m "feat(scanner): emit committed JSON snapshots without append"
```

---

### Task 4: Lecteur v3 appartenant au Scanner

**Files:**
- Create: `modules/scanner/src/InstallChecker.Scanner.Observations/InstallChecker.Scanner.Observations.csproj`
- Create: `modules/scanner/src/InstallChecker.Scanner.Observations/LecteurObservationsSqliteV3.cs`
- Create: `modules/scanner/src/InstallChecker.Scanner.Observations/SourceObservationsSqlite.cs`
- Create: `modules/scanner/tests/InstallChecker.Scanner.Observations.Tests/InstallChecker.Scanner.Observations.Tests.csproj`
- Create: `modules/scanner/tests/InstallChecker.Scanner.Observations.Tests/LecteurObservationsSqliteV3Tests.cs`
- Create: `modules/scanner/tests/InstallChecker.Scanner.Observations.Tests/SourceObservationsSqliteTests.cs`
- Modify: `InstallChecker.slnx`

**Interfaces:**
- Produces: `public static IObservationsSource SourceObservationsSqlite.Ouvrir(string cheminBase)`.
- Keeps: v1/v2 delegated to `InstallChecker.Identity.Access.Observations.LecteurDObservationsSqlite`.

- [ ] **Step 1: Créer les projets et tests rouges**

Le projet source référence `Microsoft.Data.Sqlite`, `InstallChecker.Identity` et
`InstallChecker.Identity.Access`. Le projet de tests référence le projet source et
`InstallChecker.Scanner` afin de produire une base réelle v3.

Tests requis :

```csharp
[Fact]
public void V3_projette_une_occurrence_par_chemin_meme_si_le_snapshot_est_partage()
{
    ScannerDeuxCopies(DbPath);
    var source = SourceObservationsSqlite.Ouvrir(DbPath);

    var modele = source.ProjeterModele();
    var contexte = source.ProjeterContexte();

    Assert.Equal(2, modele.Actes.Count);
    Assert.Single(modele.Actes.Select(a => a.Empreinte).Distinct());
    Assert.Equal(2, contexte.Select(c => c.Chemin).Distinct().Count());
}

[Fact]
public void V3_ne_projette_que_le_dernier_scan_du_volume()
{
    ScannerDeuxFoisLeMemeChemin(DbPath);
    Assert.Single(SourceObservationsSqlite.Ouvrir(DbPath).ProjeterModele().Actes);
}
```

Ajouter : attributs des six capacités restitués avec les provenances logiques v2 ; identité Omega
stable entre deux lectures ; base v2 manuelle déléguée au type gelé ; version 4 refusée.

- [ ] **Step 2: Vérifier l'échec de compilation**

Run:

```powershell
dotnet test modules/scanner/tests/InstallChecker.Scanner.Observations.Tests/InstallChecker.Scanner.Observations.Tests.csproj -c Release
```

Expected: échec car les classes du lecteur n'existent pas.

- [ ] **Step 3: Implémenter la fabrique de versions**

```csharp
public static class SourceObservationsSqlite
{
    public static IObservationsSource Ouvrir(string cheminBase)
    {
        var version = LireVersionSiPossible(cheminBase);
        return version == 3
            ? new LecteurObservationsSqliteV3(cheminBase)
            : new LecteurDObservationsSqlite(cheminBase);
    }
}
```

Si le fichier est absent ou SQLite illisible, retourner le lecteur historique pour conserver ses
erreurs contractuelles. Une version autre que 3 est également déléguée au lecteur historique, qui
refuse déjà les versions inconnues.

- [ ] **Step 4: Implémenter le lecteur v3**

`ProjeterModele` exécute :

```sql
SELECT e.id, s.size, s.sha256
FROM scan_entries e
JOIN observation_snapshots s ON s.id = e.snapshot_id
WHERE e.scan_id IN (SELECT MAX(id) FROM scans GROUP BY volume_id);
```

Pour chaque capacité, joindre `scan_entries` à `snapshot_*`, mais créer les attributs avec les noms
logiques historiques (`version_info`, `file_headers`, `pe_info`, `authenticode`, `msi_properties`,
`appx_manifest`). Exiger exactement une ligne de chaque capacité par acte courant.

`ProjeterContexte` lit `e.id`, `e.path`, `e.scanned_at` sur le même état courant.
`ProjeterIdentite` appelle `IdentiteDeLEtatOmega.Calculer(ProjeterModele(), 3)` après validation du
support. Reprendre les types d'erreurs `ErreurOmega` du contrat public, sans texte d'exception système
instable.

- [ ] **Step 5: Ajouter les projets à la solution et vérifier**

Ajouter le projet et ses tests sous `/modules/scanner/` dans `InstallChecker.slnx`.

Run: commande de l'étape 2.

Expected: tous les tests du nouvel adaptateur passent.

- [ ] **Step 6: Checkpoint Git proposé**

```powershell
git add modules/scanner/src/InstallChecker.Scanner.Observations modules/scanner/tests/InstallChecker.Scanner.Observations.Tests InstallChecker.slnx
git commit -m "feat(scanner): expose schema v3 as observations omega"
```

---

### Task 5: Raccorder les consommateurs sans toucher aux zones gelées

**Files:**
- Modify: `apps/cli/InstallChecker.Cli/IdentityCommand.cs`
- Modify: `apps/cli/InstallChecker.Cli/InstallChecker.Cli.csproj`
- Modify: `apps/cli/tests/InstallChecker.Cli.Tests/FrontiereDeDonneesTests.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/DuplicatesCommand.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/PlanCommand.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/RedondanceVersionneeCommand.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/LecteurDeVolumes.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/InstallChecker.DuplicateFiles.csproj`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/MultiDisqueTests.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/RedondanceVersionneeCommandTests.cs`

**Interfaces:**
- Consumes: `SourceObservationsSqlite.Ouvrir`.
- Produces: commandes compatibles v1/v2/v3.

- [ ] **Step 1: Écrire les tests d'intégration rouges v3**

Les tests existants qui scannent une base puis appellent `identity`, `duplicates`, `plan` et
`duplicates versions` deviennent naturellement rouges après Task 2 tant que les commandes utilisent
le lecteur v2. Ajouter explicitement : deux copies à deux chemins partagent un snapshot physique mais
produisent un groupe exact de deux exemplaires.

```csharp
Assert.Equal(1L, Nombre(DbPath, "observation_snapshots"));
var groupe = Assert.Single(RapportDoublons(DbPath).Groupes);
Assert.Equal(2, groupe.Exemplaires.Count);
```

- [ ] **Step 2: Vérifier les échecs ciblés**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj -c Release --no-restore
dotnet test apps/cli/tests/InstallChecker.Cli.Tests/InstallChecker.Cli.Tests.csproj -c Release --no-restore
```

Expected: erreurs de version non supportée sur les bases v3.

- [ ] **Step 3: Remplacer uniquement la construction de la source Omega**

Dans les commandes concernées :

```csharp
var omega = SourceObservationsSqlite.Ouvrir(cheminBase);
```

Ne modifier aucun appel au `Porteur`, au registre ou aux générateurs Duplicate Files. Ajouter les
références projet vers `InstallChecker.Scanner.Observations` dans les deux enveloppes consommatrices.

- [ ] **Step 4: Rendre `LecteurDeVolumes` bi-structure**

Lire `PRAGMA user_version`. Pour v1, conserver le résultat vide. Pour v2, conserver la requête
existante. Pour v3 :

```sql
SELECT e.id, s.volume_id, s.volume_label
FROM scan_entries e
JOIN scans s ON s.id = e.scan_id
WHERE e.scan_id IN (SELECT MAX(id) FROM scans GROUP BY volume_id);
```

- [ ] **Step 5: Vérifier les suites d'intégration vertes**

Run: commandes de l'étape 2.

Expected: toutes les suites passent sur les nouvelles bases v3 ; les tests oracle v1/v2 restent
inchangés.

- [ ] **Step 6: Vérifier immédiatement le gel**

```powershell
git diff -- src/InstallChecker.Identity src/InstallChecker.Identity.Access tests/InstallChecker.Identity.Tests tests/oracle docs/identity docs/conformite registre
```

Expected: aucune sortie.

- [ ] **Step 7: Checkpoint Git proposé**

```powershell
git add apps/cli/InstallChecker.Cli modules/duplicate-files/src/InstallChecker.DuplicateFiles apps/cli/tests/InstallChecker.Cli.Tests modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests
git commit -m "feat(platform): consume deduplicated scanner observations"
```

---

### Task 6: Route CLI `--json-file`

**Files:**
- Modify: `apps/cli/InstallChecker.Cli/Program.cs`
- Modify: `modules/scanner/README.md`
- Test: `apps/cli/tests/InstallChecker.Cli.Tests/ScanRouteTests.cs`

**Interfaces:**
- Consumes: paramètre `jsonFilePath` de `ScanCommand.Run`.
- Produces: `installchecker scan <root> --json-file <scan.jsonl>`.

- [ ] **Step 1: Écrire les tests rouges du routage**

Créer un test de processus CLI qui vérifie :

- `--json-file` crée un fichier JSONL sans JSON sur stdout ;
- un second passage remplace le fichier ;
- `--json --json-file` retourne `2` et affiche l'usage ;
- `--json-file` sans valeur retourne `2`.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test apps/cli/tests/InstallChecker.Cli.Tests/InstallChecker.Cli.Tests.csproj -c Release --no-restore --filter FullyQualifiedName~ScanRouteTests
```

Expected: les arguments sont refusés par la route actuelle.

- [ ] **Step 3: Étendre le parseur d'arguments sans déplacer les routes**

Ajouter `string? jsonFile = null;`, reconnaître :

```csharp
else if (options[i] == "--json-file" && i + 1 < options.Length)
    jsonFile = options[++i];
```

Après la boucle, retourner `Usage()` si `json && jsonFile is not null`. Passer `jsonFile` à
`ScanCommand.Run`. Ajouter l'option dans la ligne d'usage.

- [ ] **Step 4: Mettre à jour le README Scanner**

Documenter que `--json` est stdout pour une exécution et que `--json-file` remplace un snapshot
JSONL existant. Montrer `>` pour stdout et interdire l'exemple `>>`.

- [ ] **Step 5: Vérifier les tests verts**

Run: commande de l'étape 2.

Expected: tous les tests de route passent.

- [ ] **Step 6: Checkpoint Git proposé**

```powershell
git add apps/cli/InstallChecker.Cli/Program.cs apps/cli/tests/InstallChecker.Cli.Tests/ScanRouteTests.cs modules/scanner/README.md
git commit -m "feat(cli): add non-append scanner JSON export"
```

---

### Task 7: Mesure, gouvernance et vérification globale

**Files:**
- Create: `modules/scanner/tests/InstallChecker.Scanner.Tests/MesureDeduplicationStockageTests.cs`
- Modify: `modules/scanner/docs/specs/2026-07-19-stockage-observations-dedupplique-design.md`
- Modify: `modules/scanner/docs/adr/ADR-001-separer-snapshot-et-occurrence-de-scan.md`
- Modify: `AGENTS.md`

**Interfaces:**
- Consumes: schéma v3 et toutes les commandes raccordées.
- Produces: mesure reproductible et statut livré.

- [ ] **Step 1: Ajouter une mesure séparée**

Créer un test `[Trait("Category", "Performance")]` qui persiste directement 100 000 occurrences
réparties sur 10 scans à partir de 10 000 snapshots distincts. Mesurer durée, taille du fichier DB
et allocation, puis affirmer uniquement :

```csharp
Assert.Equal(10_000L, NombreLignes("observation_snapshots"));
Assert.Equal(100_000L, NombreLignes("scan_entries"));
Assert.Equal(10_000L, NombreLignes("snapshot_version_info"));
```

Ne fixer aucun seuil lors du premier relevé.

- [ ] **Step 2: Exécuter et consigner la mesure**

```powershell
dotnet test modules/scanner/tests/InstallChecker.Scanner.Tests/InstallChecker.Scanner.Tests.csproj -c Release --no-restore --filter Category=Performance --logger "console;verbosity=detailed"
```

Expected: test passant, valeurs temps/allocation/taille visibles. Reporter les valeurs et la machine
dans la section performance de la spécification.

- [ ] **Step 3: Mettre à jour la gouvernance**

Passer la spécification et l'ADR au statut `accepté et implémenté`. Ajouter dans `AGENTS.md` un
ADR global qui précise que l'append-only porte désormais sur les occurrences de scan, tandis que les
payloads immuables identiques peuvent être mutualisés. Ne supprimer ni ne réécrire ADR-002 ; indiquer
explicitement le raffinement apporté par le schéma v3.

- [ ] **Step 4: Exécuter toutes les suites fonctionnelles**

```powershell
dotnet test InstallChecker.slnx -c Release --no-restore --filter "Category!=Performance"
```

Expected: zéro échec dans Scanner, Scanner.Observations, Duplicate Files, CLI et Identity.

- [ ] **Step 5: Vérifier les frontières et le diff**

```powershell
git diff -- src/InstallChecker.Identity src/InstallChecker.Identity.Access tests/InstallChecker.Identity.Tests tests/oracle docs/identity docs/conformite registre
git diff --check
rg -n "UPDATE|DELETE FROM observation_snapshots|DELETE FROM snapshot_" modules/scanner/src
```

Expected : aucune modification gelée, aucune erreur de diff, aucune suppression ou mise à jour des
snapshots immuables. Les seuls `UPDATE` admis ailleurs doivent être justifiés par une table
opérationnelle, mais ce jalon n'en introduit aucun.

- [ ] **Step 6: Vérifier le scénario utilisateur final**

Créer deux fichiers de même contenu à deux chemins, scanner deux fois, puis vérifier :

```sql
SELECT COUNT(*) FROM observation_snapshots; -- 1
SELECT COUNT(*) FROM scan_entries;          -- 4
SELECT COUNT(*) FROM scans;                 -- 2
```

Exécuter `duplicates` sur la base : le rapport courant contient un groupe exact de deux exemplaires,
jamais quatre. Exécuter deux fois avec `--json-file` : le fichier contient deux lignes, jamais quatre.

- [ ] **Step 7: Checkpoint Git proposé**

```powershell
git add modules/scanner/tests/InstallChecker.Scanner.Tests/MesureDeduplicationStockageTests.cs modules/scanner/docs AGENTS.md
git commit -m "docs(scanner): record deduplicated storage contract and measurement"
```

---

## Plan Self-Review

- **Spec coverage:** snapshots partagés, occurrences distinctes, v3, compatibilité v1/v2, JSON
  post-commit, export sans append, gel Identity et mesure sont chacun couverts par une tâche.
- **Type consistency:** `SnapshotCalcule`, `CleSnapshotObservation.Calculer`,
  `SourceObservationsSqlite.Ouvrir` et `ObservationStore.ProjeterJsonDuScan` gardent les mêmes noms
  entre producteurs et consommateurs.
- **Scope:** aucun cache fondé sur date/taille, aucune migration, aucun checkpoint intermédiaire et
  aucune collecte de snapshots ne sont inclus.
