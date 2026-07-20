# Contrat UI des doublons exacts - Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Enrichir le rapport et le plan de doublons exacts avec un contrat JSON versionné, des identifiants stables, des preuves, des rôles et des actions structurées.

**Architecture:** Le moteur Duplicate Files calcule et vérifie toutes les nouvelles données à partir de W et du même snapshot Omega. Les DTO existants sont enrichis sans retrait de champ ; les commandes de l'enveloppe ne font que sérialiser les enums en chaînes. Identity et Identity.Access restent inchangés.

**Tech Stack:** C# .NET 10, `System.Security.Cryptography.SHA256`, `System.Text.Json`, xUnit.

**Statut :** terminé et vérifié le 2026-07-19 (261 tests réussis, 0 échec).

## Global Constraints

- Ne modifier aucun fichier du périmètre Identity scellé.
- Conserver tous les champs JSON existants.
- Version du contrat : `duplicate-files/exact-duplicates/v1`.
- Aucune action sur le système de fichiers.
- Les versions produit et fichier restent hors périmètre.
- Toute implémentation suit RED -> GREEN -> REFACTOR.

---

### Task 1: Identifiants stables

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/IdentifiantsStables.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/IdentifiantsStablesTests.cs`

**Interfaces:**
- Produces: `IdentifiantsStables.NormaliserSha256(string) : string`
- Produces: `IdentifiantsStables.PourGroupeExact(string) : string`
- Produces: `IdentifiantsStables.PourFichier(string, string) : string`

- [x] **Step 1: Écrire les tests en échec**

Tester les valeurs exactes suivantes :

```csharp
const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

Assert.Equal($"exact:sha256:{HashA}", IdentifiantsStables.PourGroupeExact(HashA.ToUpperInvariant()));
Assert.Equal(
    IdentifiantsStables.PourFichier(HashA, @"C:\Archives\Setup.exe"),
    IdentifiantsStables.PourFichier(HashA, @"c:/archives/setup.exe"));
Assert.NotEqual(
    IdentifiantsStables.PourFichier(HashA, @"C:\Archives\Setup.exe"),
    IdentifiantsStables.PourFichier(HashA, @"D:\Archives\Setup.exe"));
Assert.Throws<ArgumentException>(() => IdentifiantsStables.PourGroupeExact("pas-un-sha256"));
```

- [x] **Step 2: Vérifier RED**

Run: `dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore --filter IdentifiantsStablesTests`

Expected: échec de compilation, type `IdentifiantsStables` absent.

- [x] **Step 3: Implémenter le calcul minimal**

Créer une classe statique qui valide 32 octets hexadécimaux avec `Convert.TryFromHexString`, normalise le hash en minuscules, normalise le chemin avec `Replace('/', '\\').ToUpperInvariant()`, puis calcule :

```csharp
var charge = Encoding.UTF8.GetBytes($"{groupeId}\n{cheminCanonique}");
var empreinte = Convert.ToHexString(SHA256.HashData(charge)).ToLowerInvariant();
return $"file:sha256:{empreinte}";
```

- [x] **Step 4: Vérifier GREEN**

Run: le test filtré de l'étape 2.

Expected: tous les tests `IdentifiantsStablesTests` passent.

### Task 2: Types structurés et explication de la rétention

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ContratUiDoublonsExacts.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/PolitiqueRetentionV1.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/PolitiqueRetentionV1Tests.cs`

**Interfaces:**
- Produces: enums `CategorieDoublon`, `NiveauConfiance`, `TypePreuveDoublon`, `RoleExemplaire`, `CritereRetention`, `ActionFichier`, `RaisonBlocageAction`.
- Produces: records `PreuveDoublon`, `CritereClassement`, `EtatActionFichier`.
- Produces: `PolitiqueRetentionV1.Expliquer(FichierEnrichi) : IReadOnlyList<CritereClassement>`.

- [x] **Step 1: Écrire le test d'explication en échec**

```csharp
var fichier = Fichier(7, @"C:\a\setup (1).exe", authenticode: true);
var criteres = PolitiqueRetentionV1.Expliquer(fichier);

Assert.Equal(
    [CritereRetention.RichesseObservations, CritereRetention.NomDeCopie,
     CritereRetention.DateObservation, CritereRetention.Chemin, CritereRetention.ActeIdDepartage],
    criteres.Select(c => c.Critere));
Assert.Equal([1, 2, 3, 4, 5], criteres.Select(c => c.Priorite));
Assert.Equal("1/3", criteres[0].Valeur);
Assert.Equal("True", criteres[1].Valeur);
```

