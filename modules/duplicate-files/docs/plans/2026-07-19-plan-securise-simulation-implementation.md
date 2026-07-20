# Plan sécurisé et simulation - Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produire un plan autosuffisant et ajouter une commande de vérification en lecture seule qui indique, avec un journal structuré, si ce plan serait exécutable dans l'état courant des fichiers.

**Architecture:** Le moteur Duplicate Files porte le contrat du plan, sa validation structurelle et la simulation pure derrière un port d'observation en lecture seule. L'enveloppe fournit l'adaptateur `FileStream`/SHA-256 et la commande JSON ; la CLI ne fait que router `plan verify`. Aucune interface de mutation ou implémentation de Corbeille n'est créée.

**Tech Stack:** C# .NET 10, `System.Security.Cryptography.SHA256`, `System.Text.Json`, xUnit.

**Statut :** terminé et vérifié le 2026-07-19 (289 tests réussis, 0 échec).

## Global Constraints

- Ne modifier aucun fichier du périmètre Identity scellé.
- Ne jamais appeler `File.Delete`, `File.Move`, une API Shell ou une API de Corbeille.
- Conserver `Propositions`, `Contenu`, `Chemin`, `GroupeId` et `FichierId` dans le JSON public.
- Le rang 1 est toujours le témoin de conservation et n'est jamais proposé.
- Tout chemin protégé reste absent des propositions.
- La vérification ne reçoit ni base SQLite ni registre.
- Le moteur métier ne dépend ni de SQLite, ni de Scanner, ni d'Identity.Access.
- Les fichiers sont lus séquentiellement et en flux ; aucun parallélisme sans benchmark.
- La Corbeille Windows reste uniquement documentée dans la spec et l'ADR de ce jalon.
- Ne pas créer de commit ou modifier l'index Git sans demande explicite : le worktree contient déjà d'autres changements.
- Toute implémentation suit RED -> GREEN -> REFACTOR.

---

### Task 1: Plan autosuffisant et témoin toujours conservé

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ContratUiDoublonsExacts.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/PlanDeSuppression.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ConstructeurDePlan.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ConstructeurDePlanTests.cs`

**Interfaces:**
- Produces: `VersionsContratDuplicateFiles.PlanSecuriseV1`.
- Produces: `TemoinDeConservation`, `GarantieDeGroupe`.
- Changes: `PlanDeSuppression` ajoute `VersionContrat` et `GarantiesParGroupe` après `Propositions`.
- Guarantees: `ConstructeurDePlan.Construire(...)` garde toujours `Chemins[0]` et tous les chemins protégés.

- [x] **Step 1: Écrire les tests en échec**

Compléter `ConstructeurDePlanTests` avec deux assertions de contrat et un cas où le chemin protégé
n'est pas le premier :

```csharp
[Fact]
public void Le_plan_expose_la_version_et_le_temoin_de_rang_un()
{
    var plan = ConstructeurDePlan.Construire(
        new[] { Groupe(HashA, @"C:\garde.exe", @"C:\copie.exe") },
        AucunProtege);

    Assert.Equal("duplicate-files/safe-plan/v1", plan.VersionContrat);
    var garantie = Assert.Single(plan.GarantiesParGroupe);
    Assert.Equal(IdentifiantsStables.PourGroupeExact(HashA), garantie.GroupeId);
    Assert.Equal(HashA, garantie.ContenuSha256);
    Assert.Equal(@"C:\garde.exe", garantie.TemoinConservation.Chemin);
    Assert.Equal(
        IdentifiantsStables.PourFichier(HashA, @"C:\garde.exe"),
        garantie.TemoinConservation.FichierId);
}

[Fact]
public void Un_protege_non_recommande_ne_permet_pas_de_proposer_le_rang_un()
{
    var proteges = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        @"C:\Windows\copie-protegee.exe",
    };
    var plan = ConstructeurDePlan.Construire(
        new[]
        {
            Groupe(
                HashA,
                @"D:\garde-recommandee.exe",
                @"C:\Windows\copie-protegee.exe",
                @"D:\copie.exe"),
        },
        proteges);

    Assert.Equal([@"D:\copie.exe"], plan.Propositions.Select(p => p.Chemin));
    Assert.Equal(@"D:\garde-recommandee.exe", Assert.Single(plan.GarantiesParGroupe).TemoinConservation.Chemin);
}
```

Ajouter aussi au test des groupes sans proposition :

```csharp
Assert.Empty(plan.GarantiesParGroupe);
```

- [x] **Step 2: Vérifier RED**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet test modules\duplicate-files\tests\InstallChecker.DuplicateFiles.Engine.Tests\InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore --filter ConstructeurDePlanTests
```

