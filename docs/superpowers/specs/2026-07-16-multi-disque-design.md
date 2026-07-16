# Module Duplicate Files — Gestion multi-disque (conception)

**Statut** : issu d'une session de brainstorming (superpowers:brainstorming), aucune implémentation engagée.
**Date** : 2026-07-16
**Sources consultées** : `docs/projet/ameliorations-duplicate-files-v1.md` (A1, A2, A4), `docs/projet/duplicate-files.md`, `CLAUDE.md` racine (ADR-002, ADR-006, ADR-008), `src/InstallChecker/ObservationStore.cs`, `src/InstallChecker/DuplicatesCommand.cs`, `src/InstallChecker.DuplicateFiles/*`.
**Nature de ce document** : conception du chantier ouvert par **A4** — le « snapshot courant » multi-disque. Il fixe le *quoi* et le *pourquoi* ; les noms de tables et de colonnes sont donnés parce qu'ils font partie du contrat de stockage (ADR-002/ADR-008), pas comme plan d'implémentation.

---

## 1. Constat structurant (avant toute décision)

A4 a déjà tranché le principe : **une base unique pour tous les disques**, parce qu'un doublon peut exister *entre* volumes et qu'une base unique le détecte sans déduplication inter-bases. Le chantier restant est nommé par A4 lui-même : avec des rescans, l'append-only (ADR-002) empile les observations d'un même fichier → **faux doublons entre deux scans**. Il faut une notion de **snapshot courant**, appliquée **à la source des observations (Ω) avant toute dérivation**, afin que tous les consommateurs (identity, duplicates, plan) partagent le même état courant, Ω restant append-only.

Le parc visé est mixte : disques internes fixes, disques externes USB (lettres de lecteur potentiellement changeantes d'un branchement à l'autre), NAS et partages réseau (chemins UNC ou lecteurs mappés). Les scans se font tantôt disque par disque à des moments différents, tantôt tous ensemble — la base doit gérer les deux sans mode particulier.

---

## 2. Journal des décisions

Chaque décision suit le format : Contexte → Décision → Alternatives écartées → Conséquences.

### D1 — L'état courant est résolu à la lecture, dans le lecteur Ω

