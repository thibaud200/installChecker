# Redondance versionnée générique Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produire un rapport déterministe de redondances versionnées pour tout type de fichier, à
partir du nom et des métadonnées déjà observées, sans aucune action destructive.

**Architecture:** Le moteur du module projette Omega en fichiers observés, exécute des fournisseurs
de preuves indépendants, arbitre famille/version/variantes, puis regroupe les contenus différents
dans un contrat JSON commun. L'enveloppe seule instancie `LecteurDObservationsSqlite`; Identity,
Identity.Access et le Scanner restent inchangés.

**Tech Stack:** .NET 10, C# 14, BCL uniquement dans le moteur, xUnit 2.9.3,
`System.Text.Json`, adaptateur SQLite existant.

## Global Constraints

- Tout comportement métier et tout test F1 vivent sous `modules/duplicate-files`.
- Le seul code extérieur au module est le routage minimal dans `apps/cli/InstallChecker.Cli/Program.cs`.
- Ne modifier aucun fichier sous `src/InstallChecker.Identity`, `src/InstallChecker.Identity.Access`,
  `tests/InstallChecker.Identity.Tests`, `tests/oracle`, `docs/identity`, `docs/conformite` ou `registre`.
- Ne modifier ni le Scanner, ni son schéma, ni ses extracteurs.
- Ne pas ajouter de package NuGet au moteur ou aux tests.
- Ne pas rouvrir les fichiers observés, recalculer leurs hash, écrire dans SQLite, utiliser le réseau
  ou `%TEMP%` dans le code de production.
- Ne produire aucune action `Supprimer`, `AjouterAuPlanDeSuppression`, `DeplacerVersCorbeille` ou
  équivalente dans le contrat F1.
- Conserver `duplicate-files/version-redundancy/v1` comme version exacte du contrat public.
- Conserver un ordre et des identifiants déterministes, sans heure courante ni valeur aléatoire.
- Suivre TDD : un test rouge ciblé, l'implémentation minimale, puis le test vert avant chaque commit.
- Spécification normative du jalon :
  `modules/duplicate-files/docs/specs/2026-07-19-redondance-versionnee-design.md`.

---

## File Map

### Moteur à créer

- `VersionComparable.cs` : parsing, forme canonique et comparaison numérique/calendaire.
- `NormalisationVersionnee.cs` : normalisation conservatrice des textes, formats et variantes.
- `FichierObserve.cs` : projection d'un acte et de son contexte vers l'entrée du module.
- `ProjectionFichiersObserves.cs` : lecture cohérente de `IObservationsSource` en un seul snapshot.
- `PreuveVersionnee.cs` : dimensions, sources, règles, forces et diagnostics fermés.
- `IFournisseurDePreuves.cs` : contrat pur commun aux fournisseurs.
- `LectureAttributs.cs` : lecture sûre des `ValeurObservee.Texte`.
- `FournisseurNomDeFichier.cs` : fournisseur universel.
- `FournisseurVersionInfo.cs` : produit, éditeur, ProductVersion et FileVersion de repli.
- `FournisseurMsi.cs` : UpgradeCode, produit, version, fabricant et langue.
- `FournisseurAppx.cs` : Name, Publisher, Version et architecture.
- `FournisseurPe.cs` : architecture PE.
- `FournisseurAuthenticode.cs` : éditeur signé.
- `ResolutionArtefactVersionne.cs` : famille, version, variante, confiance et états résolus.
- `ResolveurArtefactVersionne.cs` : arbitrage déterministe des preuves.
- `ContratRedondanceVersionnee.cs` : DTO du rapport public.
- `GenerateurRedondanceVersionnee.cs` : déduplication par contenu, groupes et synthèse.

### Moteur à modifier

- `IdentifiantsStables.cs` : identifiant stable des familles versionnées.
- `ContratUiDoublonsExacts.cs` : ajout additif de la constante de contrat F1.

### Enveloppe et CLI

- `RedondanceVersionneeCommand.cs` : lecture SQLite, sérialisation et codes de sortie.
- `apps/cli/InstallChecker.Cli/Program.cs` : route `duplicates versions` avant la route historique.
- `modules/duplicate-files/README.md` : commande, contrat et limites F1.

### Tests à créer

- `VersionComparableTests.cs`
- `ProjectionFichiersObservesTests.cs`
- `FournisseurNomDeFichierTests.cs`
- `FournisseursMetadonneesTests.cs`
- `FournisseursPaquetsTests.cs`
- `ResolveurArtefactVersionneTests.cs`
- `ContratRedondanceVersionneeTests.cs`
- `GenerateurRedondanceVersionneeTests.cs`
- `RedondanceVersionneeCommandTests.cs`
- `MesureRedondanceVersionneeTests.cs`

---

### Task 1: Versions comparables et normalisation conservatrice

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/VersionComparable.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/NormalisationVersionnee.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/VersionComparableTests.cs`

**Interfaces:**
- Produces: `VersionComparable.TryLire(string, bool, out VersionComparable)`.
- Produces: `VersionComparable.CompareTo(VersionComparable)` et `Canonique`.
- Produces: `NormalisationVersionnee.Texte`, `Architecture` et `Format`.

- [ ] **Step 1: Écrire les tests rouges du parseur**

Créer des théories couvrant exactement :

```csharp
[Theory]
[InlineData("1", "1")]
[InlineData("1.2.0", "1.2")]
[InlineData("v1.10", "1.10")]
[InlineData("2026-07-19", "2026-07-19")]
[InlineData("2026.07.19", "2026-07-19")]
public void Une_version_reconnue_possede_une_forme_canonique(string brute, string canonique)
{
    Assert.True(VersionComparable.TryLire(brute, autoriserPrefixeV: true, out var version));
    Assert.Equal(canonique, version.Canonique);
}