Expected: échec de compilation sur `VersionContrat`, `GarantiesParGroupe` et les nouveaux DTO.

- [x] **Step 3: Ajouter le contrat et corriger la sélection**

Dans `ContratUiDoublonsExacts.cs` :

```csharp
public const string PlanSecuriseV1 = "duplicate-files/safe-plan/v1";
```

Dans `PlanDeSuppression.cs` :

```csharp
public sealed record TemoinDeConservation(string FichierId, string Chemin);

public sealed record GarantieDeGroupe(
    string GroupeId,
    string ContenuSha256,
    TemoinDeConservation TemoinConservation);

public sealed record PlanDeSuppression(
    IReadOnlyList<PropositionDeSuppression> Propositions,
    string VersionContrat,
    IReadOnlyList<GarantieDeGroupe> GarantiesParGroupe);
```

Dans `ConstructeurDePlan.Construire`, initialiser les deux listes, garder toujours le premier chemin
et n'émettre une garantie que lorsque le groupe possède une proposition :

```csharp
var propositions = new List<PropositionDeSuppression>();
var garanties = new List<GarantieDeGroupe>();

foreach (var (contenu, chemins) in groupes)
{
    if (chemins.Count < 2)
        continue;

    var contenuSha256 = IdentifiantsStables.NormaliserSha256(contenu);
    var groupeId = IdentifiantsStables.PourGroupeExact(contenuSha256);
    var temoin = chemins[0];
    var aProposer = chemins.Skip(1).Where(c => !cheminProtege(c)).ToList();

    if (aProposer.Count == 0)
        continue;

    garanties.Add(new GarantieDeGroupe(
        groupeId,
        contenuSha256,
        new TemoinDeConservation(
            IdentifiantsStables.PourFichier(contenuSha256, temoin),
            temoin)));

    foreach (var chemin in aProposer)
    {
        propositions.Add(new PropositionDeSuppression(
            contenuSha256,
            chemin,
            groupeId,
            IdentifiantsStables.PourFichier(contenuSha256, chemin)));
    }
}

return new PlanDeSuppression(
    propositions,
    VersionsContratDuplicateFiles.PlanSecuriseV1,
    garanties);
```

- [x] **Step 4: Vérifier GREEN**

Run: la commande filtrée de l'étape 2.

Expected: tous les tests `ConstructeurDePlanTests` passent, y compris les garanties historiques de
protection et d'ordre.

---

### Task 2: Validation structurelle avant tout accès disque

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/IdentifiantsStables.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/PlanInvalideException.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ValidateurStructurePlan.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ValidateurStructurePlanTests.cs`

**Interfaces:**
- Produces: `IdentifiantsStables.NormaliserCheminWindows(string) : string`.
- Produces: `ValidateurStructurePlan.Valider(PlanDeSuppression) : void`.
- Throws: `PlanInvalideException` pour toute violation du contrat `safe-plan/v1`.

- [x] **Step 1: Écrire les tests en échec**

Créer `ValidateurStructurePlanTests.cs`. Utiliser un helper qui construit un plan valide avec
`ConstructeurDePlan`, puis créer des copies `with` pour vérifier au minimum :

```csharp
[Fact]
public void Un_plan_produit_par_le_constructeur_est_valide()
{
    ValidateurStructurePlan.Valider(PlanValide());
}

[Fact]
public void Une_version_inconnue_est_refusee()
{
    var plan = PlanValide() with { VersionContrat = "duplicate-files/safe-plan/v2" };

    Assert.Throws<PlanInvalideException>(() => ValidateurStructurePlan.Valider(plan));
}

[Fact]
public void Un_FichierId_falsifie_est_refuse()
{
    var plan = PlanValide();
    var proposition = Assert.Single(plan.Propositions) with
    {
        FichierId = "file:sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
    };
    plan = plan with { Propositions = [proposition] };

    Assert.Throws<PlanInvalideException>(() => ValidateurStructurePlan.Valider(plan));
}

