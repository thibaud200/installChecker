# 005 — Stratification de l'identité logique

**Statut** : sixième document de la série `docs/identity/`. S'appuie exclusivement sur les documents 000 à 004, validés et figés.
**Périmètre** : rendre entièrement formels les niveaux d'identité que le 000 (§ 4) introduisait intuitivement — contenu, variante, version, identité, famille. **On ne décide jamais ici qu'un contenu appartient à une strate** : on définit ce qu'est une strate. Aucun code, aucun algorithme, aucun score, aucun seuil, aucun regroupement automatique, aucune donnée hors corpus 1.
**Thèse du document** : les strates ne sont pas des décisions — ce sont des **espaces mathématiques** dans lesquels les hypothèses pourront ensuite être organisées. Aucun regroupement réel n'est effectué.

---

## 1. La strate, objet formel

**Définition 1 (strate)** — Une strate est un **niveau d'abstraction construit sur les hypothèses** : l'espace de toutes les hypothèses d'origine commune formulables *à une granularité donnée*, muni de son appareil complet. Formellement, une strate 𝒮 possède :

- **son domaine** : les actes d'observation (ou classes ≡ₘ) que ses hypothèses peuvent couvrir — hérité du corpus, jamais choisi par la strate ;
- **ses conventions** : le sous-ensemble de K qui la gouverne — en premier lieu les conventions de stratification (§ 4) qui fixent sa granularité, plus les conventions d'interprétation, d'équivalence, de priorité et d'attente que ses hypothèses mobilisent ;
- **ses hypothèses** : toutes les hypothèses (003, Déf. 1) dont le contenu propositionnel postule une origine commune *à cette granularité* — le paramètre de strate que le 003 mentionnait (« à une strate donnée ») devient ici un constituant formel du contenu propositionnel ;
- **ses contradictions** : les contradictions (002 § 9 ; 003 § 5) évaluées relativement à ses hypothèses — la même paire d'observations peut être contradictoire pour une strate fine et indifférente pour une strate grossière ;
- **sa préférence** : l'ordre partiel ⪯ (003 § 4) restreint à ses hypothèses — les préférences ne se comparent jamais *entre* strates.

Deux exclusions constitutives : **une strate n'est jamais un signal** (elle ne consomme pas d'observations interprétées — elle organise des hypothèses, deux couches plus haut) ; **une strate n'est jamais une observation** (rien dans Ω ne porte une strate ; A0 tient).

Une strate est donc un *espace muni d'une structure* — au sens où l'on parle d'un espace de fonctions ou d'un espace d'états — et non un découpage effectif : l'espace existe avant toute élection, et indépendamment d'elle.

---

## 2. Le raffinement

**Définition 2 (raffinement)** — Une strate 𝒮′ *raffine* une strate 𝒮 (noté 𝒮′ ⊑ 𝒮) lorsque tout postulat d'origine commune à la granularité de 𝒮′ **implique** le postulat correspondant à la granularité de 𝒮 : si une hypothèse de 𝒮′ range deux actes sous une même origine fine, alors toute hypothèse de 𝒮 co-retenue avec elle doit les ranger sous une même origine grossière. En termes de relations postulées : l'équivalence hypothétique de 𝒮′ est, sur tout domaine commun, **incluse comme relation** dans celle de 𝒮.

Conséquences :

- **une strate fine ne peut jamais contredire une strate plus grossière** : elle ne peut que la raffiner. Affirmer « même variante » tout en niant « même version » n'est pas une tension à arbitrer — c'est une **incohérence verticale**, interdite par construction (I17, § 12). Le conflit apparent se reformule toujours : soit l'hypothèse fine est fausse, soit la grossière l'est — jamais « les deux tiennent » ;
- le raffinement est un **ordre partiel sur les strates** (réflexif, transitif, antisymétrique) ;
- le raffinement contraint les **co-rétentions**, pas les espaces : chaque strate contient toujours toutes ses hypothèses (I19) ; c'est l'acte futur de retenir *ensemble* une hypothèse fine et une grossière qui est soumis à la contrainte.

---

## 3. L'emboîtement des cinq niveaux

Le système comporte cinq niveaux. Quatre forment une chaîne de raffinement ; le cinquième est d'une autre nature mathématique (§ 8).

  **contenu ⊑ variante ⊑ version ⊑ identité**  (chaîne de raffinement)
  **famille** (construite *sur* le niveau identité, hors de la chaîne)

