# 016 — Transition de la v1 à la v2

**Statut** : premier document de la phase v2 de la série `docs/identity/`. S'appuie sur les documents 000→015, figés, et sur l'état livré de la v1 — moteur déclaré conforme après les revues d'audit de clôture, corrections de clôture intégrées (§ 1.1), référence figée par le tag `identity-v1.0`.
**Nature** : un document de transition. Il ne définit ni théorie nouvelle, ni contrat nouveau, ni implémentation — aux seules exceptions du raffinement assumé ci-dessous et des invariants I57 à I60 (§ 7) — et fixe ce que la v1 établit définitivement, ce qui demeure gelé, ce qui est reporté, et ce que la v2 devra préserver. Aucun code, aucun pseudo-code, aucune API, aucune fonctionnalité qui n'ait déjà été actée par la série ou par les revues de clôture.
**Raffinement assumé** : ce document raffine le 011 sur un point précis — le régime des familles de conventions théorisées mais non encore applicables par le moteur (§ 5.1) ; l'écart avec la lettre du 011 § 10 y est motivé. Le seul autre contenu normatif nouveau du présent document est l'ensemble des invariants I57 à I60 (§ 7).
**Règle de rédaction** : tout point reporté est restitué **intégralement dans le présent document** — aucun renvoi à un rapport extérieur à la série. C'est la leçon directe d'un défaut constaté en v1 (§ 4.2, report 10 : les références documentaires pendantes) : un document normatif ne cite jamais ce que le dépôt ne contient pas.

---

## 1. Ce que la v1 établit définitivement

### 1.1 Les acquis

- **La théorie** (000→009) : les objets — observations, signaux, hypothèses, conventions, strates, états du monde, conventions d'élection, règles de résolution —, leurs propriétés, les invariants **I1 à I36**, les limites L1–L8. Aucune définition ni aucun invariant de 000→008 n'a exigé de révision : la théorie proprement dite a tenu ; les réconciliations encore dues sont inventoriées au § 4.2 (reports 6 et 12).
- **Le contrat d'implémentation** (010→015) : les exigences **EXG-01 à EXG-39**, le contrat public du moteur, la machine abstraite C1→C7, l'architecture, les contrats internes, les invariants **I37 à I56**.
- **Le registre ℛ₀**, matérialisé sous `registre/` et vérifié **identique au caractère près** aux textes normatifs du 015 (§§ 4, 5, 6.4, 7.5) — la « copie conforme » exigée par 015 § 11 est un fait contrôlé, pas une intention.
- **L'oracle empirique** : la base archivée `tests/oracle/corpus1-postA1.db` (497 actes, 381 classes de contenu), support permanent du quadruplet EXG-39.
- **Le moteur v1** : les sept responsabilités C1→C7, la transition τ, les chaînes d'audit, le contrat d'erreur exercé à ses deux frontières — y compris, depuis les corrections de clôture : une base d'observations vide produit un W entier, sans acte et sans erreur ; toute violation du contrat de Ω produit l'erreur nommée « Ω invalide » (011 § 5), jamais une exception hors contrat ; la restitution d'audit ne laisse fuir aucune exception non contractuelle, restitue tel quel tout motif du vocabulaire normalisé (014 § 7.4), et ses réponses sont indépendantes de l'agrégation canonique des refus (014 § 7.3).
- **W₀ produit et vérifié acte par acte** : 116 actes — 112 élections de contenu (108 paires, 4 triplets) au niveau « certaine », licenciées CE-01 v1 avec dépendance EQ-01 v1, et 4 refus motivés — conformes au dénombrement du 014 § 8, dérivés de l'artefact versionné.
- **Le corpus de non-régression** : 100 tests Identity et 39 tests pipeline, tous verts, rejouables par un tiers depuis le dépôt seul.

### 1.2 La référence figée

Le tag **`identity-v1.0`** désigne l'état du dépôt qui fait foi pour la v1 : documents 000→015, registre ℛ₀, oracle, moteur corrigé, tests. Cet état est re-dérivable à jamais (I23, EXG-25) : appliqué à tout couple (Ω, ℛ) qu'il connaît, le moteur v1 reproduit ses sorties historiques. La v2 le **remplacera** ; elle ne le réécrira pas.

### 1.3 Le périmètre exact de la conformité déclarée