[Fact]
public void Le_temoin_ne_peut_pas_etre_une_proposition_sous_une_autre_casse()
{
    var plan = PlanValide();
    var garantie = Assert.Single(plan.GarantiesParGroupe);
    var proposition = Assert.Single(plan.Propositions) with
    {
        Chemin = garantie.TemoinConservation.Chemin.ToUpperInvariant(),
        FichierId = garantie.TemoinConservation.FichierId,
    };
    plan = plan with { Propositions = [proposition] };

    Assert.Throws<PlanInvalideException>(() => ValidateurStructurePlan.Valider(plan));
}
```

Ajouter des tests pour une garantie orpheline, une proposition sans garantie, un hash non normalisé,
un `GroupeId` falsifié, deux chemins équivalents avec `/` et `\`, et deux `FichierId` identiques.

Définir dans cette classe le helper utilisé par tous les cas :

```csharp
private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

private static PlanDeSuppression PlanValide() => ConstructeurDePlan.Construire(
    new[]
    {
        (HashA, (IReadOnlyList<string>)[@"C:\garde.exe", @"C:\copie.exe"]),
    },
    _ => false);
```

- [x] **Step 2: Vérifier RED**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet test modules\duplicate-files\tests\InstallChecker.DuplicateFiles.Engine.Tests\InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore --filter ValidateurStructurePlanTests
```

Expected: échec de compilation, validateur et exception absents.

- [x] **Step 3: Centraliser la normalisation de chemin**

Dans `IdentifiantsStables.cs`, extraire la règle déjà utilisée par `PourFichier` :

```csharp
public static string NormaliserCheminWindows(string chemin)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(chemin);
    return chemin.Replace('/', '\\').ToUpperInvariant();
}
```

Puis remplacer la normalisation locale de `PourFichier` par cet appel.

- [x] **Step 4: Implémenter l'exception et le validateur**

Créer l'exception :

```csharp
namespace InstallChecker.DuplicateFiles;

public sealed class PlanInvalideException(string message) : Exception(message);
```

Le validateur doit appliquer les contrôles dans cet ordre : version, collections non nulles,
garanties, propositions, puis couverture des garanties. Utiliser des dictionnaires ordinaux pour
les identifiants et un `HashSet<string>(StringComparer.Ordinal)` alimenté par
`NormaliserCheminWindows` pour les chemins.

Implémenter les helpers privés suivants afin que chaque règle reste lisible et testable par son
effet public :

```csharp
private static string ExigerSha256Normalise(string valeur)
{
    try
    {
        var normalise = IdentifiantsStables.NormaliserSha256(valeur);
        if (!StringComparer.Ordinal.Equals(valeur, normalise))
            throw new PlanInvalideException("empreinte SHA-256 non normalisee");
        return normalise;
    }
    catch (ArgumentException ex)
    {
        throw new PlanInvalideException($"empreinte SHA-256 invalide: {ex.ParamName}");
    }
}

private static void Exiger(bool condition, string message)
{
    if (!condition)
        throw new PlanInvalideException(message);
}
```

Pour chaque garantie, vérifier `GroupeId`, hash, témoin et unicité. Pour chaque proposition,
retrouver la garantie, comparer `Contenu`, recalculer ses identifiants et enregistrer son groupe dans
un `HashSet<string> groupesAvecProposition`. À la fin, exiger que chaque garantie soit dans ce set.
Ne jamais appeler une API du système de fichiers dans ce composant.

- [x] **Step 5: Vérifier GREEN**

Run: la commande filtrée de l'étape 2.

Expected: tous les tests structurels passent.

---

### Task 3: Simulation pure et journal déterministe

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ContratUiDoublonsExacts.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ContratVerificationPlan.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ValidateurDePlan.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ValidateurDePlanTests.cs`

**Interfaces:**
- Produces: `VersionsContratDuplicateFiles.VerificationPlanV1`.
- Produces: port `IObservateurDeFichier.Observer(string) : ObservationFichierCourant`.
- Produces: `ValidateurDePlan.Verifier(PlanDeSuppression, IObservateurDeFichier, Func<string,bool>) : RapportDeVerificationPlan`.

- [x] **Step 1: Écrire les tests en échec**

Créer un faux observateur fondé sur un dictionnaire et un compteur d'appels. Tester séparément :

```csharp
[Fact]
public void Un_plan_valide_dont_tous_les_hashs_correspondent_est_executable()
{
    var plan = PlanValide();
    var observateur = ObservateurDisponiblePourTous(plan, HashA);

    var rapport = ValidateurDePlan.Verifier(plan, observateur, _ => false);

    Assert.Equal("duplicate-files/safe-plan-verification/v1", rapport.VersionContrat);
    Assert.Equal(ModeVerificationPlan.Simulation, rapport.Mode);
    Assert.True(rapport.Executable);
    Assert.All(Assert.Single(rapport.Groupes).Fichiers, f => Assert.Equal(EtatVerificationFichier.Valide, f.Etat));
    Assert.Equal([1, 2], rapport.Journal.Select(e => e.Sequence));
    Assert.Equal(
        [EtapeJournalVerificationPlan.VerifierTemoin, EtapeJournalVerificationPlan.VerifierCandidat],
        rapport.Journal.Select(e => e.Etape));
}

