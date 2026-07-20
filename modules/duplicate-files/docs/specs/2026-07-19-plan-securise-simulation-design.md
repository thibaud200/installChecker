# Plan sécurisé et simulation

**Statut** : conception validée, implémentée et vérifiée le 2026-07-19.
**Périmètre** : jalon E1 du module Duplicate Files.
**Fondation** : le contrat UI des doublons exacts est disponible ; Identity et Identity.Access
restent scellés et ne sont jamais modifiés.

---

## 1. Objectif

Rendre un plan de suppression vérifiable au moment où il est relu, sans modifier le système de
fichiers. Le résultat doit indiquer si le plan serait exécutable dans l'état courant du disque et
expliquer chaque blocage sous une forme consommable par une future interface graphique.

Ce jalon prépare une future mise à la Corbeille Windows, mais n'ajoute aucune opération de
suppression, de déplacement, de copie ou de mise à la Corbeille.

## 2. Hors périmètre

- aucune suppression réelle ;
- aucun déplacement réel, y compris vers la Corbeille ;
- aucun appel à une API Windows de mutation ;
- aucune confirmation interactive ;
- aucune exécution automatique après une vérification réussie ;
- aucune redondance entre versions différentes ;
- aucune modification d'Identity, Identity.Access, Omega ou W.

## 3. Décision structurante

Le plan devient autosuffisant pour sa vérification. La commande de vérification ne reçoit ni base
SQLite ni registre et ne relance pas le moteur Identity.

Le JSON évolue de manière additive : la liste plate `Propositions` et chacun de ses champs actuels
sont conservés. Le plan ajoute :

- `VersionContrat` = `duplicate-files/safe-plan/v1` ;
- `GarantiesParGroupe`, contenant les informations nécessaires pour prouver qu'un exemplaire doit
  subsister.

Cette décision est consignée dans
`modules/duplicate-files/docs/adr/ADR-001-plan-securise-autosuffisant.md`.

## 4. Contrat du plan sécurisé

### 4.1 Garantie de groupe

Chaque groupe qui possède au moins une proposition expose exactement une garantie :

```text
GarantieDeGroupe
  GroupeId
  ContenuSha256
  TemoinConservation
    FichierId
    Chemin
```

Le témoin est toujours l'exemplaire de rang 1 produit par `PolitiqueRetentionV1`. Il n'est jamais
présent dans `Propositions`, même lorsqu'un autre chemin protégé suffirait déjà à faire subsister
une copie.

Tous les chemins protégés restent également absents des propositions. Les propositions d'un groupe
sont donc exactement les exemplaires de rang supérieur ou égal à 2 dont le chemin n'est pas
protégé.

Une garantie sans proposition n'est pas émise : il n'existe alors aucune action potentielle à
sécuriser.

### 4.2 Identifiants

`GroupeId` et `FichierId` réutilisent exclusivement `IdentifiantsStables`. Le vérificateur les
recalcule depuis `ContenuSha256` et `Chemin` et refuse toute incohérence.

Le contenu d'une proposition doit être identique au `ContenuSha256` de sa garantie. Les chemins et
identifiants de fichiers sont uniques dans un plan selon la comparaison Windows insensible à la
casse et aux différences entre `/` et `\` déjà définies par le contrat v1.

## 5. Commande de vérification

La CLI ajoute :

```text
installchecker plan verify <plan.json>
```

La commande lit le plan, valide sa structure, observe les fichiers en lecture seule et écrit un
`RapportDeVerificationPlan` JSON sur stdout.

Elle ne demande jamais la base d'observations ni le registre. Elle n'appelle jamais Identity.

## 6. Validation structurelle

La validation structurelle précède tout accès aux fichiers :

1. `VersionContrat` est exactement `duplicate-files/safe-plan/v1` ;
2. chaque proposition référence une garantie existante par `GroupeId` ;
3. les SHA-256 sont valides, normalisés et cohérents dans chaque groupe ;
4. tous les `GroupeId` et `FichierId` recalculés correspondent aux valeurs du plan ;
5. le témoin n'apparaît jamais parmi les propositions ;
6. aucun chemin ni identifiant de fichier n'est dupliqué ;
7. chaque garantie possède au moins une proposition.

Un plan structurellement invalide est une erreur de contrat : code de sortie `1`, message sur
stderr et aucune sortie JSON partielle.

## 7. Observation en lecture seule

Le moteur métier définit un port d'observation sans méthode de mutation. L'enveloppe du module
fournit l'adaptateur local fondé sur `FileStream` et `SHA256`.

Pour le témoin et chaque proposition, l'adaptateur observe :

- l'existence du chemin ;
- le fait qu'il désigne un fichier ordinaire ;
- l'absence de point de réanalyse ou de lien symbolique ;
- la possibilité de lire le contenu ;
- le SHA-256 courant, calculé en flux sans charger le fichier entier en mémoire.

Les fichiers sont vérifiés séquentiellement dans l'ordre du plan. Le parallélisme reste hors
périmètre tant qu'un benchmark ne démontre pas sa nécessité.

## 8. Résultat de simulation

Le rapport expose :

```text
RapportDeVerificationPlan
  VersionContrat = duplicate-files/safe-plan-verification/v1
  Mode = Simulation
  Executable
  Groupes
  Journal
