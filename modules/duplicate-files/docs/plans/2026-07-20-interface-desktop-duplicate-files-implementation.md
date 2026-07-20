# Interface desktop Duplicate Files - Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Construire une application WPF permettant de scanner une racine par lecteur, consulter les doublons exacts et les versions apparentées, puis reprendre les décisions de revue depuis une session JSON avec une archive maximum.

**Architecture:** Le projet Desktop est une enveloppe MVVM appartenant au module Duplicate Files. Il appelle strictement `ScanCommand`, `DuplicatesCommand` et `RedondanceVersionneeCommand`, conserve leurs JSON sans recalcul métier et les projette vers des modèles d'affichage. Un stockage de session atomique porte la bibliothèque, les rapports et les décisions de revue.

**Tech Stack:** .NET 10, WPF `net10.0-windows`, XAML, `System.Text.Json`, xUnit 2.9.3, API BCL uniquement.

## Global Constraints

- Ne modifier aucun fichier sous `src/InstallChecker.Identity`, `src/InstallChecker.Identity.Access`, `tests/InstallChecker.Identity.Tests`, `tests/oracle`, `docs/identity`, `docs/conformite` ou `registre`.
- Ne modifier ni `modules/scanner/src`, ni les moteurs sous `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine`.
- Ne créer aucune API de suppression, Corbeille, copie ou déplacement de fichiers analysés.
- Une seule racine indépendante est autorisée par `VolumeId`; plusieurs lecteurs partagent la même base.
- Le scan n'est pas annulable dans cette version.
- Les rapports métier proviennent exclusivement des commandes existantes.
- Le rapport historique sans `VersionContrat` est importé en lecture seule et ne rend pas disponible l'onglet Versions.
- La session est écrite atomiquement et possède au plus une archive `.previous.json`.
- Ne pas ajouter de package NuGet UI ou MVVM.
- Ne pas stage ni commit sans demande explicite de l'utilisateur.

---

## File Map

```text
modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/
  InstallChecker.DuplicateFiles.Desktop.csproj       application WPF et références
  App.xaml / App.xaml.cs                             composition manuelle des services
  MainWindow.xaml / MainWindow.xaml.cs               shell et code-behind limité aux dialogues
  Themes/Colors.xaml                                 palette sémantique
  Themes/Controls.xaml                               styles WPF partagés
  Infrastructure/ObservableObject.cs                 INotifyPropertyChanged minimal
  Infrastructure/RelayCommand.cs                     commandes synchrones
  Infrastructure/AsyncRelayCommand.cs                commandes asynchrones sérialisées
  Infrastructure/IUiDispatcher.cs                    frontière de retour sur le thread WPF
  Infrastructure/DispatcherUiWpf.cs                  adaptateur Application.Dispatcher
  Session/ContratSessionUi.cs                        contrat JSON de session
  Session/StockageSessionUi.cs                       lecture et remplacement atomique
  Bibliotheque/ValidateurRacines.cs                  normalisation et règle un volume
  Bibliotheque/DialogueFichiersWpf.cs                dialogues base, JSON et dossiers
  Adaptateurs/ProgressionScanTextWriter.cs           compteur sur sortie TSV existante
  Adaptateurs/ScannerBibliotheque.cs                 orchestration séquentielle ScanCommand
  Adaptateurs/AnalyseurBibliotheque.cs               appels des deux commandes métier
  Presentation/ModelesDoublonsUi.cs                  modèles de lignes exactes
  Presentation/LecteurRapportDoublonsUi.cs           contrat courant + ancien rapport
  Presentation/ModelesVersionsUi.cs                  modèles de lignes versionnées
  Presentation/LecteurRapportVersionsUi.cs           projection du contrat F1
  ViewModels/MainViewModel.cs                         workflow principal
  ViewModels/GroupeDoublonViewModel.cs               détail et revue exacts
  ViewModels/GroupeVersionViewModel.cs               détail et revue versions

modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/
  InstallChecker.DuplicateFiles.Desktop.Tests.csproj
  StockageSessionUiTests.cs
  ValidateurRacinesTests.cs
  ProgressionScanTextWriterTests.cs
  AdaptateursCommandesTests.cs
  LecteurRapportDoublonsUiTests.cs
  LecteurRapportVersionsUiTests.cs
  MainViewModelTests.cs
  StructureXamlTests.cs
```

---