[Theory]
[InlineData("1.2.3.4.5")]
[InlineData("2.0-beta")]
[InlineData("2026-13-40")]
[InlineData("version finale")]
public void Une_version_ambigue_est_refusee(string brute) =>
    Assert.False(VersionComparable.TryLire(brute, autoriserPrefixeV: true, out _));
```

Ajouter les assertions `1.10 > 1.9`, `1.2 == 1.2.0` et l'exception lors d'une comparaison entre
schémas numérique et calendaire.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter FullyQualifiedName~VersionComparableTests
```

Expected: échec de compilation car `VersionComparable` n'existe pas.

- [ ] **Step 3: Implémenter les primitives minimales**

Utiliser cette surface publique :

```csharp
public enum SchemaVersionComparable { Numerique, Calendaire }

public readonly record struct VersionComparable(
    SchemaVersionComparable Schema,
    int Composant1,
    int Composant2,
    int Composant3,
    int Composant4,
    string Canonique) : IComparable<VersionComparable>
{
    public static bool TryLire(string valeur, bool autoriserPrefixeV, out VersionComparable version);
    public int CompareTo(VersionComparable other);
}

public static class NormalisationVersionnee
{
    public static string Texte(string valeur);
    public static string? Architecture(string? valeur);
    public static string Format(string chemin);
}
```

Dans `TryLire`, tester d'abord les formes exactes `yyyy-MM-dd` et `yyyy.MM.dd`. Une date de cette
forme mais invalide retourne `false` sans repli numérique. Pour le numérique, accepter un à quatre
entiers séparés par `.`, retirer `v` uniquement lorsque le paramètre l'autorise, remplir les
composants finaux avec zéro et retirer les zéros finaux de `Canonique` en gardant au moins un
composant.

`NormalisationVersionnee.Texte` applique `Trim`, réduit toute suite d'espaces Unicode à un espace et
utilise `ToUpperInvariant` pour la clé. `Architecture` applique la table fermée suivante :

```text
x86, win32              -> x86
x64, amd64, win64       -> x64
arm                     -> arm
arm64, aarch64          -> arm64
autre valeur non vide   -> valeur textuelle normalisée
```

`Format` retourne `.tar.gz`, `.tar.bz2` ou `.tar.xz` lorsque le nom se termine ainsi, sinon
`Path.GetExtension(chemin).ToLowerInvariant()`, et `<sans-extension>` lorsque l'extension est vide.

- [ ] **Step 4: Vérifier les tests verts**

Run: même commande que l'étape 2.

Expected: tous les tests `VersionComparableTests` passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/VersionComparable.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/NormalisationVersionnee.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/VersionComparableTests.cs
git commit -m "feat(duplicate-files): add comparable version primitives"
```

---

### Task 2: Projection Omega et contrat de preuves

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FichierObserve.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ProjectionFichiersObserves.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/PreuveVersionnee.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/IFournisseurDePreuves.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/LectureAttributs.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ProjectionFichiersObservesTests.cs`

**Interfaces:**
- Consumes: `IObservationsSource`, `ActeObservation`, `ContexteObservation` et `IdentifiantsStables`.
- Produces: `ProjectionFichiersObserves.Projeter(IObservationsSource)`.
- Produces: `IFournisseurDePreuves.Extraire(FichierObserve)`.

- [ ] **Step 1: Écrire les tests rouges de projection**

Avec une fausse source en mémoire, prouver : ordre par `ActeId`, association du chemin au bon acte,
normalisation du SHA-256, stabilité du `FichierId`, refus d'un contexte manquant et refus d'un
contexte orphelin.

Le test nominal doit vérifier cette forme :

```csharp
var fichiers = ProjectionFichiersObserves.Projeter(source);

Assert.Equal([1L, 2L], fichiers.Select(f => f.ActeId));
Assert.Equal(@"C:\Corpus\outil-1.0.zip", fichiers[0].Chemin);
Assert.Equal(HashA, fichiers[0].ContenuSha256);
Assert.Equal(IdentifiantsStables.PourFichier(HashA, fichiers[0].Chemin), fichiers[0].FichierId);
```

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter FullyQualifiedName~ProjectionFichiersObservesTests
```

Expected: échec de compilation sur les nouveaux contrats.

- [ ] **Step 3: Implémenter les contrats exacts**

Créer les types suivants :

```csharp
public sealed record FichierObserve(
    long ActeId,
    string FichierId,
    string Chemin,
    long Taille,
    string ContenuSha256,
    IReadOnlyDictionary<Attribut, ValeurObservee> Attributs);

public enum DimensionPreuveVersionnee
{
    CleFamille, LibelleFamille, IdentifiantLivraison, Version, Editeur, Format,
    Architecture, Langue, Edition, Distribution
}

public enum SourcePreuveVersionnee
{
    NomFichier, VersionInfo, Msi, Appx, Pe, Authenticode, Arbitre
}

public enum ForcePreuveVersionnee { Faible, Moyenne, Forte }

public enum CodeDiagnosticVersionne
{
    AttributInvalide, VersionNonComparable, ConflitDeVersion,
    ConflitDeFamille, VarianteNonObservee
}

