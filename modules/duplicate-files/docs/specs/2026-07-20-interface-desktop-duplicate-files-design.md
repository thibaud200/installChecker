# Interface desktop du module Duplicate Files

**Statut** : implémentée et vérifiée le 2026-07-20.
**Périmètre** : application WPF du module Duplicate Files, orchestration des commandes existantes,
consultation DB/JSON et persistance d'une session de revue.
**Gel** : aucune modification d'Identity, Identity.Access, du Scanner ou des moteurs Duplicate Files
n'est nécessaire à ce jalon.

---

## 1. Objectif

Fournir une application Windows utilisable sans ligne de commande pour :

- définir une bibliothèque composée de dossiers situés sur plusieurs lecteurs ;
- lancer les scans existants vers une base SQLite unique ;
- consulter les doublons exacts et les versions apparentées ;
- ouvrir une base existante ou un rapport JSON historique ;
- reprendre ultérieurement le tri et les décisions de revue ;
- préparer visuellement les actions futures sans supprimer ni déplacer aucun fichier.

L'interface est un consommateur. Elle n'ajoute aucune règle d'identification, de classement, de
version, de rétention ou de sécurité.

## 2. Hors périmètre

- suppression, mise à la Corbeille, copie ou déplacement de fichiers ;
- activation indirecte d'une API destructive ;
- multi-racine indépendante sur un même lecteur ;
- modification des schémas SQLite ou des lecteurs Omega ;
- réécriture du Scanner, des extracteurs ou des moteurs Duplicate Files ;
- migration du rapport historique vers le contrat métier courant ;
- annulation d'un scan en cours, non exposée par l'API actuelle ;
- interface globale pour les futurs modules de la plateforme.

Le bouton de suppression est visible pour matérialiser le flux futur, mais il est désactivé avec un
motif explicite. Il n'est relié à aucune commande ni interface de mutation.

## 3. Socle technique et localisation

L'application utilise WPF sur .NET 10 Windows, sans framework UI externe. Elle vit entièrement sous
`modules/duplicate-files` :

```text
modules/duplicate-files/
  src/InstallChecker.DuplicateFiles.Desktop/
  tests/InstallChecker.DuplicateFiles.Desktop.Tests/
```

Le projet Desktop référence uniquement les enveloppes publiques existantes nécessaires : Scanner,
Duplicate Files et leurs DTO. Il ne référence jamais les couches internes d'Identity.

Une architecture MVVM légère sépare :

- les vues XAML ;
- les ViewModels et l'état de navigation ;
- les adaptateurs vers les commandes existantes ;
- le stockage du fichier de session ;
- les modèles de présentation propres à l'UI.

## 4. Réutilisation stricte de l'existant

L'interface appelle en arrière-plan :

- `ScanCommand.Run` pour chaque racine sélectionnée ;
- `DuplicatesCommand.Deriver` pour le rapport de doublons exacts ;
- `RedondanceVersionneeCommand.Deriver` pour le rapport de versions apparentées.

Les sorties JSON de ces commandes sont désérialisées vers des modèles de présentation. L'UI ne
reproduit aucun calcul métier et n'interprète pas les observations brutes.

Un `TextWriter` appartenant à l'UI compte les lignes TSV émises par le Scanner et publie le chemin
courant. Cela donne un compteur de fichiers sans modifier le Scanner et sans précompter le corpus.

Les appels sont exécutés hors du thread graphique. L'interface reste réactive, mais la V1
n'affiche pas de bouton Annuler puisque le Scanner ne porte aucun contrat d'annulation.

## 5. Bibliothèque et racines de scan

Une bibliothèque mémorise :

- un nom libre ;
- le chemin de la base SQLite globale ;
- le chemin du registre utilisé par le rapport exact ;
- les racines sélectionnées ;
- le chemin du fichier de session courant.

La V1 accepte plusieurs racines uniquement si elles appartiennent à des lecteurs différents.

Avant le scan, l'UI :

