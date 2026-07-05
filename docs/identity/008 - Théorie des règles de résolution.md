# 008 — Théorie des règles de résolution

**Statut** : neuvième document de la série `docs/identity/`. S'appuie exclusivement sur les documents 000 à 007, validés et figés.
**Périmètre** : le dernier étage théorique — les règles qui permettent aux conventions de coexister sans rendre le système incohérent. Ce document **ne décrit jamais comment résoudre** : il définit ce qu'est une règle de résolution. Aucun code, aucun algorithme, aucun score, aucun seuil, aucune heuristique, aucune convention concrète supplémentaire (les seules candidates citées sont celles déjà introduites : CE-01, A-01), aucune règle opérationnelle du futur moteur.

---

## 1. La règle de résolution, objet formel

**Définition 1 (règle de résolution)** — Un énoncé explicite décrivant **la manière dont plusieurs conventions adoptées peuvent être appliquées ensemble** sur un état du monde : quelles co-applications sont licites, sous quelles conditions de cohérence, avec quelles garanties.

Ce qu'une règle **n'est pas** — quadruple négation constitutive :

- **pas une convention** : elle n'appartient ni à K théorique ni au registre ℛ ; elle ne transforme rien (ni observation en signal, ni configuration en licence) — elle **discipline** les transformations. Les conventions sont le contenu normatif ; les règles en sont la *grammaire d'application* ;
- **pas une hypothèse** : elle n'explique rien et n'a ni domaine explicatif ni résidu ;
- **pas une élection** : elle ne retient rien ; elle borne ce que les élections dérivées conjointement peuvent être ;
- **pas un algorithme** : elle énonce des propriétés que toute application conjointe doit satisfaire — jamais une procédure pour les satisfaire.

Statut : les règles appartiennent au **cadre théorique lui-même**, au même titre que les invariants — mais leur *validité* est relative au contenu du registre (§ 10) : une règle qui discipline la co-application de deux familles de conventions devient sans objet si l'une disparaît, et peut devenir invalide si une adoption crée une interaction qu'elle ne couvrait pas.

---

## 2. Domaine

Une règle de résolution agit uniquement sur :

- **des conventions déjà adoptées** — le registre ℛ, jamais K théorique : les candidates n'interagissent pas, puisqu'elles n'ont aucun effet (007 § 8) ;
- **un état du monde existant** — les chaînes de dérivation qui s'y déploient sous l'index (Ω, K).

Et **elle ne crée jamais rien** : ni observation (I32), ni signal, ni hypothèse, ni convention (I29), ni élection. Une règle qui « produirait » quoi que ce soit aurait changé de nature — elle serait devenue une convention déguisée, non adoptée, donc un implicite (violation de I13 par l'étage qui devait précisément l'interdire).

---

## 3. La confluence

**Définition 2 (application conjointe)** — Sur un domaine et sous un index (Ω, K), l'*application conjointe* du registre est l'ensemble des dérivations que les conventions en vigueur licencient : constructions de signaux, résolutions, compositions, élections et refus, enchaînés jusqu'à un état.

**Définition 3 (confluence)** — L'application conjointe est *confluente* lorsque **le résultat ne dépend jamais de l'ordre d'application** : toutes les manières d'enchaîner les dérivations licites depuis le même index aboutissent au même état — mêmes élections, mêmes niveaux, mêmes refus, mêmes motifs.

Pourquoi la confluence est indispensable — trois dépendances exactes :

