# Module Duplicate Files — Commandes

Le module détecte les **copies exactes** de fichiers (même contenu, octet pour octet) et les
**versions antérieures candidates** d'une même famille de fichiers.

**Ce qu'il fait :** identifier les groupes de fichiers identiques · comparer des versions étayées
par le nom et les métadonnées · restituer des rapports explicables · proposer les copies exactes
redondantes supprimables.
**Ce qu'il ne fait pas :** aucune suppression automatique · aucune proposition de suppression pour
les versions antérieures · aucune modification des fichiers observés.

---

## Flux

```
scan  →  base Ω (SQLite)  ┬→  duplicates           (copies exactes)
                          ├→  duplicates versions  (versions antérieures candidates)
                          └→  plan → plan.json → plan verify → rapport de simulation
```

Toute analyse consomme une base produite par `scan`. Les flux historiques `duplicates` et `plan`
consomment aussi le registre de conventions `registre/` ; `duplicates versions` n'en a pas besoin.

---

## Organisation

```text
src/InstallChecker.DuplicateFiles.Engine/       moteur métier et DTO structurés
src/InstallChecker.DuplicateFiles/              cas d'usage, commandes et adaptateurs
src/InstallChecker.DuplicateFiles.Desktop/      interface WPF du module
tests/InstallChecker.DuplicateFiles.Engine.Tests/
tests/InstallChecker.DuplicateFiles.Tests/
tests/InstallChecker.DuplicateFiles.Desktop.Tests/
docs/                                           spécifications, plans et revues du module
registre-metier/                                politiques versionnées du module
```

Le moteur métier dépend uniquement du contrat public d'Identity. L'enveloppe compose ce moteur avec
les adaptateurs existants ; la CLI ne porte aucune règle de doublon.

---

## Prérequis

- Windows, SDK .NET 10.
- Le registre `registre/` présent à la racine du dépôt.
- L'exécutable, obtenu par :

```
dotnet build apps/cli/InstallChecker.Cli/InstallChecker.Cli.csproj -c Release
```

→ `apps/cli/InstallChecker.Cli/bin/Release/net10.0/InstallChecker.exe` (noté `installchecker` ci-dessous).

---

## Interface desktop

L'application WPF permet de choisir les dossiers à scanner, d'ouvrir une base existante ou un
rapport JSON, puis de consulter séparément les doublons exacts et les versions apparentées.

```powershell
dotnet build modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj -c Release
dotnet run --project modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj -c Release
```

Une source peut être ouverte au démarrage :

```powershell
dotnet run --project modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj -c Release -- --session bibliotheque.session.json
dotnet run --project modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj -c Release -- --db test.db
dotnet run --project modules/duplicate-files/src/InstallChecker.DuplicateFiles.Desktop/InstallChecker.DuplicateFiles.Desktop.csproj -c Release -- --json rapport-doublons.json
```

Le scan produit toujours les deux analyses. Plusieurs lecteurs peuvent partager la même base, avec
une seule racine indépendante par volume dans cette version ; un sous-dossier déjà couvert est
fusionné avec sa racine. La session JSON conserve les rapports, les filtres et les décisions de
revue. Après un rescan réussi, la session précédente est conservée dans une unique archive
`.previous.json`.

Le bouton **Supprimer** est visible pour matérialiser le workflow futur, mais reste désactivé.
L'application ne supprime, ne déplace et ne met à la Corbeille aucun fichier analysé.

---

## Commandes

### `scan` — construire la base d'observations

Parcourt récursivement un dossier et enregistre une observation par fichier dans une base SQLite.

```
installchecker scan <dossier> [--db <fichier>] [--json] [--ext <.ext,.ext>]
```

| Option | Effet |
|---|---|
| `--db <fichier>` | Base cible (défaut : `installchecker.db`). |
| `--json` | Sortie en JSON Lines au lieu du TSV par défaut. |
| `--ext <liste>` | Ne scanne que ces extensions (`exe,msi` ou `.exe,.msi`), insensible à la casse. |

La base est **append-only** : chaque scan y ajoute une ligne `scans` et ses observations, rien n'est
jamais effacé. Les analyses ne voient que l'**état courant** (le dernier scan de chaque volume, voir
« Multi-disque ») : re-scanner un dossier remplace l'état de son volume sans qu'il faille supprimer
la base.

```
del test.db
installchecker scan "C:\Program Files" --db test.db --ext exe,msi,msix
```

Sortie (stdout, une ligne par fichier — `chemin  taille  sha256`) :