### Task 1: Projets WPF, infrastructure MVVM et shell vide

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/App.xaml`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/App.xaml.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/MainWindow.xaml`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/MainWindow.xaml.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Infrastructure/ObservableObject.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Infrastructure/RelayCommand.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Infrastructure/AsyncRelayCommand.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Infrastructure/IUiDispatcher.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Infrastructure/DispatcherUiWpf.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InfrastructureMvvmTests.cs`
- Modify: `InstallChecker.slnx`

**Interfaces:**
- Produces: `ObservableObject`, `RelayCommand`, `AsyncRelayCommand`, `IUiDispatcher` et `DispatcherUiWpf` dans `InstallChecker.DuplicateFiles.Desktop.Infrastructure`.
- Produces: application WPF démarrable sans service métier.

- [ ] **Step 1: Écrire les tests rouges de commandes MVVM**

```csharp
[Fact]
public void RelayCommand_execute_laction_et_respecte_CanExecute()
{
    var executions = 0;
    var autorisee = false;
    var commande = new RelayCommand(() => executions++, () => autorisee);

    Assert.False(commande.CanExecute(null));
    autorisee = true;
    commande.Execute(null);

    Assert.Equal(1, executions);
}

[Fact]
public async Task AsyncRelayCommand_refuse_une_double_execution()
{
    var fin = new TaskCompletionSource();
    var executions = 0;
    var commande = new AsyncRelayCommand(async () => { executions++; await fin.Task; });

    commande.Execute(null);
    commande.Execute(null);
    Assert.Equal(1, executions);
    Assert.True(commande.EstEnCours);

    fin.SetResult();
    await commande.ExecutionCourante;
    Assert.False(commande.EstEnCours);
}
```

- [ ] **Step 2: Créer les deux projets et vérifier l'échec**

Le projet source utilise :

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>InstallChecker.DuplicateFiles.Desktop</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\InstallChecker.DuplicateFiles\InstallChecker.DuplicateFiles.csproj" />
    <ProjectReference Include="..\..\..\scanner\src\InstallChecker.Scanner\InstallChecker.Scanner.csproj" />
    <ProjectReference Include="..\..\..\scanner\src\InstallChecker.Scanner.Core\InstallChecker.Scanner.Core.csproj" />
  </ItemGroup>
</Project>
```

Le projet de tests cible `net10.0-windows`, reprend les versions xUnit du dépôt et référence le projet Desktop.

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj --filter FullyQualifiedName~InfrastructureMvvmTests
```

Expected: échec de compilation, types MVVM absents.

- [ ] **Step 3: Implémenter l'infrastructure minimale**

```csharp
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    protected void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

`RelayCommand` implémente `ICommand`, reçoit `Action` et `Func<bool>?`. `AsyncRelayCommand` expose
`bool EstEnCours`, `Task ExecutionCourante`, ignore une seconde exécution et relève
`CanExecuteChanged` au début et à la fin.

La frontière du thread graphique est explicite :

```csharp
public interface IUiDispatcher
{
    Task ExecuterAsync(Action action);
}

public sealed class DispatcherUiWpf : IUiDispatcher
{
    public Task ExecuterAsync(Action action) =>
        Application.Current.Dispatcher.InvokeAsync(action).Task;
}
```

Les tests injectent une implémentation immédiate qui exécute directement l'action.

- [ ] **Step 4: Créer le shell WPF minimal**

`App.xaml` déclare `StartupUri="MainWindow.xaml"`. `MainWindow.xaml` contient une fenêtre de taille
minimale `1100x680`, un `Grid` à trois lignes, une barre de commandes, un panneau gauche de largeur
`280` et un `TabControl` avec les onglets `Doublons exacts` et `Versions apparentées`. Aucun bouton
Supprimer n'est encore ajouté.

- [ ] **Step 5: Ajouter les projets à la solution et vérifier**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj
dotnet build modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj -c Release
```

Expected: 2 tests réussis et build WPF sans avertissement.

---

### Task 2: Contrat et stockage atomique de session

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Session/ContratSessionUi.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Session/StockageSessionUi.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/StockageSessionUiTests.cs`

**Interfaces:**
- Produces: `SessionDuplicateFilesUi`, `BibliothequeUi`, `DecisionRevueUi`, `EtatRevueUi`, `DiagnosticUi`.
- Produces: `StockageSessionUi.ChargerAsync(string, CancellationToken)`.
- Produces: `StockageSessionUi.SauvegarderAsync(string, SessionDuplicateFilesUi, bool tournerArchive, CancellationToken)`.

- [ ] **Step 1: Écrire les tests rouges du contrat et de la rotation**