[Fact]
public void Un_hash_different_bloque_le_groupe()
{
    var plan = PlanValide();
    var candidat = Assert.Single(plan.Propositions);
    var observateur = ObservateurDisponiblePourTous(plan, HashA);
    observateur.Observations[candidat.Chemin] =
        new ObservationFichierCourant(EtatLectureFichier.Disponible, HashB, null);

    var rapport = ValidateurDePlan.Verifier(plan, observateur, _ => false);

    Assert.False(rapport.Executable);
    Assert.Equal(
        EtatVerificationFichier.HashDifferent,
        Assert.Single(rapport.Groupes).Fichiers.Single(f => f.FichierId == candidat.FichierId).Etat);
}

[Fact]
public void Un_candidat_devenu_protege_est_bloque_sans_etre_lu()
{
    var plan = PlanValide();
    var candidat = Assert.Single(plan.Propositions);
    var observateur = ObservateurDisponiblePourTous(plan, HashA);

    var rapport = ValidateurDePlan.Verifier(
        plan,
        observateur,
        chemin => chemin == candidat.Chemin);

    Assert.False(rapport.Executable);
    Assert.Equal(EtatVerificationFichier.CheminProtege, Assert.Single(rapport.Groupes).Fichiers[1].Etat);
    Assert.DoesNotContain(candidat.Chemin, observateur.CheminsLus);
}
```

Ajouter des théories qui projettent `Absent`, `Illisible` et `TypeNonPrisEnCharge`, ainsi qu'un test
prouvant qu'un plan structurellement invalide lève avant le premier appel à l'observateur.

Définir les fixtures sans accès au disque :

```csharp
private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
private const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