1. résout l'identité de volume avec la capacité Scanner existante ;
2. supprime une racine imbriquée déjà couverte par une racine parente ;
3. refuse deux racines indépendantes portant le même `VolumeId` ;
4. explique que ce cas sera traité par une évolution ultérieure du Scanner.

Les racines valides sont scannées séquentiellement vers la même base. Le mécanisme actuel du
dernier scan par volume conserve ainsi un état par lecteur et permet la détection entre lecteurs.

Les scans de plusieurs lecteurs ne forment pas une transaction globale : chaque appel existant
valide sa propre transaction. Si un lecteur échoue après qu'un autre a réussi, l'UI signale un scan
partiel, ne tourne pas l'archive de session et propose de recalculer les vues depuis la base.

## 6. Sources ouvertes

### 6.1 Base SQLite

Une base compatible permet de recalculer automatiquement :

- le rapport des doublons exacts ;
- le rapport des versions apparentées.

La base `test.db` à la racine du dépôt sert d'artefact réel de validation après la fin de son scan.
Elle n'est jamais modifiée ou ouverte de force lorsqu'elle est occupée par un autre processus.

### 6.2 Rapport JSON métier

Le fichier `rapport-doublons.json` à la racine est un rapport historique de doublons exacts. Il est
accepté en lecture seule même en l'absence de `VersionContrat`.

Avec ce fichier seul :

- l'onglet Doublons est disponible ;
- l'onglet Versions indique que la source ne contient pas ce rapport ;
- aucune donnée absente n'est inventée ;
- le premier choix de revue propose la création d'une session courante.

Les rapports courants versionnés sont également acceptés. Aucun rapport JSON séparé n'est créé
automatiquement pour les versions : les deux rapports vivent dans la session UI.

## 7. Fichier de session

Le fichier de session est la persistance fonctionnelle de l'interface. Il contient :

- `VersionContrat` propre à l'UI ;
- la définition de la bibliothèque ;
- l'heure et le résultat du dernier scan ;
- le rapport exact JSON tel que produit par le module ;
- le rapport de versions JSON tel que produit par le module ;
- les filtres et tris utiles à la reprise ;
- les décisions de revue indexées par `GroupeId` et `FichierId` stables ;
- les diagnostics du dernier traitement.

Les états de revue initiaux sont :

- `AExaminer` ;
- `Conserver` ;
- `Prevoir` pour une future action, sans exécution ;
- `Ignorer`.

Après un rescan, les décisions dont les identifiants stables existent encore sont reprises. Les
décisions sans correspondant sont conservées comme éléments devenus introuvables et signalées dans
la synthèse de reprise.

### 7.1 Écriture atomique et archive unique

La session courante et son archive utilisent des noms déterministes, par exemple :

```text
bibliotheque.duplicate-files.session.json
bibliotheque.duplicate-files.session.previous.json
```

Après un scan et deux analyses réussis :

1. le nouveau document est écrit dans un fichier temporaire du même répertoire ;
2. le JSON complet est validé et le flux est fermé ;
3. l'ancienne archive est remplacée par la session courante ;
4. le temporaire devient la nouvelle session courante.

Il n'existe jamais plus d'une archive. Une erreur avant la rotation conserve la session courante et
l'archive précédentes. Les changements de revue réécrivent atomiquement la session courante sans
faire tourner l'archive ; la rotation est réservée aux rescans réussis.

## 8. Organisation de l'interface

L'application ouvre directement l'espace de travail :

```text
┌ Bibliothèque ─ Base ─ Ouvrir ─ Importer ─ Exporter ┐
├────────────────────┬────────────────────────────────┤
│ Racines et scan    │ Doublons exacts | Versions    │
│                    ├────────────────┬───────────────┤
│ C:\                │ Groupes        │ Détails       │
│ D:\                │ virtualisés    │ fichiers,     │
│                    │                │ preuves, rôle │
│ Ajouter / Retirer  │                │ et actions    │
│ Lancer le scan     │                │               │
├────────────────────┴────────────────┴───────────────┤
│ Session · dernier scan · fichiers · diagnostics    │
└─────────────────────────────────────────────────────┘
```

### 8.1 Doublons exacts