- **I11 (déterminisme de l'étage des hypothèses)** : si l'ordre d'application des conventions changeait les hypothèses dérivées ou leurs résidus, deux reconstructions du même index divergeraient — I11 tomberait ;
- **I24 (relativité au couple (Ω, K))** : un résultat sensible à l'ordre ferait de l'ordre un **troisième intrant caché** — précisément ce que I24 interdit ; la confluence est la forme technique de « rien d'autre que l'index » ;
- **la reproductibilité (P2)** : « deux exécutions sur un corpus identique produisent exactement les mêmes hypothèses » (003 § 11.2) ne survit à la couche normative que si aucun ordonnancement interne n'influe. La confluence est P2 propagée à travers ℛ.

Traitement des défauts de confluence — sans mécanisme :

> Lorsqu'une configuration rend l'application conjointe **non confluente** (deux conventions dont l'ordre d'application changerait le résultat), la configuration est un **conflit de conventions** : il est déclaré au registre comme une incompatibilité (007 § 6), et tant qu'il n'est pas levé — par retrait, restriction ou remplacement (007 § 10) — le domaine concerné ne produit que des **refus motivés** (« co-application non confluente »). C'est le motif du 006 § 4 étendu : là où l'ordre déciderait, le système ne décide pas.

---

## 4. La cohérence du registre

**Définition 4 (registre cohérent)** — Le registre ℛ est *cohérent* lorsqu'il satisfait simultanément :

- **compatibilité** : aucune paire de conventions en vigueur n'est déclarée incompatible sur un domaine où toutes deux s'appliquent (007 § 6) ;
- **dépendances satisfaites** : toute convention en vigueur a ses conventions présupposées en vigueur, dans des versions compatibles (007 § 5) ;
- **acyclicité** : la structure de dépendances est sans cycle (§ 7) ;
- **minimalité** : aucune convention en vigueur ne viole I14 (les violations constatées appellent la scission, 007 § 10) ;
- **confluence** : l'application conjointe des conventions en vigueur est confluente sur tout domaine, ou les défauts sont déclarés et leurs domaines en refus (§ 3).

La conséquence à établir, et elle est sévère :

> **Un registre incohérent interdit toute élection.** Toute élection cite au moins une convention adoptée (I27) ; citer une convention d'un registre incohérent importerait l'incohérence dans l'état — dépendance insatisfaite, conflit non déclaré ou ordre décisif caché. Sous ℛ incohérent, les seuls états cohérents (006, Déf. 3) sont donc faits **exclusivement de refus**, au motif « registre incohérent ». La cohérence de ℛ est le préalable de tout engagement, exactement comme la cohérence d'état est le préalable de toute assertion — les deux prédicats sont symétriques, et le second présuppose le premier.

Le prédicat est défini ; **aucune méthode de vérification n'est décrite** — c'est un prédicat, pas un procédé.

---

## 5. Les chaînes de résolution

**Définition 5 (chaîne de résolution)** — L'objet unique et intégralement tracé qui relie un acte d'un état (élection ou refus) à ses fondations :

  observation → signal → hypothèse → contradiction → priorité → élection → état

Chaque maillon est l'objet défini par son document (001, 002, 003, 002 § 9/003 § 5, 004 § 5–6, 006 § 1, 006 § 2), et la chaîne exige :

- **conservation intégrale de la provenance** (I30) : depuis l'acte final, la chaîne restitue exactement les `observation_id` et attributs consommés, les conventions appliquées à chaque maillon avec leurs versions (Dep), les hypothèses concurrentes écartées (just), les contradictions rencontrées et leur sort (assumée / résolue explicativement / résolue conventionnellement — avec la dette), et le motif de l'acte final ;
- **des maillons optionnels** : la contradiction et la priorité n'apparaissent que si la configuration les contient — une chaîne sans conflit va du signal à l'élection sans ces maillons ;
- **des points d'arrêt légitimes** : la chaîne peut s'interrompre à tout maillon — signal non défini (précondition), hypothèse non formulable, contradiction ouverte sans priorité en vigueur, configuration licenciable sans licence adoptée. **Toute interruption est un refus motivé**, et le motif nomme le maillon manquant : la chaîne interrompue est un objet aussi complet que la chaîne aboutie.

La chaîne est la **forme canonique de l'auditabilité** du système : « pourquoi cet acte ? » a pour réponse une chaîne ; « pourquoi pas d'acte ? » a pour réponse une chaîne interrompue.

---

## 6. La résolution inter-strates

Structure seule, sans la moindre règle métier :

- une chaîne peut concerner **plusieurs strates simultanément** : les actes qu'elle produit à des strates différentes sur des domaines recouvrants sont soumis, en plus de la cohérence de chaque strate, à la **cohérence verticale** (I17) — les origines fines retenues incluses dans les grossières retenues ;
- la co-application inter-strates est donc l'application conjointe (§ 3) **relevée à l'union des espaces de strates**, avec I17 comme clause de cohérence supplémentaire ; la confluence y est exigée à l'identique ;
- lorsque les licences de strates différentes entrent en tension (une élection fine licenciée dont la contrepartie grossière ne l'est pas, ou inversement), les issues **structurellement possibles** sont : l'élection jointe (si les licences couvrent l'emboîtement), l'élection partielle avec refus aux strates non couvertes (la Loi 1 du 005 § 10 garantit qu'élire grossier en refusant fin est toujours cohérent — l'inverse ne l'est pas sans la contrepartie grossière), ou le refus général ;
- **laquelle de ces issues advient dépend des conventions en vigueur, jamais de la théorie** : le présent document fixe l'espace des issues et la contrainte I17 ; « qui cède » sera l'affaire de conventions de strates explicites, adoptées et révocables — pas d'un principe caché.

---

## 7. Les dépendances entre conventions

**Définition 6 (structure de dépendances)** — Le graphe abstrait dont les nœuds sont les conventions adoptées et les arcs la relation *présuppose* (le champ « dépendances » du 007 § 5 : CE-01 présuppose la convention d'égalité de contenu ; une licence de strate variante présupposerait les interprétations d'architecture qu'elle mobilise).

Propriétés :

- **une convention peut dépendre d'une autre** — c'est le régime normal : les familles s'étagent (interprétations sous équivalences sous compositions sous licences), et la structure de dépendances reflète l'étagement des couches ;
- **aucun cycle n'est jamais obligatoire** : un cycle signifierait deux transformations conceptuelles se présupposant mutuellement — aucune ne serait définissable la première. Par I14, une telle paire est soit une transformation unique artificiellement scindée (cas de fusion, 007 § 10), soit une incohérence de conception. L'**acyclicité** est donc exigible sans perte de généralité, et fait partie du prédicat de cohérence (§ 4) ;
- la structure acyclique induit un **ordre partiel de fondation** sur les conventions (qui doit être adopté pour que quoi s'applique). Distinction essentielle : cet ordre gouverne la **définition et l'adoption**, jamais l'exécution — l'ordre d'*application* est, lui, rendu indifférent par la confluence (§ 3). Fonder est ordonné ; appliquer ne l'est pas.

---

## 8. La minimalité d'une résolution

**Définition 7 (ensemble minimal)** — Pour un acte donné (élection ou refus motivé) sur un domaine donné, un *ensemble minimal de conventions* est un sous-ensemble du registre qui suffit à dériver la chaîne complète de cet acte, et dont **aucun sous-ensemble propre ne suffit**.

Propriétés — sans construire aucun ensemble, sans nommer aucune convention :

- **existence** : toute chaîne aboutie mobilise un ensemble fini de conventions (Dep est fini) ; il contient donc au moins un ensemble minimal ;
- **non-unicité possible** : un même acte peut être dérivable par des soutiens alternatifs (deux chaînes distinctes) — plusieurs ensembles minimaux coexistent alors, et le cadre n'en privilégie aucun ;
- **rôle** : les ensembles minimaux sont la réponse exacte à la question *« que coûte cette conclusion en normes ? »* — la version conventionnelle de la question que la dette (006 § 9) pose pour les arbitrages. Ils se comparent **par inclusion, jamais par comptage** (la discipline du 003 § 1.3, appliquée une dernière fois) ;
- ils bornent l'effet des retraits : retirer une convention n'invalide un acte que si elle figure dans **tous** ses ensembles minimaux.

---

## 9. La complétude relative du registre

La distinction préalable, héritée du 007 (§ 3) et nommée ici :

- un **refus structurel** a pour motif l'état du monde lui-même : incomparabilité persistante, silence, sous-détermination, contradiction ouverte (006 § 4) — aucune convention supplémentaire n'y changerait rien sans changer Ω ;
- un **refus normatif** a pour motif le registre : « configuration licenciable, aucune convention adoptée », « dépendance non satisfaite », « conflit déclaré non levé » — le monde permettrait de conclure ; les normes en vigueur ne le permettent pas.

**Définition 8 (complétude relative)** — Le registre ℛ est *suffisamment complet* pour un domaine et une strate, sous un index Ω, lorsque **tout refus qui y subsiste est structurel** : aucune chaîne n'y est interrompue pour un motif purement normatif.

Propriétés :

- **la complétude est relative** — à un domaine, une strate, un état de Ω. **Jamais absolue** : Ω est ouvert (L8 — toute capacité ou observation nouvelle crée des configurations que ℛ n'anticipait pas), et les strates hautes peuvent exiger des conventions dont la justification n'a pas encore d'ancrage empirique ;
- la complétude ne dit **rien de la quantité d'engagements** : un registre complet sur un domaine où tout est sous-déterminé produit des refus partout — tous structurels. Complet ≠ productif ;
- le complémentaire est mesurable en droit : les refus normatifs d'un état localisent exactement ce qui manque au registre — c'est la carte des adoptions candidates, sans qu'aucune soit recommandée par la théorie.

---

## 10. Les révisions du registre et les règles

Effet d'une modification de ℛ (007 § 10) sur l'étage des règles :

- les règles étant relatives au contenu du registre (§ 1), **une règle peut devenir invalide sans qu'aucune observation change** : une garantie de confluence établie sur la disjonction des domaines de deux conventions tombe lorsqu'une troisième adoption fait se recouvrir leurs effets ; une discipline de co-application devient sans objet lorsqu'une des familles qu'elle articulait est retirée. Ω est intact ; la grammaire a changé ;
- toute transition du registre impose donc de **réétablir les garanties** : cohérence de ℛ (§ 4), confluence (§ 3), acyclicité (§ 7) — sous le nouvel état. Le cadre exige que ces garanties soient réétablies ; il ne dit pas comment ;
- la propagation vers les états du monde suit le canal unique déjà défini (006 § 6–7, 007 § 10) : re-dérivation le long des dépendances réelles, chaînes recalculées, interruptions apparaissant ou disparaissant, états antérieurs intacts sous leurs index (I23) ;
- cas remarquable : une révision peut convertir des refus **normatifs** en élections (adoption comblant un manque) ou des élections en refus **normatifs** (retrait, conflit déclaré) — les refus **structurels**, eux, ne cèdent qu'à Ω.

---

## 11. Invariants

> **I29 — Une règle de résolution ne crée jamais une convention.** La grammaire ne s'auto-alimente pas : toute convention naît d'un acte d'adoption (007 § 9), jamais d'une règle qui la déduirait. Une règle qui engendrerait du contenu normatif serait une adoption déguisée — un implicite, interdit depuis I13.

> **I30 — Toute résolution conserve intégralement la provenance.** Toute chaîne — aboutie ou interrompue — restitue exactement ses observations élémentaires, ses conventions versionnées, ses hypothèses écartées, ses contradictions et leur sort, sa dette. Aucun maillon n'absorbe ni ne résume ses fondations (extension terminale de I4, I7, I23).

> **I31 — Le résultat est indépendant de l'ordre d'application.** La confluence (§ 3) est un invariant, pas un vœu : toute configuration qui la mettrait en défaut est un conflit déclaré, et son domaine ne produit que des refus tant que le conflit tient. L'ordre d'application n'est jamais un intrant.

> **I32 — Une règle ne modifie jamais Ω.** Clôture absolue de la série descendante (I1, I15, I18, I22, I26) : l'étage le plus haut du système — celui qui gouverne les gouvernances — est soumis à la même interdiction que tous les autres. Rien, à aucun étage, ne touche les observations.

---

## 12. Exemples — corpus 1 exclusivement, sans décision réelle

**E1 — Une chaîne complète** (théorique, sur les 20 actes en condition A-01, sous un registre qui contiendrait A-01, ses interprétations présupposées et une licence d'élection du statut artefactuel).
Observations (`machine='4b50'`, `container='zip'`, `optional_header_magic=⊥`, `subsystem=⊥`) → signaux (conteneur déclaré ; lecture PE en régime suspect) → contradiction intra-acte apparente → A-01 rend formulable l'hypothèse artefactuelle h₂, qui **résout explicativement** (003, E5) → aucune priorité nécessaire (le maillon est optionnel : pas de décret, pas de dette) → élection licenciée de h₂ → acte dans l'état. La chaîne restitue : 20 `observation_id`, (A-01, v1) et ses présupposés dans Dep, h₁ écartée dans just, résidu comparé par inclusion. Aucun état réel n'est produit : la chaîne est exhibée comme objet.

**E2 — Une résolution interrompue** (les 59 actes au sujet signataire Python Software Foundation, strate version).
Observations → signaux relationnels (même signataire ; déclarations de versions) → hypothèses de découpage multiples → **incomparabilité persistante** (005, E2) : la chaîne s'arrête avant l'élection, sans contradiction ni priorité — refus **structurel**, motif « plusieurs maximales incomparables sous (Ω, K) ». Aucune adoption ne lèverait ce refus : il ne cède qu'à Ω (corpus plus riche, capacités nouvelles).

**E3 — Un registre incomplet** (les 381 classes de contenu, strate contenu, registre vide).
Configuration licenciable partout (soutien R1, domination stricte, 007 E1) — mais aucune convention d'élection adoptée : chaînes interrompues au dernier maillon, refus **normatifs** (« licenciable, aucune licence »). La carte de ces refus désigne exactement le manque : c'est la complétude relative (§ 9) constatée en creux. La théorie ne recommande pas l'adoption ; elle la localise.

**E4 — Une dépendance** (CE-01 et son présupposé).
CE-01 présuppose la convention d'égalité de contenu (004, E6) : arc de dépendance, sans cycle. Si CE-01 était adoptée sans son présupposé, le registre serait **incohérent** (dépendance insatisfaite, § 4) — et toute élection serait interdite, y compris sur des domaines étrangers à CE-01 : l'incohérence du registre est globale, c'est le prix de I27. L'ordre de fondation (égalité d'abord, licence ensuite) gouverne l'adoption — pas l'application, indifférente par confluence.

**E5 — Une révision** (l'adoption réelle de A-01, actée au 002, relue comme transition du registre).
Avant : les chaînes des 20 actes s'interrompent sur contradiction ouverte (refus structurel *sous ce registre-là*). Après : A-01 en vigueur, h₂ formulable, chaînes prolongées jusqu'à la maximalité — et interrompues au maillon suivant si aucune licence d'élection n'est en vigueur (refus devenu **normatif**). Aucune observation n'a changé ; le sort des chaînes, si. Et les garanties (§ 10) sont à réétablir : A-01 n'introduit ni cycle ni conflit — le registre reste cohérent.

---

## Conclusion

Les documents 000 → 008 constituent désormais une **théorie complète de l'identité logique** : les faits et leur intangibilité (001), les signaux et leurs régimes (002), les hypothèses, leur préférence et leur consensus (003), les conventions et leur gouvernement (004), les strates (005), les états du monde (006), les conventions d'élection (007), et les règles de résolution qui font tenir l'ensemble sans ordre caché ni implicite (008). Chaque objet est défini, tracé, révisable ; aucun n'a encore été instancié en acte réel.

Il ne manque plus que la dernière couche : **l'instanciation des premières conventions réelles** — l'ouverture du registre, les premières adoptions dans l'ordre de fondation, le premier état du monde engagé sur le corpus mesuré — qui fera l'objet du document 009.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| règle de résolution | grammaire d'application des conventions adoptées ; ni convention, ni hypothèse, ni élection, ni algorithme | 1 |
| domaine | conventions adoptées + état du monde existant ; ne crée jamais rien | 2 |
| application conjointe, confluence | le résultat ne dépend jamais de l'ordre d'application ; défaut de confluence = conflit déclaré + refus | 3 |
| cohérence de ℛ | compatibilité, dépendances satisfaites, acyclicité, minimalité, confluence ; registre incohérent ⟹ aucune élection nulle part | 4 |
| chaîne de résolution | objet tracé observation → … → état ; maillons optionnels ; toute interruption est un refus motivé | 5 |
| résolution inter-strates | application conjointe relevée à l'union des strates, I17 en clause supplémentaire ; issues possibles fixées, choix laissé aux conventions | 6 |
| structure de dépendances | graphe abstrait « présuppose », acyclique sans perte de généralité ; ordre de fondation ≠ ordre d'application | 7 |
| ensemble minimal | plus petit sous-ensemble de ℛ dérivant un acte ; existence, non-unicité, comparaison par inclusion | 8 |
| complétude relative | tout refus subsistant est structurel (vs normatif) ; relative à (domaine, strate, Ω), jamais absolue | 9 |
| révision et règles | une règle peut devenir invalide sans changement de Ω ; garanties à réétablir à chaque transition de ℛ | 10 |
| invariants | I29 aucune convention créée, I30 provenance intégrale, I31 indépendance à l'ordre, I32 Ω intouché | 11 |

**Ce que ce document ne fait volontairement pas** : vérifier la cohérence d'un registre réel, établir une confluence effective, construire un ensemble minimal, adopter quoi que ce soit, produire un état.