public sealed record PreuveVersionnee(
    string FichierId,
    DimensionPreuveVersionnee Dimension,
    string ValeurBrute,
    string ValeurNormalisee,
    SourcePreuveVersionnee Source,
    ForcePreuveVersionnee Force,
    string Regle,
    string VersionFournisseur);

public sealed record DiagnosticVersionne(
    string FichierId,
    CodeDiagnosticVersionne Code,
    SourcePreuveVersionnee Source,
    string DetailNormalise);

public sealed record ResultatFournisseur(
    IReadOnlyList<PreuveVersionnee> Preuves,
    IReadOnlyList<DiagnosticVersionne> Diagnostics);

public interface IFournisseurDePreuves
{
    ResultatFournisseur Extraire(FichierObserve fichier);
}
```

`ProjectionFichiersObserves.Projeter` appelle une seule fois `ProjeterModele` et `ProjeterContexte`,
exige exactement le même ensemble d'identifiants, puis trie par identifiant. `LectureAttributs.Texte`
retourne uniquement la valeur non vide d'une `ValeurObservee.Texte`; un entier ou une absence donne
`null` sans exception.

- [ ] **Step 4: Vérifier les tests verts**

Run: même commande que l'étape 2.

Expected: tous les tests de projection passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FichierObserve.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ProjectionFichiersObserves.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/PreuveVersionnee.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/IFournisseurDePreuves.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/LectureAttributs.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ProjectionFichiersObservesTests.cs
git commit -m "feat(duplicate-files): define version evidence contract"
```

---

### Task 3: Fournisseur universel fondé sur le nom

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurNomDeFichier.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/FournisseurNomDeFichierTests.cs`

**Interfaces:**
- Consumes: `FichierObserve`, `VersionComparable`, `NormalisationVersionnee`.
- Produces: preuves `LibelleFamille`, `Version`, `Format`, `Architecture` et `Distribution`.

- [ ] **Step 1: Écrire les tests rouges du fournisseur générique**

Tester au minimum les cas suivants :

```text
outil-1.2.zip                 -> famille OUTIL, version 1.2, format .zip
outil_v1.3_x64.zip            -> famille OUTIL, version 1.3, architecture x64
rapport-2026-07-19.pdf        -> famille RAPPORT, version calendaire
archive-1.2.tar.gz            -> famille ARCHIVE, format .tar.gz
outil-2.0-portable.zip        -> distribution portable
outil-pro-1.2.zip             -> famille OUTIL-PRO
outil-home-1.3.zip            -> famille OUTIL-HOME
outil-2.0-beta.zip            -> aucune version comparable
readme.pdf                    -> preuve de format uniquement
```

Vérifier que les preuves du nom ont `Force = Faible`, `Source = NomFichier` et une version de
fournisseur constante `filename/v1`.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter FullyQualifiedName~FournisseurNomDeFichierTests
```

Expected: échec de compilation car le fournisseur n'existe pas.

- [ ] **Step 3: Implémenter l'extraction bornée**

Déclarer :

```csharp
public sealed class FournisseurNomDeFichier : IFournisseurDePreuves
{
    public const string Version = "filename/v1";
    public ResultatFournisseur Extraire(FichierObserve fichier);
}
```

Chercher le dernier jeton correspondant soit à `v?N(.N){0,3}`, soit à `AAAA-MM-JJ`, avec une limite
non alphanumérique avant et après. Accepter après le jeton uniquement des séparateurs et ces variantes
fermées :

```text
x86, win32, x64, amd64, win64, arm, arm64, aarch64
portable
setup, installer, install
```

Normaliser `setup`, `installer` et `install` en `installable`. Ne déduire ni langue ni édition depuis
le nom en F1. Si le suffixe contient un autre mot, ne pas extraire cette occurrence de version.
Retirer les séparateurs finaux du radical sans retirer sa ponctuation interne.

Ne pas émettre `LibelleFamille` lorsque le radical normalisé vaut `SETUP`, `INSTALL`, `INSTALLER`,
`UPDATE` ou `PACKAGE`. Continuer à émettre les preuves de version, format et variante applicables.

Un jeton complet terminé par `alpha`, `beta`, `preview`, `rc`, `revA` ou `final2` produit une preuve
de version brute et le diagnostic `VersionNonComparable`, sans version résolue. Un autre jeton qui
ressemble à une version mais que `VersionComparable.TryLire` refuse produit le même diagnostic
seulement lorsqu'il occupe la position attendue. Un fichier sans jeton ne produit pas de diagnostic.

- [ ] **Step 4: Vérifier les tests verts**

Run: même commande que l'étape 2.

Expected: tous les cas de noms génériques passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurNomDeFichier.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/FournisseurNomDeFichierTests.cs
git commit -m "feat(duplicate-files): extract version evidence from filenames"
```

---

### Task 4: Fournisseurs VersionInfo, PE et Authenticode

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurVersionInfo.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurPe.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurAuthenticode.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/FournisseursMetadonneesTests.cs`

**Interfaces:**
- Consumes: `LectureAttributs`, `VersionComparable`, `NormalisationVersionnee`.
- Produces: preuves structurées de produit, éditeur, version et architecture.

- [ ] **Step 1: Écrire les tests rouges des métadonnées**

Construire des `FichierObserve` en mémoire et vérifier :