- [x] **Step 2: Vérifier RED**

Run: `dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore --filter PolitiqueRetentionV1Tests`

Expected: échec de compilation, `Expliquer` et les types de contrat sont absents.

- [x] **Step 3: Ajouter les types et l'explication**

Définir les enums avec exactement les valeurs de la spec. `Expliquer` retourne cinq
`CritereClassement(CritereRetention Critere, int Priorite, string Valeur)` dans l'ordre de la
politique v1. Réutiliser `RichesseDesObservations` et `EstNomDeCopie` ; ne pas dupliquer leur logique.

- [x] **Step 4: Vérifier GREEN**

Run: la suite filtrée de l'étape 2.

Expected: tous les tests de politique passent.

### Task 3: Rapport versionné, preuve et stabilité

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/RapportDeDoublons.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/GroupeClasse.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ExemplaireRapporte.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/GenerateurDeRapport.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/GenerateurDeRapportTests.cs`

**Interfaces:**
- `RapportDeDoublons` ajoute `string VersionContrat`.
- `GroupeClasse` ajoute `GroupeId`, `Categorie`, `Confiance`, `ContenuSha256`, `Preuves`, `FichierRecommandeId`.
- `ExemplaireRapporte` ajoute `FichierId`, `Role`, `CriteresClassement`, `Actions`.
- `GenerateurDeRapport.Generer` ajoute le paramètre optionnel `Func<string, bool>? cheminProtege = null`.

- [x] **Step 1: Remplacer les empreintes de test abrégées**

Dans `GenerateurDeRapportTests`, utiliser deux constantes de 64 caractères hexadécimaux et faire
porter `HashA` aux deux actes du groupe valide.

- [x] **Step 2: Écrire les tests du contrat en échec**

Ajouter des tests vérifiant :

```csharp
Assert.Equal("duplicate-files/exact-duplicates/v1", rapport.VersionContrat);
Assert.Equal($"exact:sha256:{HashA}", groupe.GroupeId);
Assert.Equal(CategorieDoublon.ExactDuplicate, groupe.Categorie);
Assert.Equal(NiveauConfiance.Certaine, groupe.Confiance);
Assert.Equal(new PreuveDoublon(TypePreuveDoublon.Sha256Identique, HashA), Assert.Single(groupe.Preuves));
Assert.Equal(groupe.Exemplaires[0].FichierId, groupe.FichierRecommandeId);
Assert.Equal(RoleExemplaire.RecommandeAConserver, groupe.Exemplaires[0].Role);
Assert.Equal(RoleExemplaire.Candidat, groupe.Exemplaires[1].Role);
```

Ajouter un second rapport avec les mêmes chemins et hashes mais des `ActeId` différents, puis
comparer `GroupeId` et les `FichierId`. Ajouter un groupe dont les deux actes portent `HashA` et
`HashB`, attendu `InvalidOperationException`.

- [x] **Step 3: Vérifier RED**

Run: `dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore --filter GenerateurDeRapportTests`

Expected: échec de compilation sur les nouveaux champs.

- [x] **Step 4: Implémenter l'enrichissement du rapport**

Dans le générateur, construire un dictionnaire d'actes Omega, valider explicitement chaque membre
du domaine, normaliser les hashes, exiger une seule valeur distincte, puis calculer les identifiants.
Assembler les nouveaux DTO sans modifier les calculs historiques de synthèse, classement ou espace.

- [x] **Step 5: Vérifier GREEN**

Run: la suite filtrée de l'étape 3.

Expected: tous les tests du générateur passent.

### Task 4: Actions autorisées et blocages

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/GenerateurDeRapport.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/GenerateurDeRapportTests.cs`

**Interfaces:**
- Consumes: `ProtectionDesChemins.EstProtegeParDefaut` ou le prédicat injecté.
- Produces: deux `EtatActionFichier` par exemplaire.

- [x] **Step 1: Écrire les tests d'action en échec**

Vérifier que `Conserver` est toujours autorisée, que le rang 1 bloque
`AjouterAuPlanDeSuppression` avec `FichierRecommandeAConserver`, qu'un candidat non protégé
l'autorise, et qu'un candidat `C:\Windows\...` la bloque avec `CheminProtege`.

- [x] **Step 2: Vérifier RED**

Run: la suite `GenerateurDeRapportTests` filtrée.

Expected: les assertions échouent car les actions ne sont pas encore assemblées.

