# Organisation modulaire verticale

**Statut** : conception validée par le propriétaire le 2026-07-19.
**Nature** : organisation physique et règles de dépendance. Aucun changement fonctionnel n'est inclus.

---

## 1. Contexte

Le dépôt possède déjà les bonnes frontières logiques : le moteur Identity est générique, le scanner
produit des observations de fichiers et Duplicate Files applique des règles métier. En revanche, le
code, les tests et la documentation de chaque module sont répartis entre `src/`, `tests/`, `docs/`
et `modules/`, ce qui rend la compréhension et l'ajout de modules inutilement difficiles.

La réorganisation doit rendre les frontières visibles dans l'arborescence sans réécrire les
comportements existants.

## 2. Décision fondamentale : Identity est scellé

Identity est le coeur conceptuel générique du dépôt. Il ne connaît ni les fichiers, ni les
doublons, ni les logiciels. Il est consommé comme une dépendance existante et totalement gelée.

Les emplacements suivants sont intouchables :

- `src/InstallChecker.Identity/` ;
- `src/InstallChecker.Identity.Access/` ;
- `tests/InstallChecker.Identity.Tests/` ;
- `tests/oracle/` ;
- `docs/identity/` ;
- `docs/conformite/` ;
- `registre/`.

Ils ne sont ni modifiés, ni déplacés, ni renommés par la réorganisation. Une exigence métier qui
n'est pas satisfaite par Identity est implémentée dans le moteur du module concerné. Elle ne devient
jamais rétroactivement une conclusion de `W`.

## 3. Principe d'organisation

Chaque module est une tranche verticale autonome. Son code, ses tests, sa documentation, ses règles
versionnées, ses adaptateurs et ses commandes restent sous un même dossier `modules/<module>/`.

Le caractère transversal d'un composant décrit son rôle, pas son emplacement. Un composant partagé
entre plusieurs parties d'un même module reste dans ce module. Il ne sort au niveau global que s'il
est réellement utilisé par plusieurs modules et ne porte aucune règle propre à l'un d'eux.

Aucun dossier global `Shared` ni système de plugins n'est créé par anticipation.

## 4. Structure cible

```text
src/
  InstallChecker.Identity/                 gelé
  InstallChecker.Identity.Access/          gelé

modules/
  scanner/
    src/
      InstallChecker.Scanner.Core/
      InstallChecker.Scanner/
    tests/
    docs/

  duplicate-files/
    src/
      InstallChecker.DuplicateFiles.Engine/
      InstallChecker.DuplicateFiles/
    tests/
    docs/
    registre-metier/

apps/
  cli/
    InstallChecker.Cli/

  desktop/                                  futur, non créé maintenant
```

Deux projets au maximum sont prévus par module dans cette étape : un coeur ou moteur pur et une
enveloppe contenant les cas d'usage et les adaptateurs. Une séparation supplémentaire ne sera faite
que si un besoin concret la justifie.

## 5. Module Scanner

Le scanner est transversal dans son usage, mais reste un module. Il produit des observations de
fichiers sans connaître le consommateur ni la décision future.

`InstallChecker.Scanner.Core` contient :

- les modèles d'observation propres aux fichiers ;
- les extracteurs PE, MSI, APPX, Authenticode et en-têtes ;
- l'identification des volumes ;
- les traitements purs nécessaires à l'observation.

`InstallChecker.Scanner` contient :

- le cas d'usage de scan ;
- l'accès au système de fichiers ;
- le stockage SQLite du scanner ;
- les adaptateurs exposant les observations aux contrats publics existants ;
- la commande CLI du scanner.

Le scanner ne possède aucun mode métier « doublon ». Les filtres de chemin ou d'extension restent
des paramètres génériques de sélection du corpus.

## 6. Module Duplicate Files

Le moteur métier existe déjà dans le projet actuel `InstallChecker.DuplicateFiles`. La
réorganisation le rend explicite ; elle ne crée pas un second moteur théorique.

`InstallChecker.DuplicateFiles.Engine` contient :

- l'extraction et la classification des groupes ;
- l'enrichissement métier ;
- les politiques de rétention ;
- la protection des chemins ;
- la construction des plans ;
- les futures analyses de redondance versionnée et de similarité ;
- les modèles et résultats structurés du module.