private sealed class ObservateurDeTest : IObservateurDeFichier
{
    public Dictionary<string, ObservationFichierCourant> Observations { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public List<string> CheminsLus { get; } = [];

    public ObservationFichierCourant Observer(string chemin)
    {
        CheminsLus.Add(chemin);
        return Observations[chemin];
    }
}

private static PlanDeSuppression PlanValide() => ConstructeurDePlan.Construire(
    new[]
    {
        (HashA, (IReadOnlyList<string>)[@"C:\garde.exe", @"C:\copie.exe"]),
    },
    _ => false);

private static ObservateurDeTest ObservateurDisponiblePourTous(
    PlanDeSuppression plan,
    string hash)
{
    var observateur = new ObservateurDeTest();
    foreach (var garantie in plan.GarantiesParGroupe)
    {
        observateur.Observations[garantie.TemoinConservation.Chemin] =
            new ObservationFichierCourant(EtatLectureFichier.Disponible, hash, null);
    }

    foreach (var proposition in plan.Propositions)
    {
        observateur.Observations[proposition.Chemin] =
            new ObservationFichierCourant(EtatLectureFichier.Disponible, hash, null);
    }

    return observateur;
}
```

- [x] **Step 2: Vérifier RED**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet test modules\duplicate-files\tests\InstallChecker.DuplicateFiles.Engine.Tests\InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore --filter ValidateurDePlanTests
```

Expected: échec de compilation sur les DTO, le port et le validateur.

- [x] **Step 3: Ajouter le contrat de vérification**

Ajouter la constante :

```csharp
public const string VerificationPlanV1 = "duplicate-files/safe-plan-verification/v1";
```

Créer dans `ContratVerificationPlan.cs` :

```csharp
namespace InstallChecker.DuplicateFiles;

public enum ModeVerificationPlan { Simulation }
public enum EtatLectureFichier { Disponible, Absent, Illisible, TypeNonPrisEnCharge }
public enum EtatVerificationFichier { Valide, Absent, Illisible, HashDifferent, CheminProtege, TypeNonPrisEnCharge }
public enum RoleFichierPlan { TemoinConservation, Candidat }
public enum EtapeJournalVerificationPlan { VerifierTemoin, VerifierCandidat }

public sealed record ObservationFichierCourant(
    EtatLectureFichier Etat,
    string? HashObserve,
    string? Detail);

public interface IObservateurDeFichier
{
    ObservationFichierCourant Observer(string chemin);
}

public sealed record VerificationFichierPlan(
    string GroupeId,
    string FichierId,
    string Chemin,
    string HashAttendu,
    string? HashObserve,
    RoleFichierPlan Role,
    EtatVerificationFichier Etat,
    string? Detail);

public sealed record VerificationGroupePlan(
    string GroupeId,
    bool Executable,
    IReadOnlyList<EtatVerificationFichier> Blocages,
    IReadOnlyList<VerificationFichierPlan> Fichiers);

public sealed record EntreeJournalVerificationPlan(
    int Sequence,
    string GroupeId,
    string FichierId,
    EtapeJournalVerificationPlan Etape,
    EtatVerificationFichier Etat);

public sealed record RapportDeVerificationPlan(
    string VersionContrat,
    ModeVerificationPlan Mode,
    bool Executable,
    IReadOnlyList<VerificationGroupePlan> Groupes,
    IReadOnlyList<EntreeJournalVerificationPlan> Journal);
```

- [x] **Step 4: Implémenter le validateur pur**

`ValidateurDePlan.Verifier` commence obligatoirement par
`ValidateurStructurePlan.Valider(plan)`. Pour chaque garantie, vérifier le témoin en premier, puis
les propositions du même groupe dans leur ordre d'entrée. Un candidat protégé produit directement
`CheminProtege` sans appel au port.

Projeter les lectures avec cette table exacte :

```csharp
private static EtatVerificationFichier DeriverEtat(
    ObservationFichierCourant observation,
    string hashAttendu) => observation.Etat switch
{
    EtatLectureFichier.Absent => EtatVerificationFichier.Absent,
    EtatLectureFichier.Illisible => EtatVerificationFichier.Illisible,
    EtatLectureFichier.TypeNonPrisEnCharge => EtatVerificationFichier.TypeNonPrisEnCharge,
    EtatLectureFichier.Disponible when
        StringComparer.Ordinal.Equals(observation.HashObserve, hashAttendu) => EtatVerificationFichier.Valide,
    EtatLectureFichier.Disponible => EtatVerificationFichier.HashDifferent,
    _ => throw new InvalidOperationException("etat de lecture inconnu"),
};
```

Les `Blocages` d'un groupe sont les états non `Valide`, distincts dans leur ordre d'apparition. Le
journal reçoit une séquence continue à partir de 1. Le rapport global est exécutable si et seulement
si tous les groupes le sont ; un plan vide est un no-op valide et exécutable avec un journal vide.

- [x] **Step 5: Vérifier GREEN**

Run: la commande filtrée de l'étape 2.

Expected: tous les tests du validateur passent et le faux observateur confirme l'absence d'accès
disque pour un plan invalide ou un chemin protégé.

---

### Task 4: Adaptateur local strictement en lecture seule

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/ObservateurDeFichierLocal.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/ObservateurDeFichierLocalTests.cs`

**Interfaces:**
- Implements: `IObservateurDeFichier`.
- Produces: SHA-256 minuscule calculé en flux pour un fichier ordinaire lisible.
- Guarantees: aucune API de mutation et aucune exception locale de fichier propagée.

- [x] **Step 1: Écrire les tests en échec**

Avec un répertoire temporaire propre à chaque test, vérifier :

```csharp
[Fact]
public void Un_fichier_ordinaire_est_lu_sans_etre_modifie()
{
    var chemin = Path.Combine(_root, "fichier.bin");
    File.WriteAllText(chemin, "contenu");
    var avant = File.ReadAllBytes(chemin);

    var observation = new ObservateurDeFichierLocal().Observer(chemin);

    Assert.Equal(EtatLectureFichier.Disponible, observation.Etat);
    Assert.Equal(Convert.ToHexString(SHA256.HashData(avant)).ToLowerInvariant(), observation.HashObserve);
    Assert.Equal(avant, File.ReadAllBytes(chemin));
}

[Fact]
public void Un_chemin_absent_est_un_resultat_local()
{
    var observation = new ObservateurDeFichierLocal().Observer(Path.Combine(_root, "absent.bin"));

    Assert.Equal(EtatLectureFichier.Absent, observation.Etat);
    Assert.Null(observation.HashObserve);
}

[Fact]
public void Un_repertoire_nest_pas_un_fichier_pris_en_charge()
{
    var observation = new ObservateurDeFichierLocal().Observer(_root);

    Assert.Equal(EtatLectureFichier.TypeNonPrisEnCharge, observation.Etat);
    Assert.Null(observation.HashObserve);
}
```

Ne pas créer de lien symbolique dans les tests : sa création dépend des privilèges Windows. La
branche `FileAttributes.ReparsePoint` reste testée indirectement dans le validateur pur par
`TypeNonPrisEnCharge`.

- [x] **Step 2: Vérifier RED**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet test modules\duplicate-files\tests\InstallChecker.DuplicateFiles.Tests\InstallChecker.DuplicateFiles.Tests.csproj --no-restore --filter ObservateurDeFichierLocalTests
```

Expected: échec de compilation, adaptateur absent.

- [x] **Step 3: Implémenter l'observateur local**

Créer un adaptateur qui appelle d'abord `File.GetAttributes`, refuse `Directory` et `ReparsePoint`,
puis ouvre le fichier avec :

```csharp
using var flux = new FileStream(
    chemin,
    FileMode.Open,
    FileAccess.Read,
    FileShare.Read,
    bufferSize: 128 * 1024,
    FileOptions.SequentialScan);
var hash = Convert.ToHexString(SHA256.HashData(flux)).ToLowerInvariant();
return new ObservationFichierCourant(EtatLectureFichier.Disponible, hash, null);
```

Mapper `FileNotFoundException` et `DirectoryNotFoundException` vers `Absent` ;
`UnauthorizedAccessException`, `IOException` et `System.Security.SecurityException` vers
`Illisible` ; `ArgumentException` et `NotSupportedException` vers `TypeNonPrisEnCharge`. Utiliser
des détails constants du module (`"fichier absent"`, `"fichier illisible"`,
`"type de chemin non pris en charge"`) et ne jamais exposer `Exception.Message`.

- [x] **Step 4: Vérifier GREEN et l'absence d'API destructive**

Run: la commande filtrée de l'étape 2.

Run:

```powershell
rg -n "File\.(Delete|Move)|SHFileOperation|IFileOperation|RecycleBin|Corbeille" modules\duplicate-files\src --glob "*.cs"
```

Expected: tests verts et aucune occurrence de mutation.

---

### Task 5: Commande JSON `plan verify`

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/PlanVerificationCommand.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/PlanCommandTests.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/PlanVerificationCommandTests.cs`

**Interfaces:**
- Produces: `PlanVerificationCommand.Verifier(string cheminPlan, TextWriter output, TextWriter errors) : int`.
- Exit codes: `0` exécutable, `1` entrée/contrat invalide, `3` simulation bloquée.
- Serializes: tous les enums sous forme de chaînes.

- [x] **Step 1: Étendre le test de génération du plan**

Dans `PlanCommandTests`, conserver toutes les assertions existantes et ajouter :

```csharp
Assert.Equal("duplicate-files/safe-plan/v1", plan.RootElement.GetProperty("VersionContrat").GetString());
var garantie = Assert.Single(plan.RootElement.GetProperty("GarantiesParGroupe").EnumerateArray());
Assert.Equal(groupe.GetProperty("GroupeId").GetString(), garantie.GetProperty("GroupeId").GetString());
Assert.Equal(
    cheminAConserver,
    garantie.GetProperty("TemoinConservation").GetProperty("Chemin").GetString());
```

- [x] **Step 2: Écrire les tests de commande en échec**

Créer des fichiers réels, calculer leur hash, construire le plan avec `ConstructeurDePlan`, puis le
sérialiser dans le répertoire temporaire. Couvrir :

```csharp
[Fact]
public void Un_plan_valide_produit_un_rapport_executable_et_le_code_zero()
{
    var cheminPlan = EcrirePlanValide();
    var output = new StringWriter();
    var errors = new StringWriter();

    var code = PlanVerificationCommand.Verifier(cheminPlan, output, errors);

    Assert.Equal(0, code);
    Assert.Equal(string.Empty, errors.ToString());
    using var json = JsonDocument.Parse(output.ToString());
    Assert.Equal("duplicate-files/safe-plan-verification/v1", json.RootElement.GetProperty("VersionContrat").GetString());
    Assert.Equal("Simulation", json.RootElement.GetProperty("Mode").GetString());
    Assert.True(json.RootElement.GetProperty("Executable").GetBoolean());
}

[Fact]
public void Un_candidat_modifie_produit_un_rapport_bloque_et_le_code_trois()
{
    var (cheminPlan, candidat) = EcrirePlanValideAvecChemins();
    File.WriteAllText(candidat, "contenu modifie");
    var output = new StringWriter();

    var code = PlanVerificationCommand.Verifier(cheminPlan, output, new StringWriter());

    Assert.Equal(3, code);
    using var json = JsonDocument.Parse(output.ToString());
    Assert.False(json.RootElement.GetProperty("Executable").GetBoolean());
    Assert.Contains(
        json.RootElement.GetProperty("Journal").EnumerateArray(),
        e => e.GetProperty("Etat").GetString() == "HashDifferent");
}
```

Ajouter les cas plan absent, JSON malformé et identifiant falsifié : code `1`, stderr non vide et
stdout vide. Après chaque simulation réussie ou bloquée, vérifier que témoin et candidat existent
toujours et que leurs octets n'ont pas changé du fait de la commande.

La classe de test implémente `IDisposable` et définit ces helpers complets :

```csharp
private static readonly JsonSerializerOptions OptionsJson = new()
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() },
};

