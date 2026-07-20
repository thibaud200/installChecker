# Stockage dédupliqué des observations du Scanner

**Statut** : accepté et implémenté le 2026-07-19.
**Périmètre** : module Scanner, stockage SQLite et export JSON du scan.
**Gel** : `InstallChecker.Identity` et `InstallChecker.Identity.Access` ne sont jamais modifiés.

---

## 1. Problème constaté

Le schéma v2 écrit une observation complète à chaque passage. Deux scans réussis du même fichier
inchangé créent donc deux lignes de cœur et deux jeux complets de capacités VersionInfo, PE,
Authenticode, MSI et Appx.

Ce comportement ne fabrique pas de doublon dans l'état courant : les lecteurs v2 sélectionnent
uniquement le dernier scan de chaque volume. Il augmente néanmoins inutilement la taille physique de
la base.

Un scan interrompu ne laisse actuellement aucune ligne SQLite, car tout le scan vit dans une seule
transaction. La relance recommence le travail, mais ne duplique pas de transaction partielle.

La sortie `--json` est un flux stdout, pas un fichier géré par le Scanner. Elle est actuellement
émise avant le commit SQLite ; une interruption peut donc laisser un JSONL partiel sans équivalent
validé dans la base.

## 2. Objectif

Lorsqu'un fichier produit exactement les mêmes observations brutes lors de plusieurs scans :

- stocker une seule fois le contenu logique de l'observation ;
- conserver une occurrence légère pour chaque scan dans lequel le fichier a été vu ;
- préserver l'historique des scans et l'état courant multi-volume ;
- restituer les mêmes observations Omega aux consommateurs ;
- ne modifier ni le moteur Identity, ni Identity.Access ;
- ne jamais dédupliquer sur le seul chemin ou sur le seul hash du fichier.

La déduplication est une optimisation de stockage, jamais une décision métier sur les doublons.

## 3. Alternatives étudiées

### A. Conserver le schéma v2

Avantage : aucun changement. Inconvénient : toutes les données sont recopiées à chaque scan. Cette
option ne répond pas au besoin.

### B. Ignorer seulement un rescan entièrement identique

Avantage : modification limitée. Inconvénient : dès qu'un seul fichier change, toutes les autres
observations sont encore recopiées. Cette optimisation est trop partielle.

### C. Séparer snapshot et occurrence de scan

Avantage : les données lourdes sont stockées une seule fois, tandis que l'historique reste exact.
Inconvénient : nouveau schéma et nouveau lecteur nécessaires. Cette option est retenue.

## 4. Modèle physique v3

```text
scans
  id
  volume_id
  volume_label
  root_path
  started_at
  extensions

observation_snapshots
  id
  snapshot_key UNIQUE
  extraction_contract
  canonical_payload
  size
  sha256

snapshot_version_info       snapshot_id UNIQUE, ...
snapshot_file_headers       snapshot_id UNIQUE, ...
snapshot_pe_info            snapshot_id UNIQUE, ...
snapshot_authenticode       snapshot_id UNIQUE, ...
snapshot_msi_properties     snapshot_id UNIQUE, ...
snapshot_appx_manifest      snapshot_id UNIQUE, ...

scan_entries
  id
  scan_id
  snapshot_id
  path
  path_key
  scanned_at
  UNIQUE(scan_id, path_key)
```

`observation_snapshots` et ses six tables de capacités portent l'observation réutilisable.
`scan_entries` porte exclusivement le contexte propre au passage : scan, chemin et date.

Deux chemins différents peuvent référencer le même snapshot. Deux scans différents du même chemin
inchangé peuvent également référencer le même snapshot. Un contenu ou une observation brute
différente produit un nouveau snapshot.

### 4.1 Exemple : deux copies à deux endroits

```text
observation_snapshots
  snapshot 42 : sha256=ABC..., taille=100, métadonnées=...

scan_entries
  entrée 101 : scan=7, chemin=C:\Téléchargements\outil.exe, snapshot=42
  entrée 102 : scan=7, chemin=D:\Archives\outil.exe,        snapshot=42
```

Le lecteur projette deux actes Omega distincts :

```text
acte 101 : empreinte ABC..., contexte C:\Téléchargements\outil.exe
acte 102 : empreinte ABC..., contexte D:\Archives\outil.exe
```

Le module Duplicate Files reçoit donc toujours deux fichiers et détecte un doublon exact de deux
exemplaires. Seules les valeurs lourdes communes sont mutualisées physiquement dans le snapshot.

À l'inverse, si le même chemin est rescanné plus tard sans changement, l'ancienne occurrence reste
dans l'historique mais le lecteur de l'état courant ne projette que celle du dernier scan du volume.
Les deux passages temporels ne deviennent donc jamais un faux doublon. Deux copies présentes sur
deux volumes distincts restent visibles, car l'état courant conserve le dernier scan de chaque
volume.

## 5. Clé de snapshot

La clé est un SHA-256 d'un encodage canonique à préfixes de longueur contenant :

1. la constante `scanner-observation/v1` ;
2. la taille et le SHA-256 du contenu ;
3. toutes les valeurs brutes des capacités, dans un ordre fixe ;
4. un marqueur distinct pour chaque valeur absente.

Le chemin, la date et l'identifiant du scan sont exclus. Une chaîne vide et une absence restent
distinctes. La constante versionne le contrat d'extraction : une évolution incompatible des
extracteurs utilisera une nouvelle constante et ne réutilisera pas silencieusement un ancien
snapshot.

