# ADR-002 — Redondance versionnée par fournisseurs de preuves

**Statut** : accepté le 2026-07-19.
**Portée** : module Duplicate Files uniquement.

## Contexte

La redondance versionnée doit fonctionner pour tous les types de fichiers, sans être limitée aux
EXE, MSI ou MSIX. Ces formats disposent néanmoins de métadonnées plus fortes que le nom de fichier.

Une logique unique fondée sur le plus petit dénominateur commun perdrait ces garanties. Des moteurs
indépendants par format dupliqueraient la résolution des versions, la gestion des variantes et le
contrat de restitution. Modifier Identity pour y introduire ces règles métier violerait sa frontière
générique et son gel.

## Décision

Le module adopte des fournisseurs de preuves indépendants suivis d'un arbitre générique.

Chaque fournisseur transforme les observations brutes qu'il comprend en preuves normalisées et
traçables de famille, version, éditeur ou variante. Il ne regroupe pas les fichiers et ne décide
d'aucune action.

L'arbitre combine ces preuves, résout les accords, bloque les contradictions, sépare les variantes
et construit les candidats de redondance versionnée. Le nom de fichier constitue le fournisseur
universel ; VersionInfo, MSI, MSIX/Appx, PE et Authenticode renforcent le résultat lorsqu'ils sont
applicables.

Tout ce comportement reste sous `modules/duplicate-files`. L'enveloppe du module utilise les
adaptateurs de lecture existants. Identity et Identity.Access ne sont pas modifiés.

## Alternatives

### Analyseur unique de toutes les métadonnées

Rejeté : le composant mélangerait extraction, connaissance des formats, arbitrage, comparaison et
restitution. Chaque nouveau format augmenterait son couplage et son risque de régression.

### Moteur indépendant pour chaque format

Rejeté : chaque moteur devrait réimplémenter la comparaison des versions, les variantes, la
confiance, les identifiants stables et le JSON public. Les fichiers génériques resteraient sans
solution cohérente.

### Limiter F1 à MSI et MSIX/Appx

Rejeté : cette option maximise la précision initiale mais contredit le caractère général du module
Duplicate Files et exclut les archives, documents, sauvegardes et autres fichiers versionnés.

### Étendre le moteur Identity

Rejeté : les règles concernent la redondance et la remplaçabilité de fichiers, pas l'identité
logique générique. Identity est scellé et ne doit connaître ni les formats, ni les décisions métier
du module.

## Conséquences

- tout type de fichier peut produire un candidat depuis son nom ou ses métadonnées disponibles ;
- les formats riches conservent leurs preuves spécifiques sans contaminer l'arbitre générique ;
- un nouveau format ajoute un fournisseur local au module ;
- le contrat de preuves et ses fournisseurs doivent être versionnés pour préserver la
  reproductibilité ;
- l'arbitrage est plus structuré qu'un analyseur monolithique, mais chaque composant reste petit et
  testable indépendamment ;
- les résultats faibles ou contradictoires restent visibles et explicables sans devenir
  actionnables ;
- F1 ne crée aucun droit de suppression et ne modifie pas le plan sécurisé ;
- Identity, Identity.Access et le Scanner restent inchangés.

