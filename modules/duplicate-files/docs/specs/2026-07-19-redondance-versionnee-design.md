# Redondance versionnée générique

**Statut** : conception validée et jalon F1 implémenté le 2026-07-19.
**Périmètre** : jalon F1 du module Duplicate Files.
**Fondation** : le Scanner fournit les observations existantes ; Identity et Identity.Access restent
scellés et ne sont jamais modifiés.

---

## 1. Objectif

Détecter, pour tout type de fichier, des contenus différents qui représentent probablement des
versions différentes d'une même famille de fichiers.

Le module ne se limite pas aux exécutables et aux installateurs. Les formats MSI, MSIX/Appx et EXE
fournissent des preuves structurées plus fortes, tandis que tout autre fichier peut participer à
une famille versionnée lorsque son nom ou ses métadonnées disponibles exposent une version.

Le résultat est un rapport explicable destiné à une revue humaine et directement consommable par
une future interface graphique.

## 2. Invariants et hors périmètre

- Les doublons exacts et les redondances versionnées restent deux domaines distincts.
- Un même SHA-256 relève exclusivement des doublons exacts.
- Une version plus ancienne n'est jamais assimilée à une copie exacte.
- Tous les candidats F1 exigent une revue humaine.
- Aucun candidat F1 n'entre dans le plan de suppression sécurisé.
- Aucune suppression, mise à la Corbeille, copie ou réorganisation n'est ajoutée.
- Aucun fichier observé n'est rouvert et aucun hash n'est recalculé.
- Aucune écriture SQLite, aucun fichier temporaire et aucun accès réseau ne sont autorisés.
- Les observations Omega ne sont jamais modifiées ou normalisées à la source.
- Identity, Identity.Access, leurs tests, leur théorie et leur registre restent inchangés.
- La recherche de dernière version en ligne et les catalogues externes restent hors périmètre.
- Les versions préliminaires ou textuelles ambiguës ne sont pas ordonnées en F1.

## 3. Décision d'architecture

F1 adopte une architecture composée de fournisseurs de preuves indépendants et d'un arbitre
générique. Cette décision est consignée dans
`modules/duplicate-files/docs/adr/ADR-002-redondance-versionnee-par-preuves.md`.

```text
Observations existantes
        |
        v
Fournisseurs de preuves
        |
        v
Résolveur de famille
        |
        v
Résolveur de version
        |
        v
Séparateur de variantes
        |
        v
Constructeur de candidats
        |
        v
Rapport versionné
```

Le moteur du module ne connaît pas les mécanismes de stockage. Son enveloppe lit les observations
par les adaptateurs existants, appelle le moteur et sérialise ses DTO. La CLI ne porte que le
routage des arguments.

Le terme « identité » n'est pas utilisé pour nommer les composants F1 afin d'éviter toute confusion
avec le moteur Identity. Les concepts métier sont `FamilleVersionnee`, `PreuveDeVersion`,
`ArtefactVersionne` et `ResolveurDeFamille`.

## 4. Frontières physiques

Tout le comportement F1 vit sous `modules/duplicate-files` :

```text
src/InstallChecker.DuplicateFiles.Engine/
  preuves, résolution, comparaison, regroupement et DTO publics

src/InstallChecker.DuplicateFiles/
  lecture des observations, commande du module et sérialisation JSON

tests/InstallChecker.DuplicateFiles.Engine.Tests/
  tests unitaires du métier

tests/InstallChecker.DuplicateFiles.Tests/
  tests d'intégration de l'enveloppe

docs/
  spécification, ADR et plan F1
```

Le seul changement extérieur au module prévu par l'implémentation est le routage minimal de la
nouvelle commande dans l'application CLI.

Le moteur métier consomme le port public `IObservationsSource` et les DTO d'observation déjà utilisés
par le module, mais ne dépend ni de SQLite, ni d'Identity.Access, ni du Scanner. Il ne construit pas
un second état W et ne remplace aucune décision du moteur Identity. L'enveloppe conserve les
dépendances techniques existantes sans les modifier.

## 5. Modèle de preuve

Un fournisseur reçoit les observations brutes d'un fichier et produit zéro ou plusieurs preuves.
Il ne regroupe jamais les fichiers et ne prend aucune décision de conservation.

Chaque preuve contient au minimum :