- `ProductName + CompanyName + ProductVersion` produisent trois preuves moyennes ;
- `FileVersion` n'est pas émise lorsque `ProductVersion` est présente ;
- `FileVersion` est utilisée lorsque `ProductVersion` est absente ;
- une version textuelle invalide produit `VersionNonComparable` sans exception ;
- `pe_info.machine=AMD64` devient `x64` ;
- `authenticode.subject` produit une preuve forte d'éditeur ;
- les attributs absents produisent un résultat vide.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter FullyQualifiedName~FournisseursMetadonneesTests
```

Expected: échec de compilation sur les trois fournisseurs.

- [ ] **Step 3: Implémenter les fournisseurs purs**

Chaque classe implémente `IFournisseurDePreuves`, expose une constante de version et lit seulement
les attributs suivants :

```text
FournisseurVersionInfo version_info/v1
  version_info.product_name
  version_info.company_name
  version_info.product_version
  version_info.file_version

FournisseurPe pe/v1
  pe_info.machine

FournisseurAuthenticode authenticode/v1
  authenticode.subject
```

Utiliser les règles publiques `VersionInfoProductName`, `VersionInfoCompanyName`,
`VersionInfoProductVersion`, `VersionInfoFileVersion`, `PeMachine` et `AuthenticodeSubject`.
Une donnée absente signifie « fournisseur non applicable », sans diagnostic. Une donnée présente mais
invalide produit un diagnostic stable qui ne contient pas de texte d'exception système.

- [ ] **Step 4: Vérifier les tests verts**

Run: même commande que l'étape 2.

Expected: tous les tests des trois fournisseurs passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurVersionInfo.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurPe.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurAuthenticode.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/FournisseursMetadonneesTests.cs
git commit -m "feat(duplicate-files): add file metadata evidence providers"
```

---

### Task 5: Fournisseurs MSI et MSIX/Appx

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurMsi.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurAppx.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/FournisseursPaquetsTests.cs`

**Interfaces:**
- Consumes: contrat de preuves des Tasks 1 et 2.
- Produces: clés natives fortes et variantes propres aux paquets.

- [ ] **Step 1: Écrire les tests rouges MSI/Appx**

Prouver les sorties suivantes :

```text
MSI UpgradeCode valide       -> CleFamille forte, GUID canonique minuscule
MSI ProductName              -> LibelleFamille forte
MSI ProductVersion           -> Version forte
MSI Manufacturer             -> Editeur forte
MSI ProductLanguage          -> Langue forte
MSI ProductCode              -> IdentifiantLivraison secondaire
MSI architecture absente     -> aucune preuve d'architecture
MSI UpgradeCode invalide     -> AttributInvalide, aucune CleFamille

Appx Name + Publisher        -> CleFamille forte déterministe
Appx Version                 -> Version forte
Appx ProcessorArchitecture   -> Architecture forte normalisée
Appx incomplet               -> aucune CleFamille native
```

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter FullyQualifiedName~FournisseursPaquetsTests
```

Expected: échec de compilation sur `FournisseurMsi` et `FournisseurAppx`.

- [ ] **Step 3: Implémenter les clés natives**

Créer deux classes `IFournisseurDePreuves` de versions `msi/v1` et `appx/v1`.

Pour MSI, valider `upgrade_code` avec `Guid.TryParse` et normaliser par `guid.ToString("D")`. Émettre
le format natif `.msi` lorsqu'au moins une propriété MSI est présente. Conserver `product_code`
uniquement comme preuve explicative avec la règle `MsiProductCode`, sans l'utiliser comme
`CleFamille`.

Pour Appx, n'émettre la clé native que lorsque `name` et `publisher` sont tous deux présents. Former
sa valeur normalisée par l'encodage longueur-valeur de leurs deux formes normalisées, puis SHA-256,
avec le préfixe `appx-family:`. Émettre comme format natif l'extension lorsqu'elle vaut `.appx`,
`.msix`, `.appxbundle` ou `.msixbundle`, sinon `<appx-package>`. L'arbitre choisit ce format natif
avant la preuve faible du nom.

- [ ] **Step 4: Vérifier les tests verts**

Run: même commande que l'étape 2.

Expected: tous les tests MSI/Appx passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurMsi.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/FournisseurAppx.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/FournisseursPaquetsTests.cs
git commit -m "feat(duplicate-files): add package evidence providers"
```

---

### Task 6: Arbitrage de famille, version, variantes et confiance

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ResolutionArtefactVersionne.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ResolveurArtefactVersionne.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ResolveurArtefactVersionneTests.cs`

**Interfaces:**
- Consumes: un SHA-256, ses fichiers physiques et l'ensemble dédupliqué de leurs preuves.
- Produces: un `ArtefactVersionne` au niveau du contenu, jamais un vote par chemin.

- [ ] **Step 1: Écrire les tests rouges de l'arbitre**

Tester séparément :

- clé MSI prioritaire sur les libellés ;
- clé Appx prioritaire sur le nom ;
- `ProductName + CompanyName` utilisés lorsque la clé native manque ;
- radical du nom utilisé seul avec confiance faible ;
- nom `2.0` et ProductVersion `1.9` produisent `ConflitDeVersion` ;
- deux familles structurées incompatibles produisent `ConflitDeFamille` ;
- ProductVersion `2.0` et FileVersion `2.0.1534.0` ne produisent aucun conflit ;
- architecture connue d'un seul côté reste dans la variante résolue et empêchera son rapprochement ;
- absence d'architecture MSI produit `VarianteNonObservee` et plafonne la confiance à moyenne ;
- concordance indépendante nom + métadonnées augmente la confiance d'un cran au maximum ;
- deux chemins de même hash portant des noms contradictoires produisent un conflit si aucune preuve
  structurée ne les départage.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter FullyQualifiedName~ResolveurArtefactVersionneTests
