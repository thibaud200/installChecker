# Organisation modulaire verticale - Plan d'implementation

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Regrouper Scanner, Duplicate Files et la CLI dans des tranches verticales visibles sans modifier le comportement ni aucun fichier du périmètre Identity scellé.

**Architecture:** Le dépôt conserve Identity et Identity.Access à leur emplacement actuel. Scanner devient un module producteur d'observations en deux projets, Duplicate Files devient un module métier en deux projets, et l'application CLI ne conserve que le routage et la commande Identity. Les espaces de noms publics restent inchangés pendant cette migration.

**Tech Stack:** C# .NET 10, projets SDK-style, xUnit, SQLite, solution `.slnx`.

## Global Constraints

- Ne modifier, déplacer ou renommer aucun fichier sous `src/InstallChecker.Identity/`, `src/InstallChecker.Identity.Access/`, `tests/InstallChecker.Identity.Tests/`, `tests/oracle/`, `docs/identity/`, `docs/conformite/` et `registre/`.
- Conserver les sorties CLI, les codes de retour, les espaces de noms et les 250 scénarios de test existants.
- Ne mélanger aucun changement fonctionnel à la migration physique.
- Ne créer ni dossier global `Shared`, ni système de plugins, ni interface graphique.
- Ne pas introduire d'usage fonctionnel de `%TEMP%`.

---

### Task 1: Verrouiller la decision et la reference Identity

**Files:**
- Modify: `AGENTS.md`
- Create: `docs/superpowers/plans/2026-07-19-organisation-modulaire-verticale-implementation.md`

**Interfaces:**
- Consumes: spécification `docs/superpowers/specs/2026-07-19-organisation-modulaire-verticale-design.md`.
- Produces: ADR-012 et plan de migration vérifiable.

- [x] **Step 1: Enregistrer l'etat initial**

Run: `git status --short` puis `git diff --name-only -- src/InstallChecker.Identity src/InstallChecker.Identity.Access tests/InstallChecker.Identity.Tests tests/oracle docs/identity docs/conformite registre`.

Expected: aucune modification non validée dans le périmètre scellé.

- [x] **Step 2: Ajouter ADR-012**

Documenter le regroupement vertical, le maintien physique d'Identity et le refus d'une abstraction `Shared` anticipée.

- [x] **Step 3: Vérifier la base**

Run: `dotnet test InstallChecker.slnx --no-restore`.

Expected: 250 tests réussis.

### Task 2: Deplacer le module Scanner

**Files:**
- Move: `src/InstallChecker.Core/*` -> `modules/scanner/src/InstallChecker.Scanner.Core/`
- Move: `src/InstallChecker/ScanCommand.cs` -> `modules/scanner/src/InstallChecker.Scanner/ScanCommand.cs`
- Move: `src/InstallChecker/ObservationStore.cs` -> `modules/scanner/src/InstallChecker.Scanner/ObservationStore.cs`
- Move: `tests/InstallChecker.Tests/ScanCommandTests.cs` -> `modules/scanner/tests/InstallChecker.Scanner.Tests/ScanCommandTests.cs`
- Move: `tests/InstallChecker.Tests/VolumeIdentityExtractorTests.cs` -> `modules/scanner/tests/InstallChecker.Scanner.Tests/VolumeIdentityExtractorTests.cs`
- Move: `tests/InstallChecker.Tests/MsiTestFixture.cs` -> `modules/scanner/tests/InstallChecker.Scanner.Tests/MsiTestFixture.cs`
- Create: `modules/scanner/src/InstallChecker.Scanner/InstallChecker.Scanner.csproj`
- Create: `modules/scanner/tests/InstallChecker.Scanner.Tests/InstallChecker.Scanner.Tests.csproj`

**Interfaces:**
- Produces: `ScanCommand.Run(...)`, `ObservationStore`, `VolumeIdentityExtractor` et les extracteurs existants, avec leurs signatures inchangées.
- Depends on: BCL pour le coeur ; SQLite et `InstallChecker.Scanner.Core` pour l'enveloppe.

- [x] **Step 1: Déplacer le coeur et créer les deux projets**

Conserver les sources sans changer leur espace de noms. Le projet `InstallChecker.Scanner` référence `InstallChecker.Scanner.Core` et porte les dépendances SQLite.