```text
PreuveVersionnee
  FichierId
  Dimension       cle_famille | libelle_famille | identifiant_livraison | version |
                  editeur | format | architecture | langue | edition | distribution
  ValeurBrute
  ValeurNormalisee
  Source
  Force           Forte | Moyenne | Faible
  Regle
  VersionFournisseur
```

La valeur brute permet l'audit. La valeur normalisée sert uniquement au raisonnement réversible du
module et ne remplace jamais l'observation d'origine.

Après arbitrage, un fichier est représenté par :

```text
ArtefactVersionne
  FichierId
  ContenuSha256
  Chemins
  Famille
  VersionBrute
  VersionNormalisee
  SchemaVersion
  Variante
  Confiance
  Preuves
  Conflits
  Blocages
```

## 6. Fournisseurs F1

### 6.1 Nom de fichier

Ce fournisseur s'applique à tous les types de fichiers.

Il recherche un jeton de version délimité par un séparateur et placé à la fin du radical, ou suivi
uniquement de variantes explicitement reconnues. Il retire seulement le jeton reconnu et ces
variantes pour proposer un radical de famille.

Une preuve structurée de format fournie par un fournisseur applicable définit le type. À défaut,
l'extension finale définit le type générique. Les extensions composées courantes `.tar.gz`,
`.tar.bz2` et `.tar.xz` sont traitées comme un seul type. La comparaison du type est insensible à
la casse.

La preuve issue du nom est faible lorsqu'elle est seule. Aucun rapprochement approximatif,
phonétique ou fondé sur une distance de chaînes n'est réalisé.

Les radicaux génériques `setup`, `install`, `installer`, `update` et `package` ne produisent pas de
preuve de famille. Ils peuvent encore produire une preuve de version ou de variante qui renforcera
une famille structurée, sans créer entre eux une famille artificielle à l'échelle du disque.

### 6.2 VersionInfo

Le fournisseur utilise, lorsqu'elles existent :

- `version_info.product_name` pour la famille ;
- `version_info.company_name` pour l'éditeur ;
- `version_info.product_version` comme version produit ;
- `version_info.file_version` comme version technique de repli.

`FileVersion` ne contredit pas automatiquement `ProductVersion`, car les deux valeurs peuvent
décrire des niveaux différents. Elle n'est retenue pour comparer les versions que lorsque la
version produit est absente.

### 6.3 MSI

Le fournisseur utilise :

- `msi_properties.upgrade_code` comme clé native de famille après validation du GUID ;
- `product_name` comme libellé ;
- `manufacturer` comme éditeur ;
- `product_version` comme version ;
- `product_language` comme variante de langue.

`ProductCode` identifie une livraison précise et constitue une preuve secondaire, jamais la clé de
famille principale.

L'architecture MSI n'étant pas observée actuellement, le candidat reste visible avec
`VarianteNonObservee`, une confiance réduite et une revue humaine obligatoire.

### 6.4 MSIX/Appx

Le fournisseur utilise :

- `appx_manifest.name` et `publisher` comme clé native de famille ;
- `version` comme version ;
- `processor_architecture` comme variante d'architecture.

Des architectures connues différentes créent des sous-groupes non comparables.

### 6.5 PE et Authenticode

`pe_info.machine` fournit la variante d'architecture d'un PE. La signature Authenticode fournit une
preuve forte d'éditeur lorsqu'un sujet est observé.

Pour un EXE dépourvu d'identifiant natif de paquet, la famille exige au minimum `ProductName` et
`CompanyName`. Un sujet Authenticode concordant renforce la confiance sans devenir, seul, une clé de
famille.

### 6.6 Extensions futures

Un nouveau format ajoute un fournisseur dans le module et produit le même contrat de preuves. Il ne
modifie ni l'arbitre générique, ni Identity, ni le Scanner lorsque les observations nécessaires
existent déjà. Une nouvelle observation réellement absente relève d'un jalon Scanner séparé.

## 7. Résolution des familles

Les clés de famille sont examinées dans l'ordre suivant :

1. identifiant natif du format, par exemple `UpgradeCode` ou `Name + Publisher` ;
2. `ProductName + CompanyName` ;
3. radical exact dérivé du nom de fichier.

La normalisation des textes se limite à :

- retirer les espaces de début et de fin ;
- réduire les suites d'espaces internes à un espace ;
- comparer sans tenir compte de la casse selon une règle invariante.

La ponctuation, les accents et les suffixes juridiques des entreprises sont conservés. Il n'existe
ni correspondance floue, ni suppression générale de mots, ni repli sur le seul nom lorsque des
preuves structurées se contredisent.