`InstallChecker.DuplicateFiles` contient :

- les cas d'usage applicatifs ;
- les adaptateurs de lecture nécessaires au module ;
- les commandes `duplicates` et `plan` ;
- la sérialisation destinée à la CLI ;
- plus tard, les adaptateurs requis par une interface graphique.

Les conclusions propres au module sont restituées dans un résultat métier distinct. Elles ne
modifient ni `W`, ni Omega, ni le registre d'Identity.

## 7. Flux et dépendances

```text
Scanner -> snapshot d'observations -> Identity gelé -> W
                    |                              |
                    +------> Duplicate Engine <---+
                                      |
                                      v
                          DuplicateAnalysisResult
                                      |
                              CLI puis future UI
```

Règles obligatoires :

- Identity ne référence aucun module ;
- Scanner ne référence jamais Duplicate Files ;
- Duplicate Files peut consommer les contrats publics d'Identity et du Scanner ;
- Duplicate Files ne dépend jamais du stockage interne du Scanner ;
- l'application CLI assemble les modules sans porter de règle métier ;
- aucune dépendance cyclique n'est admise.

## 8. Correspondance avec l'existant

La réorganisation est principalement mécanique :

- `src/InstallChecker.Core/` devient le coeur du module Scanner ;
- `ScanCommand` et `ObservationStore` rejoignent l'enveloppe du Scanner ;
- `src/InstallChecker.DuplicateFiles/` constitue la base du moteur Duplicate Files ;
- la logique métier encore présente dans `PlanCommand` rejoint le module ;
- `DuplicatesCommand`, `PlanCommand` et `LecteurDeVolumes` rejoignent l'enveloppe du module ;
- les tests et documents métier sont regroupés sous leur module ;
- l'hôte CLI racine conserve uniquement le routage et la composition nécessaires.

Les noms publics et les espaces de noms sont conservés pendant le déplacement lorsque cela réduit
le risque. Les renommages esthétiques ne sont pas mélangés à la migration physique.

## 9. Données, stockage et compatibilité

`InstallChecker.Identity.Access` étant gelé, aucun nouveau besoin métier ne doit exiger sa
modification. Le contrat de données qu'il sait déjà consommer reste compatible.

Les données nouvelles propres au Scanner ou à Duplicate Files appartiennent au module concerné.
Elles utilisent un stockage ou des tables dont le cycle de vie est contrôlé par ce module, sans
changer le comportement historique d'Identity.

Le produit n'utilise pas `%TEMP%` comme mémoire ou stockage fonctionnel. Son usage reste limité aux
tests isolés qui créent puis suppriment leurs propres fichiers temporaires.

## 10. Stratégie de migration

La migration doit être réalisée par étapes courtes :

1. consigner la décision structurante dans un ADR ;
2. créer les dossiers et projets cibles sans déplacer Identity ;
3. déplacer le Scanner avec comportement et tests inchangés ;
4. déplacer Duplicate Files avec comportement et tests inchangés ;
5. amincir l'hôte CLI ;
6. mettre à jour les références de projets et la documentation courante ;
7. exécuter la totalité des tests après chaque étape.

Les corrections fonctionnelles du plan de suppression et les nouvelles fonctions du module sont
traitées dans des changements séparés. Une réorganisation ne doit pas masquer une modification de
comportement.

## 11. Critères d'acceptation

- aucun fichier du périmètre Identity scellé n'est modifié, déplacé ou renommé ;
- chaque fichier propre au Scanner se trouve sous `modules/scanner/` ;
- chaque fichier propre à Duplicate Files se trouve sous `modules/duplicate-files/` ;
- l'hôte CLI ne contient aucune règle métier de module ;
- les directions de dépendance définies au paragraphe 7 sont respectées ;
- les sorties CLI et les codes de retour restent identiques ;
- les 250 tests existants restent verts après la migration ;
- aucun usage fonctionnel de `%TEMP%` n'est introduit ;
- les documents canoniques de chaque module résident dans son dossier.

## 12. Hors périmètre

- modification ou déplacement d'Identity et de ses adaptateurs ;
- ajout d'un système de plugins dynamiques ;
- création de l'interface graphique ;
- modification des règles de doublon ou de rétention ;
- exécution d'une suppression ou d'un déplacement de fichier ;
- ajout des redondances versionnées et des suspects.