- [x] **Step 2: Déplacer les tests Scanner**

Le projet de test référence les deux projets Scanner et conserve les mêmes packages xUnit.

- [x] **Step 3: Vérifier Scanner**

Run: `dotnet test modules/scanner/tests/InstallChecker.Scanner.Tests/InstallChecker.Scanner.Tests.csproj --no-restore`.

Expected: tous les anciens tests `ScanCommand`, `VolumeIdentityExtractor` et MSI passent.

### Task 3: Deplacer le moteur Duplicate Files

**Files:**
- Move: `src/InstallChecker.DuplicateFiles/*` -> `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/`
- Move: `tests/InstallChecker.DuplicateFiles.Tests/*` -> `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/`
- Rename project: `InstallChecker.DuplicateFiles.csproj` -> `InstallChecker.DuplicateFiles.Engine.csproj`
- Rename test project: `InstallChecker.DuplicateFiles.Tests.csproj` -> `InstallChecker.DuplicateFiles.Engine.Tests.csproj`

**Interfaces:**
- Produces: tous les DTO, politiques, enrichisseurs, rapports et plans du module dans l'espace de noms `InstallChecker.DuplicateFiles` inchangé.
- Depends on: contrat public `InstallChecker.Identity` uniquement.

- [x] **Step 1: Déplacer le projet moteur**

Mettre à jour uniquement le chemin de sa référence vers `src/InstallChecker.Identity/InstallChecker.Identity.csproj`.

- [x] **Step 2: Déplacer le projet de tests moteur**

Référencer le nouveau projet Engine sans modifier les scénarios.

- [x] **Step 3: Vérifier le moteur métier**

Run: `dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Engine.Tests/InstallChecker.DuplicateFiles.Engine.Tests.csproj --no-restore`.

Expected: 33 tests réussis.

### Task 4: Creer l'enveloppe Duplicate Files