```csharp
[Fact]
public async Task Sauvegarder_puis_charger_preserve_rapports_et_decisions()
{
    var session = SessionFixture("groupe-1", EtatRevueUi.Prevoir);
    await _stockage.SauvegarderAsync(_courant, session, false, default);

    var relue = await _stockage.ChargerAsync(_courant, default);

    Assert.Equal(VersionsContratUi.SessionV1, relue.VersionContrat);
    Assert.Equal(EtatRevueUi.Prevoir, relue.Decisions["groupe-1"].Etat);
    Assert.Equal(1, relue.RapportDoublons!.Value.GetProperty("Groupes").GetArrayLength());
}

[Fact]
public async Task Rescan_conserve_exactement_une_archive()
{
    await _stockage.SauvegarderAsync(_courant, SessionFixture("v1"), false, default);
    await _stockage.SauvegarderAsync(_courant, SessionFixture("v2"), true, default);
    await _stockage.SauvegarderAsync(_courant, SessionFixture("v3"), true, default);

    Assert.Equal("v3", (await _stockage.ChargerAsync(_courant, default)).Bibliotheque.Nom);
    Assert.Equal("v2", (await _stockage.ChargerAsync(_archive, default)).Bibliotheque.Nom);
    Assert.Equal(2, Directory.GetFiles(_dir, "*.json").Length);
}

[Fact]
public async Task Echec_avant_remplacement_conserve_la_session_courante()
{
    await _stockage.SauvegarderAsync(_courant, SessionFixture("stable"), false, default);
    var stockage = new StockageSessionUi((_, _) => throw new IOException("simulation"));

    await Assert.ThrowsAsync<IOException>(() =>
        stockage.SauvegarderAsync(_courant, SessionFixture("nouvelle"), true, default));

    Assert.Equal("stable", (await _stockage.ChargerAsync(_courant, default)).Bibliotheque.Nom);
}
```

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj --filter FullyQualifiedName~StockageSessionUiTests
```

Expected: échec de compilation, contrat absent.

- [ ] **Step 3: Implémenter le contrat JSON UI**

```csharp
public static class VersionsContratUi
{
    public const string SessionV1 = "duplicate-files/desktop-session/v1";
}

public enum EtatRevueUi { AExaminer, Conserver, Prevoir, Ignorer }

public sealed record BibliothequeUi(
    string Nom,
    string CheminBase,
    string CheminRegistre,
    IReadOnlyList<string> Racines,
    string CheminSession);

public sealed record DecisionRevueUi(
    string GroupeId,
    string? FichierId,
    EtatRevueUi Etat,
    DateTimeOffset ModifieeLe);

public sealed record DiagnosticUi(string Code, string Message, string? Chemin = null);

public sealed record EtatFiltresUi(string Recherche, EtatRevueUi? Etat, string? Confiance);

public sealed record SessionDuplicateFilesUi(
    string VersionContrat,
    BibliothequeUi Bibliotheque,
    DateTimeOffset? DernierScan,
    JsonElement? RapportDoublons,
    JsonElement? RapportVersions,
    IReadOnlyDictionary<string, DecisionRevueUi> Decisions,
    EtatFiltresUi FiltresDoublons,
    EtatFiltresUi FiltresVersions,
    IReadOnlyList<DiagnosticUi> Diagnostics);
```

- [ ] **Step 4: Implémenter l'écriture atomique**

`StockageSessionUi` sérialise avec `JsonSerializerOptions { WriteIndented = true }` et
`JsonStringEnumConverter`. Il écrit dans `<courant>.tmp-<guid>`, relit ce temporaire avec
`JsonDocument.ParseAsync`, puis :

```csharp
if (!File.Exists(cheminCourant))
    File.Move(temporaire, cheminCourant);
else if (tournerArchive)
{
    var archive = CheminArchive(cheminCourant);
    if (File.Exists(archive)) File.Delete(archive);
    File.Replace(temporaire, cheminCourant, archive, ignoreMetadataErrors: true);
}
else
{
    File.Replace(temporaire, cheminCourant, null, ignoreMetadataErrors: true);
}
```

Le `finally` supprime seulement le temporaire restant. `ChargerAsync` refuse toute version autre
que `duplicate-files/desktop-session/v1` avec `InvalidDataException`.

- [ ] **Step 5: Vérifier la tâche**

Run: commande de l'étape 2.

Expected: tous les tests `StockageSessionUiTests` réussissent.

---

### Task 3: Bibliothèque et validation des racines

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Bibliotheque/ValidateurRacines.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Bibliotheque/DialogueFichiersWpf.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/ValidateurRacinesTests.cs`

**Interfaces:**
- Consumes: `VolumeIdentityExtractor.Resolve(string)` existant.
- Produces: `ResultatValidationRacines ValidateurRacines.Valider(IEnumerable<string>)`.
- Produces: `IDialogueFichiers` et son implémentation WPF.

- [ ] **Step 1: Écrire les tests rouges des racines**