La conformité v1 a été déclarée **au niveau du contenu logique de W** : chaque acte de W₀ vérifié champ par champ, déterminisme et indépendance à l'ordre prouvés sur des représentations internes. La **forme canonique matérielle** de W (013 § 4 : document unique, encodage spécifié, identité bit à bit par égalité de fichiers) et le fichier W₀ attendu produit par un oracle indépendant (014 § 10, É7) **n'ont pas été livrés** : c'est le report 3 du § 4.1. La déclaration de conformité v1 est donc relative à l'inventaire du § 4, connu, consigné et daté — pas absolue.

---

## 2. Ce qui reste volontairement gelé

| Objet gelé | Régime | Fondement |
|---|---|---|
| **documents 000→015** | toute évolution passe par un acte documentaire propre, jamais par le code ni par le registre | I36 |
| **le pipeline d'observation** | producteur pur d'Ω, contrat `user_version = 1` ; aucune modification hors bug avéré d'extraction — le seul cas autorisant une modification du code du pipeline (002 § 8.2) | 002 § 8.2, 013 § 2 |
| **ℛ₀ comme état** | les versions adoptées (`EQ-01/v1.md`, `CE-01/v1.md`) sont immuables ; le registre évolue exclusivement par les cinq opérations tracées | 013 § 3.1, 007 § 10 |
| **l'artefact `corpus1-postA1.db`** | immuable ; toute campagne future produit un artefact nouveau | 013 § 11 |
| **W₀ comme oracle** | tout écart d'une implémentation est un défaut de l'implémentation ; la révision de l'oracle est un acte documentaire préalable, jamais une accommodation du code | EXG-28, I36 |

---

## 3. Les trois voies d'évolution — rappel opposable

Le 015 (§ 11) a clos la v1 sur une règle que la v2 reprend sans l'amender : toute évolution du système passe par l'une de ces trois voies, jamais une quatrième —

1. **une évolution de registre** (adoption, révision, retrait, remplacement, scission, fusion — un commit de gouvernance sous `registre/`) ;
2. **une évolution de théorie** (un document de la série, postérieur au présent 016) ;
3. **une implémentation** (du code conforme aux contrats 010→015, sans liberté normative résiduelle).

Le § 4 classe chaque report dans sa voie. La règle de précédence est celle de I36 : lorsque le report exige à la fois un acte documentaire et du code, **le document précède le code**.

---

## 4. L'inventaire des reports

Les revues de clôture de la v1 ont établi un inventaire de points volontairement non traités. Il est restitué ici intégralement — chaque report cite les passages qui le fondent. Les numéros ci-dessous sont les identifiants de suivi de la v2.

### 4.1 Reports vers l'implémentation (voie 3)

| # | Report | Contrat concerné | Objectif v2 |
|---|---|---|---|
| **1** | le moteur applique les conventions **par identifiant codé** (EQ-01, CE-01), non par famille ; une convention en vigueur qu'il ne sait pas appliquer est silencieusement ignorée | 011 § 10 (« le moteur n'a pas besoin de changer pour que le système apprenne »), 012 § 1.1, EXG-13, I34 | appliquer chaque convention selon sa **famille** ; toute convention en vigueur non applicable par le moteur produit une **erreur nommée**, jamais un W silencieusement partiel — cette erreur n'existant pas dans la liste close du 011 § 5, son introduction est une extension du contrat d'erreur : un **acte documentaire préalable** à l'implémentation (§ 3 : le document précède le code) |
| **2** | la fonction `(Ω, ℛ) → W` (EXG-01) n'a pas de porteur : la composition C1→C6 n'existe que dans les tests ; le consommateur CLI (jalon É9 du 013 § 8) n'a pas été livré tel que défini | EXG-01, 011 § 4 (« entier ou absent », préconditions), EXG-15 | un composant unique portant les préconditions, les postconditions et le contrat d'erreur de la boîte noire (011) ; le consommateur prévu par 013 § 1.1 |
| **3** | la forme canonique matérielle de W n'existe pas ; le test d'or bit à bit contre un W₀ produit par oracle **indépendant** n'existe pas | 013 § 4, 014 §§ 7–8 et § 10 (É7), EXG-18, EXG-26 | la sérialisation canonique spécifiée, le fichier W₀ attendu produit hors moteur et versionné, l'égalité de fichiers comme test de conformité |
| **4** | C6 ne vérifie pas la cohérence d'état avant livraison | 006 § 3, 012 § 2, 014 C6 (« garantit la vérification de cohérence d'état ... refuse un ensemble d'actes incohérent ») | la vérification effective, préalable à toute livraison de W |