```

Expected: échec de compilation sur les types de résolution.

- [ ] **Step 3: Implémenter le résultat résolu**

Utiliser ces types :

```csharp
public enum NiveauConfianceVersionnee { Faible, Moyenne, Forte }
public enum EtatResolutionVersionnee { Comparable, VersionNonComparable, ConflitDeVersion, ConflitDeFamille }

public sealed record VarianteVersionnee(
    string Format,
    string? Architecture,
    string? Langue,
    string? Edition,
    string? Distribution,
    bool Partielle);

public sealed record ArtefactVersionne(
    string ContenuSha256,
    IReadOnlyList<FichierObserve> Fichiers,
    string? CleFamille,
    string? LibelleFamille,
    SourcePreuveVersionnee? SourceFamille,
    VersionComparable? Version,
    VarianteVersionnee Variante,
    NiveauConfianceVersionnee Confiance,
    EtatResolutionVersionnee Etat,
    IReadOnlyList<PreuveVersionnee> Preuves,
    IReadOnlyList<DiagnosticVersionne> Diagnostics);

public static class ResolveurArtefactVersionne
{
    public static ArtefactVersionne Resoudre(
        string contenuSha256,
        IReadOnlyList<FichierObserve> fichiers,
        IReadOnlyList<PreuveVersionnee> preuves,
        IReadOnlyList<DiagnosticVersionne> diagnostics);
}
```

Dédupliquer les preuves par `(Dimension, ValeurNormalisee, Source, Regle)`, sans compter leur nombre
comme renforcement. Résoudre la famille dans l'ordre clé native, `ProductName + CompanyName` issus
de VersionInfo, puis libellé du nom. Un sujet Authenticode ne remplace jamais `CompanyName` dans la
clé ; il sert seulement au renforcement du groupe. Résoudre la version dans l'ordre MSI/Appx,
ProductVersion, FileVersion de repli, nom.

`CleFamille` contient toujours la forme normalisée utilisée pour le regroupement.
`LibelleFamille` conserve la `ValeurBrute` de la preuve retenue pour l'affichage.
`SourceFamille` conserve la source de la clé retenue. Les diagnostics créés par le résolveur portent
`Source = Arbitre`.

Si une version structurée applicable et le nom diffèrent, produire le conflit. Pour le format,
préférer MSI/Appx à l'extension. Pour l'architecture, la langue, l'édition et la distribution,
conserver une valeur unique ou produire le conflit de famille lorsque deux valeurs fortes de la
même dimension divergent sur un même contenu.

Considérer l'architecture comme requise pour `.exe`, `.msi`, `.appx`, `.msix`, `.appxbundle` et
`.msixbundle`. Pour les autres formats, son absence n'est pas partielle. Considérer langue, édition
et distribution comme applicables à un groupe dès qu'au moins un artefact de ce groupe les expose ;
une valeur connue d'un seul côté interdit alors la relation.

- [ ] **Step 4: Vérifier les tests verts**

Run: même commande que l'étape 2.

Expected: tous les tests d'arbitrage passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ResolutionArtefactVersionne.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ResolveurArtefactVersionne.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ResolveurArtefactVersionneTests.cs
git commit -m "feat(duplicate-files): resolve versioned artifacts"
```

---

### Task 7: Identifiants stables et contrat public F1

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/IdentifiantsStables.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ContratUiDoublonsExacts.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ContratRedondanceVersionnee.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/IdentifiantsStablesTests.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ContratRedondanceVersionneeTests.cs`

**Interfaces:**
- Produces: `IdentifiantsStables.PourGroupeVersionne`.
- Produces: tous les DTO sérialisables de `duplicate-files/version-redundancy/v1`.

- [ ] **Step 1: Écrire les tests rouges du contrat**

Vérifier qu'un groupe conserve son ID lorsque seule une version ou un chemin change, et change d'ID
si la famille, le format ou une variante change. Vérifier aussi que le JSON sérialise les enums en
chaînes et n'expose aucune action destructive.

Le test d'identifiant appelle exactement :

```csharp
var id = IdentifiantsStables.PourGroupeVersionne(
    "msi", "{cle-famille}", SchemaVersionComparable.Numerique,
    ".msi", null, "1036", null, null);

Assert.Matches("^version:sha256:[0-9a-f]{64}$", id);
```

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter "FullyQualifiedName~IdentifiantsStablesTests|FullyQualifiedName~ContratRedondanceVersionneeTests"
```

Expected: échec de compilation sur la méthode et les DTO F1.

- [ ] **Step 3: Implémenter l'encodage et les DTO**

Ajouter à `IdentifiantsStables` :

```csharp
public static string PourGroupeVersionne(
    string sourceFamille,
    string cleFamille,
    SchemaVersionComparable schemaVersion,
    string format,
    string? architecture,
    string? langue,
    string? edition,
    string? distribution);
```

Encoder `version-family/v1` puis les huit champs avec, pour chacun, une longueur 32 bits non signée
en ordre réseau suivie des octets UTF-8. Encoder `null` par `uint.MaxValue` et une chaîne vide par la
longueur zéro. Calculer SHA-256 et préfixer par `version:sha256:`.