- [x] **Step 3: Implémenter les actions**

Créer exactement deux états par exemplaire. Accumuler les raisons de blocage du plan, puis définir
`Autorisee` à `blocages.Count == 0`. Ne jamais appeler `File.Exists`, `File.Delete` ou une API de
mutation du système de fichiers.

- [x] **Step 4: Vérifier GREEN**

Run: la suite filtrée de l'étape 2.

Expected: tous les tests passent.

### Task 5: Identifiants du plan et cohérence croisée

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/PlanDeSuppression.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/ConstructeurDePlan.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/ConstructeurDePlanTests.cs`

**Interfaces:**
- `PropositionDeSuppression` ajoute `GroupeId` et `FichierId`.
- Consumes: `IdentifiantsStables` partagé avec le rapport.

- [x] **Step 1: Normaliser les hashes des fixtures du plan**

Remplacer `h`, `h1`, `h2` et `empreinte-xyz` par des constantes SHA-256 valides afin que les tests
exercent le contrat réel.

- [x] **Step 2: Écrire le test de cohérence en échec**

```csharp
var proposition = Assert.Single(plan.Propositions);
Assert.Equal(IdentifiantsStables.PourGroupeExact(HashA), proposition.GroupeId);
Assert.Equal(IdentifiantsStables.PourFichier(HashA, proposition.Chemin), proposition.FichierId);
```

- [x] **Step 3: Vérifier RED**

Run: `dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore --filter ConstructeurDePlanTests`

Expected: échec de compilation sur les deux nouveaux champs.

- [x] **Step 4: Implémenter les identifiants du plan**

Normaliser le contenu une fois par groupe, calculer `GroupeId`, puis `FichierId` pour chaque
proposition. Ne modifier ni l'ordre ni les garanties de conservation/protection.

- [x] **Step 5: Vérifier GREEN**

Run: la suite filtrée de l'étape 3.

Expected: tous les tests du constructeur passent.

### Task 6: Sérialisation JSON et documentation

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/DuplicatesCommand.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/PlanCommand.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/DuplicatesCommandTests.cs`
- Modify: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/PlanCommandTests.cs`
- Modify: `modules/duplicate-files/README.md`

**Interfaces:**
- Produces: enums JSON sous forme de chaînes dans les deux commandes.

- [x] **Step 1: Écrire les assertions JSON en échec**

Dans le test `duplicates`, vérifier `VersionContrat`, `GroupeId`, `Categorie == "ExactDuplicate"`,
`Confiance == "Certaine"`, les preuves, le fichier recommandé et les actions textuelles. Dans le
test `plan`, vérifier `GroupeId` et `FichierId` tout en conservant les assertions `Contenu`/`Chemin`.

- [x] **Step 2: Vérifier RED**

Run: `dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj --no-restore`

Expected: échec sur les champs absents ou enums numériques.

- [x] **Step 3: Configurer la sérialisation**

Ajouter `using System.Text.Json.Serialization;` puis :

```csharp
Converters = { new JsonStringEnumConverter() },
```

aux deux `JsonSerializerOptions`. Mettre à jour les exemples JSON du README avec un seul groupe
abrégé et documenter `VersionContrat` ainsi que la séparation avec les versions produit/fichier.

- [x] **Step 4: Vérifier GREEN**

Run: la suite de l'étape 2.

Expected: tous les tests d'intégration Duplicate Files passent.

### Task 7: Vérification finale

**Files:**
- Verify: all files above.

**Interfaces:**
- Produces: preuve de conformité du jalon et de scellement d'Identity.

- [x] **Step 1: Exécuter les tests du moteur métier**

Run: `dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore`

Expected: 0 échec.

- [x] **Step 2: Exécuter la solution complète séquentiellement**

Run: `dotnet test InstallChecker.slnx --no-restore --maxcpucount:1`

Expected: tous les tests historiques et nouveaux passent.

- [x] **Step 3: Vérifier Identity et les frontières**

Run: `git diff --name-only -- src/InstallChecker.Identity src/InstallChecker.Identity.Access tests/InstallChecker.Identity.Tests tests/oracle docs/identity docs/conformite registre`

Expected: aucune sortie.

Run: `rg -n "Identity.Access|Microsoft.Data.Sqlite|InstallChecker.Scanner" modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine --glob "*.cs" --glob "*.csproj"`

Expected: aucune dépendance de code ou de projet ; les mentions documentaires de frontière sont admises.