private readonly string _root = Directory.CreateDirectory(
    Path.Combine(Path.GetTempPath(), "plan-verification-tests-" + Guid.NewGuid())).FullName;

public void Dispose() => Directory.Delete(_root, recursive: true);

private string EcrirePlanValide() => EcrirePlanValideAvecChemins().CheminPlan;

private (string CheminPlan, string Candidat) EcrirePlanValideAvecChemins()
{
    var temoin = Path.Combine(_root, "garde.bin");
    var candidat = Path.Combine(_root, "copie.bin");
    File.WriteAllText(temoin, "contenu identique");
    File.WriteAllText(candidat, "contenu identique");

    var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(temoin))).ToLowerInvariant();
    var plan = ConstructeurDePlan.Construire(
        new[] { (hash, (IReadOnlyList<string>)[temoin, candidat]) },
        _ => false);
    var cheminPlan = Path.Combine(_root, "plan.json");
    File.WriteAllText(cheminPlan, JsonSerializer.Serialize(plan, OptionsJson));
    return (cheminPlan, candidat);
}
```

Ajouter en tête du fichier `using System.Security.Cryptography;`, `using System.Text.Json;` et
`using System.Text.Json.Serialization;`.

- [x] **Step 3: Vérifier RED**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet test modules\duplicate-files\tests\InstallChecker.DuplicateFiles.Tests\InstallChecker.DuplicateFiles.Tests.csproj --no-restore --filter "PlanCommandTests|PlanVerificationCommandTests"
```