```csharp
[Fact]
public void Deux_lecteurs_sont_acceptes()
{
    var validateur = Validateur(id => id.StartsWith("C:") ? "volume-c" : "volume-d");
    var resultat = validateur.Valider([@"C:\Corpus", @"D:\Archives"]);

    Assert.True(resultat.EstValide);
    Assert.Equal(2, resultat.Racines.Count);
}

[Fact]
public void Sous_dossier_deja_couvert_est_retire()
{
    var resultat = Validateur(_ => "volume-c").Valider([@"C:\Corpus", @"C:\Corpus\Sous"]);

    Assert.True(resultat.EstValide);
    Assert.Equal([@"C:\Corpus"], resultat.Racines);
}

[Fact]
public void Deux_racines_independantes_du_meme_volume_sont_refusees()
{
    var resultat = Validateur(_ => "volume-d").Valider([@"D:\Photos", @"D:\Archives"]);

    Assert.False(resultat.EstValide);
    Assert.Contains(resultat.Diagnostics, d => d.Code == "RacinesMemeVolume");
}
```

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj --filter FullyQualifiedName~ValidateurRacinesTests
```

Expected: échec de compilation.

- [ ] **Step 3: Implémenter la validation pure**

```csharp
public sealed record RacineValidee(string Chemin, string VolumeId);

public sealed record ResultatValidationRacines(
    bool EstValide,
    IReadOnlyList<string> Racines,
    IReadOnlyList<DiagnosticUi> Diagnostics);

public sealed class ValidateurRacines(Func<string, string> resoudreVolume)
{
    public ResultatValidationRacines Valider(IEnumerable<string> chemins)
    {
        // Path.GetFullPath, suppression du séparateur final sauf racine,
        // Distinct OrdinalIgnoreCase, résolution de volume, retrait des descendants,
        // puis diagnostic si un volume garde plus d'une racine indépendante.
    }
}
```

Le constructeur public par défaut appelle `VolumeIdentityExtractor.Resolve(path).VolumeId`. Un
constructeur recevant le délégué reste public pour les tests sans lecteurs physiques multiples.

- [ ] **Step 4: Ajouter les dialogues WPF sans logique métier**

```csharp
public interface IDialogueFichiers
{
    string? ChoisirDossier(string? initial);
    string? OuvrirBase(string? initial);
    string? OuvrirJson(string? initial);
    string? SauvegarderSession(string? initial);
}
```

`DialogueFichiersWpf` utilise `Microsoft.Win32.OpenFolderDialog`, `OpenFileDialog` et
`SaveFileDialog`. Les filtres sont `SQLite (*.db)|*.db` et `JSON (*.json)|*.json`.

- [ ] **Step 5: Vérifier la tâche**

Run: commande de l'étape 2.

Expected: tests verts.

---

### Task 4: Adaptateurs des commandes existantes

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Adaptateurs/ProgressionScanTextWriter.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Adaptateurs/ScannerBibliotheque.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Adaptateurs/AnalyseurBibliotheque.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/ProgressionScanTextWriterTests.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/AdaptateursCommandesTests.cs`

**Interfaces:**
- Consumes: `ScanCommand.Run`, `DuplicatesCommand.Deriver`, `RedondanceVersionneeCommand.Deriver`.
- Produces: `ScannerBibliotheque.ExecuterAsync` et `AnalyseurBibliotheque.AnalyserAsync`.

- [ ] **Step 1: Tester le compteur TSV**

```csharp
[Fact]
public void Une_ligne_TSV_publie_un_fichier_traite()
{
    ProgressionScanUi? derniere = null;
    using var writer = new ProgressionScanTextWriter(p => derniere = p);

    writer.WriteLine(@"D:\a.exe\t12\tabcd");

    Assert.Equal(1, derniere!.FichiersTraites);
    Assert.Equal(@"D:\a.exe", derniere.CheminCourant);
}
```

- [ ] **Step 2: Tester les adaptateurs par ports injectables**

Définir dans les tests des délégués simulant les commandes :

```csharp
[Fact]
public async Task Analyseur_restitue_les_deux_JSON_sans_les_modifier()
{
    var analyseur = new AnalyseurBibliotheque(
        (_, _, output, _) => { output.Write("{\"Groupes\":[]}"); return 0; },
        (_, output, _) => { output.Write("{\"VersionContrat\":\"duplicate-files/version-redundancy/v1\",\"Groupes\":[]}"); return 0; });

    var resultat = await analyseur.AnalyserAsync("base.db", "registre", default);

    Assert.True(resultat.Reussi);
    Assert.Equal(0, resultat.RapportDoublons!.Value.GetProperty("Groupes").GetArrayLength());
    Assert.Equal(VersionsContratDuplicateFiles.RedondanceVersionneeV1,
        resultat.RapportVersions!.Value.GetProperty("VersionContrat").GetString());
}
```

Tester aussi : premier lecteur réussi puis second échoué donne `Partiel=true`, et aucune analyse ne
retourne `Reussi=true` si une commande métier retourne 1.