Une contradiction entre deux preuves de famille applicables produit `ConflitDeFamille`. Le fichier
reste explicable dans les diagnostics mais ne participe pas à une relation d'obsolescence.

## 8. Résolution des versions

F1 reconnaît deux schémas comparables.

### 8.1 Version numérique

- un à quatre composants entiers séparés par des points ;
- préfixe `v` optionnel dans un nom de fichier ;
- comparaison numérique composant par composant ;
- zéros initiaux ignorés ;
- composants finaux absents assimilés à zéro.

Ainsi, `1.2` est égal à `1.2.0` et `1.10` est supérieur à `1.9`.

### 8.2 Version calendaire

- date civile valide au format `AAAA-MM-JJ` ou `AAAA.MM.JJ` ;
- comparaison chronologique ;
- aucune comparaison avec une version numérique.

Une valeur à trois composants commençant par une année sur quatre chiffres est d'abord testée comme
date. Si le mois ou le jour est invalide, elle n'est pas requalifiée silencieusement en version
numérique.

### 8.3 Versions non comparables

Les suffixes et canaux comme `alpha`, `beta`, `preview`, `rc`, `revA` ou `final2` sont conservés
comme valeurs brutes mais ne sont pas ordonnés en F1. Une chaîne libre n'est jamais comparée
lexicographiquement.

### 8.4 Priorité et conflits

La version structurée propre au format précède `ProductVersion`, puis une métadonnée générique
explicite, `FileVersion` de repli et enfin le nom du fichier.

Des valeurs normalisées équivalentes se renforcent mutuellement. Si le nom indique `2.0` et la
version produit applicable indique `1.9`, aucune valeur n'est choisie : le fichier reçoit
`ConflitDeVersion` et sa comparaison est bloquée.

## 9. Variantes

La relation « version plus ancienne » n'est construite qu'entre variantes compatibles. Les
dimensions F1 sont :

- type ou format de fichier ;
- architecture ;
- langue ;
- édition explicitement observée ;
- distribution explicitement observée, par exemple portable ou installable.

Les règles sont :

- variantes connues différentes : aucune comparaison ;
- variantes connues identiques : comparaison autorisée ;
- variante absente des deux côtés : comparaison autorisée avec confiance réduite et
  `VarianteNonObservee` ;
- variante connue d'un seul côté : aucune comparaison entre les deux artefacts ;
- langues MSI différentes : sous-groupes distincts par `ProductLanguage` ;
- formats différents : sous-groupes distincts, même si la famille et la version concordent.

Une dimension n'est exigée que lorsqu'elle est applicable : l'architecture pour les exécutables et
paquets qui la déclarent ou devraient la déclarer, la langue pour MSI lorsqu'elle est observée, et
l'édition ou la distribution dès qu'au moins un artefact comparable de la famille porte cette
dimension. L'absence d'architecture sur un PDF ordinaire n'est donc ni un avertissement ni une
perte de confiance.

Un ZIP, un EXE, un MSI et un PDF ne sont donc jamais déclarés interchangeables.

## 10. Construction des candidats

Les artefacts sont indexés par clé de famille et de variante. Les comparaisons sont réalisées dans
chaque groupe, jamais par paires sur l'ensemble du corpus.

Avant la comparaison versionnée, les artefacts de même SHA-256 sont réduits à un contenu logique.
Leurs chemins restent disponibles pour l'affichage, mais ils ne multiplient pas les votes ni les
candidats. Leur traitement actionnable demeure celui des doublons exacts.

Les preuves identiques issues de plusieurs chemins de ce contenu sont dédupliquées. Si des noms de
fichier différents produisent des familles ou versions contradictoires et qu'aucune preuve plus
forte ne résout le désaccord, le contenu reçoit le conflit correspondant au lieu de choisir un nom
arbitrairement. Un chemin sans version n'annule pas une version observée sur un autre chemin du même
contenu.

Dans un groupe comparable :

- la version maximale observée reçoit `ReferenceRecente` ;
- une version strictement inférieure reçoit `VersionAnterieure` ;
- des contenus différents portant la même version reçoivent `MemeVersion` ;
- une preuve illisible reçoit `VersionNonComparable` ;
- les conflits et variantes incomplètes conservent leur statut explicite.