- **Contexte** : A4 exige que le snapshot courant s'applique à la source de Ω, avant toute dérivation ; ADR-002 interdit UPDATE/DELETE.
- **Décision** : le filtre « état courant » vit dans `LecteurDObservationsSqlite` (l'adaptateur C1/C2 côté Access). Il retient, **pour chaque volume, le dernier scan de ce volume**, et ne charge que les observations de ces scans. Aucun consommateur ne change : `identity`, `duplicates` et `plan` reçoivent mécaniquement le même état courant.
- **Alternatives écartées** :
  - filtrage dans chaque module consommateur (rejeté — contredit frontalement A4 ; trois implémentations du même filtre qui divergeraient) ;
  - marquage à l'écriture (flag « courant » mis à jour par le nouveau scan) (rejeté — UPDATE interdit par ADR-002 ; perdrait la possibilité de rejouer un état passé).
- **Conséquences** : l'historique complet reste dans la base ; rejouer un état passé reste possible, simplement non exposé dans cette version. Le sens de « courant » est défini en un seul endroit.

### D2 — Un scan est une entité de la base : table `scans`, schéma v2

- **Contexte** : pour retenir « le dernier scan par volume », la base doit savoir ce qu'est un scan — aujourd'hui seules les observations existent, sans regroupement.
- **Décision** : schéma **v2** (`PRAGMA user_version = 2`). Nouvelle table append-only :

  ```sql
  CREATE TABLE scans (
      id           INTEGER PRIMARY KEY AUTOINCREMENT,
      volume_id    TEXT NOT NULL,   -- identité du volume (D3)
      volume_label TEXT,            -- étiquette observée du volume, NULL si absente
      root_path    TEXT NOT NULL,   -- racine passée au scan, telle quelle
      started_at   TEXT NOT NULL,
      extensions   TEXT             -- filtre --ext tel que passé, NULL si aucun
  );
  ```

  `scan_observations` gagne une colonne `scan_id INTEGER NOT NULL`. Un scan = une ligne `scans` + ses observations ; jamais d'UPDATE. `root_path` et `extensions` sont conservés pour que l'éviction d'un état précédent soit toujours **explicable** (règle « toute décision doit être explicable »).
- **Alternatives écartées** :
  - déduire le scan de `scanned_at` sans table (rejeté — fragile : deux scans peuvent se chevaucher dans le temps, et l'identité du volume n'aurait nulle part où vivre) ;
  - migration des bases v1 (rejeté — ADR-008 : aucune migration, les bases existantes sont jetables ; une base v1 est rejetée avec l'erreur `user_version` existante, on rescanne).
- **Conséquences** : bump v1→v2, rescans nécessaires ; le mécanisme de version existant couvre le refus des bases anciennes sans code nouveau.

### D3 — Identité de volume observée automatiquement (numéro de série, UNC normalisé)

- **Contexte** : les lettres des disques USB changent d'un branchement à l'autre ; le remplacement « dernier scan par volume » exige de reconnaître le même disque physique. La garde A1 n'autorise au niveau du scan que des **métadonnées du système de fichiers** — l'identité de volume en est une.
- **Décision** : `volume_id` est résolu au démarrage du scan, sans intervention de l'utilisateur :
  - chemin UNC (`\\serveur\partage\...`) → racine UNC normalisée en minuscules (`\\serveur\partage`) ;
  - lettre mappée réseau → résolue en UNC (`WNetGetConnection`), puis règle ci-dessus — le même partage scanné via `Z:` ou via UNC est le même volume ;
  - lettre locale → numéro de série du volume via `GetVolumeInformationW` (P/Invoke, même style qu'ADR-006), formaté en hexadécimal.

  Si l'identité est irrésoluble, le scan **refuse de démarrer** avec une erreur explicite.
- **Alternatives écartées** :
  - nom de source déclaré par l'utilisateur (`--source "NAS-archives"`) (écarté — repose sur la discipline de l'utilisateur : deux noms différents = deux disques fantômes) ;
  - mixte série + nom optionnel (écarté — deux identités concurrentes pour le même volume, surface d'incohérence sans besoin démontré) ;
  - fallback silencieux quand l'identité est irrésoluble (rejeté — un volume mal identifié corromprait le remplacement ; pas de boîte noire).
- **Conséquences** : aucun paramètre nouveau côté utilisateur ; reformater un volume change son numéro de série, donc l'ancien état sort du courant au premier rescan — comportement accepté (c'est physiquement un autre volume).

### D4 — Le remplacement porte sur le volume entier

- **Contexte** : il faut définir ce qu'un nouveau scan fait sortir de l'état courant. Décision prise explicitement au cadrage (2026-07-16).
- **Décision** : **tout scan sur un volume remplace tout l'état courant de ce volume** — quelle que soit la racine scannée ou le filtre `--ext` employé. « Remplacer » signifie seulement sortir de l'état courant ; rien n'est effacé (ADR-002).
- **Alternatives écartées** :
  - remplacement par racine scannée (couple volume + `root_path`) (écarté au cadrage — l'utilisateur scanne des disques entiers ; la granularité par racine ajoute des états partiels à raisonner sans besoin réel) ;
  - choix explicite à chaque scan (écarté — surface d'options et de tests sans besoin démontré).
- **Conséquences assumées** : un scan partiel (sous-dossier, ou `--ext` étroit) fait disparaître de l'état courant les fichiers du volume non re-scannés — `root_path` et `extensions` sur la ligne `scans` rendent cette éviction explicable. Un disque non rebranché depuis des mois garde son dernier état dans le courant (voulu : disques d'archive).

### D5 — Restitution : le volume accompagne chaque exemplaire

- **Contexte** : avec des lettres changeantes, le chemin observé ne suffit plus à dire *sur quel disque physique* se trouve une copie.
- **Décision** : chaque exemplaire du rapport `duplicates` est enrichi de `volume_id` et `volume_label` (jointure `scan_id → scans`, aucune valeur recalculée). Les groupes traversent naturellement les volumes : même SHA-256 sur deux disques = un groupe — c'est l'intérêt de la base unique (A4).
- **Alternatives écartées** : synthèse par volume (comptes, tailles par disque) (non retenue — aucun besoin démontré ; ajout trivial plus tard).
- **Conséquences** : la commande `plan` est inchangée dans sa logique ; ses garanties tiennent telles quelles (au moins une copie par contenu, chemins protégés exclus), les chemins proposés pouvant désormais s'étaler sur plusieurs volumes.

---

## 3. Hors périmètre (nommé, non conçu)

- **Garde « volume actuellement connecté » dans `plan`** : aucune suppression n'est exécutée dans cette version ; cette garde est un prérequis du chantier exécution, pas de celui-ci.
- **Exposition d'états passés** (rejouer le monde d'il y a deux scans) : l'historique est dans la base, l'exposition attendra un besoin réel.
- **Synthèse par volume** dans le rapport (cf. D5).
- **Points de montage NTFS** : un scan dont la traversée franchit un point de montage attribue les fichiers rencontrés au volume de la racine scannée. Cas jugé marginal pour le parc visé ; à traiter si l'usage le rencontre.

---

## 4. Erreurs

- Identité de volume irrésoluble → erreur explicite au démarrage du scan, aucun fallback (D3).
- Base v1 ouverte par le binaire v2 → erreur `user_version` existante, inchangée (D2).
- Aucun autre cas nouveau : une base sans table `scans` est nécessairement une base v1, couverte ci-dessus.

---

## 5. Tests attendus

- **Lecteur Ω / état courant** : deux scans du même volume → seules les observations du dernier sont lues ; deux volumes scannés à des dates différentes → union des derniers scans de chacun ; base sans scan → Ω vide.
- **Identité de volume** : normalisation UNC (casse, slash final) ; lettre locale → série hexadécimale ; erreur explicite si irrésoluble.
- **Bout en bout** : scan D + scan NAS + rescan D → le rapport `duplicates` combine dernier-scan-de-D + dernier-scan-du-NAS, sans faux doublon entre les deux scans de D.
- **Rapport** : `volume_id`/`volume_label` des exemplaires fidèles aux lignes `scans`.
- **Schéma** : base v1 rejetée explicitement par le binaire v2.
