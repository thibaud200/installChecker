# ADR-001 — Plan sécurisé autosuffisant

**Statut** : accepté le 2026-07-19.
**Portée** : module Duplicate Files uniquement.

## Contexte

Le plan existant contient uniquement les copies proposées à la suppression. Il ne décrit pas le
fichier qui doit subsister et ne peut donc pas démontrer seul qu'une copie valide existe encore au
moment de sa relecture.

Une vérification reconstruite depuis la base et le registre couplerait durablement le plan à son
environnement de création et relancerait Identity pour une décision qui appartient au module.

## Décision

Le plan JSON évolue de manière additive et embarque, pour chaque groupe actionnable, un témoin de
conservation avec son chemin, son identifiant stable et le SHA-256 du groupe.

La vérification consomme uniquement ce plan et l'état courant du système de fichiers par un port en
lecture seule. Elle ne reçoit ni base SQLite ni registre et n'appelle pas Identity.

Le témoin est toujours le fichier classé rang 1. Il n'est jamais proposé, même lorsqu'un autre
fichier protégé garantit déjà qu'une copie subsistera.

## Alternatives

### Reconstruire depuis la base et le registre

Rejeté : le plan ne serait pas portable, archivable ou vérifiable indépendamment. Une future UI
devrait conserver trois artefacts cohérents au lieu d'un seul.

### Remplacer le plan plat par un manifeste entièrement groupé

Rejeté : cette représentation casserait le contrat JSON existant. L'ajout de garanties groupées à
côté des propositions plates apporte la sécurité nécessaire sans supprimer les champs publics.

### Considérer tout chemin protégé comme témoin suffisant

Rejeté : cela permettrait encore de proposer le fichier recommandé de rang 1 et contredirait le
contrat UI du jalon D. La politique conservatrice garde toujours le rang 1 et tous les chemins
protégés.

## Conséquences

- le plan est plus volumineux mais autosuffisant ;
- les anciens consommateurs conservent `Propositions`, `Contenu`, `Chemin`, `GroupeId` et
  `FichierId` ;
- les anciens fichiers de plan sans `VersionContrat` ne sont pas vérifiables par la nouvelle
  commande ; ils doivent être régénérés ;
- une vérification réussie ne vaut que pour l'instant observé et n'autorise aucune action ;
- une future mise à la Corbeille devra revalider le plan immédiatement avant chaque déplacement ;
- Identity et Identity.Access restent inchangés.