Propriétés de chaque niveau — sans jamais dire comment les calculer :

| Niveau | Nature | Ce qui distingue deux éléments | Statut |
|---|---|---|---|
| **contenu** | équivalence effective ≡ₘ | les octets eux-mêmes | la seule strate **observée** ; espace d'hypothèses dégénéré (003 § 9) |
| **variante** | strate hypothétique la plus fine | des matérialisations simultanées d'un même état édité (§ 6) | hypothétique |
| **version** | strate hypothétique | des états édités successifs ou parallèles d'une même origine (§ 5) | hypothétique |
| **identité** | strate hypothétique la plus grossière de la chaîne | des origines éditoriales distinctes (§ 7) | hypothétique ; l'objet central |
| **famille** | relation de parenté entre éléments du niveau identité | — (pas une partition, § 8) | hypothétique, non emboîtée |

La chaîne reprend l'emboîtement du 000 (§ 4.1) et lui donne son statut exact : ce sont les **espaces** qui s'emboîtent par la contrainte de raffinement — pas des découpages déjà faits, puisqu'aucun découpage n'existe encore.

---

## 4. Les conventions de stratification

**Définition 3 (convention de stratification)** — Une convention κ ∈ K (forme complète du 004, Déf. 1 : identifiant, version, domaine d'application, justification, historique, provenance documentaire) dont la transformation T(κ) **ne produit pas un signal** mais une **organisation d'hypothèses** : elle fixe, pour une portion du répertoire des différences possibles entre contenus, la granularité à laquelle cette différence est conventionnellement lue — intra-variante, inter-variantes, inter-versions, inter-identités.

La distinction demandée est celle-ci :

> **Interpréter** (002, 004 § 3) : transformer une observation en propriété conventionnelle — opération de la couche 2, dont la sortie est un signal.
> **Stratifier** : fixer à quelle granularité une différence *déjà interprétée* est lue — opération de la couche normative sur l'**espace des hypothèses**, dont la sortie est un placement (quel contenu propositionnel appartient à quelle strate), jamais un signal, jamais une élection.

Propriétés, héritées du 004 :

- une convention de stratification obéit à **I13** (explicite, versionnée) et **I14** (minimale : une frontière conceptuelle par convention — la frontière version/variante pour un type de différence donné est une convention ; la frontière identité/version en est une autre) ;
- elle obéit à **I16** : elle ne modifie jamais le domaine explicatif d'une hypothèse ni son contenu — elle classe des contenus propositionnels par granularité ; réviser une convention de stratification re-place des hypothèses (I20, § 12), elle n'en crée ni n'en détruit (I19) ;
- elle rend enfin formelle la limite L6 du 000 : les frontières de strates sont **conventionnelles, non découvrables** — deux états de K peuvent stratifier différemment les mêmes hypothèses, tous deux cohérents. Aucune convention de stratification n'est adoptée dans ce document.

---

## 5. La version

**Définition 4 (version)** — Une version est un **objet logique** : une classe d'équivalence hypothétique à la strate version — un *état édité postulé* d'une origine commune, tel que ses matérialisations sont supposées relever du même geste de publication. Une version n'est **jamais une valeur observée** : les observations ne font que **proposer des indices** (déclarations, structures, signatures), et aucun attribut de Ω n'est ni nécessaire ni suffisant pour constituer une version.

**Définition 5 (ordre des versions)** — Les versions d'une même identité peuvent être munies d'une **relation de succession postulée** : « v′ succède à v » est une *hypothèse* (avec domaine, signaux mobilisés, provenance, résidu — le quintuplet du 003 s'applique), jamais un fait. La clôture de la succession engendre un **ordre partiel** sur les versions d'une identité.

- **Branches** : une branche est une **chaîne** de cet ordre partiel (000, Déf. 9) — une lignée totalement ordonnée en son sein. L'ordre global reste partiel : plusieurs branches coexistent.
- **Comparabilité** : deux versions sont comparables si la succession postulée les relie (directement ou transitivement) — la transitivité est ici licite car elle opère *à l'intérieur d'une même relation postulée*, pas entre relations distinctes (la prohibition du 003 § 8.2 vise la composition inter-relations, non la clôture d'un ordre unique).
- **Incomparabilité** : deux versions incomparables le sont pour l'une de deux raisons qu'il faut distinguer — *ignorance* (aucun indice ne relie les branches : l'ordre pourrait exister, il n'est pas soutenu) ou *parallélisme postulé* (l'hypothèse d'ordre affirme des lignées disjointes). Le premier cas est un silence ; le second un contenu.
- **Aucune convention d'ordre n'est imposée** : la forme des indices qui soutiendront une succession (déclarations mises en forme canonique, chronologies) relève de conventions d'interprétation futures ; l'ordre lui-même reste une hypothèse soumise à préférence et révision.

---

## 6. La variante

**Définition 6 (variante)** — Une variante est une classe d'équivalence hypothétique à la strate la plus fine : des matérialisations **distinctes mais simultanées** d'un même état édité, destinées à des contextes de déploiement différents. La variante raffine la version (§ 2) — mais elle **ne représente pas une évolution** : la relation entre variantes d'une même version n'est pas la succession.

Pourquoi deux variantes peuvent être incomparables — et le sont par nature :

- l'ordre des versions (§ 5) est porté par la succession postulée ; **cette relation ne s'applique pas à l'intérieur d'une version** : entre deux matérialisations d'un même état édité, il n'y a pas de « avant » — il y a des axes de différenciation (architecture cible, langue, canal de diffusion, format d'empaquetage) qui sont des **dimensions non ordonnées** ;
- l'incomparabilité des variantes est donc **constitutive**, non ignorante : ce n'est pas qu'on ne sait pas ordonner une matérialisation x86-64 et une matérialisation AArch64 — c'est qu'aucun ordre n'est *défini* entre elles. La distinction rejoint celle du § 5 : ignorance (versions) vs absence de relation (variantes) ;
- une variante hérite de tout l'appareil hypothétique : postuler « même variante » est une hypothèse comme une autre, concurrençable, dominée ou dominante, révisable.

---

## 7. L'identité

**Définition 7 (identité logique, forme stratifiée)** — L'identité est la classe d'équivalence hypothétique **la plus grossière de la chaîne** : l'origine éditoriale commune supposée, celle que le 000 (Déf. 5–6) définit comme hypothèse abductive et que toute la série construit. C'est **l'objet central du moteur** — les autres strates existent pour la raffiner (variante, version) ou pour la relier (famille).

Propriétés définitoires :

- **elle ne dépend pas du contenu** : l'identité est la seule strate dont la persistance ne requiert la constance d'**aucune** observation. Elle peut survivre :
  - *aux versions* — tous les octets changent d'un état édité à l'autre ; l'identité traverse ;
  - *aux variantes* — des matérialisations disjointes en tout point (architectures, langues) relèvent de la même origine ;
  - *aux signatures* — les certificats expirent et tournent (le corpus 1 montre des chaînes de signature d'émetteurs de générations différentes, bornes de validité disjointes) ; l'origine postulée traverse les rotations ;
  - *aux observations elles-mêmes* — aucun attribut de Ω n'est constitutif de l'identité : tout indice qui la soutient aujourd'hui peut disparaître du prochain état édité sans que l'hypothèse d'identité tombe ;
- cette survie a un prix théorique déjà acté : puisque rien d'observé n'est constitutif, l'identité est intégralement **hypothétique et défaisable** (000 § 3.3), et sa continuité dans le temps est elle-même une hypothèse de continuité — jamais un invariant observationnel ;
- l'identité est la strate où la sous-détermination (000 L1) est la plus large : plus la strate est grossière, plus les découpages compatibles avec les mêmes observations sont nombreux.

---

## 8. La famille

**Définition 8 (famille)** — Une famille est une **parenté conventionnelle entre identités** : une relation (réflexive, symétrique, **non nécessairement transitive** — 000, Déf. 10) construite *sur* le niveau identité, exprimant un lien éditorial postulé (suite, édition, écosystème, déclinaison).

À souligner sans réserve :

- **une famille n'est pas une partition.** La chaîne contenu–variante–version–identité est faite d'équivalences emboîtées ; la famille n'en est pas une. Les familles forment au mieux un **recouvrement** du niveau identité : elles peuvent se chevaucher, laisser des identités isolées, et ne découpent rien ;
- **une identité peut appartenir à plusieurs familles** — c'est une propriété voulue de l'objet, pas un cas limite : un même composant postulé peut être parent d'une suite logicielle *et* d'un écosystème d'exécution, les deux parentés étant des hypothèses distinctes, de provenances distinctes ;
- la non-transitivité interdit toute clôture d'office : « A apparenté à B » et « B apparenté à C » ne disent rien de A et C — l'argument du 003 (§ 8.2) s'applique à l'étage au-dessus ;
- **aucune convention de parenté n'est choisie ici** : la forme des liens (quels indices soutiennent une parenté) relève de conventions futures ; le présent document fixe seulement que ces conventions produiront des *relations entre identités*, jamais des partitions.

---

## 9. La composition conventionnelle

Le 003 (§ 8.2) interdit toute composition implicite de relations. Le présent document définit l'objet licite :

**Définition 9 (composition conventionnelle)** — Une convention κ ∈ K dont T(κ) **licencie explicitement** la composition de relations ou de signaux désignés, en vue de soutenir des hypothèses à une strate désignée — par exemple : composer une relation de co-signature et une relation de co-déclaration de lignée pour soutenir des hypothèses de strate identité. Conditions de validité, toutes trois obligatoires :

- **provenance intégrale** : la composition porte l'union exacte des provenances de ses composantes (I7 ; 002 § 7.2) — chaque lien composé est décomposable jusqu'aux `observation_id` ;
- **dépendances conservées** : Dep de toute hypothèse soutenue par la composition contient la convention de composition elle-même *et* les dépendances de chaque composante (004 § 10) — la question « de quoi dépends-tu ? » traverse la composition sans perte ;
- **aucune disparition d'information** : la composition **s'ajoute** aux composantes, elle ne les remplace jamais ; les relations composées restent disponibles séparément, avec leurs régimes propres ; le composé hérite du **pire régime** de ses composantes (002 § 7.2).

Bornes : une composition conventionnelle produit du **soutien d'hypothèses** — jamais un groupe effectif (l'élection reste hors de ce document et des précédents), jamais un signal nouveau prétendant à une fiabilité que ses composantes n'ont pas. Et comme toute convention, elle est minimale (une composition = une convention), datée, justifiée, révocable.

---

## 10. La certitude à travers les strates

Les niveaux du 000 (§ 5 : impossible < possible < probable < certaine) se propagent le long de l'emboîtement selon deux lois, énoncées sans formule :

**Loi 1 — Propagation ascendante par implication.** Le raffinement (§ 2) fait que toute hypothèse fine *implique* l'hypothèse grossière correspondante (« même variante » implique « même version », qui implique « même identité »). Par l'asymétrie réfutation/confirmation (000 § 5.2) : toute réfutation de l'hypothèse grossière réfute l'hypothèse fine — donc **le niveau assignable à l'hypothèse grossière est toujours au moins celui de l'hypothèse fine** correspondante. Savoir « même version » au niveau probable rend « même identité » au moins probable ; l'inverse est faux : « même identité » probable ne dit rien de « même version ».

**Loi 2 — Conjonction bornée par le maillon le plus faible.** Une **assignation composée** — retenir ensemble une identité, une version et une variante pour les mêmes actes — a un niveau **borné par le plus faible de ses maillons** (000 § 5.2, non-composabilité, appliquée à l'emboîtement) : probable sur l'identité et seulement possible sur la version donnent une assignation composée au mieux possible. Aucune accumulation de maillons ne relève le plafond — les niveaux ne s'additionnent pas, ne se moyennent pas, ne se compensent pas.

Cas dégénéré cohérent : au niveau contenu, ≡ₘ atteint le « certain » conventionnel (000 § 5.1 ; 004, E6) — et la Loi 1 ne propage vers le haut que la trivialité « même contenu ⟹ candidats naturels aux mêmes strates », qui reste à l'état d'hypothèse pour chaque strate hypothétique : la certitude du contenu **ne remonte pas** la chaîne, puisque l'implication va dans l'autre sens (c'est « même variante » qui implique... rien sur le contenu, deux variantes ayant précisément des contenus distincts). Le seul point de la chaîne où l'observation touche la certitude est sa base.

---

## 11. Les hypothèses extrêmes

**Définition 10 (hypothèse minimale)** — Dans une strate 𝒮 et sur un domaine D : l'hypothèse postulant que **toutes les origines sont distinctes** à la granularité de 𝒮 (aucun lien). Son résidu contient **toutes les convergences observées** sur D (chaque consensus constaté devient une coïncidence à postuler).

**Définition 11 (hypothèse maximale)** — Symétriquement : l'hypothèse postulant que **tout le domaine procède d'une seule origine** à la granularité de 𝒮. Son résidu contient **toutes les divergences** (chaque contradiction, chaque hétérogénéité devient une anomalie à sa charge).

Propriétés :

- **existence dans chaque strate** : les deux découpages triviaux (discret et total) existent pour toute granularité et tout domaine — l'espace d'hypothèses d'une strate n'est donc **jamais vide**, quelle que soit la pauvreté des signaux ;
- **rôle de bornes** : toute autre hypothèse de la strate se situe « entre » elles (elle raffine la maximale et grossit la minimale) ; elles bornent l'espace comme le vide et le total bornent un treillis de partitions ;
- **rôle d'étalons** : leurs résidus explicites matérialisent ce que *toute* hypothèse doit expliquer ou postuler — une hypothèse intermédiaire se juge (par inclusion, 003 § 4) contre ces deux extrêmes ;
- elles sont rarement maximales au sens de la préférence (leurs résidus sont typiquement les plus gros), mais **jamais retirées de l'espace** (I19) — et dans les configurations de silence total (000 L2), la minimale peut rester la seule non dominée : c'est la forme exacte que prend « aucune hypothèse formulable » dans le cadre stratifié.

---

## 12. Invariants

> **I17 — Cohérence verticale des strates.** Toute co-rétention d'hypothèses à des strates différentes respecte le raffinement : les origines communes postulées à une strate fine sont incluses, comme relations, dans celles postulées à toute strate plus grossière co-retenue. Une violation de I17 n'est pas une contradiction à arbitrer — c'est une incohérence de construction, interdite (extension verticale de P1).

> **I18 — Une strate ne crée aucune observation.** L'appareil stratifié (strates, placements, compositions, ordres postulés) vit intégralement au-dessus de la couche 1 : rien de ce qu'il produit n'entre dans Ω, ne modifie Ω, ni ne prétend au statut d'observation (prolongement de A0, I1, I15 au sommet de l'édifice).

> **I19 — Une strate ne détruit jamais une hypothèse.** Stratifier organise l'espace des hypothèses ; aucune opération de stratification — placement, re-placement, composition, préférence intra-strate — ne retire une hypothèse de l'espace. Les hypothèses dominées ou écartées restent dérivables avec leur provenance (prolongement de 003 § 4.2 et de I10).

> **I20 — Une hypothèse conserve son identité lorsqu'elle change de strate.** Le placement d'une hypothèse dans une strate est un attribut dépendant de K (conventions de stratification), pas un constituant de l'objet : lorsqu'une révision de K re-place un contenu propositionnel (ce qui était lu comme frontière de version devient frontière de variante), l'hypothèse re-placée est **le même objet** — mêmes Dom, Obs, Sig, prov, mêmes dépendances augmentées de la nouvelle version de la convention de stratification. Aucun re-placement ne crée ni ne détruit d'hypothèse (cohérence avec I16 et I19).

---

## 13. Exemples — corpus 1 exclusivement, sous forme théorique

Chaque exemple construit des hypothèses stratifiées **sans jamais conclure qu'elles existent réellement** : le corpus fournit les observations ; les strates fournissent l'espace ; aucune élection n'a lieu.

**E1 — Même contenu** (497 actes, 381 classes ≡ₘ, jusqu'à 3 actes par classe).
La strate contenu est la seule effective : ses « hypothèses » dégénèrent (003 § 9). Théoriquement : les 3 actes d'une même classe ≡ₘ sont, à toute strate supérieure, des candidats *naturels mais non acquis* à la même variante — la Loi 1 du § 10 ne joue pas vers le haut, et l'assignation de strate reste une hypothèse même pour des octets identiques.

**E2 — Versions différentes** (théorique, sur les 59 actes au sujet signataire Python Software Foundation).
Espace de strate version : des hypothèses de découpage de ces actes en états édités, munies de successions postulées sur des indices déclaratifs mis en forme canonique (conventions futures). Entre l'hypothèse minimale (59 origines-versions distinctes) et la maximale (un seul état édité), l'espace contient tous les découpages intermédiaires et leurs ordres partiels — branches parallèles comprises. Rien n'est élu ; l'espace est défini.

**E3 — Même identité à travers les signatures** (théorique, sur les chaînes de signature du corpus : émetteurs de générations distinctes, fenêtres de validité disjointes).
Une hypothèse de strate identité peut couvrir des actes signés par des certificats différents (rotation) : l'identité ne dépend d'aucune observation constante (§ 7). Le résidu porte alors la divergence de signature comme fait à expliquer — qu'une hypothèse enrichie (« rotation de clés d'une même origine ») peut résoudre explicativement (003, Déf. 7). Toujours aucune conclusion : deux mondes (continuité / origines distinctes) restent formulables.

**E4 — Familles multiples** (théorique, sur les 192 actes au sujet signataire Microsoft).
Sous des hypothèses d'identités multiples au sein de ces actes (des origines distinctes partageant un signataire — 003, E4), des parentés de famille peuvent être postulées selon des axes différents : parenté de suite (composants d'un même produit assemblé) et parenté d'écosystème (composants d'un même runtime). Une même identité postulée peut appartenir aux deux familles ; les deux relations se chevauchent sans se confondre — la famille n'est pas une partition (§ 8), et le corpus l'illustre sans qu'aucune famille ne soit déclarée exister.

**E5 — Variantes** (théorique, sur les architectures observées : 49 actes `machine='014c'`, 10 `'8664'`, 2 `'aa64'`).
Espace de strate variante : pour toute hypothèse de version couvrant des actes d'architectures interprétées différentes, les hypothèses de variantes se différencient le long d'un axe non ordonné — l'incomparabilité est constitutive (§ 6) : aucun « avant » entre une matérialisation x86 et une AArch64. Les 20 actes en condition A-01 rappellent au passage que l'axe lui-même repose sur des signaux dont le régime importe : une « variante » soutenue par un signal artefactuel hérite de son régime (002 § 7.2).

---

## Conclusion

Les strates définies ici ne décident de rien : ce sont des **espaces mathématiques** — chacun avec son domaine, ses conventions, ses hypothèses, ses contradictions et sa préférence — reliés par l'ordre de raffinement et bornés dans chaque cas par les hypothèses extrêmes. Le contenu y est le seul niveau observé ; variante, version et identité sont des granularités d'hypothèses ; la famille est une relation au-dessus des identités. **Aucun regroupement réel n'a été effectué** : l'organisation effective des hypothèses dans ces espaces — l'élection, la rétention, l'assignation des niveaux de certitude — appartient aux documents suivants, qui devront se conformer aux invariants I1–I20.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| strate 𝒮 | espace des hypothèses d'une granularité, muni de domaine, conventions, hypothèses, contradictions, préférence ; ni signal, ni observation | 1 |
| raffinement ⊑ | ordre partiel entre strates : les origines fines impliquent les grossières ; contrainte de co-rétention, jamais de contradiction fine/grossière | 2 |
| emboîtement | contenu ⊑ variante ⊑ version ⊑ identité ; famille hors chaîne | 3 |
| convention de stratification | κ ∈ K produisant une organisation d'hypothèses (placement par granularité), jamais un signal ; stratifier ≠ interpréter | 4 |
| version | état édité postulé ; ordre partiel par succession postulée ; branches = chaînes ; incomparabilité par ignorance ou par parallélisme | 5 |
| variante | matérialisations simultanées d'un même état édité ; incomparabilité constitutive (axes non ordonnés) | 6 |
| identité | origine commune supposée, indépendante du contenu ; survit aux versions, variantes, signatures, observations ; objet central | 7 |
| famille | parenté conventionnelle entre identités ; relation non transitive ; recouvrement, jamais partition ; appartenances multiples | 8 |
| composition conventionnelle | composition licenciée par κ : provenance intégrale, dépendances conservées, aucune disparition d'information, pire régime hérité | 9 |
| certitude stratifiée | Loi 1 : propagation ascendante par implication ; Loi 2 : conjonction bornée par le maillon le plus faible | 10 |
| hypothèses extrêmes | minimale (tout distinct) et maximale (tout commun) : existent dans chaque strate, bornes et étalons de l'espace | 11 |
| invariants | I17 cohérence verticale, I18 aucune observation créée, I19 aucune hypothèse détruite, I20 identité de l'hypothèse préservée au re-placement | 12 |

**Ce que ce document ne fait volontairement pas** : adopter une convention de stratification, d'ordre de versions ou de parenté ; élire une hypothèse ; assigner un niveau de certitude à une assignation réelle ; effectuer un regroupement.