```
C:\Program Files\Notepad++\notepad++.exe	8444400	4fb7ba1c4a712c1007b08a0cb99b27c8d904957b9ee809f3518c27167ed9461f
```

Résumé (stderr) :

```
Scan terminé : 16384 fichier(s), 495 erreur(s) locale(s). Base : test.db
```

Les erreurs locales (fichier verrouillé, accès refusé) sont isolées par fichier : le scan continue,
le fichier concerné n'a simplement aucune observation.

### `duplicates` — rapport des doublons

Restitue les groupes de fichiers identiques, avec le classement suggéré et les métriques d'espace.

```
installchecker duplicates <base.db> <registre>
```

```
installchecker duplicates test.db registre > rapport.json
```

Sortie (stdout, JSON) :

```json
{
  "VersionContrat": "duplicate-files/exact-duplicates/v1",
  "Synthese": {
    "NombreDeGroupes": 4083,
    "NombreDeFichiersRedondants": 7765,
    "EspaceRecuperableOctets": 33841151856,
    "NombreDeFichiersAConserver": 4083,
    "NombreDeCandidatsASuppression": 7765
  },
  "Note": null,
  "Groupes": [
    {
      "Domaine": [5, 1123, 3106],
      "MotifCourt": "unique-maximale (CE-01 v1, niveau Certaine)",
      "TailleUnitaire": 8444400,
      "EspaceRecuperableOctets": 16888800,
      "GroupeId": "exact:sha256:4fb7ba1c4a712c1007b08a0cb99b27c8d904957b9ee809f3518c27167ed9461f",
      "Categorie": "ExactDuplicate",
      "Confiance": "Certaine",
      "ContenuSha256": "4fb7ba1c4a712c1007b08a0cb99b27c8d904957b9ee809f3518c27167ed9461f",
      "Preuves": [
        {
          "Type": "Sha256Identique",
          "Valeur": "4fb7ba1c4a712c1007b08a0cb99b27c8d904957b9ee809f3518c27167ed9461f"
        }
      ],
      "FichierRecommandeId": "file:sha256:...",
      "Exemplaires": [
        {
          "Fichier": { "Chemin": "C:\\...\\notepad++.exe", "Taille": 8444400 },
          "Rang": 1,
          "Etiquette": "à conserver",
          "Motif": "richesse=2/3, nomDeCopie=False, ...",
          "FichierId": "file:sha256:...",
          "Role": "RecommandeAConserver",
          "CriteresClassement": [
            { "Critere": "RichesseObservations", "Priorite": 1, "Valeur": "2/3" }
          ],
          "Actions": [
            { "Action": "Conserver", "Autorisee": true, "Blocages": [] },
            {
              "Action": "AjouterAuPlanDeSuppression",
              "Autorisee": false,
              "Blocages": ["FichierRecommandeAConserver"]
            }
          ]
        }
      ]
    }
  ]
}
```

> Chaque `Fichier` porte aussi `ActeId`, `SignatureAuthenticodePresente`, `EstUnPeLisible`,
> `PresenceMetadonneesMsi`, `DateDObservation`, `VolumeId`, `VolumeLabel` (abrégés ci-dessus).

`VersionContrat` versionne la structure JSON publique. Elle ne représente ni la version du fichier
ni celle du logiciel.

### `duplicates versions` — rapport des versions antérieures candidates

Regroupe des contenus différents lorsqu'ils appartiennent à la même famille, portent des versions
comparables et ont la même variante observée.

```
installchecker duplicates versions <base.db>
```

```
installchecker duplicates versions test.db > versions.json
```

Le contrat JSON est `duplicate-files/version-redundancy/v1`. Chaque groupe expose la version de
référence la plus récente, les contenus classés `ReferenceRecente` ou `VersionAnterieure`, les
preuves utilisées, les diagnostics et les exclusions agrégées. Les actions possibles sont
uniquement `Examiner` et `Ignorer` : la suppression automatique est toujours bloquée.

La famille et la version sont arbitrées à partir de plusieurs preuves : nom du fichier,
`VersionInfo`, propriétés MSI, manifeste Appx/MSIX, architecture PE et signature Authenticode. Les
versions numériques de un à quatre composants et les dates `AAAA-MM-JJ` sont comparées séparément.
Une contradiction entre le nom et une métadonnée structurée exclut le contenu du regroupement.

Les variantes connues d'architecture, de langue, d'édition, de distribution et de format sont
séparées. Pour les formats où l'architecture est pertinente (`exe`, MSI, Appx/MSIX), deux
architectures différentes ou une architecture connue face à une architecture absente ne sont pas
comparées. Deux MSI dont l'architecture est absente restent candidats, avec confiance réduite,
`VarianteNonObservee` et revue humaine obligatoire.