- [ ] **Step 3: Vérifier les échecs**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj --filter "FullyQualifiedName~ProgressionScanTextWriterTests|FullyQualifiedName~AdaptateursCommandesTests"
```

Expected: échec de compilation.

- [ ] **Step 4: Implémenter les contrats d'adaptation**

```csharp
public sealed record ProgressionScanUi(long FichiersTraites, string CheminCourant);
public sealed record ResultatScanUi(bool Reussi, bool Partiel, long FichiersTraites, IReadOnlyList<DiagnosticUi> Diagnostics);
public sealed record ResultatAnalyseUi(bool Reussi, JsonElement? RapportDoublons, JsonElement? RapportVersions, IReadOnlyList<DiagnosticUi> Diagnostics);
```

`ScannerBibliotheque.ExecuterAsync(BibliothequeUi, IProgress<ProgressionScanUi>?, CancellationToken)`
utilise `Task.Run`, appelle `ScanCommand.Run` pour chaque racine validée, `jsonOutput:false`, un
`ProgressionScanTextWriter`, puis agrège stderr en diagnostics. Le token n'annule que l'attente
avant le démarrage d'une racine suivante ; il n'est jamais présenté comme une annulation du scan
actif.

`AnalyseurBibliotheque.AnalyserAsync` appelle les deux commandes dans `Task.Run`, parse leurs
sorties avec `JsonDocument.Parse(...).RootElement.Clone()` et ne transforme aucun champ.

- [ ] **Step 5: Vérifier la tâche**

Run: commande de l'étape 3.

Expected: tests verts.

---

### Task 5: Projection UI des rapports courant, historique et versionné

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Presentation/ModelesDoublonsUi.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Presentation/LecteurRapportDoublonsUi.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Presentation/ModelesVersionsUi.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Presentation/LecteurRapportVersionsUi.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/LecteurRapportDoublonsUiTests.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/LecteurRapportVersionsUiTests.cs`

**Interfaces:**
- Consumes: `JsonElement` exact courant ou historique et versionné F1.
- Produces: `RapportDoublonsUi` et `RapportVersionsUi`, sans décision métier nouvelle.

- [ ] **Step 1: Tester le rapport historique réel**

Le test localise la racine du dépôt et ouvre `rapport-doublons.json` en flux :

```csharp
[Fact]
public async Task Rapport_historique_reel_expose_sa_synthese_et_ses_groupes()
{
    await using var flux = File.OpenRead(Path.Combine(RacineDepot(), "rapport-doublons.json"));
    using var document = await JsonDocument.ParseAsync(flux);

    var rapport = new LecteurRapportDoublonsUi().Lire(document.RootElement);

    Assert.True(rapport.EstHistorique);
    Assert.Equal(6009, rapport.NombreGroupes);
    Assert.Equal(8579, rapport.NombreCandidats);
    Assert.NotEmpty(rapport.Groupes);
}
```

Ajouter un petit fixture courant vérifiant `GroupeId`, `FichierId`, `Role`, preuves et blocages.

- [ ] **Step 2: Tester le rapport versionné**

```csharp
[Fact]
public void Rapport_versionne_projette_variante_confiance_et_blocages()
{
    using var json = JsonDocument.Parse(FixtureVersionnee);
    var rapport = new LecteurRapportVersionsUi().Lire(json.RootElement);

    var groupe = Assert.Single(rapport.Groupes);
    Assert.Equal("Outil", groupe.Famille);
    Assert.Equal("Forte", groupe.Confiance);
    Assert.Equal("x64", groupe.Architecture);
    Assert.Contains("RevueHumaineObligatoire", groupe.Blocages);
}
```

Ajouter un test refusant un JSON de version de contrat inconnue.

- [ ] **Step 3: Vérifier les échecs ciblés**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj --filter "FullyQualifiedName~LecteurRapportDoublonsUiTests|FullyQualifiedName~LecteurRapportVersionsUiTests"
```

Expected: échec de compilation.

- [ ] **Step 4: Implémenter les modèles de présentation exacts**

```csharp
public sealed record FichierDoublonUi(
    string FichierId, long ActeId, string Chemin, long Taille, int Rang,
    string Role, string? Volume, IReadOnlyList<string> Blocages);

public sealed record GroupeDoublonUi(
    string GroupeId, long TailleUnitaire, long EspaceRecuperable,
    string Confiance, string? Sha256, IReadOnlyList<FichierDoublonUi> Fichiers);

public sealed record RapportDoublonsUi(
    bool EstHistorique, int NombreGroupes, int NombreCandidats,
    long EspaceRecuperable, IReadOnlyList<GroupeDoublonUi> Groupes);
```

Le lecteur courant copie les identifiants stables. Le lecteur historique utilise uniquement pour
la reprise UI `legacy:domaine:<ids triés>` et `legacy:acte:<ActeId>`. Ces identifiants sont marqués
historiques et ne sont jamais présentés comme les identifiants du contrat métier courant.

- [ ] **Step 5: Implémenter les modèles de présentation versionnés**

```csharp
public sealed record ArtefactVersionUi(
    string ContenuSha256, string Version, string? Role,
    IReadOnlyList<string> Chemins, IReadOnlyList<string> Blocages);