### 4.2 Reports vers la théorie (voie 2 — acte documentaire préalable, I36)

| # | Report | Documents concernés | Objectif v2 |
|---|---|---|---|
| **5** | l'empreinte d'état d'Ω n'inclut pas les identifiants d'actes (deux Ω de mêmes contenus et de numérotations différentes partagent un index alors que leurs W diffèrent) ; la concaténation n'est non ambiguë que si l'empreinte est de longueur fixe, ce que le contrat de Ω ne garantit pas ; la fonction d'empreinte du support n'est pas exposée par ce contrat | 014 § 7.2, 014 § 6 | réviser la définition de l'identité d'un état d'Ω avant toute réimplémentation |
| **6** | la carte des refus de W₀ diverge entre documents : 009 § 6 (et EXG-26 qui la cite) décrit des refus **structurels** (silence L2, sous-détermination), 014 § 8 fixe quatre refus **normatifs** — le raffinement (« motif = premier maillon manquant ») n'a jamais été acté par révision de 009/010 | 009 § 6, 010 EXG-26, 014 §§ 7.3 et 8 | un acte documentaire unique réconciliant la carte, l'exigence et la règle du motif canonique |
| **7** | le journal et l'état du registre : 015 § 6.2 annonce « cinq sous-sections » quand sa grammaire en montre quatre ; la **date** d'une entrée ne vit que dans un titre « jamais lu par C2 » — l'ordre chronologique (015 § 6.3, check-list § 8 point 10) est invérifiable mécaniquement ; les points 17, 18 et 19 de la check-list (compte d'actes, date logique, index documentaire) sont présentés comme causes de rejet sans être exigibles de C2 par 014 § 5.3 ; l'exemple du 015 § 7.3 contredit sa propre règle (« triée par ordre alphabétique d'identifiant », puis, pour ℛ₀, EQ-01 cité avant CE-01 — ordre que le § 7.5 du même document donne correctement) | 015 §§ 6.2–6.3, 7.3–7.4, 8 ; 014 § 5 | fixer la représentation normative de la date des entrées, le partage exact des contrôles entre C2 et la relecture humaine, et corriger l'exemple fautif |
| **8** | la référence d'acte (strate, plus petit identifiant du domaine) n'est pas garantie **totale** : deux actes d'une même strate peuvent partager leur plus petit identifiant dès que des refus partiels coexistent avec des élections | 012 § 5 (« clé totale »), 014 § 7.5 | garantir l'unicité de la référence, ou la redéfinir |
| **9** | τ : la cause est fournie par l'appelant sans vérification contre les deux index ; les continuités déclarées ne sont jamais peuplées, alors que 006 (E5) décrit des continuités triviales | 006 §§ 5–7, EXG-30, 014 § 7.5 | spécifier la dérivation et la vérification de la cause, et le régime des continuités |
| **10** | hygiène documentaire et de dépôt : trois références pendantes (009, « l'écart documenté en fin de document » ; 011 § 5, « motivé en fin de document » ; 014 § 7.3, « le rapport de livraison ») ; le vocabulaire des **espèces** de refus (normatif / structurel) employé de façon relâchée dans des commentaires ; la suite de tests Identity dépend du projet pipeline, contre le graphe du 013 §§ 1.2 et 2 ; le `CLAUDE.md` du dépôt à réconcilier avec les sections que le 013 cite | 009, 011 § 5, 014 § 7.3, 013 §§ 1.2 et 2 | résorber chaque référence pendante par un acte documentaire ; rétablir la séparation des suites de tests |
| **11** | le contrat de C5 ne prévoit pas l'une de ses entrées réelles : la clause « reçoit » (014 § 1, C5 : « les productions de C4 + les conventions d'élection et de priorité ») ne porte pas l'énumération des identifiants d'actes d'Ω dont les refus de domaine maximal ont besoin (014 § 8 : « les 497 actes, énumérés ») — les productions de C4 ne couvrent que les domaines des hypothèses, jamais la totalité des actes | 014 § 1 (C5), 014 § 8 | réviser la clause « reçoit » de C5 pour couvrir l'entrée réellement nécessaire |
| **12** | la complétude des actes sur un domaine multi-actes **sans signal constructible** est indécidée : 005 § 11 pose que les hypothèses extrêmes existent « pour toute granularité et tout domaine » (l'espace n'est jamais vide), 006 § 3 interdit les zones muettes, et 009 § 5 restreint la complétude « aux domaines où l'espace d'hypothèses est non trivial » — la réconciliation n'est faite que pour les classes singletons | 005 § 11, 006 § 3, 009 § 5 | trancher la notion d'espace trivial au-delà des singletons |

