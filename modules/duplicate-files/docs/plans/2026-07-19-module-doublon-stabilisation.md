# Module Doublon Stabilisation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stabiliser le module doublon en rendant le plan cohérent avec le rapport et en excluant les chemins système protégés par défaut.

**Architecture:** La correction reste dans le module métier et la CLI. `PlanCommand` réutilise l'enrichissement et la politique de rétention déjà utilisés par `GenerateurDeRapport`; le constructeur de plan reçoit un prédicat de protection pour permettre les protections par préfixe.

**Tech Stack:** C# .NET 10, xUnit, SQLite via les adaptateurs existants.

## Global Constraints

- Ne pas modifier `InstallChecker.Identity`.
- Ne pas mélanger doublons exacts, redondances versionnées et suspects.
- Aucune suppression automatique.
- Les sorties doivent rester consommables par une future interface graphique.
- Les chemins protégés ne doivent jamais être proposés dans un plan.

---

### Task 1: Plan cohérent avec le classement de rétention

**Files:**
- Modify: `tests/InstallChecker.Tests/PlanCommandTests.cs`
- Modify: `src/InstallChecker/PlanCommand.cs`

**Interfaces:**
- Consumes: `ExtracteurDeGroupes.Extraire(W)`, `EnrichisseurDeGroupe.Enrichir(...)`, `PolitiqueRetentionV1.Classer(...)`
- Produces: `PlanCommand.Deriver(...)` qui propose uniquement les exemplaires de rang supérieur ou égal à 2.

- [ ] **Step 1: Write the failing test**

Add a test that scans two identical files where the richer retention policy keeps the second file, then assert the plan does not propose that kept file.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/InstallChecker.Tests/InstallChecker.Tests.csproj --no-restore --filter PlanCommandTests`

Expected: FAIL because the current plan preserves raw domain order instead of retention order.

- [ ] **Step 3: Write minimal implementation**

Change `PlanCommand` so each duplicate group is enriched and sorted with `PolitiqueRetentionV1.Classer`; pass the resulting ordered paths to `ConstructeurDePlan`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/InstallChecker.Tests/InstallChecker.Tests.csproj --no-restore --filter PlanCommandTests`

Expected: PASS.

### Task 2: Chemins protégés par défaut

**Files:**
- Create: `src/InstallChecker.DuplicateFiles/ProtectionDesChemins.cs`
- Modify: `src/InstallChecker.DuplicateFiles/ConstructeurDePlan.cs`
- Modify: `src/InstallChecker/PlanCommand.cs`
- Modify: `tests/InstallChecker.DuplicateFiles.Tests/ConstructeurDePlanTests.cs`

**Interfaces:**
- Produces: `ProtectionDesChemins.EstProtegeParDefaut(string chemin) : bool`
- Produces: `ConstructeurDePlan.Construire(IEnumerable<(string Contenu, IReadOnlyList<string> Chemins)> groupes, Func<string, bool> cheminProtege) : PlanDeSuppression`

- [ ] **Step 1: Write failing tests**

Add tests proving `C:\Windows\...` and `C:\Program Files\...` are protected by prefix, and that a protected first path causes all non-protected copies to be proposed while the protected path remains absent from the plan.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj --no-restore --filter ConstructeurDePlanTests`

Expected: FAIL because the default protection predicate does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add `ProtectionDesChemins` with default protected roots and a prefix check. Add a `Func<string,bool>` overload to `ConstructeurDePlan`, keep the existing `IReadOnlySet<string>` overload for compatibility, and make `PlanCommand` pass `ProtectionDesChemins.EstProtegeParDefaut`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj --no-restore --filter ConstructeurDePlanTests`

Expected: PASS.

### Task 3: Verification

**Files:**
- Verify all changed code and tests.

- [ ] **Step 1: Run targeted tests**

Run:

```powershell
dotnet test tests/InstallChecker.Tests/InstallChecker.Tests.csproj --no-restore --filter PlanCommandTests
dotnet test tests/InstallChecker.DuplicateFiles.Tests/InstallChecker.DuplicateFiles.Tests.csproj --no-restore --filter ConstructeurDePlanTests
```

- [ ] **Step 2: Run full solution tests**

Run: `dotnet test InstallChecker.slnx --no-restore`

Expected: all tests pass.