public sealed record GroupeVersionUi(
    string GroupeId, string Famille, string VersionReference, string Confiance,
    string Format, string? Architecture, string? Langue, bool VariantePartielle,
    IReadOnlyList<string> Blocages, IReadOnlyList<ArtefactVersionUi> Artefacts);

public sealed record RapportVersionsUi(
    int NombreGroupes, int NombreVersionsAnterieures,
    IReadOnlyList<GroupeVersionUi> Groupes);
```

Les enums JSON restent des chaînes affichées. Aucune comparaison de versions n'est effectuée.

- [ ] **Step 6: Vérifier la tâche**

Run: commande de l'étape 3.

Expected: tests verts, y compris le rapport réel de 24,5 Mo.

---

### Task 6: Workflow du MainViewModel et reprise des décisions

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/ViewModels/MainViewModel.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/ViewModels/GroupeDoublonViewModel.cs`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/ViewModels/GroupeVersionViewModel.cs`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/MainViewModelTests.cs`

**Interfaces:**
- Consumes: services des Tasks 2 à 5.
- Produces: propriétés et commandes bindées par `MainWindow.xaml`.

- [ ] **Step 1: Écrire les tests rouges du workflow**

```csharp
[Fact]
public async Task Ouvrir_base_calcule_les_deux_onglets_et_sauvegarde_la_session()
{
    var vm = FixtureViewModel(analyseReussie: true);
    await vm.OuvrirBaseAsync("bibliotheque.db");

    Assert.Single(vm.GroupesDoublons);
    Assert.Single(vm.GroupesVersions);
    Assert.True(vm.VersionsDisponibles);
    Assert.Equal("Session sauvegardée", vm.Etat);
}

[Fact]
public async Task Import_historique_desactive_seulement_les_versions()
{
    var vm = FixtureViewModel();
    await vm.ImporterRapportAsync("rapport-doublons.json");

    Assert.NotEmpty(vm.GroupesDoublons);
    Assert.False(vm.VersionsDisponibles);
    Assert.Contains("ne contient pas", vm.MessageVersions);
}

[Fact]
public async Task Rescan_reapplique_les_decisions_stables_et_archive_une_fois()
{
    var vm = FixtureViewModelAvecDecision("exact:sha256:abc", EtatRevueUi.Prevoir);
    await vm.ScannerAsync();

    Assert.Equal(EtatRevueUi.Prevoir,
        vm.GroupesDoublons.Single(g => g.GroupeId == "exact:sha256:abc").EtatRevue);
    Assert.True(_stockage.DernierAppelTournerArchive);
}

[Fact]
public void Supprimer_est_toujours_inexecutable()
{
    var vm = FixtureViewModel();
    Assert.False(vm.SupprimerCommand.CanExecute(null));
}
```

Ajouter : scan partiel ne tourne pas l'archive ; modification de revue déclenche une sauvegarde sans
rotation ; décision disparue apparaît dans `DecisionsIntrouvables`.

- [ ] **Step 2: Vérifier l'échec ciblé**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj --filter FullyQualifiedName~MainViewModelTests
```

Expected: échec de compilation.

- [ ] **Step 3: Implémenter les propriétés et commandes**

`MainViewModel` expose au minimum :

```csharp
ObservableCollection<string> Racines { get; }
ObservableCollection<GroupeDoublonViewModel> GroupesDoublons { get; }
ObservableCollection<GroupeVersionViewModel> GroupesVersions { get; }
IReadOnlyList<DiagnosticUi> Diagnostics { get; }
AsyncRelayCommand ScannerCommand { get; }
AsyncRelayCommand OuvrirBaseCommand { get; }
AsyncRelayCommand ImporterRapportCommand { get; }
RelayCommand AjouterRacineCommand { get; }
RelayCommand RetirerRacineCommand { get; }
RelayCommand SupprimerCommand { get; }
bool EstOccupe { get; }
bool VersionsDisponibles { get; }
long FichiersTraites { get; }
string Etat { get; }
```

`SupprimerCommand` reçoit `canExecute: () => false` et une action vide qui n'appelle aucun service.

- [ ] **Step 4: Implémenter les workflows**

- `OuvrirSessionAsync` charge sans recalcul et restaure les filtres.
- `OuvrirBaseAsync` appelle les deux analyses puis sauvegarde une session sans archive.
- `ImporterRapportAsync` lit le JSON exact, marque `VersionsDisponibles=false` et attend une décision
  avant de demander un chemin de session.
- `ScannerAsync` valide les racines, appelle le scan, refuse la rotation en cas d'échec/partiel,
  analyse la base, fusionne les décisions par IDs, puis sauvegarde avec rotation.
- `ChangerDecisionAsync` met à jour la décision et sauvegarde sans rotation.

Toutes les collections sont remplacées via `IUiDispatcher.ExecuterAsync` après le retour des tâches
de fond. Le ViewModel ne référence jamais directement `Application.Current`.

- [ ] **Step 5: Vérifier la tâche**

Run: commande de l'étape 2.

Expected: tests verts.

---

### Task 7: Thème, onglet Doublons et détail des exemplaires

**Files:**
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Themes/Colors.xaml`
- Create: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/Themes/Controls.xaml`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/App.xaml`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/MainWindow.xaml`
- Create: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/StructureXamlTests.cs`