---

## 5. Les limitations : celles qui deviennent des objectifs, celles qui demeurent

### 5.1 Les restrictions de la v1 qui deviennent des objectifs

Ces restrictions ne sont pas des défauts : elles sont l'application du moindre engagement (P7) au plan de réalisation lui-même (013 § 7 : « on construit ce que ℛ₀ exige, rien de ce que ℛ₁ exigera peut-être »). La v2 a pour objectif de les lever **au rythme du registre**, jamais en avance sur lui :

- **une seule famille appliquée** (interprétation, via EQ-01), **une seule licence** (CE-01), **une seule strate décidée** (contenu) : la feuille de route du 009 § 7 reste la feuille de route — interprétations, équivalences, l'adoption formelle de A-01, attentes, compositions, conventions de versions, conventions de familles — chaque adoption étant une transition de ℛ. Le 011 § 10 ne connaît que deux cas : la convention d'une famille connue (une donnée — « le moteur n'a pas besoin de changer ») et la famille nouvelle pour la théorie (théorie d'abord, moteur ensuite). La clôture de la v1 en a révélé un troisième, que le 011 ne prévoit pas : la famille **théorisée mais que le moteur ne sait pas encore appliquer** (report 1). **Raffinement assumé du 011 § 10** : rendre une telle famille applicable est une évolution du moteur, intégralement revalidée (011 § 8) ; une fois la famille applicable, la lettre du 011 § 10 redevient exacte — toute convention supplémentaire de cette famille est une donnée, sans changement de moteur ;
- **la restitution d'audit** n'enrichit que les motifs que ℛ₀ produit ; les motifs réservés du 014 § 7.4 sont restitués tels quels — l'enrichissement suivra les motifs réellement produits par les registres futurs ;
- **τ** n'a jamais été exercé sur une transition réelle d'index : les scénarios de transitions Ω et ℛ du 013 § 9 restent à jouer ;
- **le corpus 2** demeure le préalable empirique des familles hautes (009 § 7, dernier alinéa ; I35 : la mesure rend adopté) et le point d'entrée du travail d'échelle (013 § 10 : jamais avant).

### 5.2 Les limites théoriques demeurent

Les limites **L1 à L8** du 000 § 7 ne sont pas des objectifs et ne le deviendront jamais (I60) : la sous-détermination (L1), l'observation vide (L2), les signaux contradictoires (L3), les artefacts inconnus (L4), l'indiscernabilité des distributions rebadgées (L5), les frontières conventionnelles des strates (L6), le socle probabiliste (L7) et l'ouverture du domaine (L8) bornent la v2 exactement comme elles bornaient la v1. Un refus structurel de la v1 reste un refus structurel de la v2 : il ne cède qu'à Ω — et certains jamais.

---

## 6. La compatibilité : ce que la v2 doit préserver

- **(Ω_corpus1, ℛ₀) → W₀**, à l'acte près — et, dès que le report 3 est livré, au bit près sur la forme canonique. W₀ reste le premier oracle de tout moteur, v2 comprise (EXG-26, EXG-39) ;
- **des sorties identiques sur tout index passé** (011 § 9) : une version nouvelle du moteur qui change W à index constant est non conforme — ou prouve que l'ancienne l'était, auquel cas l'écart est publié et justifié acte par acte ;
- **le contrat public du 011, jamais rétréci** : les extensions ajoutent, elles ne retirent pas (011 § 10) — la forme des sept questions d'audit, la sémantique de W et de τ, les six erreurs nommées demeurent ;
- **la grammaire du registre** (015 § 3), démontrée stable sous extension (I56) : aucun identifiant réattribué, aucune version rééditée, aucun cinquième type de document ;
- **le contrat de Ω version 1** toujours supporté — la rejouabilité historique l'exige (011 § 6, EXG-25 : tout index passé, l'oracle compris (EXG-39), est au contrat v1 et reste invocable) ; une version nouvelle du contrat d'observations s'ajoute selon le régime du 011 § 9, elle ne retire pas la v1 ;
- **les invariants I1 à I56**, normatifs sans exception (I58) — la v2 en ajoute, elle n'en retranche pas.