- synthèse : groupes, fichiers redondants, espace récupérable ;
- recherche par chemin ;
- filtres par état de revue et blocage ;
- tableau virtualisé des groupes ;
- détail des exemplaires, du fichier recommandé, des candidats et des volumes ;
- preuve SHA-256 et critères de classement ;
- états `Conserver`, `Prevoir` et `Ignorer` ;
- bouton Supprimer visible, désactivé et accompagné du motif.

### 8.2 Versions apparentées

- familles et artefacts détectés ;
- versions, fournisseur, architecture, langue et confiance ;
- filtres par confiance et revue humaine obligatoire ;
- preuves ayant conduit au regroupement ;
- états `AExaminer`, `Conserver` et `Ignorer` ;
- aucune proposition de suppression.

### 8.3 Diagnostics et états vides

- base absente ou incompatible ;
- base temporairement occupée ;
- rapport historique sans vue Versions ;
- scan partiel entre plusieurs lecteurs ;
- erreurs locales par fichier ;
- session chargée dont la base a été déplacée ;
- aucun doublon ou aucune famille versionnée.

Chaque état indique l'action possible sans exposer de détail interne ou de trace brute par défaut.

## 9. Direction visuelle et ergonomie

L'interface est claire, dense et utilitaire. Elle utilise :

- fond gris froid `#F4F6F8` ;
- surfaces blanches `#FFFFFF` ;
- texte principal `#17212B` ;
- turquoise conservation `#147D78` ;
- ambre revue `#B56A00` ;
- rouge blocage `#B42318` ;
- bleu de sélection `#2563EB`.

`Segoe UI Variable` porte l'interface et `Cascadia Mono`, avec repli `Consolas`, les hashes et les
versions. Les listes et tableaux ont des dimensions stables et la virtualisation est activée.

Une ligne d'identité relie visuellement les exemplaires d'un même groupe exact dans le panneau de
détail. Elle encode la preuve de contenu commun et ne constitue pas une décoration indépendante.

La navigation clavier, les focus visibles, les contrastes et les lecteurs d'écran sont couverts.
Les libellés restent fonctionnels et aucune aide permanente ne décrit l'interface à l'écran.

## 10. Sûreté

- aucune API de suppression ou de déplacement n'est référencée par le projet Desktop ;
- aucune action désactivée ne possède de commande exécutable ;
- les rapports métier sont affichés sans modifier leurs décisions ;
- une base verrouillée n'est jamais forcée ni copiée silencieusement ;
- un échec de session ne modifie pas les fichiers analysés ni les rapports précédents ;
- le chemin de la base et des sessions est toujours visible dans l'espace de travail ;
- Identity et Identity.Access restent sans diff.

## 11. Tests d'acceptation

- création et réouverture d'une bibliothèque ;
- ajout de racines appartenant à plusieurs lecteurs ;
- fusion d'une racine imbriquée et refus de deux racines indépendantes du même lecteur ;
- scan en arrière-plan avec compteur de fichiers ;
- production et affichage automatique des deux rapports ;
- ouverture d'une base existante sans rescan ;
- import du `rapport-doublons.json` historique avec onglet Versions indisponible ;
- affichage fluide d'au moins 6 009 groupes et 8 579 candidats ;
- sauvegarde et reprise des décisions de revue ;
- rotation atomique avec une seule archive ;
- reprise des décisions stables après rescan ;
- conservation de la session précédente après échec ;
- bouton Supprimer visible mais impossible à invoquer ;
- navigation clavier et absence de chevauchement aux tailles minimales supportées ;
- tests existants Scanner, Duplicate Files et Identity inchangés et verts.

## 12. Limites assumées

La V1 ne peut pas annuler un scan et ne peut pas consolider deux racines indépendantes du même
lecteur sans faire évoluer le Scanner. Elle ne cherche pas à contourner ces limites dans l'UI.

Les rapports volumineux sont conservés dans la session pour permettre une reprise immédiate. Si la
taille ou le temps d'écriture devient problématique, une mesure dédiée devra précéder tout format
indexé ou stockage incrémental.