Créer les enums `CategorieRedondanceVersionnee`, `RoleComparaisonVersionnee`,
`ActionVersionnee`, `RaisonBlocageVersionnee` et `MotifExclusionVersionnee`, puis les records :

```csharp
public sealed record RapportRedondanceVersionnee(
    string VersionContrat,
    SourceRapportVersionnee Source,
    SyntheseRedondanceVersionnee Synthese,
    IReadOnlyList<GroupeRedondanceVersionnee> Groupes,
    IReadOnlyDictionary<MotifExclusionVersionnee, int> ExclusionsParMotif);

public sealed record GroupeRedondanceVersionnee(
    string GroupeId,
    CategorieRedondanceVersionnee Categorie,
    string Famille,
    VarianteVersionnee Variante,
    NiveauConfianceVersionnee Confiance,
    string VersionReference,
    IReadOnlyList<ArtefactVersionneRapporte> Artefacts,
    IReadOnlyList<RaisonBlocageVersionnee> Blocages);

public sealed record ArtefactVersionneRapporte(
    string ContenuSha256,
    IReadOnlyList<FichierVersionneRapporte> Fichiers,
    string Version,
    RoleComparaisonVersionnee? Role,
    IReadOnlyList<PreuveVersionnee> Preuves,
    IReadOnlyList<DiagnosticVersionne> Diagnostics,
    IReadOnlyList<ActionVersionnee> Actions,
    IReadOnlyList<RaisonBlocageVersionnee> Blocages);
```

Définir exactement les valeurs fermées et DTO auxiliaires suivants :

```csharp
public enum CategorieRedondanceVersionnee { VersionRedundancyCandidate }
public enum RoleComparaisonVersionnee { ReferenceRecente, VersionAnterieure, MemeVersion }
public enum ActionVersionnee { Examiner, Ignorer }

public enum RaisonBlocageVersionnee
{
    RevueHumaineObligatoire,
    SuppressionAutomatiqueInterdite,
    ConfianceFaible,
    VarianteNonObservee,
    MetadonneesContradictoires
}

public enum MotifExclusionVersionnee
{
    FamilleInsuffisante,
    AucuneVersion,
    VersionNonComparable,
    ConflitDeVersion,
    ConflitDeFamille,
    VarianteIncompatible,
    MemeVersionSeulement
}

public sealed record SourceRapportVersionnee(int NombreFichiersObserves);

public sealed record SyntheseRedondanceVersionnee(
    int NombreGroupes,
    int NombreContenus,
    int NombreReferencesRecentes,
    int NombreVersionsAnterieures,
    int NombreConflits);

public sealed record FichierVersionneRapporte(
    string FichierId,
    string Chemin,
    long Taille);
```

`SourceRapportVersionnee` est dérivé de la projection déjà chargée afin de ne pas relire Omega.
Ajouter `VersionsContratDuplicateFiles.RedondanceVersionneeV1` dans
`ContratUiDoublonsExacts.cs`, à côté de la constante existante, sans renommer ni déplacer
`DoublonsExactsV1`.

- [ ] **Step 4: Vérifier les tests verts**

Run: même commande que l'étape 2.

Expected: identifiants et sérialisation passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/IdentifiantsStables.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ContratUiDoublonsExacts.cs modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ContratRedondanceVersionnee.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/IdentifiantsStablesTests.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ContratRedondanceVersionneeTests.cs
git commit -m "feat(duplicate-files): define version redundancy report contract"
```

---

### Task 8: Génération des groupes versionnés

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/GenerateurRedondanceVersionnee.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/GenerateurRedondanceVersionneeTests.cs`

**Interfaces:**
- Consumes: `IObservationsSource` et tous les fournisseurs F1.
- Produces: `RapportRedondanceVersionnee Generer(IObservationsSource)`.

- [ ] **Step 1: Écrire les scénarios rouges de bout en bout moteur**

Avec une source Omega en mémoire, couvrir :

- `outil-1.0.zip` et `outil-2.0.zip` donnent un groupe faible avec ancien et référence ;
- deux PDF calendaires donnent un groupe ;
- deux contenus de même version seulement ne donnent aucun groupe et incrémentent
  `MemeVersionSeulement` ;
