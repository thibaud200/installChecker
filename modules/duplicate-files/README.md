# Module Duplicate Files — Commandes

Le module détecte les **copies exactes** de fichiers (même contenu, octet pour octet) et en produit
un rapport ou un plan de suppression revu par un humain.

**Ce qu'il fait :** identifier les groupes de fichiers identiques · restituer un rapport explicable ·
proposer les copies redondantes supprimables.
**Ce qu'il ne fait pas :** aucune suppression automatique · aucun regroupement de versions d'un même
logiciel · aucune modification des fichiers observés.

---

## Flux

```
scan  →  base Ω (SQLite)  ┬→  duplicates  (rapport des groupes)
                          └→  plan        (propositions de suppression)
```

Toute analyse consomme une base produite par `scan` **et** le registre de conventions `registre/`.

---

## Prérequis

- Windows, SDK .NET 10.
- Le registre `registre/` présent à la racine du dépôt.
- L'exécutable, obtenu par :

```
dotnet build src/InstallChecker/InstallChecker.csproj -c Release
```

→ `src/InstallChecker/bin/Release/net10.0/InstallChecker.exe` (noté `installchecker` ci-dessous).

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
      "Exemplaires": [
        {
          "Fichier": { "Chemin": "C:\\...\\notepad++.exe", "Taille": 8444400 },
          "Rang": 1,
          "Etiquette": "à conserver",
          "Motif": "richesse=2/3, nomDeCopie=False, ..."
        }
      ]
    }
  ]
}
```

> Chaque `Fichier` porte aussi `ActeId`, `SignatureAuthenticodePresente`, `EstUnPeLisible`,
> `PresenceMetadonneesMsi`, `DateDObservation`, `VolumeId`, `VolumeLabel` (abrégés ci-dessus).

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
  "Propositions": [
    {
      "Contenu": "db1131b5060bcfad80fc21d7bd333d9a7de3eee5190c5586d0a3a096fd563b87",
      "Chemin": "C:\\Windows\\System32\\notepad.exe"
    }
  ]
}
```

Garanties : pour chaque contenu, **au moins une copie n'est jamais proposée** ; un chemin protégé
n'est **jamais** proposé ; le plan **n'exécute rien**.

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

En cas d'erreur, le message est écrit sur **stderr** et **aucune** sortie partielle n'est produite sur
stdout.

---

## Limites actuelles

- La protection des chemins est **implémentée dans le plan** mais n'est **alimentée par aucune liste**
  de répertoires protégés (A1) : un ensemble vide lui est passé, donc des chemins système peuvent
  apparaître dans un plan.