```

Chaque groupe et chaque fichier portent leur état et leurs blocages structurés. Chaque fichier
indique aussi son rôle `TemoinConservation` ou `Candidat`, afin que le rapport reste compréhensible
sans devoir relire le plan d'entrée. Les valeurs fermées sont sérialisées sous forme de chaînes.
Les états possibles sont :

- `Valide` ;
- `Absent` ;
- `Illisible` ;
- `HashDifferent` ;
- `CheminProtege` ;
- `TypeNonPrisEnCharge`.

`HashObserve` vaut `null` lorsque le contenu n'a pas pu être lu. Aucun message libre n'est nécessaire
pour déterminer l'état. Un détail humain normalisé par le module peut accompagner une erreur locale ;
les textes bruts et variables des exceptions du système ne font pas partie du JSON public.

La protection des chemins est réévaluée pendant cette phase : un candidat devenu protégé porte
`CheminProtege` et bloque la simulation sans rendre le fichier JSON malformé.

`Executable` vaut `true` uniquement lorsque le témoin et toutes les propositions sont `Valide`.
Cette valeur signifie « exécutable selon les observations de cette simulation », jamais une
autorisation ni une garantie durable.

## 9. Journal déterministe

Le rapport contient un journal ordonné des vérifications. Chaque entrée expose au minimum :

- un numéro de séquence commençant à 1 ;
- `GroupeId` ;
- `FichierId` ;
- l'étape `VerifierTemoin` ou `VerifierCandidat` ;
- l'état obtenu.

Le journal de simulation ne contient ni horodatage ni identifiant aléatoire afin que le même plan
observé dans le même état produise le même JSON. Il ne prétend pas être le futur journal persistant
d'une opération réelle.

## 10. Codes de sortie

- `0` : vérification terminée et `Executable = true` ;
- `1` : plan absent, illisible, JSON malformé ou contrat structurellement invalide ;
- `2` : usage CLI incorrect ;
- `3` : vérification terminée, rapport JSON produit, mais `Executable = false`.

Le code `3` permet l'automatisation sans confondre un blocage de sécurité avec une erreur de lecture
du contrat.

## 11. Frontières du module

- Les DTO, invariants et décisions de validation vivent dans
  `modules/duplicate-files/src/InstallChecker.DuplicateFiles.Engine/`.
- La lecture JSON, la sérialisation et l'observation locale des fichiers vivent dans
  `modules/duplicate-files/src/InstallChecker.DuplicateFiles/`.
- La CLI ne contient que le routage des arguments.
- Aucune dépendance vers SQLite, Scanner ou Identity.Access n'entre dans le moteur métier.
- Aucun fichier ne sort du module sauf le routage minimal de la CLI et la référence de projet déjà
  nécessaires.

## 12. Préparation de la Corbeille Windows

La future opération réelle aura pour unique destination autorisée `CorbeilleWindows`. La
suppression définitive et tout repli vers `File.Delete` resteront interdits.

Ce jalon ne crée ni interface de mutation inactive, ni enum d'exécution inutilisé, ni route CLI
`execute`. Il prépare cette évolution par deux frontières réutilisables :

- le plan autosuffisant ;
- le validateur indépendant de l'adaptateur d'observation.

Lorsque l'exécution sera ouverte, elle devra :

1. refaire les validations immédiatement avant chaque déplacement ;
2. vérifier le témoin avant chaque groupe ;
3. recalculer le hash du candidat juste avant son déplacement ;
4. arrêter le groupe au premier écart ;
5. utiliser une API de Corbeille Windows sans repli destructif ;
6. écrire un journal persistant avant et après chaque tentative.

La simulation actuelle ne résout pas le risque TOCTOU entre une vérification et une future action.
Seule la revalidation au dernier moment pourra le réduire.

## 13. Tests d'acceptation

- le plan conserve tous les champs JSON historiques ;
- le rang 1 devient toujours le témoin et n'est jamais proposé ;
- un chemin protégé n'est jamais proposé ;
- un plan peut être vérifié sans base SQLite ni registre ;
- un témoin ou candidat absent bloque le groupe ;
- un hash courant différent bloque le groupe ;
- un fichier illisible, non ordinaire ou point de réanalyse bloque le groupe ;
- une incohérence d'identifiant est refusée avant l'accès disque ;
- un plan entièrement valide produit `Executable = true` et le code `0` ;
- un blocage local produit un rapport complet et le code `3` ;
- le journal est stable et ordonné ;
- aucun test ne constate une modification du système de fichiers ;
- les tests de toute la solution restent verts ;
- le diff du périmètre Identity scellé reste vide.