- deux chemins de même hash comptent un contenu logique et conservent deux fichiers affichés ;
- deux architectures connues différentes ne sont jamais dans le même groupe ;
- deux architectures absentes se comparent avec `VarianteNonObservee` ;
- une architecture connue et une inconnue ne créent aucune relation ;
- deux langues MSI différentes donnent des groupes distincts ;
- un conflit reste diagnostiqué mais ne reçoit aucun rôle ;
- deux générations du même snapshot donnent des objets égaux et le même ordre.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter FullyQualifiedName~GenerateurRedondanceVersionneeTests
```

Expected: échec de compilation sur le générateur.

- [ ] **Step 3: Implémenter l'orchestration linéaire**

Déclarer :

```csharp
public static class GenerateurRedondanceVersionnee
{
    public static RapportRedondanceVersionnee Generer(
        IObservationsSource omega,
        IReadOnlyList<IFournisseurDePreuves>? fournisseurs = null);
}
```

La liste par défaut est, dans cet ordre stable : nom, VersionInfo, MSI, Appx, PE, Authenticode.

Algorithme obligatoire :

1. projeter modèle et contexte Omega une seule fois ;
2. grouper les fichiers par SHA-256 normalisé ;
3. trier les chemins de chaque contenu par chemin Windows canonique puis exécuter les fournisseurs ;
4. dédupliquer les preuves avant arbitrage en conservant comme origine le premier chemin canonique ;
5. résoudre un `ArtefactVersionne` par contenu ;
6. indexer les comparables par clé famille + schéma de version + variante complète ;
7. laisser les variantes connues/inconnues dans des clés distinctes ;
8. exiger au moins deux versions distinctes par groupe public ;
9. choisir le maximum comme référence et affecter les rôles ;
10. trier groupes par `GroupeId`, artefacts par version décroissante puis SHA-256, fichiers par
    chemin Windows canonique, preuves et blocages par valeur d'enum ;
11. remplir la synthèse et les exclusions sans sérialiser tous les fichiers sans version.

La confiance du groupe est le minimum des confiances de ses artefacts. Si tous portent le même sujet
Authenticode non vide, l'éditeur signé concordant renforce le groupe d'un cran au maximum, sans
dépasser `Forte` ni dépasser le plafond `Moyenne` imposé par une variante requise non observée.

Tous les artefacts publics portent `Examiner` et `Ignorer`, ainsi que
`RevueHumaineObligatoire` et `SuppressionAutomatiqueInterdite`. Ajouter `ConfianceFaible`,
`VarianteNonObservee` ou `MetadonneesContradictoires` lorsque applicable.

- [ ] **Step 4: Vérifier les tests verts et la non-régression moteur**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj
```

Expected: tous les tests du moteur Duplicate Files passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/GenerateurRedondanceVersionnee.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/GenerateurRedondanceVersionneeTests.cs
git commit -m "feat(duplicate-files): generate version redundancy groups"
```

---

### Task 9: Enveloppe SQLite et contrat de commande

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/RedondanceVersionneeCommand.cs`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/RedondanceVersionneeCommandTests.cs`

**Interfaces:**
- Consumes: `LecteurDObservationsSqlite` existant et `GenerateurRedondanceVersionnee`.
- Produces: `RedondanceVersionneeCommand.Deriver` avec codes `0` et `1`.

- [ ] **Step 1: Écrire les tests rouges de commande**

Créer dans un dossier temporaire `outil-1.0.zip` et `outil-2.0.zip`, les scanner dans une base avec
`ScanCommand.Run`, puis appeler :

```csharp
var code = RedondanceVersionneeCommand.Deriver(dbPath, sortie, erreurs);

Assert.Equal(0, code);
Assert.Empty(erreurs.ToString());
using var json = JsonDocument.Parse(sortie.ToString());
Assert.Equal("duplicate-files/version-redundancy/v1",
    json.RootElement.GetProperty("VersionContrat").GetString());
Assert.Single(json.RootElement.GetProperty("Groupes").EnumerateArray());
```

Ajouter : rapport vide valide, base absente avec code `1` et stdout vide, deux émissions identiques,
et rescan du même volume sans observation fantôme.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj --filter FullyQualifiedName~RedondanceVersionneeCommandTests
```

Expected: échec de compilation car la commande n'existe pas.

- [ ] **Step 3: Implémenter l'enveloppe en lecture seule**

Créer :

```csharp
public static class RedondanceVersionneeCommand
{
    public static int Deriver(
        string cheminBase,
        TextWriter output,
        TextWriter errors);
}
```

Instancier `LecteurDObservationsSqlite`, générer le rapport et le sérialiser avec
`WriteIndented = true`, `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` et
`JsonStringEnumConverter`. Capturer uniquement `ErreurOmega`, écrire son message sur stderr et
retourner `1`. Toute autre exception reste visible comme défaillance d'implémentation. Ne produire
aucun JSON partiel en cas d'erreur.

- [ ] **Step 4: Vérifier les tests verts et l'enveloppe complète**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj
```

Expected: tous les tests de l'enveloppe Duplicate Files passent.

- [ ] **Step 5: Commit ciblé**

```powershell
git add modules/duplicate-files/src/InstallChecker.DuplicateFiles/RedondanceVersionneeCommand.cs modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/RedondanceVersionneeCommandTests.cs
git commit -m "feat(duplicate-files): expose version redundancy command"
```

---

### Task 10: Routage CLI et documentation utilisateur

**Files:**
- Modify: `apps/cli/InstallChecker.Cli/Program.cs`
- Modify: `modules/duplicate-files/README.md`

**Interfaces:**
- Consumes: `RedondanceVersionneeCommand.Deriver`.
- Produces: `installchecker duplicates versions <base.db>`.

- [ ] **Step 1: Constater le smoke test rouge**

Run:

```powershell
dotnet run --project apps/cli/InstallChecker.Cli/InstallChecker.Cli.csproj -- duplicates versions base-absente.db
```

Expected avant modification : la route historique interprète `versions` comme la base et
`base-absente.db` comme le registre ; le message ne désigne donc pas la base attendue de F1.

- [ ] **Step 2: Ajouter la route avant `duplicates <base> <registre>`**

Ajouter exactement :

```csharp
if (args is ["duplicates", "versions", var cheminBaseVersions])
    return RedondanceVersionneeCommand.Deriver(cheminBaseVersions, Console.Out, Console.Error);