**Interfaces:**
- Consumes: `MainViewModel` et `GroupeDoublonViewModel`.
- Produces: vue opérationnelle virtualisée des doublons exacts.

- [ ] **Step 1: Écrire les tests structurels XAML rouges**

```csharp
[Fact]
public void Grilles_de_resultats_activent_la_virtualisation()
{
    var xaml = File.ReadAllText(CheminSource("MainWindow.xaml"));
    Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", xaml);
    Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", xaml);
    Assert.DoesNotContain("CanUserAddRows=\"True\"", xaml);
}

[Fact]
public void Suppression_est_visible_et_explicitement_desactivee()
{
    var xaml = File.ReadAllText(CheminSource("MainWindow.xaml"));
    Assert.Contains("Content=\"Supprimer\"", xaml);
    Assert.Contains("Command=\"{Binding SupprimerCommand}\"", xaml);
    Assert.Contains("Exécution non disponible", xaml);
}
```

- [ ] **Step 2: Vérifier les échecs**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj --filter FullyQualifiedName~StructureXamlTests
```

Expected: assertions absentes du shell minimal.

- [ ] **Step 3: Implémenter les ressources visuelles**

`Colors.xaml` déclare les brosses sémantiques exactes de la spec :

```xml
<SolidColorBrush x:Key="BackgroundBrush" Color="#F4F6F8" />
<SolidColorBrush x:Key="SurfaceBrush" Color="#FFFFFF" />
<SolidColorBrush x:Key="TextBrush" Color="#17212B" />
<SolidColorBrush x:Key="KeepBrush" Color="#147D78" />
<SolidColorBrush x:Key="ReviewBrush" Color="#B56A00" />
<SolidColorBrush x:Key="BlockedBrush" Color="#B42318" />
<SolidColorBrush x:Key="SelectionBrush" Color="#2563EB" />
```

`Controls.xaml` définit boutons icônes/textes, champs, `DataGrid`, `TabControl`, focus clavier visible
et tooltips. Rayon maximal : 6 px. Aucun gradient, aucune carte imbriquée.

- [ ] **Step 4: Construire le shell complet et l'onglet exact**

Le panneau gauche contient la liste des racines, boutons Ajouter/Retirer et `Scanner`. La zone
centrale contient recherche/filtres, synthèse compacte et `DataGrid` groupes. La zone droite affiche
une `ItemsControl` des fichiers avec la ligne d'identité, rôle, volume, chemin, blocages et choix de
revue. Le bouton Supprimer est bindé et porte :

```xml
<Button Content="Supprimer"
        Command="{Binding SupprimerCommand}"
        ToolTip="Exécution non disponible dans cette version"
        AutomationProperties.HelpText="Action visible mais désactivée" />
```

Le `DataGrid` utilise `EnableRowVirtualization="True"`, `EnableColumnVirtualization="True"`,
`VirtualizingPanel.IsVirtualizing="True"`, `VirtualizingPanel.VirtualizationMode="Recycling"` et
`ScrollViewer.CanContentScroll="True"`.

- [ ] **Step 5: Vérifier la tâche**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj --filter FullyQualifiedName~StructureXamlTests
dotnet build modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj -c Release
```

Expected: tests verts et XAML compilé.

---

### Task 8: Onglet Versions, composition réelle et validation de bout en bout

**Files:**
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/App.xaml.cs`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/MainWindow.xaml`
- Modify: `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/MainWindow.xaml.cs`
- Modify: `modules/duplicate-files/README.md`
- Modify: `modules/duplicate-files/docs/specs/2026-07-20-interface-desktop-duplicate-files-design.md`
- Test: `modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/MainViewModelTests.cs`

**Interfaces:**
- Consumes: tous les services et ViewModels précédents.
- Produces: application WPF utilisable et documentée.

- [ ] **Step 1: Ajouter les tests du démarrage et de l'onglet Versions**