La clé est une convention technique de stockage. Elle n'est ni une identité métier ni une entrée du
registre Identity.

## 6. Écriture

Pour chaque fichier observé :

1. calculer la clé du snapshot à partir des valeurs déjà extraites ;
2. insérer le snapshot et ses capacités seulement si la clé n'existe pas ;
3. relire son identifiant ;
4. insérer une `scan_entry` pour le passage courant.

Le scan reste une transaction atomique. Une interruption avant commit ne laisse ni scan, ni entrée,
ni snapshot orphelin. Il n'existe pas de checkpoint ou de reprise au milieu du répertoire dans ce
jalon : une relance recommence l'énumération, mais réutilise les snapshots déjà validés par des scans
antérieurs.

## 7. Lecture sans modification d'Identity

Le schéma v3 est lu par un adaptateur placé sous `modules/scanner`. Il implémente le port public
`IObservationsSource` et projette chaque `scan_entry` courante comme un `ActeObservation` :

- l'identifiant de l'acte est l'identifiant de `scan_entries` ;
- taille, hash et attributs proviennent du snapshot référencé ;
- chemin et date proviennent de l'entrée de scan ;
- l'état courant reste le dernier scan de chaque volume.

Une fabrique du module Scanner sélectionne :

- le lecteur gelé Identity.Access pour les bases v1 et v2 ;
- le lecteur Scanner pour les bases v3.

Les commandes `identity`, `duplicates`, `duplicates versions` et `plan` consomment cette fabrique.
Le moteur Identity, sa théorie, ses tests, son registre et Identity.Access restent inchangés.

## 8. Version et compatibilité

- Les nouvelles bases produites par Scanner utilisent `PRAGMA user_version = 3`.
- Scanner n'ajoute rien à une base v1 ou v2 : il demande explicitement une nouvelle base.
- Les bases v1 et v2 restent lisibles par toutes les commandes grâce au lecteur historique.
- Aucune migration automatique et aucune réécriture d'une base existante ne sont ajoutées.

Cette politique suit le principe actuel des bases de scan jetables tout en évitant toute ambiguïté
entre les deux structures physiques.

## 9. JSON

La projection JSON d'un scan n'est émise qu'après le commit SQLite et est reconstruite depuis les
entrées validées. Une interruption de l'analyse ne peut donc plus produire des lignes JSON pour une
transaction annulée.

`--json` reste un flux JSONL sur stdout pour compatibilité. Ce flux représente une exécution et ne
doit pas être concaténé avec `>>` pour construire un état courant.

Une option `--json-file <chemin>` écrit le snapshot JSONL de l'exécution en remplaçant le fichier
existant. Elle n'utilise jamais le mode append. `--json` et `--json-file` sont mutuellement exclusifs.

Deux scans peuvent donc produire deux exports séparés, mais un même fichier JSON géré par la
commande ne contient jamais deux exécutions concaténées.

## 10. Erreurs et invariants

- une collision de `snapshot_key` avec des valeurs différentes est une défaillance globale ;
- une capacité manquante pour un snapshot viole l'invariant 1:1 ;
- une `scan_entry` sans snapshot est refusée ;
- deux chemins Windows équivalents dans un même scan sont refusés ;
- aucune erreur locale ne valide une observation partielle ;
- aucune suppression de snapshot n'est réalisée dans ce jalon ;
- aucune logique métier Duplicate Files n'entre dans le Scanner.

## 11. Tests d'acceptation

- deux scans du même fichier inchangé créent un snapshot et deux entrées de scan ;
- deux chemins de même contenu et mêmes observations partagent un snapshot ;
- une valeur brute différente crée un nouveau snapshot ;
- l'état courant après deux scans contient une seule occurrence du fichier ;
- les DTO Omega v3 sont identiques aux observations produites avant normalisation physique ;
- les bases v1 et v2 restent lisibles ;
- Identity et Duplicate Files produisent les mêmes résultats sur un corpus logique équivalent ;
- un JSON n'est émis qu'après commit ;
- `--json-file` remplace le fichier et ne concatène jamais deux scans ;
- les zones gelées ne présentent aucun diff.

## 12. Limites

La déduplication réduit le stockage, pas le coût d'extraction : le fichier est encore lu et analysé
afin de prouver que son snapshot n'a pas changé. L'usage futur de taille et date de modification
comme cache rapide demandera une mesure et une spécification distinctes, car ces valeurs seules ne
prouvent pas l'identité du contenu.

## 13. Premier relevé de performance

Mesure séparée exécutée en `Release` le 2026-07-19 avec .NET 10.0.10 sur Windows
10.0.26200.0 (`DESKTOP-DK6H5L7`) :

- 10 scans de 10 000 chemins, soit 100 000 occurrences ;
- 10 000 observations brutes distinctes réutilisées entre les scans ;
- durée : 4,82 s ;
- allocations mesurées sur le thread : 639 695 640 octets ;
- taille du fichier SQLite : 20 393 984 octets ;
- cardinalités : 10 000 snapshots, 100 000 entrées, 10 000 lignes VersionInfo.

Ce relevé établit un point de comparaison, pas un seuil de conformité. Les temps, allocations et
tailles devront être comparés sur la même machine et avec le même scénario avant toute optimisation.