```

Ajouter dans `Usage()` :

```csharp
Console.Error.WriteLine("        installchecker duplicates versions <base.db>");
```

Ne déplacer et ne réécrire aucune autre route.

- [ ] **Step 3: Mettre à jour le README du module**

Ajouter le flux `duplicates versions`, sa syntaxe, le contrat JSON, les niveaux de confiance, les
variantes et l'interdiction de suppression. Remplacer uniquement la limite disant que le module ne
regroupe aucune version par une formulation distinguant l'état actuel après F1 des futures versions
préliminaires et sources externes.

- [ ] **Step 4: Vérifier le routage construit**

Run:

```powershell
dotnet build apps/cli/InstallChecker.Cli/InstallChecker.Cli.csproj -c Release
dotnet run --project apps/cli/InstallChecker.Cli/InstallChecker.Cli.csproj -- duplicates versions base-absente.db
```

Expected: build réussi ; code de sortie `1` et message stderr désignant `base-absente.db` comme
support d'observations introuvable.

- [ ] **Step 5: Commit ciblé**

```powershell
git add apps/cli/InstallChecker.Cli/Program.cs modules/duplicate-files/README.md
git commit -m "feat(cli): route version redundancy reports"
```

---

### Task 11: Mesure, vérification globale et garde-fous

**Files:**
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/MesureRedondanceVersionneeTests.cs`
- Modify: `modules/duplicate-files/docs/specs/2026-07-19-redondance-versionnee-design.md`
- Modify: `modules/duplicate-files/docs/specs/2026-07-19-module-doublon-roadmap-design.md`

**Interfaces:**
- Consumes: le générateur F1 terminé.
- Produces: mesure reproductible documentée et statut F1 livré.

- [ ] **Step 1: Ajouter la mesure synthétique séparée**

Créer un test `[Trait("Category", "Performance")]` qui construit 100 000 observations réparties en
10 000 familles de dix versions numériques, mesure `Stopwatch.Elapsed` et
`GC.GetTotalAllocatedBytes(precise: true)`, puis écrit les deux valeurs via `ITestOutputHelper`.

Les seules assertions fonctionnelles sont : 10 000 groupes, 90 000 versions antérieures et contrat
F1 correct. Ne pas fixer de seuil temporel ou mémoire dans ce premier relevé.

- [ ] **Step 2: Exécuter la mesure explicitement**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --filter Category=Performance --logger "console;verbosity=detailed"
```

Expected: test passant et relevé temps/mémoire visible. Reporter les valeurs observées dans la
section Performance de la spec avec la machine et la configuration `Release`.

- [ ] **Step 3: Exécuter toutes les suites**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj -c Release --filter "Category!=Performance"
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj -c Release
dotnet test apps/cli/tests/InstallChecker.Cli.Tests/InstallChecker.Cli.Tests.csproj -c Release
dotnet test tests/InstallChecker.Identity.Tests/InstallChecker.Identity.Tests.csproj -c Release
dotnet test modules/scanner/tests/InstallChecker.Scanner.Tests/InstallChecker.Scanner.Tests.csproj -c Release
```

Expected: toutes les suites passent. Les avertissements NU1900 liés à l'indisponibilité du service
de vulnérabilités NuGet sont acceptables s'ils restent les seuls avertissements externes.

- [ ] **Step 4: Vérifier les frontières et l'absence de mutation**

Run:

```powershell
git diff -- src/InstallChecker.Identity src/InstallChecker.Identity.Access tests/InstallChecker.Identity.Tests tests/oracle docs/identity docs/conformite registre
rg -n "File\.Delete|File\.Move|Directory\.Delete|SHFileOperation|IFileOperation|Recycle|Corbeille" modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine modules/duplicate-files/src/InstallChecker.DuplicateFiles
rg -n "InstallChecker\.Identity\.Access|Microsoft\.Data\.Sqlite" modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine
```

Expected: diff Identity vide ; aucune API de mutation ajoutée par F1 ; aucune dépendance Access ou
SQLite dans le moteur. Les occurrences historiques documentant l'interdiction d'une mutation sont
acceptables après inspection.

- [ ] **Step 5: Marquer F1 livré dans les deux documents**

Remplacer le statut de la spec par « implémentée et vérifiée », ajouter les nombres de tests et la
mesure réellement observés, puis remplacer `F1 conçu` par `F1 livré` dans la roadmap. Ne modifier
aucun autre jalon.

- [ ] **Step 6: Commit ciblé final**

```powershell
git add modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/MesureRedondanceVersionneeTests.cs modules/duplicate-files/docs/specs/2026-07-19-redondance-versionnee-design.md modules/duplicate-files/docs/specs/2026-07-19-module-doublon-roadmap-design.md
git commit -m "test(duplicate-files): verify version redundancy milestone"
```

---

## Completion Criteria

- `duplicates versions` fonctionne sur une base v1 ou v2 en lecture seule.
- Les noms de fichiers génériques, VersionInfo, MSI, MSIX/Appx, PE et Authenticode alimentent le
  même contrat de preuves.
- Les conflits de famille ou de version sont visibles et bloquants.
- Les formats, architectures, langues, éditions et distributions incompatibles ne sont pas
  comparés.
- Les contenus identiques ne deviennent jamais des redondances versionnées actionnables.
- Le rapport est déterministe, versionné et exploitable par une future UI.
- Aucun résultat F1 ne rejoint le plan de suppression.
- Le périmètre Identity scellé reste bit à bit inchangé.
- Tous les tests fonctionnels passent et la première mesure de charge est consignée.
