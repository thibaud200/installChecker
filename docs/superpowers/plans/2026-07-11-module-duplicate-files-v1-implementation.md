# Module Duplicate Files v1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the v1 slice of the Duplicate Files module — a read-only consumer of the identity engine that, given one designated Ω state, groups byte-identical installer files (strate contenu, CE-01/EQ-01), suggests a retention order within each group via a versioned business policy, and surfaces any strate-contenu refusals without dropping them — exposed as a new `duplicates` CLI command.

**Architecture:** New class library `InstallChecker.DuplicateFiles` (references `InstallChecker.Identity` only) implements five pure, stateless components chained by a `GenerateurDeRapport` orchestrator: extract groups from `W`, enrich them with raw `Ω` attributes, rank exemplaires per a versioned retention policy, assemble a JSON report. A new `DuplicatesCommand` in the existing `InstallChecker` CLI project wires the two `InstallChecker.Identity.Access` adapters (already used by `IdentityCommand`) into `Porteur.Deriver`, then into the new module — mirroring `IdentityCommand.Deriver` exactly. The module's own retention policy is documented as a versioned artifact under `modules/duplicate-files/registre-metier/`, separate from the engine's `registre/`.

**Tech Stack:** .NET 10 (C# 13, `ImplicitUsings`/`Nullable` enabled), xUnit 2.9.3, `Microsoft.Data.Sqlite` (already a CLI dependency, untouched by this plan), `.slnx` solution format.

**Design reference:** `docs/superpowers/specs/2026-07-11-module-duplicate-files-design.md` (decisions D1–D6). This plan implements **only D2's v1 scope**: one designated Ω state, strate contenu exclusively. D1's v2/v3 and the strate version/identité extension (spec §8) are explicitly out of scope.

## Global Constraints

- Target framework: `net10.0` everywhere (matches every existing `.csproj` in the repo).
- `ImplicitUsings=enable`, `Nullable=enable` on every new project (matches existing convention — no explicit `using System;`/`using System.Linq;` needed).
- Test framework: xUnit 2.9.3 + `xunit.runner.visualstudio` 3.1.4 + `Microsoft.NET.Test.Sdk` 17.14.1 + `coverlet.collector` 6.0.4, with `<Using Include="Xunit" />` as a project-level global using (no `using Xunit;` line in test files) — copy the exact versions from `tests/InstallChecker.Identity.Tests/InstallChecker.Identity.Tests.csproj`.
- Test method names are full French sentences describing the behavior under test (e.g. `Le_plus_riche_en_observations_est_classe_premier`), matching every existing test file in the repo. No `[Theory]`-with-inline-data style is required — plain `[Fact]` per scenario, matching existing style.
- Dependency direction: `InstallChecker.Identity ← InstallChecker.DuplicateFiles ← InstallChecker` (CLI). The new module project references **only** `InstallChecker.Identity` — never `InstallChecker.Identity.Access`, never `InstallChecker.Core`, never SQLite. Only the CLI project and its test project may reference `InstallChecker.Identity.Access`.
- One type per file, matching the granularity of `src/InstallChecker.Identity/Etat/*.cs`.
- XML `<summary>` doc comments on every new public type, citing the relevant design decision (`D1`–`D6` of the 2026-07-11 design doc) instead of engine theory documents (those are for engine-owned types only) — matches this repo's established documentation-heavy convention.
- No new external NuGet dependency of any kind (YAGNI: 4 ordered criteria and a JSON report need nothing beyond `System.Text.Json`, `System.Text.RegularExpressions`, and LINQ, all already available).
- Every step's `dotnet` command is run from the repo root (`C:\git\installChecker`).

---

### Task 1: Project scaffolding + Extracteur de groupes

**Files:**
- Create: `src/InstallChecker.DuplicateFiles/InstallChecker.DuplicateFiles.csproj`
- Create: `src/InstallChecker.DuplicateFiles/ExtracteurDeGroupes.cs`
- Create: `tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
- Create: `tests/InstallChecker.DuplicateFiles.Tests/ExtracteurDeGroupesTests.cs`
- Modify: `InstallChecker.slnx` (add two `<Project>` entries)

**Interfaces:**
- Produces: `InstallChecker.DuplicateFiles.ExtracteurDeGroupes.Extraire(W w) : (IReadOnlyList<ActeW> Groupes, IReadOnlyList<ActeW> Refus)` — consumed by Task 5 (`GenerateurDeRapport`).

- [ ] **Step 1: Create the two project files and register them in the solution**

`src/InstallChecker.DuplicateFiles/InstallChecker.DuplicateFiles.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- D6 (spec 2026-07-11) : le module ne dépend que du contrat public du moteur pur — jamais de
       Identity.Access, jamais de SQLite. Seule la CLI relie les deux. -->
  <ItemGroup>
    <ProjectReference Include="..\InstallChecker.Identity\InstallChecker.Identity.csproj" />
  </ItemGroup>

</Project>
```

`tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\InstallChecker.DuplicateFiles\InstallChecker.DuplicateFiles.csproj" />
  </ItemGroup>

</Project>
```

Replace the full content of `InstallChecker.slnx` with:
```xml
<Solution>
  <Folder Name="/src/">
    <Project Path="src/InstallChecker/InstallChecker.csproj" />
    <Project Path="src/InstallChecker.Core/InstallChecker.Core.csproj" />
    <Project Path="src/InstallChecker.Identity/InstallChecker.Identity.csproj" />
    <Project Path="src/InstallChecker.Identity.Access/InstallChecker.Identity.Access.csproj" />
    <Project Path="src/InstallChecker.DuplicateFiles/InstallChecker.DuplicateFiles.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/InstallChecker.Tests/InstallChecker.Tests.csproj" />
    <Project Path="tests/InstallChecker.Identity.Tests/InstallChecker.Identity.Tests.csproj" />
    <Project Path="tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj" />
  </Folder>
</Solution>
```

- [ ] **Step 2: Write the failing test**

`tests/InstallChecker.DuplicateFiles.Tests/ExtracteurDeGroupesTests.cs`:
```csharp
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.DuplicateFiles.Tests;

public class ExtracteurDeGroupesTests
{
    private static readonly IndexEtat IndexDeTest = new(new IndexOmega(1, 0, "empreinte-de-test"), []);

    private static ActeW Election(Strate strate, Niveau niveau, string? licence, params long[] domaine) => new(
        TypeActe.Election, strate, domaine, "même contenu", niveau, "motif de test", Espece: null,
        Licences: licence is null ? null : [new ConventionRef(licence, 1)], Dependances: null, Dette: null);

    private static ActeW Refus(Strate strate, params long[] domaine) => new(
        TypeActe.Refus, strate, domaine, Contenu: null, Niveau: null, "motif de refus", Espece.Structurel,
        Licences: null, Dependances: null, Dette: null);

    [Fact]
    public void Une_election_certaine_licenciee_par_CE01_en_strate_contenu_devient_un_groupe()
    {
        var w = new W(IndexDeTest, [Election(Strate.Contenu, Niveau.Certaine, "CE-01", 1, 2)]);

        var (groupes, refus) = ExtracteurDeGroupes.Extraire(w);

        Assert.Single(groupes);
        Assert.Equal([1L, 2L], groupes[0].Domaine);
        Assert.Empty(refus);
    }

    [Fact]
    public void Une_election_de_niveau_non_certaine_nest_pas_un_groupe()
    {
        var w = new W(IndexDeTest, [Election(Strate.Contenu, Niveau.Probable, "CE-01", 1, 2)]);

        var (groupes, _) = ExtracteurDeGroupes.Extraire(w);

        Assert.Empty(groupes);
    }

    [Fact]
    public void Une_election_hors_strate_contenu_nest_pas_un_groupe()
    {
        var w = new W(IndexDeTest, [Election(Strate.Variante, Niveau.Certaine, "CE-01", 1, 2)]);

        var (groupes, _) = ExtracteurDeGroupes.Extraire(w);

        Assert.Empty(groupes);
    }

    [Fact]
    public void Un_refus_en_strate_contenu_est_restitue_separement_jamais_elimine()
    {
        var w = new W(IndexDeTest, [Refus(Strate.Contenu, 3, 4)]);

        var (groupes, refus) = ExtracteurDeGroupes.Extraire(w);

        Assert.Empty(groupes);
        Assert.Single(refus);
        Assert.Equal([3L, 4L], refus[0].Domaine);
    }
}
```

- [ ] **Step 3: Run to verify it fails**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
Expected: build error — `ExtracteurDeGroupes` does not exist in namespace `InstallChecker.DuplicateFiles`.

- [ ] **Step 4: Implement `ExtracteurDeGroupes`**

`src/InstallChecker.DuplicateFiles/ExtracteurDeGroupes.cs`:
```csharp
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Sépare les actes de W en deux flux, strate contenu exclusivement (D1/D2 : le v1 se limite à
/// cette strate — les autres n'ont aucune convention en vigueur) : les élections certaines
/// licenciées par CE-01 deviennent des groupes de doublons ; tout refus rencontré à cette strate
/// est restitué séparément, jamais éliminé silencieusement (spec § 6).
/// </summary>
public static class ExtracteurDeGroupes
{
    public static (IReadOnlyList<ActeW> Groupes, IReadOnlyList<ActeW> Refus) Extraire(W w)
    {
        var groupes = w.Actes
            .Where(a => a.Type == TypeActe.Election
                     && a.Strate == Strate.Contenu
                     && a.Niveau == Niveau.Certaine
                     && a.Licences is not null
                     && a.Licences.Any(l => l.Identifiant == "CE-01"))
            .ToList();

        var refus = w.Actes
            .Where(a => a.Type == TypeActe.Refus && a.Strate == Strate.Contenu)
            .ToList();

        return (groupes, refus);
    }
}
```

- [ ] **Step 5: Run to verify it passes**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
Expected: 4 passed, 0 failed.

- [ ] **Step 6: Commit**

```bash
git add InstallChecker.slnx src/InstallChecker.DuplicateFiles tests/InstallChecker.DuplicateFiles.Tests
git commit -m "feat(duplicate-files): scaffold module project, extract groups and refusals from W"
```

---

### Task 2: Enrichisseur de groupe

**Files:**
- Create: `src/InstallChecker.DuplicateFiles/FichierEnrichi.cs`
- Create: `src/InstallChecker.DuplicateFiles/EnrichisseurDeGroupe.cs`
- Create: `tests/InstallChecker.DuplicateFiles.Tests/EnrichisseurDeGroupeTests.cs`

**Interfaces:**
- Consumes: nothing from Task 1.
- Produces: `InstallChecker.DuplicateFiles.FichierEnrichi(long ActeId, string Chemin, bool SignatureAuthenticodePresente, bool MetadonneesPeCompletes, bool MetadonneesMsiCompletes, string DateDObservation)` and `InstallChecker.DuplicateFiles.EnrichisseurDeGroupe.Enrichir(IReadOnlyList<long> domaine, IReadOnlyDictionary<long, ActeObservation> actes, IReadOnlyDictionary<long, ContexteObservation> contextes) : IReadOnlyList<FichierEnrichi>` — consumed by Task 5 (`GenerateurDeRapport`) and Task 3's tests.

- [ ] **Step 1: Write the failing test**

`tests/InstallChecker.DuplicateFiles.Tests/EnrichisseurDeGroupeTests.cs`:
```csharp
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class EnrichisseurDeGroupeTests
{
    private static ActeObservation Acte(long id, IReadOnlyDictionary<Attribut, ValeurObservee> attributs) =>
        new(id, Taille: 100, Empreinte: "A", attributs);

    [Fact]
    public void Les_attributs_bruts_presents_sont_lus_comme_vrais()
    {
        var actes = new Dictionary<long, ActeObservation>
        {
            [1] = Acte(1, new Dictionary<Attribut, ValeurObservee>
            {
                [new Attribut("authenticode", "subject")] = new ValeurObservee.Texte("Contoso"),
                [new Attribut("pe_info", "machine")] = new ValeurObservee.Texte("x64"),
                [new Attribut("msi_properties", "product_name")] = new ValeurObservee.Texte("Contoso Setup"),
            }),
        };
        var contextes = new Dictionary<long, ContexteObservation>
        {
            [1] = new(1, @"C:\installers\setup.exe", "2026-07-01T00:00:00.0000000Z"),
        };

        var fichiers = EnrichisseurDeGroupe.Enrichir([1], actes, contextes);

        Assert.Equal(@"C:\installers\setup.exe", fichiers[0].Chemin);
        Assert.True(fichiers[0].SignatureAuthenticodePresente);
        Assert.True(fichiers[0].MetadonneesPeCompletes);
        Assert.True(fichiers[0].MetadonneesMsiCompletes);
        Assert.Equal("2026-07-01T00:00:00.0000000Z", fichiers[0].DateDObservation);
    }

    [Fact]
    public void Un_attribut_absent_ou_manquant_du_dictionnaire_nest_pas_une_erreur()
    {
        // spec § 6 : ⊥ (absence) est une observation légitime, jamais une erreur — le dictionnaire
        // peut même ne pas contenir la clé du tout (invariant 1:1 non garanti par un fixture de test).
        var actes = new Dictionary<long, ActeObservation>
        {
            [1] = Acte(1, new Dictionary<Attribut, ValeurObservee>
            {
                [new Attribut("authenticode", "subject")] = ValeurObservee.Absente.Instance,
            }),
        };
        var contextes = new Dictionary<long, ContexteObservation> { [1] = new(1, @"C:\installers\a.exe", "2026-07-01T00:00:00Z") };

        var fichiers = EnrichisseurDeGroupe.Enrichir([1], actes, contextes);

        Assert.False(fichiers[0].SignatureAuthenticodePresente);
        Assert.False(fichiers[0].MetadonneesPeCompletes);
        Assert.False(fichiers[0].MetadonneesMsiCompletes);
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
Expected: build error — `FichierEnrichi`/`EnrichisseurDeGroupe` do not exist.

- [ ] **Step 3: Implement `FichierEnrichi` and `EnrichisseurDeGroupe`**

`src/InstallChecker.DuplicateFiles/FichierEnrichi.cs`:
```csharp
namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Un fichier d'un groupe de doublons, enrichi des attributs bruts d'Ω nécessaires au classement
/// (D4) : le module lit Ω directement pour sa propre présentation, comme l'autorise 011 § 11
/// (« consommateurs → contrat public de Ω, pour leurs propres besoins de présentation »).
/// </summary>
public sealed record FichierEnrichi(
    long ActeId,
    string Chemin,
    bool SignatureAuthenticodePresente,
    bool MetadonneesPeCompletes,
    bool MetadonneesMsiCompletes,
    string DateDObservation);
```

`src/InstallChecker.DuplicateFiles/EnrichisseurDeGroupe.cs`:
```csharp
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Relit dans Ω (jamais dans W) les attributs bruts nécessaires au classement d'un groupe (D4).
/// Un attribut absent — explicitement ⊥ ou simplement hors du dictionnaire — n'est jamais une
/// erreur : c'est une observation légitime, traitée comme "faux" pour ce critère (spec § 6).
/// </summary>
public static class EnrichisseurDeGroupe
{
    public static IReadOnlyList<FichierEnrichi> Enrichir(
        IReadOnlyList<long> domaine,
        IReadOnlyDictionary<long, ActeObservation> actes,
        IReadOnlyDictionary<long, ContexteObservation> contextes) =>
        domaine.Select(id =>
        {
            var acte = actes[id];
            var contexte = contextes[id];
            return new FichierEnrichi(
                id,
                contexte.Chemin,
                ValeurPresente(acte, new Attribut("authenticode", "subject")),
                ValeurPresente(acte, new Attribut("pe_info", "machine")),
                ValeurPresente(acte, new Attribut("msi_properties", "product_name")),
                contexte.DateDeScan);
        }).ToList();

    private static bool ValeurPresente(ActeObservation acte, Attribut attribut) =>
        acte.Attributs.TryGetValue(attribut, out var valeur) && valeur is not ValeurObservee.Absente;
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
Expected: 6 passed, 0 failed (4 from Task 1 + 2 new).

- [ ] **Step 5: Commit**

```bash
git add src/InstallChecker.DuplicateFiles/FichierEnrichi.cs src/InstallChecker.DuplicateFiles/EnrichisseurDeGroupe.cs tests/InstallChecker.DuplicateFiles.Tests/EnrichisseurDeGroupeTests.cs
git commit -m "feat(duplicate-files): enrich duplicate groups with raw Omega attributes"
```

---

### Task 3: Politique de rétention v1 (classeur)

**Files:**
- Create: `src/InstallChecker.DuplicateFiles/ExemplaireClasse.cs`
- Create: `src/InstallChecker.DuplicateFiles/PolitiqueRetentionV1.cs`
- Create: `tests/InstallChecker.DuplicateFiles.Tests/PolitiqueRetentionV1Tests.cs`

**Interfaces:**
- Consumes: `FichierEnrichi` (Task 2).
- Produces: `InstallChecker.DuplicateFiles.ExemplaireClasse(FichierEnrichi Fichier, int Rang, string Motif)` and `InstallChecker.DuplicateFiles.PolitiqueRetentionV1.Classer(IReadOnlyList<FichierEnrichi> fichiers) : IReadOnlyList<ExemplaireClasse>` — consumed by Task 5 (`GenerateurDeRapport`). `Rang` 1 = exemplaire suggéré à conserver.

This is the direct code implementation of D4/D6: the criteria order below is the literal translation of `modules/duplicate-files/registre-metier/politique-retention/v1.md` (written in Task 4) — any future revision of the order requires a new `vN.md` **before** this file changes.

- [ ] **Step 1: Write the failing tests**

`tests/InstallChecker.DuplicateFiles.Tests/PolitiqueRetentionV1Tests.cs`:
```csharp
using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class PolitiqueRetentionV1Tests
{
    private static FichierEnrichi Fichier(
        long acteId, string chemin, bool authenticode = false, bool pe = false, bool msi = false,
        string date = "2026-01-01T00:00:00.0000000Z") =>
        new(acteId, chemin, authenticode, pe, msi, date);

    [Fact]
    public void Le_plus_riche_en_observations_est_classe_premier()
    {
        var pauvre = Fichier(1, @"C:\a\setup.exe");
        var riche = Fichier(2, @"C:\b\setup.exe", authenticode: true, pe: true, msi: true);

        var classement = PolitiqueRetentionV1.Classer([pauvre, riche]);

        Assert.Equal(riche.ActeId, classement[0].Fichier.ActeId);
        Assert.Equal(1, classement[0].Rang);
        Assert.Equal(pauvre.ActeId, classement[1].Fichier.ActeId);
        Assert.Equal(2, classement[1].Rang);
    }

    [Fact]
    public void A_richesse_egale_le_nom_qui_ne_ressemble_pas_a_une_copie_est_prefere()
    {
        var original = Fichier(1, @"C:\a\setup.exe");
        var copie = Fichier(2, @"C:\b\setup (1).exe");

        var classement = PolitiqueRetentionV1.Classer([copie, original]);

        Assert.Equal(original.ActeId, classement[0].Fichier.ActeId);
    }

    [Fact]
    public void A_richesse_et_nom_egaux_le_plus_recemment_observe_est_prefere()
    {
        var ancien = Fichier(1, @"C:\a\setup.exe", date: "2020-01-01T00:00:00.0000000Z");
        var recent = Fichier(2, @"C:\b\setup.exe", date: "2026-01-01T00:00:00.0000000Z");

        var classement = PolitiqueRetentionV1.Classer([ancien, recent]);

        Assert.Equal(recent.ActeId, classement[0].Fichier.ActeId);
    }

    [Fact]
    public void A_tout_egal_lordre_alphabetique_du_chemin_departage_de_facon_stable()
    {
        var b = Fichier(1, @"C:\b\setup.exe");
        var a = Fichier(2, @"C:\a\setup.exe");

        var premier = PolitiqueRetentionV1.Classer([b, a]);
        var second = PolitiqueRetentionV1.Classer([b, a]);

        Assert.Equal(a.ActeId, premier[0].Fichier.ActeId);
        Assert.Equal(premier.Select(e => e.Fichier.ActeId), second.Select(e => e.Fichier.ActeId));
    }

    [Fact]
    public void Labsence_de_tout_signal_ne_provoque_aucune_erreur()
    {
        var f1 = Fichier(1, @"C:\a\setup.exe");
        var f2 = Fichier(2, @"C:\b\setup.exe");

        var classement = PolitiqueRetentionV1.Classer([f1, f2]);

        Assert.Equal(2, classement.Count);
        Assert.Equal([1, 2], classement.Select(e => e.Rang));
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
Expected: build error — `ExemplaireClasse`/`PolitiqueRetentionV1` do not exist.

- [ ] **Step 3: Implement `ExemplaireClasse` and `PolitiqueRetentionV1`**

`src/InstallChecker.DuplicateFiles/ExemplaireClasse.cs`:
```csharp
namespace InstallChecker.DuplicateFiles;

/// <summary>Un fichier classé au sein d'un groupe (D3) : Rang 1 est l'exemplaire suggéré à conserver — jamais une décision automatique (D3 : « l'utilisateur choisit »).</summary>
public sealed record ExemplaireClasse(FichierEnrichi Fichier, int Rang, string Motif);
```

`src/InstallChecker.DuplicateFiles/PolitiqueRetentionV1.cs`:
```csharp
using System.Text.RegularExpressions;

namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Politique de rétention v1 (D4/D6) — implémentation littérale de
/// modules/duplicate-files/registre-metier/politique-retention/v1.md. Ordre des critères, du plus
/// au moins déterminant : richesse des observations, qualité du nom de fichier, ancienneté
/// (le plus récent préféré), emplacement (ordre alphabétique, aucune notion de dossier canonique
/// en v1). L'identifiant d'acte est un cinquième départage strictement mécanique, garantissant un
/// ordre total — il ne reflète aucun jugement métier. Une révision de cet ordre exige d'abord une
/// nouvelle version documentée (vN.md), jamais un patch silencieux de ce fichier (D6).
/// </summary>
public static class PolitiqueRetentionV1
{
    private static readonly Regex NomDeCopie =
        new(@"(\(\d+\)|-\s*copie|-\s*copy)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<ExemplaireClasse> Classer(IReadOnlyList<FichierEnrichi> fichiers) =>
        fichiers
            .OrderByDescending(RichesseDesObservations)
            .ThenBy(EstNomDeCopie)
            .ThenByDescending(f => f.DateDObservation, StringComparer.Ordinal)
            .ThenBy(f => f.Chemin, StringComparer.Ordinal)
            .ThenBy(f => f.ActeId)
            .Select((f, index) => new ExemplaireClasse(f, index + 1, Motif(f)))
            .ToList();

    private static int RichesseDesObservations(FichierEnrichi f) =>
        (f.SignatureAuthenticodePresente ? 1 : 0)
        + (f.MetadonneesPeCompletes ? 1 : 0)
        + (f.MetadonneesMsiCompletes ? 1 : 0);

    private static bool EstNomDeCopie(FichierEnrichi f) => NomDeCopie.IsMatch(Path.GetFileNameWithoutExtension(f.Chemin));

    private static string Motif(FichierEnrichi f) =>
        $"richesse={RichesseDesObservations(f)}/3, nomDeCopie={EstNomDeCopie(f)}, " +
        $"dateObservation={f.DateDObservation}, chemin={f.Chemin}";
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
Expected: 11 passed, 0 failed (6 from Tasks 1–2 + 5 new).

- [ ] **Step 5: Commit**

```bash
git add src/InstallChecker.DuplicateFiles/ExemplaireClasse.cs src/InstallChecker.DuplicateFiles/PolitiqueRetentionV1.cs tests/InstallChecker.DuplicateFiles.Tests/PolitiqueRetentionV1Tests.cs
git commit -m "feat(duplicate-files): implement v1 retention policy ranking"
```

---

### Task 4: Registre métier du module (documentation)

**Files:**
- Create: `modules/duplicate-files/registre-metier/politique-retention/v1.md`
- Create: `modules/duplicate-files/registre-metier/historique.md`

**Interfaces:** none (documentation only — D6 deliberately does not build a markdown parser for 4 hardcoded criteria; see spec D6 alternatives. `PolitiqueRetentionV1.cs`, written in Task 3, is the literal code translation of this document; they must be read together, never one without the other).

This task has no test step: it is a governance artifact, not executable code — the same regime as `registre/conventions/CE-01/v1.md` in the engine (never programmatically validated against its own prose, reviewed by a human instead).

- [ ] **Step 1: Write the policy document**

`modules/duplicate-files/registre-metier/politique-retention/v1.md`:
```markdown
# Politique de rétention — Duplicate Files (v1)

## identifiant

politique-retention

## version

1

## domaine d'application

Tout groupe de fichiers élus « même contenu » (strate contenu, niveau certaine, licence CE-01) par
le moteur d'identité, pour le module Duplicate Files v1 exclusivement (un état Ω désigné — voir
spec D2). Cette politique ne dit rien du regroupement de versions d'un même logiciel : hors
périmètre du v1 (spec § 8).

## nature de la décision

Cette politique produit une **suggestion**, jamais une décision automatique (D3). L'utilisateur
reste seul décisionnaire de ce qui est conservé ou supprimé.

## critères de classement, par ordre de priorité

1. **Richesse des observations** — nombre d'attributs présents parmi : signature Authenticode
   (`authenticode.subject`), complétude PE (`pe_info.machine`), complétude MSI
   (`msi_properties.product_name`). Un exemplaire plus riche est préféré.
2. **Qualité du nom de fichier** — un nom qui ne porte pas la marque d'une copie du système de
   fichiers (suffixe `(N)`, `- copie`, `- copy`) est préféré à un nom qui la porte.
3. **Ancienneté de l'observation** — l'exemplaire observé le plus récemment est préféré (pas le
   plus ancien).
4. **Emplacement sur disque** — ordre alphabétique du chemin complet. Aucune notion de dossier
   canonique/préféré configurable n'existe en v1 (choix explicite, spec D4).

Un cinquième critère, mécanique et sans signification métier — l'identifiant d'acte croissant —
garantit un ordre total lorsque les quatre critères ci-dessus ne départagent pas (ce qui ne peut
arriver que si deux fichiers du même groupe partagent aussi le même chemin).

## gestion de l'absence de signal

L'absence d'un attribut (⊥, jamais observé) n'est pas une erreur : le critère concerné est traité
comme non satisfait pour ce fichier (équivalent à « faux »), et le classement passe au critère
suivant. Ce comportement fait partie de la politique elle-même, pas une improvisation du code.

## justification

L'ordre reflète la confiance décroissante des signaux disponibles à ce jour dans le pipeline
d'observation : la richesse d'observation est vérifiable programmatiquement et directement liée à
la qualité de la provenance du fichier ; le nom de fichier et l'ancienneté sont des indices plus
faibles mais utiles à richesse égale ; l'emplacement est le signal le plus faible, retenu en
dernier recours uniquement pour garantir un résultat déterministe.

## conditions de révision

Toute modification de l'ordre, de l'ajout ou du retrait d'un critère produit une version nouvelle
(`v2.md`), tracée dans `historique.md`. Le présent fichier n'est jamais modifié après son adoption
— exactement le régime de `registre/conventions/` côté moteur, appliqué ici au niveau du module.

## date

2026-07-11

## autorité

Propriétaire du projet.
```

- [ ] **Step 2: Write the adoption history**

`modules/duplicate-files/registre-metier/historique.md`:
```markdown
# Historique du registre métier — Duplicate Files

## politique-retention

- v1, adoptée le 2026-07-11 — première version, quatre critères ordonnés (richesse des
  observations, nom de fichier, ancienneté, emplacement) — voir `politique-retention/v1.md`.
```

- [ ] **Step 3: Commit**

```bash
git add modules/duplicate-files
git commit -m "docs(duplicate-files): adopt v1 retention policy as a versioned business registry entry"
```

---

### Task 5: Rapport et générateur

**Files:**
- Create: `src/InstallChecker.DuplicateFiles/GroupeClasse.cs`
- Create: `src/InstallChecker.DuplicateFiles/RapportDeDoublons.cs`
- Create: `src/InstallChecker.DuplicateFiles/GenerateurDeRapport.cs`
- Create: `tests/InstallChecker.DuplicateFiles.Tests/GenerateurDeRapportTests.cs`

**Interfaces:**
- Consumes: `ExtracteurDeGroupes.Extraire` (Task 1), `EnrichisseurDeGroupe.Enrichir` (Task 2), `PolitiqueRetentionV1.Classer` (Task 3).
- Produces: `InstallChecker.DuplicateFiles.GroupeClasse(IReadOnlyList<long> Domaine, string MotifCourt, IReadOnlyList<ExemplaireClasse> Exemplaires)`, `InstallChecker.DuplicateFiles.RapportDeDoublons(IReadOnlyList<GroupeClasse> Groupes, IReadOnlyList<ActeW> NonTranches)`, and `InstallChecker.DuplicateFiles.GenerateurDeRapport.Generer(W w, IObservationsSource omega) : RapportDeDoublons` — consumed by Task 6 (`DuplicatesCommand`).

- [ ] **Step 1: Write the failing test**

`tests/InstallChecker.DuplicateFiles.Tests/GenerateurDeRapportTests.cs`:
```csharp
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class GenerateurDeRapportTests
{
    private sealed class OmegaDeTest(ModeleObservations modele, IReadOnlyList<ContexteObservation> contexte)
        : IObservationsSource
    {
        public ModeleObservations ProjeterModele() => modele;
        public IReadOnlyList<ContexteObservation> ProjeterContexte() => contexte;
        public IndexOmega ProjeterIdentite() => throw new NotSupportedException("non consommé par GenerateurDeRapport");
    }

    private static readonly IndexEtat IndexDeTest = new(new IndexOmega(1, 0, "empreinte-de-test"), []);

    [Fact]
    public void Un_groupe_elu_devient_un_groupe_classe_et_un_refus_est_restitue_a_part()
    {
        var election = new ActeW(
            TypeActe.Election, Strate.Contenu, [1, 2], "même contenu", Niveau.Certaine, "motif d'élection",
            Espece: null, Licences: [new ConventionRef("CE-01", 1)], Dependances: null, Dette: null);
        var refus = new ActeW(
            TypeActe.Refus, Strate.Contenu, [3, 4], Contenu: null, Niveau: null, "motif de refus",
            Espece.Structurel, Licences: null, Dependances: null, Dette: null);
        var w = new W(IndexDeTest, [election, refus]);

        var omega = new OmegaDeTest(
            new ModeleObservations([
                new ActeObservation(1, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
                new ActeObservation(2, 1, "A", new Dictionary<Attribut, ValeurObservee>
                {
                    [new Attribut("authenticode", "subject")] = new ValeurObservee.Texte("Contoso"),
                }),
            ]),
            [
                new ContexteObservation(1, @"C:\a\setup.exe", "2026-01-01T00:00:00.0000000Z"),
                new ContexteObservation(2, @"C:\b\setup.exe", "2026-01-01T00:00:00.0000000Z"),
            ]);

        var rapport = GenerateurDeRapport.Generer(w, omega);

        Assert.Single(rapport.Groupes);
        Assert.Equal([1L, 2L], rapport.Groupes[0].Domaine);
        Assert.Contains("CE-01", rapport.Groupes[0].MotifCourt);
        Assert.Equal(2, rapport.Groupes[0].Exemplaires.Count);
        Assert.Equal(2L, rapport.Groupes[0].Exemplaires[0].Fichier.ActeId); // le plus riche (signature) classé premier

        Assert.Single(rapport.NonTranches);
        Assert.Equal([3L, 4L], rapport.NonTranches[0].Domaine);
    }
}
```

- [ ] **Step 2: Run to verify it fails**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
Expected: build error — `GroupeClasse`/`RapportDeDoublons`/`GenerateurDeRapport` do not exist.

- [ ] **Step 3: Implement the three types**

`src/InstallChecker.DuplicateFiles/GroupeClasse.cs`:
```csharp
namespace InstallChecker.DuplicateFiles;

/// <summary>Un groupe de doublons prêt à être restitué (D5) : le motif court par défaut ; la chaîne d'audit complète du moteur reste accessible à la demande via la commande <c>identity audit</c>, jamais dupliquée ici.</summary>
public sealed record GroupeClasse(IReadOnlyList<long> Domaine, string MotifCourt, IReadOnlyList<ExemplaireClasse> Exemplaires);
```

`src/InstallChecker.DuplicateFiles/RapportDeDoublons.cs`:
```csharp
using InstallChecker.Identity.Etat;

namespace InstallChecker.DuplicateFiles;

/// <summary>Le rapport complet du module (spec § 5) : les groupes classés, et les refus rencontrés à la strate contenu — jamais éliminés silencieusement (spec § 6).</summary>
public sealed record RapportDeDoublons(IReadOnlyList<GroupeClasse> Groupes, IReadOnlyList<ActeW> NonTranches);
```

`src/InstallChecker.DuplicateFiles/GenerateurDeRapport.cs`:
```csharp
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Orchestre les quatre composants du module (spec § 3-5) : extraction, enrichissement, classement,
/// assemblage. Lit Ω une seule fois (pas par groupe) — un état Ω désigné peut porter des centaines
/// de milliers d'actes (CLAUDE.md racine § 10 : optimisation I/O critique).
/// </summary>
public static class GenerateurDeRapport
{
    public static RapportDeDoublons Generer(W w, IObservationsSource omega)
    {
        var (groupesActeW, refus) = ExtracteurDeGroupes.Extraire(w);

        var actes = omega.ProjeterModele().Actes.ToDictionary(a => a.Identifiant);
        var contextes = omega.ProjeterContexte().ToDictionary(c => c.Identifiant);

        var groupes = groupesActeW.Select(acteGroupe =>
        {
            var fichiers = EnrichisseurDeGroupe.Enrichir(acteGroupe.Domaine, actes, contextes);
            var exemplaires = PolitiqueRetentionV1.Classer(fichiers);
            return new GroupeClasse(acteGroupe.Domaine, MotifCourt(acteGroupe), exemplaires);
        }).ToList();

        return new RapportDeDoublons(groupes, refus);
    }

    private static string MotifCourt(ActeW acte) =>
        $"{acte.Motif} ({string.Join(", ", acte.Licences!.Select(l => $"{l.Identifiant} v{l.Version}"))}, niveau {acte.Niveau})";
}
```

- [ ] **Step 4: Run to verify it passes**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`
Expected: 12 passed, 0 failed (11 from Tasks 1–3 + 1 new).

- [ ] **Step 5: Commit**

```bash
git add src/InstallChecker.DuplicateFiles/GroupeClasse.cs src/InstallChecker.DuplicateFiles/RapportDeDoublons.cs src/InstallChecker.DuplicateFiles/GenerateurDeRapport.cs tests/InstallChecker.DuplicateFiles.Tests/GenerateurDeRapportTests.cs
git commit -m "feat(duplicate-files): assemble the duplicate report from W and Omega"
```

---

### Task 6: Commande CLI `duplicates`

**Files:**
- Create: `src/InstallChecker/DuplicatesCommand.cs`
- Modify: `src/InstallChecker/Program.cs` (add a `duplicates` branch and update `Usage()`)
- Modify: `src/InstallChecker/InstallChecker.csproj` (add `ProjectReference` to `InstallChecker.DuplicateFiles`)
- Create: `tests/InstallChecker.Tests/DuplicatesCommandTests.cs`

**Interfaces:**
- Consumes: `GenerateurDeRapport.Generer` (Task 5), `Porteur.Deriver`, `LecteurDObservationsSqlite`, `LecteurDeRegistreMarkdown` (all pre-existing).
- Produces: `InstallChecker.DuplicatesCommand.Deriver(string cheminBase, string cheminRegistre, TextWriter output, TextWriter errors) : int` — the CLI entry point, exit code 0 on success, 1 on a contractual engine error (message written to `errors`, unchanged from the engine).

- [ ] **Step 1: Add the project reference**

Modify `src/InstallChecker/InstallChecker.csproj` — add one line to the existing `<ItemGroup>` of `ProjectReference`s (after the `InstallChecker.Identity.Access` line):
```xml
    <ProjectReference Include="..\InstallChecker.DuplicateFiles\InstallChecker.DuplicateFiles.csproj" />
```

- [ ] **Step 2: Write the failing test**

`tests/InstallChecker.Tests/DuplicatesCommandTests.cs`:
```csharp
using System.Text.Json;

namespace InstallChecker.Tests;

/// <summary>
/// La commande <c>duplicates</c> — v1 du module Duplicate Files (spec 2026-07-11, D1/D2) : un état
/// Ω désigné, strate contenu exclusivement. Bout-en-bout sur l'artefact et le registre réels, même
/// régime d'erreur que <c>identity</c> puisqu'elle délègue au même <c>Porteur.Deriver</c>
/// (voir <see cref="IdentityCommandTests"/> pour la batterie complète des sept erreurs, déjà
/// prouvée sur cet appel — non redupliquée ici).
/// </summary>
public class DuplicatesCommandTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "duplicates-cli-tests-" + Guid.NewGuid())).FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static string RacineDuDepot()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        return repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
    }

    private static string CheminOracle() => Path.Combine(RacineDuDepot(), "tests", "oracle", "corpus1-postA1.db");

    private static string CheminRegistreReel() => Path.Combine(RacineDuDepot(), "registre");

    [Fact]
    public void Le_corpus_reel_produit_112_groupes_et_4_non_tranches()
    {
        // 112 classes multi-actes (108 paires, 4 triplets — EQ-01 v1) et 4 refus à la strate
        // contenu : les mêmes comptes que le W du corpus (IdentityCommandTests), vus depuis le
        // rapport du module plutôt que depuis W directement.
        var sortie = new StringWriter();
        var erreurs = new StringWriter();

        var code = DuplicatesCommand.Deriver(CheminOracle(), CheminRegistreReel(), sortie, erreurs);

        Assert.Equal(0, code);
        Assert.Empty(erreurs.ToString());

        using var json = JsonDocument.Parse(sortie.ToString());
        var racine = json.RootElement;

        Assert.Equal(112, racine.GetProperty("Groupes").GetArrayLength());
        Assert.Equal(4, racine.GetProperty("NonTranches").GetArrayLength());
    }

    [Fact]
    public void Deux_emissions_du_rapport_sont_identiques()
    {
        var premiere = new StringWriter();
        var seconde = new StringWriter();

        Assert.Equal(0, DuplicatesCommand.Deriver(CheminOracle(), CheminRegistreReel(), premiere, new StringWriter()));
        Assert.Equal(0, DuplicatesCommand.Deriver(CheminOracle(), CheminRegistreReel(), seconde, new StringWriter()));

        Assert.Equal(premiere.ToString(), seconde.ToString());
    }

    [Fact]
    public void Une_base_absente_produit_lerreur_du_moteur_telle_quelle()
    {
        var erreurs = new StringWriter();

        var code = DuplicatesCommand.Deriver(
            Path.Combine(_root, "absente.db"), CheminRegistreReel(), new StringWriter(), erreurs);

        Assert.Equal(1, code);
        Assert.False(string.IsNullOrWhiteSpace(erreurs.ToString()));
    }
}
```

- [ ] **Step 3: Run to verify it fails**

Run: `dotnet test tests/InstallChecker.Tests/InstallChecker.Tests.csproj`
Expected: build error — `DuplicatesCommand` does not exist.

- [ ] **Step 4: Implement `DuplicatesCommand` and wire `Program.cs`**

`src/InstallChecker/DuplicatesCommand.cs`:
```csharp
using System.Text.Encodings.Web;
using System.Text.Json;
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Frontiere;

namespace InstallChecker;

/// <summary>
/// La commande <c>duplicates</c> (module Duplicate Files v1, spec 2026-07-11) : câble les mêmes
/// adaptateurs que <see cref="IdentityCommand.Deriver"/> sur <see cref="Porteur.Deriver"/>, puis
/// passe W et la même source Ω à <see cref="GenerateurDeRapport.Generer"/>. Aucune logique métier
/// ici : le classement et l'extraction vivent dans <c>InstallChecker.DuplicateFiles</c>. Toute
/// erreur du moteur est restituée telle quelle, jamais traduite (même régime que 018 § 6).
/// </summary>
public static class DuplicatesCommand
{
    private static readonly JsonSerializerOptions OptionsJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static int Deriver(string cheminBase, string cheminRegistre, TextWriter output, TextWriter errors)
    {
        try
        {
            var omega = new LecteurDObservationsSqlite(cheminBase);
            var w = Porteur.Deriver(omega, new LecteurDeRegistreMarkdown(cheminRegistre));
            var rapport = GenerateurDeRapport.Generer(w, omega);

            output.WriteLine(JsonSerializer.Serialize(rapport, OptionsJson));
            return 0;
        }
        catch (Exception ex) when (ex is ErreurOmega or ErreurDeRegistre)
        {
            errors.WriteLine(ex.Message);
            return 1;
        }
    }
}
```

Modify `src/InstallChecker/Program.cs` — insert this branch after the existing `identity audit` branch (before `return Usage();`):
```csharp
if (args is ["duplicates", var cheminBaseDup, var cheminRegistreDup])
    return DuplicatesCommand.Deriver(cheminBaseDup, cheminRegistreDup, Console.Out, Console.Error);
```

And add one line inside `Usage()`, after the `identity audit` usage line:
```csharp
    Console.Error.WriteLine("        installchecker duplicates <base.db> <registre>");
```

- [ ] **Step 5: Run to verify it passes**

Run: `dotnet test tests/InstallChecker.Tests/InstallChecker.Tests.csproj`
Expected: all tests pass, including the 3 new `DuplicatesCommandTests` and the pre-existing `IdentityCommandTests`/`ScanCommandTests`/`FrontiereDeDonneesTests` (unaffected).

Then run the full solution once:
Run: `dotnet test`
Expected: every test project passes (Identity.Tests, DuplicateFiles.Tests, InstallChecker.Tests).

- [ ] **Step 6: Commit**

```bash
git add src/InstallChecker/DuplicatesCommand.cs src/InstallChecker/Program.cs src/InstallChecker/InstallChecker.csproj tests/InstallChecker.Tests/DuplicatesCommandTests.cs
git commit -m "feat(duplicate-files): wire the duplicates CLI command end to end"
```

---

## Self-review

**Spec coverage** (against `docs/superpowers/specs/2026-07-11-module-duplicate-files-design.md`):
- D1/D2 (v1 scope, one designated Ω state) → the whole plan operates on a single `IObservationsSource`/`W` pair, no cumulative or τ logic anywhere (Tasks 1–6).
- D3 (suggestion, not automatic) → `ExemplaireClasse.Rang` is a suggestion; nothing in the module writes or deletes files (Task 3).
- D4 (four ordered criteria) → `PolitiqueRetentionV1.Classer` (Task 3), documented verbatim in Task 4's `v1.md`.
- D5 (short motif by default, full audit on demand) → `GroupeClasse.MotifCourt` (Task 5); full audit remains the pre-existing `identity audit` command, deliberately not duplicated (Task 6 doc comment).
- D6 (versioned business registry, Approach A) → Task 4, cross-referenced from `PolitiqueRetentionV1.cs`'s doc comment.
- Spec § 6 (refusals never dropped, ⊥ is not an error) → `ExtracteurDeGroupes` refusal test (Task 1), `EnrichisseurDeGroupe` absence test (Task 2), `PolitiqueRetentionV1` absence test (Task 3).
- Spec § 7 (reproducibility, groups of 2 and 3+) → `Deux_emissions_du_rapport_sont_identiques` (Task 6) and the real oracle's 108 pairs + 4 triplets exercised via `Le_corpus_reel_produit_112_groupes_et_4_non_tranches` (Task 6).
- Spec § 9 (contractual errors propagate unchanged) → Task 6, one representative case; the exhaustive 7-error battery is not reduplicated because it already covers this exact `Porteur.Deriver` call (`IdentityCommandTests.Les_sept_erreurs_du_contrat_sont_restituees_par_la_commande`) — noted explicitly in the test file's doc comment.

**Placeholder scan:** no "TBD"/"TODO"/"handle appropriately" anywhere; every step shows complete code.

**Type consistency:** `FichierEnrichi` (Task 2) → consumed identically in Task 3 and Task 5; `ExemplaireClasse` (Task 3) → consumed identically in `GroupeClasse` (Task 5); `ExtracteurDeGroupes.Extraire`'s tuple shape (Task 1) → consumed identically in `GenerateurDeRapport.Generer` (Task 5); `GenerateurDeRapport.Generer(W, IObservationsSource)` (Task 5) → called identically in `DuplicatesCommand.Deriver` (Task 6). Verified consistent across all six tasks.