Expected: le test de génération échoue sur les nouveaux champs ou la compilation échoue sur la
commande absente.

- [x] **Step 4: Implémenter la commande**

Définir deux options JSON privées : lecture stricte et écriture indentée avec
`JsonStringEnumConverter`. La méthode suit cet ordre : lire tout le fichier, désérialiser, appeler
`ValidateurDePlan.Verifier` avec `ObservateurDeFichierLocal` et
`ProtectionDesChemins.EstProtegeParDefaut`, sérialiser seulement lorsque le rapport complet existe,
puis retourner `0` ou `3`.

Capturer uniquement :

```csharp
catch (FileNotFoundException)
{
    errors.WriteLine("plan introuvable");
    return 1;
}
catch (UnauthorizedAccessException)
{
    errors.WriteLine("plan illisible");
    return 1;
}
catch (IOException)
{
    errors.WriteLine("plan illisible");
    return 1;
}
catch (JsonException)
{
    errors.WriteLine("plan JSON invalide");
    return 1;
}
catch (PlanInvalideException ex)
{
    errors.WriteLine(ex.Message);
    return 1;
}
```

Une désérialisation `null` devient explicitement `new PlanInvalideException("plan JSON vide")`.
Ne pas capturer les autres exceptions d'implémentation.

- [x] **Step 5: Vérifier GREEN**

Run: la commande filtrée de l'étape 3.

