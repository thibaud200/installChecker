# Module Scanner

Scanner observe des fichiers et produit un snapshot SQLite exploitable par Identity ou par un
module métier. Il ne connaît ni les doublons, ni les logiciels, ni les décisions de conservation.

## Organisation

```text
src/InstallChecker.Scanner.Core/       extracteurs et observations de fichiers
src/InstallChecker.Scanner/            cas d'usage scan et stockage SQLite
src/InstallChecker.Scanner.Observations/ projection SQLite v3 vers le port Omega
tests/InstallChecker.Scanner.Tests/    tests unitaires et d'intégration du module
tests/InstallChecker.Scanner.Observations.Tests/ tests du lecteur Omega v3
docs/mesures/                           campagnes de mesure historiques
```

Le coeur utilise uniquement les API .NET et Windows nécessaires à l'observation. L'enveloppe ouvre
le système de fichiers, orchestre les extracteurs et écrit une occurrence append-only par chemin et
par scan. Les observations brutes identiques sont mutualisées dans un snapshot immuable. Aucun
projet Scanner ne référence Duplicate Files.

## Utilisation

L'hôte CLI expose le module avec :

```text
installchecker scan <dossier> [--db <fichier>] [--json | --json-file <fichier>] [--ext <.ext,.ext>]
```

`--json` émet le scan validé sur la sortie standard. Une redirection remplace donc naturellement le
fichier cible :

```powershell
installchecker scan C:\Logiciels --db logiciels.db --json > scan.jsonl
```

`--json-file` écrit directement le même JSONL et remplace le contenu antérieur. Il ne peut pas être
combiné avec `--json` :

```powershell
installchecker scan C:\Logiciels --db logiciels.db --json-file scan.jsonl
```

Construction de l'exécutable :

```powershell
dotnet build apps/cli/InstallChecker.Cli/InstallChecker.Cli.csproj -c Release
```
