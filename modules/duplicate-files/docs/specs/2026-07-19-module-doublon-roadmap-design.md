# Module Doublon — Roadmap et contrat fonctionnel

**Statut** : conception validée par le propriétaire le 2026-07-19 ; jalon F1 livré.
**Périmètre** : module doublon uniquement. Le moteur `InstallChecker.Identity` reste une fondation consommée, non modifiée.

---

## 1. Intention

Le module doublon devient un assistant local de maintenance de bibliothèque de fichiers, orienté d'abord vers les installateurs Windows mais utilisable sur d'autres types de fichiers.

Il ne doit pas seulement répondre à « quels fichiers ont le même hash ? ». Il doit séparer clairement :

- les copies exactes, prouvées par le contenu ;
- les recommandations de rétention, qui choisissent quelle copie garder ;
- les redondances versionnées, qui signalent une ancienne version candidate ;
- les suspects, qui exigent une revue humaine.

Aucune action destructive ne doit être implicite. Toute action future passe par un plan vérifiable.

---

## 2. Principes

- Le moteur Identity n'est pas étendu pour les besoins du module.
- Le module produit des résultats métier structurés, consommables demain par une interface graphique.
- La CLI, une future UI et des exports consomment les mêmes DTO.
- Une copie exacte et une ancienne version ne sont jamais mélangées.
- Les décisions automatiques ne s'appliquent qu'aux copies exactes et restent soumises aux chemins protégés.
- Les redondances versionnées et les suspects restent en revue humaine obligatoire.

---

## 3. Roadmap

### A — Stabilisation

- Corriger `plan` : il doit réutiliser le classement de rétention du rapport.
- Ajouter une protection par défaut des chemins système.
- Stabiliser les sorties structurées en vue de la future UI.

### B — Doublons exacts fiables

- Même SHA-256 = même contenu = doublon certain.
- Rapport et plan cohérents.
- Multi-disque conservé.
- Aucune suppression automatique.

### C — Assistant de rétention

- Dossiers préférés ou évités.
- Nom de copie.
- Signature présente.
- Métadonnées riches.
- Date, volume, chemin.
- Raisons structurées.

### D — Contrat pour interface graphique

- Identifiants stables de groupe et de fichier.
- Catégorie, confiance, preuves.
- Fichier recommandé à conserver.
- Candidats.
- Actions autorisées, interdites, et raisons de blocage.

### E — Plan sécurisé

- **E1 livré** : plan autosuffisant avec témoin de conservation.
- **E1 livré** : vérification d'existence, de type et de SHA-256 courant.
- **E1 livré** : journal déterministe et mode simulation sans mutation.
- **E2 différé** : mise à la Corbeille Windows avec revalidation juste avant chaque action.
- La suppression définitive reste interdite.

### F — Redondance versionnée

- **F1 livré** : détection générique pour tout type de fichier par fournisseurs de preuves et
  arbitre commun.
- Le nom de fichier fournit la preuve universelle ; VersionInfo, MSI, MSIX/Appx, PE et Authenticode
  renforcent la famille, la version, l'éditeur ou les variantes lorsqu'ils sont disponibles.
- Les formats, architectures, langues, éditions et distributions incompatibles ne sont jamais
  comparés.
- Statut : `VersionRedundancyCandidate` ; suppression automatique interdite et revue humaine
  obligatoire.
- Spécification détaillée : `2026-07-19-redondance-versionnee-design.md`.

### G — Doublons suspects

- Même taille et nom proche.
- Même nom normalisé et taille proche.
- Même éditeur et nom proche.
- Statut : `NeedsReview`.

### H — Clones de dossiers

- Dossiers strictement identiques.
- Dossiers quasi identiques.
- Backups copiés.

### I — Organisation des fichiers conservés

- Déplacer ou copier les fichiers gardés vers un répertoire cible configurable.
- Classement par type dans un premier temps.
- Organisation par fournisseur ou logiciel seulement lorsque l'identification est assez fiable.
- Toute opération passe par un manifeste et une vérification.

---

## 4. État du jalon

Le jalon E1 est livré :

- plan JSON autosuffisant et versionné ;
- rang 1 toujours conservé comme témoin ;
- commande `plan verify` indépendante de la base et du registre ;
- simulation structurée avec codes de sortie `0`, `1`, `2` et `3` ;
- aucune mutation du système de fichiers.

E2 reste volontairement différé. F1, la redondance versionnée générique, est livré avec la commande
`duplicates versions`, le contrat `duplicate-files/version-redundancy/v1` et une première mesure sur
100 000 observations. La revue humaine reste obligatoire pour tous ses résultats.