Un groupe public n'est émis que s'il contient au moins deux versions normalisées distinctes et
comparables. Plusieurs contenus portant uniquement la même version sont comptés sous
`MemeVersionSeulement` dans les exclusions ; ils ne deviennent pas, à eux seuls, une redondance
versionnée. `MemeVersion` reste utile dans une famille qui contient par ailleurs au moins deux
versions distinctes.

La catégorie publique d'un groupe F1 est `VersionRedundancyCandidate`. Elle exprime une hypothèse à
examiner, jamais un droit de suppression.

## 11. Confiance

Le niveau initial dépend de la clé de famille :

- `Forte` pour un identifiant natif cohérent et des variantes complètement compatibles ;
- `Moyenne` pour `ProductName + CompanyName` cohérents ;
- `Faible` pour un nom de fichier seul.

Une concordance de famille et de version provenant d'un autre fournisseur augmente le niveau d'un
cran au maximum, sans dépasser `Forte`. Une variante nécessaire absente des deux côtés plafonne le
niveau à `Moyenne`. Une variante connue d'un seul côté ou un conflit bloque la relation, quel que
soit le niveau calculé. Deux preuves sont indépendantes lorsque leur champ `Source` désigne deux
fournisseurs différents.

La confiance sert à ordonner et filtrer l'affichage. Elle n'autorise aucune action destructive.

## 12. Contrat de rapport

Le contrat public est :

```text
duplicate-files/version-redundancy/v1
```

Le rapport contient :

```text
RapportRedondanceVersionnee
  VersionContrat
  Source
  Synthese
  Groupes
  ExclusionsParMotif
```

Chaque groupe contient son identifiant stable, sa famille, sa variante, sa confiance, sa version de
référence, ses artefacts triés, ses preuves, ses avertissements et ses blocages.

Le rôle de comparaison, lorsqu'il existe, est l'une des valeurs suivantes :

- `ReferenceRecente` ;
- `VersionAnterieure` ;
- `MemeVersion`.

Les diagnostics structurés, indépendants du rôle, sont :

- `VersionNonComparable` ;
- `ConflitDeVersion` ;
- `ConflitDeFamille` ;
- `VarianteNonObservee`.

Un MSI ancien peut ainsi porter simultanément le rôle `VersionAnterieure` et le diagnostic
`VarianteNonObservee`. Un conflit bloquant ne reçoit aucun rôle de comparaison.

Les seules actions descriptives sont `Examiner` et `Ignorer`. Les blocages structurés comprennent :

- `RevueHumaineObligatoire` ;
- `SuppressionAutomatiqueInterdite` ;
- `ConfianceFaible` ;
- `VarianteNonObservee` ;
- `MetadonneesContradictoires`.

Les fichiers sans aucune version exploitable ne sont pas tous sérialisés. Ils sont comptés dans
`ExclusionsParMotif`. Les fichiers ayant participé à une famille candidate conservent leurs preuves
utiles et leurs valeurs brutes.

Les valeurs fermées sont sérialisées sous forme de chaînes. L'ordre des groupes, artefacts, preuves
et blocages est déterministe.

## 13. Identifiants stables

Un groupe versionné reçoit un identifiant de la forme :

```text
version:sha256:<SHA-256 de la clé canonique versionnée>
```

La clé canonique encode en UTF-8, avec préfixe de longueur pour chaque champ :

```text
version-family/v1
source de famille
clé de famille normalisée
schéma de version numérique ou calendaire
type de fichier
architecture ou marqueur absent
langue ou marqueur absent
édition ou marqueur absent
distribution ou marqueur absent
```

Elle n'inclut ni la valeur de version du fichier, ni le chemin, ni l'identifiant d'acte, afin que le
groupe survive à un nouveau scan et à l'arrivée d'une version plus récente. Les marqueurs d'absence
sont distincts d'une chaîne vide observée.

Les fichiers réutilisent le calcul stable existant fondé sur le SHA-256 du contenu et le chemin
canonique. Un même contenu au même chemin conserve donc le même `FichierId` dans les rapports exact
et versionné.

## 14. Commande et erreurs

La route CLI prévue est :

```text
installchecker duplicates versions <base.db>
```

Elle doit être testée avant la route historique `duplicates <base.db> <registre>` afin de ne pas
être interprétée comme ses deux arguments. Elle produit le rapport JSON sur stdout.

F1 lit le même état courant multi-volume que les autres consommateurs de la base. Il ne demande pas
de registre, car les règles de redondance versionnée sont celles du module et ne modifient pas la
dérivation W du moteur Identity.

