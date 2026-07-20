# Contrat UI des doublons exacts

**Statut** : conception validée puis implémentée et vérifiée le 2026-07-19.
**Périmètre** : jalon D appliqué aux copies exactes du jalon B.
**Fondation** : Identity et Identity.Access restent scellés et ne sont jamais modifiés.

---

## 1. Objectif

Faire du rapport et du plan Duplicate Files des contrats structurés directement consommables par
une future interface graphique, sans retirer ni renommer les champs JSON existants.

Le contrat doit permettre à un consommateur de répondre sans analyser une phrase libre :

- quel groupe et quel fichier sont désignés ;
- pourquoi les fichiers sont considérés comme des copies exactes ;
- quel fichier est recommandé à la conservation ;
- quels fichiers sont candidats ;
- quelle action est disponible ou bloquée, et pour quelle raison.

## 2. Hors périmètre

- aucune interface graphique ;
- aucune suppression, copie ou déplacement de fichier ;
- aucune préférence de dossier configurable ;
- aucune redondance entre versions différentes ;
- aucune exposition de `product_version` ou `file_version` dans ce jalon ;
- aucune modification de W, Omega, Identity, Identity.Access ou du registre Identity.

Les versions produit et fichier existent dans les observations du Scanner, mais ne servent pas à
qualifier deux copies exactes : des fichiers de même SHA-256 ont déjà les mêmes octets. Elles seront
traitées par le jalon F « Redondance versionnée ».

## 3. Stratégie de compatibilité

Le JSON évolue de façon additive :

- `Synthese`, `Note`, `Groupes`, `Domaine`, `MotifCourt`, les métriques, `Exemplaires`, `Fichier`,
  `Rang`, `Etiquette`, `Motif`, `Contenu` et `Chemin` sont conservés ;
- les nouveaux champs sont ajoutés au rapport, aux groupes, aux exemplaires et aux propositions ;
- les valeurs fermées sont sérialisées sous forme de chaînes, jamais sous forme de nombres ;
- la version initiale du contrat est `duplicate-files/exact-duplicates/v1`.

La compatibilité garantie concerne le JSON public. Les constructeurs C# des DTO appartiennent au
module et peuvent être adaptés avec tous leurs appels et tests dans la même évolution.

## 4. Preuve d'un doublon exact

Une sortie de catégorie `ExactDuplicate` n'est émise que si tous les actes du groupe portent la
même empreinte SHA-256 dans Omega.

Le module relit les empreintes brutes des actes du domaine et vérifie :

1. que chaque acte existe dans le snapshot fourni ;
2. que chaque empreinte contient exactement 64 caractères hexadécimaux ;
3. que toutes les empreintes normalisées en minuscules sont identiques.

Une incohérence entre W et Omega est une défaillance interne : le module lève une
`InvalidOperationException` et n'émet aucun rapport trompeur. La CLI continue de ne capturer que
les erreurs contractuelles déjà définies ; elle ne masque pas cette défaillance.

Chaque groupe expose une preuve structurée :

```text
Type  = Sha256Identique
Valeur = empreinte SHA-256 normalisée
```

## 5. Identifiants stables

### Groupe

L'identifiant d'un groupe exact est :

```text
exact:sha256:<empreinte en minuscules>
```

Il ne dépend ni des identifiants d'actes, ni de l'ordre du scan, ni du volume. Un même contenu
retrouvé lors d'un nouveau scan conserve donc son identifiant.

### Fichier

L'identifiant d'un exemplaire est :

```text
file:sha256:<SHA-256 UTF-8 de "<groupeId>\n<cheminCanonique>">
```

Le chemin canonique est obtenu sans accès au système de fichiers : séparateurs `/` remplacés par
`\`, puis comparaison Windows rendue insensible à la casse avec `ToUpperInvariant()`.

L'identifiant reste stable pour le même contenu au même chemin malgré un nouveau `ActeId`. Il
change lorsque le contenu ou le chemin change. Les alias physiques, jonctions et chemins `\\?\`
ne sont pas fusionnés dans ce jalon.

## 6. Contrat du rapport

### Rapport

`RapportDeDoublons` ajoute :

- `VersionContrat` = `duplicate-files/exact-duplicates/v1`.

### Groupe

`GroupeClasse` ajoute :

- `GroupeId` ;
- `Categorie` = `ExactDuplicate` ;
- `Confiance` = `Certaine` ;
- `ContenuSha256` ;
- `Preuves` ;
- `FichierRecommandeId`, égal à l'identifiant de l'exemplaire de rang 1.

Le champ historique `Domaine` reste disponible pour l'audit mais ne constitue jamais un
identifiant UI stable.

### Exemplaire

`ExemplaireRapporte` ajoute :

- `FichierId` ;
- `Role` : `RecommandeAConserver` ou `Candidat` ;
- `CriteresClassement`, liste structurée et ordonnée des critères effectivement appliqués ;
- `Actions`, état structuré des actions du jalon.

Les critères de la politique v1 sont exposés avec leur priorité : richesse des observations, nom de
copie, date d'observation, chemin, puis identifiant d'acte comme départage mécanique. Le champ
historique `Motif` reste présent.

## 7. Actions et blocages

Deux actions descriptives sont exposées par exemplaire :

- `Conserver` : toujours autorisée ;
- `AjouterAuPlanDeSuppression` : autorisée uniquement pour un exemplaire de rang supérieur ou égal
  à 2 dont le chemin n'est pas protégé.

Les blocages possibles dans ce jalon sont :

- `FichierRecommandeAConserver` ;
- `CheminProtege`.

Plusieurs blocages peuvent être présents. Une action autorisée a une liste de blocages vide. Ces
actions décrivent ce que l'UI peut proposer ; elles n'exécutent rien.

Le générateur de rapport utilise par défaut `ProtectionDesChemins.EstProtegeParDefaut`. Un prédicat
peut être injecté dans les tests et dans une future configuration.

## 8. Contrat du plan

Chaque `PropositionDeSuppression` conserve `Contenu` et `Chemin` et ajoute :

- `GroupeId`, calculé par le même composant que le rapport ;
- `FichierId`, calculé par le même composant que le rapport.

Pour un même contenu et un même chemin, les identifiants du rapport et du plan sont identiques.
Le plan reste une proposition plate, sans exécution et sans nouveau droit de suppression.

## 9. Sérialisation

Les catégories, niveaux de confiance, types de preuve, rôles, critères, actions et raisons de
blocage sont des enums C# sérialisés en chaînes via `JsonStringEnumConverter` dans les commandes
`duplicates` et `plan`.

Les noms de valeurs constituent le contrat v1 et ne peuvent pas être renommés silencieusement.

## 10. Tests d'acceptation

- deux scans logiquement équivalents avec des `ActeId` différents produisent les mêmes identifiants ;
- changer le chemin ou le contenu change le `FichierId` ;
- un groupe aux empreintes différentes est refusé avant émission ;
- le groupe expose catégorie, confiance et preuve SHA-256 structurées ;
- le rang 1 est référencé par `FichierRecommandeId` ;
- les critères de classement sont structurés dans l'ordre de la politique v1 ;
- un candidat non protégé peut être ajouté au plan ;
- un chemin protégé et le fichier recommandé exposent leurs blocages ;
- rapport et plan produisent les mêmes identifiants ;
- le JSON sérialise les enums sous forme de chaînes et conserve les anciens champs ;
- les tests du module et les 250 tests existants restent verts.