Expected: tous les tests de génération et de vérification passent.

---

### Task 6: Routage CLI et documentation utilisateur

**Files:**
- Modify: `apps/cli/InstallChecker.Cli/Program.cs`
- Modify: `modules/duplicate-files/README.md`
- Modify: `modules/duplicate-files/docs/specs/2026-07-19-module-doublon-roadmap-design.md`
- Modify: `modules/duplicate-files/docs/specs/2026-07-19-plan-securise-simulation-design.md`

**Interfaces:**
- Produces: `installchecker plan verify <plan.json>`.
- Keeps: `installchecker plan <base.db> <registre>` inchangé.
- Documents: code de sortie `3` et frontière future Corbeille.

- [x] **Step 1: Ajouter le routage minimal**

Insérer avant la route historique `plan` :

```csharp
if (args is ["plan", "verify", var cheminPlanAVerifier])
    return PlanVerificationCommand.Verifier(cheminPlanAVerifier, Console.Out, Console.Error);
```

Ajouter à `Usage()` :

```csharp
Console.Error.WriteLine("        installchecker plan verify <plan.json>");
```

- [x] **Step 2: Documenter le flux complet**

Mettre à jour le README avec :

```text
scan -> base Omega -> plan -> plan.json -> plan verify -> rapport de simulation
```

Documenter les deux versions de contrat, `GarantiesParGroupe`, les états de vérification, les codes
`0/1/2/3` et un exemple minimal de sortie. Écrire explicitement :

```text
Une simulation réussie ne modifie aucun fichier et ne constitue pas une autorisation différée.
Le hash devra être revérifié immédiatement avant une future mise à la Corbeille.
```

Dans la roadmap, marquer E1 « plan autosuffisant et simulation » comme livré et conserver la mise à
la Corbeille réelle dans un futur E2. Dans la spec E1, passer le statut à « implémentée et vérifiée »
uniquement après la Task 7.

- [x] **Step 3: Vérifier la compilation de la CLI**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet build apps\cli\InstallChecker.Cli\InstallChecker.Cli.csproj --no-restore
```

Expected: build réussi ; seuls les avertissements `NU1900` liés à l'accès au service NuGet sont
tolérés.

---

### Task 7: Vérification finale et frontières

**Files:**
- Verify: tous les fichiers ci-dessus.

**Interfaces:**
- Produces: preuve du jalon E1 sans mutation et sans modification d'Identity.

- [x] **Step 1: Exécuter tous les tests du moteur métier**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet test modules\duplicate-files\tests\InstallChecker.DuplicateFiles.Engine.Tests\InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore
```

Expected: 0 échec.

- [x] **Step 2: Exécuter les tests d'enveloppe du module**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet test modules\duplicate-files\tests\InstallChecker.DuplicateFiles.Tests\InstallChecker.DuplicateFiles.Tests.csproj --no-restore
```

Expected: 0 échec.

- [x] **Step 3: Exécuter toute la solution séquentiellement**

Run:

```powershell
$env:NUGET_PACKAGES='C:\Users\thibs\.nuget\packages'
dotnet test InstallChecker.slnx --no-restore --maxcpucount:1
```

Expected: tous les tests historiques et nouveaux passent.

- [x] **Step 4: Vérifier l'absence de mutation et le scellement**

Run:

```powershell
rg -n "File\.(Delete|Move)|SHFileOperation|IFileOperation|RecycleBin|Corbeille" modules\duplicate-files\src --glob "*.cs"
```

Expected: aucune occurrence.

Run:

```powershell
git diff --name-only -- src/InstallChecker.Identity src/InstallChecker.Identity.Access tests/InstallChecker.Identity.Tests tests/oracle docs/identity docs/conformite registre
```

Expected: aucune sortie.

Run:

```powershell
rg -n "Identity.Access|Microsoft.Data.Sqlite|InstallChecker.Scanner" modules\duplicate-files\src\InstallChecker.DuplicateFiles.Engine --glob "*.cs" --glob "*.csproj"
```

Expected: aucune dépendance de code ou de projet ; le commentaire de frontière du `.csproj` peut
nommer `Identity.Access` et SQLite.

- [x] **Step 5: Fermer les documents après les preuves**

Passer la spec E1 à « implémentée et vérifiée », cocher ce plan et consigner le nombre exact de tests
réussis. Ne modifier la roadmap qu'avec l'état réellement vérifié.