Les niveaux de confiance sont `Faible`, `Moyenne` et `Forte`. Même au niveau `Forte`, ce rapport ne
constitue jamais une autorisation de suppression ou de déplacement.

### `plan` — plan de suppression

Produit la liste des copies redondantes supprimables. Le plan est **soumis à l'humain** : rien n'est
exécuté.

```
installchecker plan <base.db> <registre>
```

```
installchecker plan test.db registre > plan.json
```

Sortie (stdout, JSON) — une liste plate de propositions, chacune étiquetée de l'empreinte du contenu :

```json
{
  "VersionContrat": "duplicate-files/safe-plan/v1",
  "Propositions": [
    {
      "Contenu": "db1131b5060bcfad80fc21d7bd333d9a7de3eee5190c5586d0a3a096fd563b87",
      "Chemin": "C:\\Users\\alice\\Downloads\\notepad - Copy.exe",
      "GroupeId": "exact:sha256:db1131b5060bcfad80fc21d7bd333d9a7de3eee5190c5586d0a3a096fd563b87",
      "FichierId": "file:sha256:..."
    }
  ],
  "GarantiesParGroupe": [
    {
      "GroupeId": "exact:sha256:db1131b5060bcfad80fc21d7bd333d9a7de3eee5190c5586d0a3a096fd563b87",
      "ContenuSha256": "db1131b5060bcfad80fc21d7bd333d9a7de3eee5190c5586d0a3a096fd563b87",
      "TemoinConservation": {
        "FichierId": "file:sha256:...",
        "Chemin": "C:\\Archives\\notepad.exe"
      }
    }
  ]
}
```

Garanties : pour chaque contenu, **au moins une copie n'est jamais proposée** ; un chemin protégé
n'est **jamais** proposé ; le plan **n'exécute rien**. Pour un même contenu au même chemin,
`GroupeId` et `FichierId` sont identiques dans le rapport et dans le plan.

### `plan verify` — vérifier et simuler le plan

Relit un plan autosuffisant, vérifie le témoin et chaque candidat dans l'état courant du disque,
puis produit un rapport structuré. La base SQLite et le registre ne sont pas nécessaires.

```
installchecker plan verify plan.json > verification.json
```

Sortie abrégée :

```json
{
  "VersionContrat": "duplicate-files/safe-plan-verification/v1",
  "Mode": "Simulation",
  "Executable": true,
  "Groupes": [
    {
      "GroupeId": "exact:sha256:...",
      "Executable": true,
      "Blocages": [],
      "Fichiers": [
        {
          "FichierId": "file:sha256:...",
          "Role": "TemoinConservation",
          "Etat": "Valide",
          "HashObserve": "db1131b5060bcfad80fc21d7bd333d9a7de3eee5190c5586d0a3a096fd563b87"
        }
      ]
    }
  ],
  "Journal": [
    {
      "Sequence": 1,
      "Etape": "VerifierTemoin",
      "Etat": "Valide"
    }
  ]
}
```

Les états possibles sont `Valide`, `Absent`, `Illisible`, `HashDifferent`, `CheminProtege` et
`TypeNonPrisEnCharge`. Un témoin ou un candidat non valide bloque le groupe.

Une simulation réussie ne modifie aucun fichier et ne constitue pas une autorisation différée. Le
hash devra être revérifié immédiatement avant une future mise à la Corbeille Windows. Aucune mise à
la Corbeille n'est implémentée dans ce jalon.

---

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

---

## Codes de sortie

| Code | Signification |
|---|---|
| `0` | Succès (le `scan` retourne `0` même avec des erreurs locales par fichier). |
| `1` | Erreur contractuelle : base ou registre absent / invalide, racine introuvable. |
| `2` | Usage incorrect (arguments manquants ou inconnus). |
| `3` | `plan verify` terminé, rapport produit, mais simulation bloquée. |

En cas d'erreur, le message est écrit sur **stderr** et **aucune** sortie partielle n'est produite sur
stdout.

---

## Limites actuelles

- La redondance versionnée F1 accepte les versions numériques stables et calendaires. Les versions
  préliminaires (`beta`, `rc`, etc.), les catalogues externes et la recherche de dernière version
  publiée restent hors de ce jalon.
- Le plan ne supprime et ne déplace aucun fichier : il produit seulement des propositions à revoir.
- Les chemins système Windows et Program Files sont protégés par défaut et ne peuvent pas apparaître
  dans les propositions.