```csharp
[Fact]
public void App_compose_le_ViewModel_sans_conteneur_externe()
{
    var source = File.ReadAllText(CheminSource("App.xaml.cs"));
    Assert.Contains("new StockageSessionUi", source);
    Assert.Contains("new ScannerBibliotheque", source);
    Assert.Contains("new AnalyseurBibliotheque", source);
    Assert.DoesNotContain("ServiceCollection", source);
}

[Fact]
public void Onglet_versions_expose_confiance_variante_et_revue_humaine()
{
    var xaml = File.ReadAllText(CheminSource("MainWindow.xaml"));
    Assert.Contains("Header=\"Versions apparentées\"", xaml);
    Assert.Contains("Binding Confiance", xaml);
    Assert.Contains("Binding Architecture", xaml);
    Assert.Contains("Revue humaine", xaml);
}
```

- [ ] **Step 2: Compléter l'onglet Versions et les diagnostics**

Ajouter la synthèse versionnée, les filtres confiance/revue, le tableau familles, le détail des
artefacts et preuves, ainsi qu'un bandeau pour `VersionsDisponibles=false`. Ajouter en bas de fenêtre
le statut de session, le nombre de fichiers traités et un panneau de diagnostics ouvrable.

- [ ] **Step 3: Composer l'application**

Retirer `StartupUri` de `App.xaml`. Dans `App.OnStartup`, créer manuellement
`StockageSessionUi`, `ValidateurRacines`,
`ScannerBibliotheque`, `AnalyseurBibliotheque`, les deux lecteurs de rapport et
`DialogueFichiersWpf`, injecter `DispatcherUiWpf`, puis créer et afficher une seule `MainWindow`
avec `DataContext = new MainViewModel(...)`.

Accepter les arguments optionnels :

```text
--session <fichier.session.json>
--db <base.db>
--json <rapport-doublons.json>
```

Un seul argument source est accepté. Il sert aux tests manuels et à l'ouverture Windows sans créer
une seconde logique de chargement.

- [ ] **Step 4: Mettre à jour la documentation**

Documenter dans `modules/duplicate-files/README.md` : construction, lancement, sources acceptées,
limite d'une racine par lecteur, session/archive et absence de suppression. Passer la spec au statut
`implémentée et vérifiée` seulement après les étapes suivantes.

- [ ] **Step 5: Exécuter les tests Desktop complets**

Run:

```powershell
dotnet test modules/duplicate-files/tests/InstallChecker.DuplicateFiles.Desktop.Tests/InstallChecker.DuplicateFiles.Desktop.Tests.csproj -c Release
```

Expected: zéro échec.

- [ ] **Step 6: Exécuter toute la solution**

Run:

```powershell
dotnet test InstallChecker.slnx -c Release --no-restore --filter "Category!=Performance"
```

Expected: tous les tests existants et Desktop réussissent.

- [ ] **Step 7: Vérifier le gel et la frontière UI**

Run:

```powershell
git diff -- src/InstallChecker.Identity src/InstallChecker.Identity.Access tests/InstallChecker.Identity.Tests tests/oracle docs/identity docs/conformite registre modules/scanner/src modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine
rg -n "File\.Delete|File\.Move|Recycle|Corbeille|SHFileOperation" modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop
git diff --check
```

Expected : aucun diff dans les zones gelées ou moteurs. Les seuls `File.Delete`/`File.Move` du projet
Desktop sont confinés à `StockageSessionUi` et ne reçoivent que des chemins de session/temporaire.
Aucune API de Corbeille ou mutation des fichiers analysés n'existe.

- [ ] **Step 8: Valider sur les artefacts réels sans perturber un scan actif**

Attendre que `test.db-journal` ait disparu avant toute ouverture de `test.db`. Lancer :

```powershell
dotnet run --project modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj -c Release -- --json rapport-doublons.json
```

Vérifier manuellement : 6 009 groupes, 8 579 candidats, défilement fluide, onglet Versions
indisponible, bouton Supprimer désactivé, aucun chevauchement à `1100x680` et `1920x1080` avec mise à
l'échelle Windows 100 % et 150 %. Après la fin du scan actif, ouvrir `test.db` et vérifier les deux
onglets sans lancer de rescan.

---

## Plan Self-Review

- **Spec coverage:** bibliothèque, multi-lecteur, DB, ancien JSON, deux rapports, session, archive
  unique, reprise des décisions, diagnostics, virtualisation, accessibilité et suppression inactive
  sont chacun couverts par une tâche.
- **Boundary check:** toutes les nouvelles classes vivent dans le projet Desktop ; les trois
  commandes existantes constituent l'unique accès fonctionnel.
- **Type consistency:** `SessionDuplicateFilesUi`, `ResultatScanUi`, `ResultatAnalyseUi`,
  `RapportDoublonsUi`, `RapportVersionsUi` et les signatures des services restent identiques entre
  tâches productrices et consommatrices.
- **No placeholders:** aucune étape ne demande une logique indéfinie ou un moteur futur.
- **Known limitation:** la vérification visuelle WPF reste manuelle ; les tests automatisés couvrent
  les ViewModels, contrats, projections et invariants XAML.