---

## 7. Invariants

> **I57 — La v1 est une référence immuable.** Le tag `identity-v1.0` désigne un état re-dérivable à jamais : appliqué à tout index qu'il connaît, le moteur v1 reproduit ses sorties historiques (I23, EXG-25). Aucun travail de v2 ne modifie, ne réécrit ni ne réinterprète cet état — la v2 le remplace, elle ne le corrige pas rétroactivement ; une non-conformité v1 découverte après coup suit la voie du 011 § 9 : écart publié, justifié acte par acte.

> **I58 — Aucun objectif de v2 n'affaiblit un invariant antérieur.** I1 à I56 demeurent le contrat. Toute tension entre un objectif et un invariant se résout par une révision documentaire explicite du document porteur (I36) — jamais par le code, jamais par une réinterprétation, jamais par une exception « pragmatique ».

> **I59 — La conformité v2 contient la conformité v1.** Tout moteur v2 produit exactement W₀ depuis (Ω_corpus1, ℛ₀), et des sorties identiques à celles de la v1 sur tout index que la v1 connaissait — sous les seules exceptions déjà prévues : la correction publiée d'une non-conformité antérieure (011 § 9) ou la révision documentaire préalable de l'oracle (EXG-28, I36 ; le report 6 en est le cas d'espèce déjà identifié).

> **I60 — Les limites théoriques ne sont pas des objectifs.** L1–L8 bornent tout moteur respectant A0–A1 (000 § 7). Aucun objectif de v2 ne prétend lever une sous-détermination, donner prise au silence ou discerner l'indiscernable ; un objectif qui l'exigerait est le signe qu'une révision du 000 est nécessaire — un acte documentaire, préalable et explicite, jamais une ambition de code.

---

## Récapitulatif

| Objet | Contenu | § |
|---|---|---|
| acquis de la v1 | théorie 000→009 (I1–I36), contrat 010→015 (EXG-01→39, I37–I56), ℛ₀ conforme au caractère près, oracle archivé, moteur C1→C7 avec les corrections de clôture, W₀ vérifié acte par acte, 100 + 39 tests | 1.1 |
| référence figée | tag `identity-v1.0` — état re-dérivable à jamais, remplacé et jamais réécrit | 1.2, I57 |
| périmètre de conformité | contenu logique de W vérifié ; forme canonique matérielle non livrée (report 3) — conformité relative à l'inventaire, pas absolue | 1.3 |
| gels | documents 000→015, pipeline, ℛ₀, artefact corpus 1, W₀ comme oracle | 2 |
| voies d'évolution | registre / théorie / implémentation — jamais une quatrième ; le document précède le code | 3 |
| reports moteur | 1 familles et conventions inapplicables, 2 fonction (Ω, ℛ) → W et consommateur, 3 forme canonique et oracle indépendant, 4 cohérence d'état de C6 | 4.1 |
| reports théorie | 5 identité de l'état d'Ω, 6 carte des refus de W₀, 7 journal et état du registre, 8 référence d'acte totale, 9 cause et continuités de τ, 10 hygiène documentaire, 11 contrat de C5, 12 complétude au-delà des singletons | 4.2 |
| restrictions → objectifs | familles/licences/strates au rythme du registre (feuille de route 009 § 7, raffinement assumé du 011 § 10), motifs d'audit, transitions réelles de τ, corpus 2 | 5.1 |
| limites qui demeurent | L1–L8, jamais des objectifs | 5.2, I60 |
| compatibilité | W₀ préservé, sorties identiques sur index passés, contrat public jamais rétréci, grammaire du registre stable, contrat Ω v1 supporté, I1–I56 intacts | 6 |
| invariants | I57 référence immuable, I58 aucun affaiblissement, I59 la conformité v2 contient la v1, I60 les limites ne sont pas des objectifs | 7 |

**Ce que ce document ne fait volontairement pas** : réviser un document antérieur (chaque report du § 4.2 exigera son acte propre), adopter une convention, ouvrir le corpus 2, définir l'architecture de la v2, découper un plan d'étapes, écrire du code, fixer un calendrier.