**Files:**
- Move: `src/InstallChecker/DuplicatesCommand.cs` -> `modules/duplicate-files/src/InstallChecker.DuplicateFiles/DuplicatesCommand.cs`
- Move: `src/InstallChecker/PlanCommand.cs` -> `modules/duplicate-files/src/InstallChecker.DuplicateFiles/PlanCommand.cs`
- Move: `src/InstallChecker/LecteurDeVolumes.cs` -> `modules/duplicate-files/src/InstallChecker.DuplicateFiles/LecteurDeVolumes.cs`
- Move: `tests/InstallChecker.Tests/DuplicatesCommandTests.cs` -> `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/DuplicatesCommandTests.cs`
- Move: `tests/InstallChecker.Tests/PlanCommandTests.cs` -> `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/PlanCommandTests.cs`
- Move: `tests/InstallChecker.Tests/LecteurDeVolumesTests.cs` -> `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/LecteurDeVolumesTests.cs`
- Move: `tests/InstallChecker.Tests/MultiDisqueTests.cs` -> `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/MultiDisqueTests.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles/InstallChecker.DuplicateFiles.csproj`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj`

**Interfaces:**
- Produces: commandes `duplicates` et `plan`, sérialisation JSON et lecture annexe des volumes.
- Depends on: Duplicate Engine, Scanner pour les scénarios d'intégration, Identity et Identity.Access sans modification de ces derniers.

- [x] **Step 1: Créer l'enveloppe applicative**

Référencer Engine, Identity et Identity.Access ; ajouter `Microsoft.Data.Sqlite` pour `LecteurDeVolumes`.

- [x] **Step 2: Déplacer les tests d'intégration métier**

Le projet de test référence l'enveloppe Duplicate Files et Scanner pour les parcours multi-disque.

- [x] **Step 3: Vérifier l'enveloppe**

Run: `dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj --no-restore`.

Expected: tous les anciens tests `duplicates`, `plan`, volumes et multi-disque passent.

### Task 5: Amincir et deplacer la CLI

**Files:**
- Move: `src/InstallChecker/Program.cs` -> `apps/cli/InstallChecker.Cli/Program.cs`
- Move: `src/InstallChecker/IdentityCommand.cs` -> `apps/cli/InstallChecker.Cli/IdentityCommand.cs`
- Move: `src/InstallChecker/InstallChecker.csproj` -> `apps/cli/InstallChecker.Cli/InstallChecker.Cli.csproj`
- Move: `tests/InstallChecker.Tests/IdentityCommandTests.cs` -> `apps/cli/tests/InstallChecker.Cli.Tests/IdentityCommandTests.cs`
- Move: `tests/InstallChecker.Tests/FrontiereDeDonneesTests.cs` -> `apps/cli/tests/InstallChecker.Cli.Tests/FrontiereDeDonneesTests.cs`
- Move: `tests/InstallChecker.Tests/InstallChecker.Tests.csproj` -> `apps/cli/tests/InstallChecker.Cli.Tests/InstallChecker.Cli.Tests.csproj`

**Interfaces:**
- Produces: exécutable `InstallChecker`, routage des commandes et commande Identity inchangés.
- Depends on: Scanner, Duplicate Files, Identity et Identity.Access.

- [x] **Step 1: Déplacer l'hôte**

Conserver `Program.cs` comme routeur pur. Configurer `AssemblyName` à `InstallChecker` afin de préserver le nom de l'exécutable.

- [x] **Step 2: Déplacer les tests de frontière**

Référencer la CLI et Scanner ; conserver les chemins vers l'oracle et les fixtures Identity inchangés.

- [x] **Step 3: Vérifier la CLI**

Run: `dotnet test apps/cli/tests/InstallChecker.Cli.Tests/InstallChecker.Cli.Tests.csproj --no-restore`.

Expected: tous les tests Identity CLI et frontière de données passent.

### Task 6: Regrouper les documents et actualiser la solution

**Files:**
- Modify: `InstallChecker.slnx`
- Move: documents Duplicate Files de `docs/projet/` et `docs/superpowers/` -> `modules/duplicate-files/docs/`
- Move: `docs/mesures/` -> `modules/scanner/docs/mesures/`
- Modify: liens et commandes qui citent les anciens chemins de projets ou documents.

**Interfaces:**
- Produces: une solution organisée par dossiers `Identity`, `modules` et `apps`, et des chemins documentaires canoniques par domaine.

- [x] **Step 1: Mettre à jour la solution**

Inclure les huit projets déplacés ou créés et conserver les trois projets Identity exactement à leur emplacement historique.

- [x] **Step 2: Déplacer les documents métier**

Conserver la spécification et le présent plan d'organisation au niveau global ; déplacer uniquement les documents propres à Scanner ou Duplicate Files.

- [x] **Step 3: Corriger les références textuelles actives**

Run: `rg -n "src/InstallChecker\.Core|src/InstallChecker\.DuplicateFiles|src/InstallChecker/|tests/InstallChecker\.Tests|tests/InstallChecker\.DuplicateFiles\.Tests" README.md CLAUDE.md AGENTS.md docs modules apps`.

Expected: les références restantes désignent explicitement un chemin historique ou appartiennent aux documents figés.

### Task 7: Verification finale et preuve de scellement

**Files:**
- Verify: all moved projects and documentation.

**Interfaces:**
- Produces: migration mécanique complète et preuve qu'Identity n'a pas été touché.

- [x] **Step 1: Vérifier les tests ciblés**

Run: chaque projet de test Scanner, Duplicate Engine, Duplicate Files et CLI avec `--no-restore`.

Expected: les mêmes scénarios passent dans leurs nouveaux domaines.

- [x] **Step 2: Vérifier la solution complète**

Run: `dotnet test InstallChecker.slnx --no-restore`.

Expected: 250 tests réussis, 0 échec.

- [x] **Step 3: Vérifier Identity**

Run: `git diff --name-only -- src/InstallChecker.Identity src/InstallChecker.Identity.Access tests/InstallChecker.Identity.Tests tests/oracle docs/identity docs/conformite registre`.

Expected: aucune sortie.

- [x] **Step 4: Vérifier les frontières physiques**

Run: `rg --files src tests` et `rg --files modules apps`.

Expected: `src/` et `tests/` ne contiennent plus que le périmètre Identity scellé ; tout Scanner et Duplicate Files se trouve sous son module ; la CLI se trouve sous `apps/cli/`.