Les erreurs locales deviennent des diagnostics : version illisible, GUID MSI invalide, attribut
absent, fournisseur non applicable ou métadonnées contradictoires. Les autres fournisseurs du même
fichier continuent.

Une base absente, illisible ou d'une version non prise en charge est une erreur globale : code `1`,
message sur stderr et aucune sortie JSON partielle. Un usage CLI incorrect retourne `2`. Un rapport
valide, même vide ou composé uniquement de candidats bloqués, retourne `0`.

## 15. Performance

- un parcours des fichiers observés ;
- un nombre borné de fournisseurs par fichier ;
- des dictionnaires indexés par famille, variante et contenu ;
- aucune comparaison quadratique globale ;
- complexité cible proche de `O(n)` avant les tris de sortie ;
- mémoire proportionnelle aux fichiers possédant au moins une preuve utile ;
- aucune optimisation sans mesure préalable.

Première mesure reproductible, réalisée en `Release` sur 100 000 observations réparties en 10 000
familles de dix versions :

- durée du générateur : de `5,122 s` à `5,994 s` sur deux passages ;
- allocation gérée du générateur : de `1510,9 MiB` à `1511,6 MiB` ;
- résultat : 10 000 groupes et 90 000 versions antérieures.

Configuration : Windows `10.0.26200` x64, .NET SDK `10.0.302`, 16 processeurs logiques, environ
24 Gio de mémoire disponible, processeur déclaré `Intel64 Family 6 Model 165 Stepping 5`. Le corpus
est construit avant le relevé d'allocation. Aucun seuil temporel ou mémoire n'est imposé par cette
première mesure ; l'allocation constitue une base chiffrée pour une optimisation ultérieure.

## 16. Robustesse

Une erreur d'un fournisseur est isolée au fichier concerné et normalisée en diagnostic. Les textes
variables des exceptions système ne font pas partie du contrat JSON public.

Une contradiction est un résultat métier attendu, pas une exception. Une incohérence interne qui
rendrait le rapport trompeur reste une défaillance d'implémentation et ne doit pas être masquée par
la CLI.

Le rapport ne contient ni horodatage courant, ni identifiant aléatoire. Le même état observé avec la
même version des fournisseurs produit le même JSON.

## 17. Tests d'acceptation

### Fournisseurs

- extraire une version depuis les noms de ZIP, PDF et autres fichiers génériques ;
- extraire famille, éditeur et versions depuis VersionInfo ;
- extraire la famille MSI depuis un `UpgradeCode` valide ;
- séparer les langues MSI ;
- extraire famille, version et architecture MSIX/Appx ;
- enrichir un EXE avec son architecture PE et son éditeur Authenticode ;
- isoler les observations absentes, invalides et non applicables.

### Résolution

- considérer `1.2` et `1.2.0` comme équivalentes ;
- considérer `1.10` comme supérieure à `1.9` ;
- comparer deux dates valides ;
- refuser une comparaison entre version numérique et calendaire ;
- refuser les suffixes préliminaires en F1 ;
- privilégier `ProductVersion` et utiliser `FileVersion` seulement en repli ;
- produire `ConflitDeVersion` lorsque nom et métadonnée applicable divergent ;
- produire `ConflitDeFamille` lorsque les preuves structurées divergent.

### Groupes et variantes

- détecter deux versions d'un ZIP et deux versions d'un PDF ;
- regrouper des MSI de même `UpgradeCode` ;
- regrouper des EXE de même produit et même éditeur ;
- ne jamais comparer des architectures, langues, éditions ou formats différents ;
- conserver un MSI sans architecture comme candidat à confiance réduite ;
- écarter du raisonnement versionné les contenus de même SHA-256 ;
- ne pas émettre un groupe qui ne contient que plusieurs contenus de même version ;
- ne jamais rapprocher deux familles seulement similaires.

### Contrat et intégration

- produire `duplicate-files/version-redundancy/v1` avec des enums en chaînes ;
- permettre à un artefact d'exposer un rôle et `VarianteNonObservee` simultanément ;
- conserver des identifiants et un ordre stables entre deux scans logiquement équivalents ;
- n'exposer aucune action de suppression ;
- produire un rapport vide valide lorsqu'aucun candidat n'existe ;
- lire une base synthétique et restituer les erreurs contractuelles attendues ;
- préserver tous les tests existants du module et de la solution ;
- constater un diff vide dans le périmètre Identity scellé ;
- mesurer un corpus synthétique volumineux avant toute optimisation.
