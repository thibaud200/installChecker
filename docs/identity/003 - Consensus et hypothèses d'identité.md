# 003 — Consensus et hypothèses d'identité

**Statut** : quatrième document de la série `docs/identity/`. S'appuie sur les documents 000, 001 et 002, validés et figés.
**Périmètre** : définition des couches *relation*, *consensus* et *hypothèse* — la première où apparaît l'idée d'identité. L'identité n'y est **toujours pas calculée** : aucun algorithme, aucun score, aucune pondération, aucune probabilité, aucune règle de décision, aucun regroupement effectif. Uniquement les objets mathématiques qui permettront un jour d'exprimer une identité.
**Rappels tenus pour acquis** : les observations sont vraies (comme rapports de lecture, 001 § 1.3) ; les signaux sont des interprétations reconstructibles (002, I5) ; une identité n'est jamais observée, toujours supposée (000 § 3) ; un consensus n'est jamais une vérité — seulement la meilleure explication disponible à un instant donné, relative à l'état de la base et au répertoire des conventions.

---

## 1. L'hypothèse d'identité, objet formel

### 1.1 Définition

**Définition 1 (hypothèse d'identité)** — Un quintuplet

  h = ( Dom(h), Obs(h), Sig(h), prov(h), just(h) )

où :

- **Dom(h) — le domaine explicatif** : l'ensemble des actes d'observation (ou des classes de contenu ≡ₘ) dont h postule qu'ils procèdent d'une **origine commune**, à une strate donnée (identité, version ou variante au sens de 000 § 4). L'origine postulée est un objet abstrait : h ne nomme jamais un logiciel — elle affirme seulement « ces actes ont une origine éditoriale commune » ;
- **Obs(h)** : l'ensemble exact des observations élémentaires concernées (transitivement, via les signaux) ;
- **Sig(h)** : l'ensemble des instances de signaux mobilisées, **chacune avec son régime** (R1–R5, 002 § 5) — une hypothèse peut mobiliser un signal ambigu ou artefactuel, à condition de le dire ;
- **prov(h)** : la provenance complète — la chaîne intégralement restituable h → signaux → observations élémentaires → actes (extension de I4 et I7 à cette couche) ;
- **just(h) — l'historique de construction** : la dérivation qui produit h depuis les couches inférieures (quelles relations, quel consensus, § 7), y compris les hypothèses concurrentes écartées si h résulte d'une préférence. just(h) n'est pas un journal stocké : c'est une **dérivation reconstructible** (§ 11).

### 1.2 Aucune certitude intrinsèque

Une hypothèse **ne porte aucune certitude en propre**. Les niveaux du 000 § 5 (impossible < possible < probable < certaine) sont des statuts **assignés de l'extérieur**, relatifs au couple (Ω, K) — l'état de la base et le répertoire des conventions — et révisables. Deux conséquences :

- une hypothèse ne « pèse » rien : elle n'a ni force, ni poids, ni confiance (§ 4.4) ;
- la même hypothèse (même quintuplet) peut être possible sous un Ω et impossible sous un autre : le statut n'appartient pas à l'objet.

### 1.3 Le résidu

**Définition 2 (résidu)** — Pour une hypothèse h, le *résidu* Res(h) est l'**ensemble** de ce que h laisse inexpliqué sur son domaine : les coïncidences qu'elle doit postuler (des convergences de signaux qu'elle n'explique pas), les contradictions qu'elle assume (§ 5), les artefacts qu'elle suppose sans les établir.

Le résidu est un **ensemble structuré, jamais un nombre**. Il se compare par **inclusion**, jamais par comptage — compter les éléments d'un résidu serait réintroduire un score par la fenêtre (violation de I9). Cette discipline fonde la relation de préférence (§ 4).

---

## 2. Le consensus

**Définition 3 (consensus)** — Un ensemble d'instances de signaux S est *en consensus* relativement à une origine postulée lorsqu'il existe une hypothèse h telle que **chaque signal de S est nominalement attendu sous h**, sans qu'aucun n'exige de coïncidence, de falsification ou d'accident supplémentaire — c'est-à-dire : Res(h) ne contient rien qui provienne de S.

Ce que le consensus **est** : un fait structurel de cohérence jointe — l'existence d'au moins une explication commune. Ce qu'il **n'est pas** :

- **pas une moyenne** : il n'y a rien à moyenner — les signaux n'ont pas de valeur numérique (I9) ;
- **pas un vote** : les signaux ne sont pas dénombrés ; un consensus entre deux signaux n'est ni plus ni moins un consensus qu'entre dix — la différence éventuelle relève du statut de certitude assigné plus tard, pas de l'objet ;
- **pas une majorité** : un signal discordant ne « perd » pas contre les autres ; il fait sortir la configuration du consensus et ouvre une contradiction (§ 5) ;
- **pas une vérité** : le consensus est relatif à (Ω, K) ; l'arrivée d'une observation ou la révision d'une convention peut le dissoudre (§ 10).

Le consensus est le **matériau** des hypothèses : une hypothèse digne d'être formulée s'appuie sur un consensus ; un consensus n'impose à lui seul aucune hypothèse unique (plusieurs origines postulées peuvent expliquer le même consensus — § 3).

---

## 3. Les hypothèses concurrentes

**Définition 4 (concurrence)** — Deux hypothèses h et h′ sont *concurrentes* lorsque leurs domaines se recouvrent (Dom(h) ∩ Dom(h′) ≠ ∅) et que leurs contenus propositionnels sont **incompatibles** : elles ne peuvent être toutes deux retenues sans violer la cohérence (P1 du 000) — typiquement, h postule une origine commune là où h′ postule des origines distinctes, ou elles découpent différemment la même portion du corpus en strates.

Propriétés :

- la concurrence est **symétrique**, jamais résolue par l'objet lui-même : aucune hypothèse n'est vraie (000 § 3.3) ;
- des hypothèses **non concurrentes coexistent librement** (domaines disjoints, ou domaines recouvrants avec contenus compatibles — h sur l'identité, h′ raffinant en versions) ;
- l'ensemble des hypothèses concurrentes sur un même domaine est **ouvert** : il existe toujours au moins l'« hypothèse nulle » (origines toutes distinctes, aucun lien) et l'« hypothèse totale » (origine unique) ; entre les deux, la combinatoire des découpages. Le cadre n'impose pas de les énumérer — il définit leur statut.

---

## 4. La préférence entre hypothèses

### 4.1 Définition

Le 000 (Déf. 6) exige de retenir l'hypothèse qui explique « mieux » — en postulant moins de coïncidences, de falsifications ou d'accidents. Cette relation est définie ici **sans aucun calcul** :

**Définition 5 (domination)** — h′ *domine* h (noté h ≺ h′) lorsque :

1. Dom(h′) ⊇ Dom(h) — h′ explique au moins ce que h explique ; et
2. Res(h′) ⊊ Res(h) restreint au domaine commun — h′ laisse strictement moins d'inexpliqué, **au sens de l'inclusion ensembliste** : tout ce que h′ postule sans l'expliquer, h le postulait aussi, et h postulait au moins une chose dont h′ rend compte.

**Définition 6 (préférence)** — La relation ⪯ engendrée par la domination (clôture réflexive et transitive). C'est un **ordre partiel** sur les hypothèses d'un même domaine : réflexif, transitif, antisymétrique à équivalence explicative près (deux hypothèses de même domaine et même résidu sont *explicativement équivalentes* — le cadre ne les distingue pas).

### 4.2 Incomparabilité et coexistence

L'ordre est **partiel par essence**, et cette partialité est une propriété voulue, pas une faiblesse :

- **Incomparabilité** : si Res(h) et Res(h′) ne sont pas inclus l'un dans l'autre (chacune explique quelque chose que l'autre postule), ou si leurs domaines diffèrent sans inclusion, alors h et h′ sont *incomparables*. **Aucun mécanisme de ce cadre ne force leur comparaison** : les totaliser exigerait de peser un type d'inexpliqué contre un autre — c'est-à-dire un score, interdit (I9) et reporté aux conventions futures s'il doit exister.
- **Coexistence** : des hypothèses incomparables demeurent toutes candidates. Le moteur n'aura jamais l'obligation d'élire ; le refus de conclure est une conclusion (000 § 5.2).
- **Domination** : seule la domination stricte élimine — et elle n'élimine pas l'objet : une hypothèse dominée reste dans just(·) des hypothèses préférées (provenance des écartements, P5).

### 4.3 Maximalité, pas maximum

Sur un domaine donné, l'ensemble des hypothèses **maximales** (non dominées) peut contenir plusieurs éléments incomparables. « La meilleure explication » du 000 se lit donc rigoureusement : *un élément maximal de l'ordre de préférence* — l'unicité n'est jamais garantie (sous-détermination, 000 L1) ; quand elle fait défaut, le niveau de certitude assignable s'en ressent (au mieux « possible » pour chacune, 000 § 5.1).

### 4.4 La préférence n'est pas une confiance

Distinction imposée et fondamentale pour toute la suite :

> **Une hypothèse ne possède jamais une « force ». Elle possède uniquement un domaine explicatif.**

- La **préférence** (⪯) compare des *couvertures explicatives* : qui explique quoi, en laissant quoi. C'est une relation entre hypothèses, ensembliste, sans unité de mesure.
- La **confiance** (les niveaux du 000 § 5) qualifie la *position d'une hypothèse face à ses concurrentes sous (Ω, K)*. C'est un statut assigné, ordinal, externe à l'objet.
- Les deux ne se confondent jamais : deux hypothèses peuvent être **également compatibles avec les observations tout en expliquant des ensembles différents** — elles sont alors incomparables en préférence, et rien n'autorise à dire que l'une est « plus sûre » que l'autre. Inversement, une hypothèse peut être maximale (rien ne la domine) et rester seulement « possible » (des incomparables subsistent).

> **I12 — Neutralité de la préférence (invariant).** La relation de préférence est un ordre partiel fondé sur l'inclusion des domaines et des résidus. Elle ne porte aucune grandeur, ne se convertit en aucune mesure de confiance, et ne force jamais la comparaison d'hypothèses incomparables.

---

## 5. Le rôle des contradictions

Une contradiction (002 § 9) **ne détruit pas nécessairement une hypothèse** : elle entre dans son résidu et réduit ainsi son pouvoir explicatif — c'est-à-dire sa position dans l'ordre de préférence, par le mécanisme de la Définition 5, sans jamais d'effet quantitatif.

Trois états d'une contradiction **relativement à une hypothèse** :

**Définition 7** —
- *Contradiction **assumée*** : h maintient son postulat d'origine commune **en portant la contradiction dans Res(h)**, comme fait inexpliqué à sa charge. h dit : « origine commune, malgré ceci, que je n'explique pas. »
- *Contradiction **résolue*** : h **explique la contradiction elle-même** — son contenu propositionnel rend les deux constituants attendus (exemple canonique : l'hypothèse « la lecture PE relève de la condition d'artefact A-01 » rend attendue la conjonction `container='zip'` + `machine` renseigné, qui cesse d'être contradictoire *sous h*). La contradiction sort alors de Res(h). Résoudre au sens de ce cadre est un acte **explicatif** (une hypothèse plus riche), jamais un arbitrage de priorité — les conventions de priorité restent hors périmètre (000 L3, document 004).
- *Contradiction **ouverte*** : aucune hypothèse disponible ne l'explique ; elle figure dans le résidu de **toutes** les hypothèses du domaine concerné. Elle est représentée, avec sa double provenance (002 § 9), et attend — soit une observation nouvelle, soit une hypothèse nouvelle, soit une convention future.

Le même fait contradictoire peut être assumé par h, résolu par h′ et rester ouvert pour h″ : l'état est **relatif à l'hypothèse**, et la Définition 5 fait mécaniquement préférer, toutes choses égales par ailleurs, l'hypothèse qui résout à celle qui assume — puisque son résidu est strictement plus petit au sens de l'inclusion.

---

## 6. Le rôle du silence

Le silence — l'absence d'un signal, héritée de l'état ⊥ des observations (001 § 4.2–4.3) — obéit à une chaîne de conséquences strictement décroissante :

  **absence → ignorance → absence de contradiction → absence de corroboration**

- **Absence** : le signal n'est pas défini (précondition non satisfaite, 002 § 1.1) — et ⊥ recouvre à jamais l'absent réel et l'illisible projeté.
- **Ignorance** : rien n'est su de la propriété que le signal aurait explicitée. L'ignorance est l'état, pas une valeur.
- **Absence de contradiction** : le silence **ne réfute rien** — aucune hypothèse ne peut être déclarée impossible sur la foi d'un ⊥. Un contenu sans certificat observé n'infirme pas l'hypothèse d'une origine signée (catalogues, 001 § 4.2).
- **Absence de corroboration** : symétriquement, le silence **ne soutient rien** — aucune hypothèse ne peut citer un ⊥ dans Sig(h) comme appui. « Il n'y a pas de contre-indication » n'est pas un argument : c'est l'état par défaut de toute hypothèse sur tout silence.

> **Principe du silence** — Le silence n'entre ni dans le support d'une hypothèse, ni dans son résidu. Il est **neutre par construction** : il laisse coexister toutes les hypothèses compatibles avec le reste, sans en avancer aucune. Ce principe deviendra fondamental pour la suite : toute convention future qui voudrait faire d'une absence une *attente déçue* (« un MSI nominal déclare ses propriétés ») devra être une convention explicite, documentée, assumant de contredire la projection ⊥ — elle n'existe pas à ce jour.

Conséquence mesurée : les 439 actes sans VersionInfo du corpus 1 ne corroborent ni ne contredisent aucune hypothèse d'origine par leur silence déclaratif ; toute hypothèse à leur sujet repose exclusivement sur leurs signaux présents (contenu, conteneur, signature…).

---

## 7. La chaîne raffinée

Le présent document raffine l'arc *signal → hypothèse* de la chaîne fondamentale (001 § 5), comme le 001 avait raffiné le 000 :

  **signal → relation → consensus → hypothèse → identité**

| Couche | Consomme | Produit | Défini dans |
|---|---|---|---|
| signal | observations interprétées | instances qualifiées, avec régimes | 002 |
| **relation** | signaux relationnels | relations partielles entre actes, tenues séparées | § 8 |
| **consensus** | relations et signaux d'un domaine | faits de cohérence jointe | § 2 |
| **hypothèse** | consensus (et contradictions, silences) | explications candidates ordonnées par ⪯ | § 1, 4 |
| identité | hypothèses maximales et leurs statuts | hypothèses retenues, révisables | 000 § 3 ; documents futurs |

Chaque couche **ne consomme que la précédente** (001 § 5, règle 1). En particulier : une hypothèse ne lit jamais une observation directement — elle mobilise des signaux ; un consensus ne compare jamais des octets — il constate la cohérence de signaux ; l'identité ne manipulera jamais un consensus brut — elle retiendra des hypothèses.

---

## 8. Les relations : exister sans regrouper

### 8.1 Définition

**Définition 8 (relation induite)** — Tout type de signal relationnel σ (002 § 7.1) induit une *relation partielle* R_σ sur les actes : x R_σ y lorsque σ est défini sur (x, y) et que sa sortie affirme le lien (« même signataire », « contenu identique », « même `upgrade_code` déclaré »). *Partielle* : là où les préconditions de σ ne tiennent pas (⊥), la relation **n'est ni vraie ni fausse — elle n'est pas définie**.

Chaque R_σ hérite des propriétés de son signal : provenance (I7), régime (un lien fondé sur un signal artefactuel est un lien en régime R5), déterminisme (I6). Lorsque σ repose sur une égalité ou une équivalence interprétative (transitivité obligatoire, 002 § 6.2), R_σ est une **équivalence partielle** sur son domaine de définition : symétrique, transitive *là où elle est définie*.

### 8.2 La non-transitivité entre relations

Propriété centrale, à développer sans détour :

> x R_σ y et y R_σ′ z **n'impliquent jamais** x R_σ″ z, quels que soient σ, σ′, σ″ distincts.

A « même signataire » que B, B « même `upgrade_code` » que C : rien ne relie A et C — les deux relations portent sur des propriétés différentes, garanties par des mécanismes différents, et leur composition n'a **aucun statut** dans ce cadre. Plus généralement :

- chaque R_σ peut être transitive *en elle-même* ; la **famille** {R_σ} ne se fusionne pas en une relation unique : l'union de deux équivalences n'est en général pas transitive, et sa clôture transitive fabriquerait des liens qu'aucun signal n'affirme — de l'information créée de toutes pièces, en violation du principe « un signal n'ajoute rien » (002 § 1.2), étendu ici : **une relation n'ajoute rien non plus** ;
- en conséquence, **la couche relation ne produit aucun groupe**. Les relations existent, séparées, chacune avec sa provenance ; tout regroupement effectif — décider quelles relations composer, jusqu'où, et ce qu'est une classe — est un acte d'hypothèse (postuler une origine commune sur un domaine), soumis à la concurrence et à la préférence (§ 3–4), et son éventuelle systématisation est reportée aux couches futures ;
- la tentation inverse (transitiver d'office « même signataire ») est également écartée : même une équivalence légitime ne fonde pas à elle seule une origine commune — 192 contenus du corpus 1 partagent un sujet Microsoft en couvrant des produits distincts. La relation est vraie ; le groupe serait une hypothèse, et une hypothèse dominée.

---

## 9. L'identité matérielle : un consensus dégénéré

Le cadre général doit retrouver ≡ₘ (000, Déf. 4) comme cas particulier — c'est un test de cohérence interne :

- **Un seul signal** : « contenu identique » (relationnel, fondé sur l'égalité byte-à-byte des condensats — type P, discriminance et fiabilité maximales, 002 § 4.1).
- **Une relation** : R_sha est une équivalence partielle totale sur les actes (le condensat n'est jamais ⊥) — donc une équivalence pleine, la seule du système.
- **Un consensus trivial** : un ensemble réduit à un signal exact est en consensus avec lui-même par construction (Définition 3, S singleton, résidu vide).
- **Une seule explication, aucune concurrence effective** : l'hypothèse « ces actes portent le même contenu » a pour seule concurrente « les condensats coïncident par collision » — dont le résidu contient une coïncidence d'ordre supérieur (000 L7) que la première n'a pas à postuler. La domination (Déf. 5) est immédiate ; l'ensemble maximal est un singleton.
- **Statut assignable** : c'est l'unique configuration où le niveau « certaine » conventionnel du 000 § 5.1 est envisageable.

≡ₘ n'est donc pas un objet à part : c'est le point du cadre où toutes les définitions dégénèrent en trivialité — signal unique, relation totale, consensus automatique, préférence à vainqueur unique. Tout le reste du problème d'identité consiste précisément en ce que **rien d'autre ne dégénère ainsi**.

---

## 10. La révision

Une hypothèse est indexée par le couple **(Ω, K)** — l'état des observations persistées et le répertoire des conventions (types de signaux, équivalences, catalogue d'artefacts). Elle ne peut changer que si l'index change, pour exactement **deux causes, à jamais distinctes** :

**Définition 9 (révision par observation)** — Ω croît (nouveaux actes). Des signaux nouveaux apparaissent, des consensus se font ou se défont, des dominations s'inversent, des contradictions s'ouvrent ou se résolvent. C'est la non-monotonie du 000 § 5.2 : subie, légitime, jamais rétroactive sur les observations (I2).

**Définition 10 (révision par convention)** — K change (convention révisée, équivalence redéfinie, entrée ajoutée au catalogue d'artefacts). Les observations n'ont **pas bougé** ; c'est la lecture qui change. Tout l'étage des signaux se re-dérive (I6), et les hypothèses avec lui.

Exigences communes :

- les deux causes sont **tracées séparément** : une hypothèse révisée référence l'index (Ω, K) avant et après, et la cause exacte (quels actes ajoutés, ou quelle convention modifiée en quelle version). Confondre les deux rendrait les révisions inauditables ;
- la provenance est **conservée à travers la révision** : l'hypothèse détrônée reste dérivable sous son ancien index — rien n'est perdu, puisque rien de cette couche n'est source de vérité (§ 11) ;
- aucune révision ne modifie, ne supprime ni ne corrige quoi que ce soit en dessous : les observations sont intangibles (I1), les signaux se recalculent (I5).

---

## 11. Propriétés mathématiques des hypothèses

### 11.1 Reconstructibilité

Conséquence directe et imposée de I5 : les signaux étant intégralement reconstructibles depuis (Ω, K), et toutes les constructions de ce document (relations, consensus, résidus, préférence, hypothèses) étant des **fonctions déterministes** des signaux et les unes des autres,

> **I10 — Reconstructibilité des hypothèses (invariant).** Une hypothèse n'a **aucune existence propre**. L'intégralité de l'étage — relations, consensus, hypothèses, ordre de préférence, historiques de construction — est reconstructible à partir des seules observations persistées et du répertoire des conventions. Le stockage d'une hypothèse ne peut être qu'une **optimisation technique** ; jamais une nécessité théorique ; une hypothèse stockée qui ne serait plus re-dérivable à l'identique depuis (Ω, K) est invalide par définition.

### 11.2 Déterminisme et stabilité

> **I11 — Déterminisme de l'étage (invariant).** À couple (Ω, K) identique, l'ensemble des hypothèses, leurs résidus, l'ordre de préférence et les statuts de contradiction sont **identiques** — quels que soient la machine, l'instant, et l'ordre de reconstruction. En particulier : **deux exécutions sur un corpus identique produisent exactement les mêmes hypothèses.** C'est la propriété P2 du 000 propagée jusqu'ici ; elle est atteignable parce que chaque étage intermédiaire est déterministe (I6) et qu'aucune étape ne dépend du contexte (I8) ni d'un aléa.

Autres propriétés, héritées ou propagées :

| Propriété | Contenu | Source |
|---|---|---|
| traçabilité | prov(h) restituable jusqu'aux `observation_id` ; just(h) restituable jusqu'aux consensus et aux concurrentes écartées | I4, I7, P5 |
| reproductibilité | conséquence de I11 | P2 |
| réversibilité | toute hypothèse est jetable et re-dérivable ; se tromper se répare par recalcul | P4, I5, I10 |
| provenance | composante constitutive de l'objet (Déf. 1) | I4 |
| non-monotonie | subie, canalisée par les deux causes de révision (§ 10), jamais rétroactive | 000 § 5.2 |
| dépendance au corpus | toute hypothèse est indexée par (Ω, K) ; hors index, elle n'affirme rien | 000 § 3.3 |
| stabilité sous corpus inchangé | Ω et K constants ⟹ hypothèses constantes (cas particulier de I11) | P3 |

---

## 12. Ce qui n'est PAS une hypothèse

| Notion | Pourquoi ce n'est pas une hypothèse | Couche |
|---|---|---|
| une décision | l'hypothèse propose ; élire, appliquer, agir sont des actes postérieurs | couches futures |
| un classement | l'ordre ⪯ est partiel et non totalisable sans conventions ; « classer » suppose un ordre total, donc un score | interdite à ce jour |
| une vérité | une hypothèse est compatible, préférable ou réfutée — jamais vraie (000 § 3.3) | aucune |
| un logiciel, une version | ce sont les *origines postulées* — le contenu propositionnel des hypothèses, pas les objets eux-mêmes | identité (strates du 000 § 4) |
| une famille | relation entre identités retenues | après l'identité (000, Déf. 10) |
| un doublon | « contenu identique » est un signal ; « doublon » ajoute un jugement de redondance, puis d'action | signal, puis décision |
| une suppression | acte du monde physique, hors du domaine du moteur (A0) | hors système |

Règle générale : **une hypothèse explique ; tout ce qui tranche, classe, nomme définitivement ou agit n'est pas une hypothèse.**

---

## 13. Exemples — corpus 1 exclusivement

Chaque exemple montre des hypothèses **plurielles et compatibles avec les mêmes observations**, et leur situation dans le cadre (consensus, résidu, préférence, silence).

**E1 — Contenus identiques** (497 actes, 381 classes ≡ₘ, jusqu'à 3 actes par contenu).
Signal « contenu identique » ; consensus dégénéré (§ 9). Hypothèse h₁ « même contenu » domine h₂ « collision de condensats » (Res(h₂) contient une coïncidence cryptographique que h₁ n'a pas à postuler). Ensemble maximal singleton — le seul du corpus. Rien n'autorise pour autant « doublon à supprimer » : jugement et acte hors couche (§ 12).

**E2 — Les 10 `.msp` en OLE-CFB.**
Observations : conteneur présent, propriétés MSI ⊥. Silence déclaratif total → aucune corroboration possible d'aucune hypothèse par ces attributs (§ 6). Hypothèses coexistantes sur un même acte `.msp` et un acte `.msi` voisin : h₁ « origines distinctes sans lien », h₂ « origine commune de lignée » (si un signal relationnel les lie par ailleurs — signataire). Sans autre signal, h₁ et h₂ sont **incomparables** : chacune postule ce que l'autre explique. Aucune n'est élue ; c'est le fonctionnement nominal du cadre, pas un échec.

**E3 — Le silence VersionInfo** (439 actes sur 496).
Toute hypothèse sur ces actes a un Sig(h) sans aucun signal déclaratif VersionInfo — et un résidu qui n'en contient pas davantage (le silence n'entre nulle part, § 6). Conséquence structurelle : sur ce sous-corpus, les hypothèses ne se distingueront que par contenu, conteneur et signature — la sous-détermination (000 L1) y est mécaniquement plus large. Le cadre le *représente* ; il n'y remédie pas.

**E4 — Certificats sans ProductName** (360 ole-cfb signés, 0 VersionInfo).
Relation R_signataire : équivalence partielle liant, par exemple, les 59 actes au sujet Python Software Foundation. Hypothèses concurrentes sur ce domaine : h₁ « une origine commune unique » (une seule identité), h₂ « des origines distinctes partageant un signataire » (plusieurs identités d'un même éditeur), h₃ « une origine commune par lignée, distinctes par version » (strates). Le consensus « même signataire » est compatible avec **les trois**. h₁ doit porter dans son résidu les divergences de déclarations MSI entre ces actes (`upgrade_code` distincts) ; h₂ et h₃ les expliquent. Illustration de la Définition 5 : la préférence se joue sur l'inclusion des résidus, sans qu'aucun compte ne soit jamais fait — et il reste des incomparables (h₂ vs h₃ découpent différemment).

**E5 — L'artefact `4b50`** (20 actes).
Sur chacun, contradiction intra-acte apparente : conteneur ZIP + champs PE renseignés (002, Déf. 9). Hypothèses : h₁ « l'acte porte un objet réellement double (archive contenant du COFF à l'offset zéro) » — assume la contradiction dans son résidu ; h₂ « la lecture PE relève de la condition d'artefact A-01 » — **résout** la contradiction (la conjonction devient attendue sous h₂, § 5). Res(h₂) ⊊ Res(h₁) sur le même domaine : h₂ domine. La contradiction est *résolue relativement à h₂*, *assumée relativement à h₁*, et h₁ demeure dérivable dans just(h₂) comme concurrente écartée. Aucune règle du type « les ZIP n'ont pas d'architecture » n'a été introduite : seulement une hypothèse préférée par inclusion de résidus.

---

## 14. Récapitulatif

| Objet | Définition | § |
|---|---|---|
| hypothèse h = (Dom, Obs, Sig, prov, just) | explication candidate d'une origine commune ; aucune certitude intrinsèque | 1 |
| résidu Res(h) | ensemble de l'inexpliqué à charge ; comparé par inclusion, jamais compté | 1.3 |
| consensus | cohérence jointe : existence d'une explication commune sans résidu sur ces signaux ; ni moyenne, ni vote, ni majorité, ni vérité | 2 |
| concurrence | domaines recouvrants, contenus incompatibles ; symétrique, jamais auto-résolue | 3 |
| préférence ⪯ | ordre partiel par inclusion des domaines et résidus ; incomparabilité préservée ; maximalité sans unicité | 4 |
| préférence ≠ confiance | la relation compare des couvertures ; les niveaux de certitude sont des statuts externes (I12) | 4.4 |
| contradictions | assumée (dans le résidu) / résolue (expliquée par h) / ouverte (dans tous les résidus) — relatives à l'hypothèse | 5 |
| silence | absence → ignorance → ni contradiction ni corroboration ; neutre par construction | 6 |
| chaîne raffinée | signal → relation → consensus → hypothèse → identité | 7 |
| relations | partielles, séparées, jamais composées entre types ; aucune formation de groupe à cette couche | 8 |
| ≡ₘ | consensus dégénéré : signal unique, équivalence totale, domination immédiate | 9 |
| révision | par observation (Ω) ou par convention (K) — causes à jamais distinctes, tracées, provenance conservée | 10 |
| invariants | I10 reconstructibilité des hypothèses, I11 déterminisme de l'étage, I12 neutralité de la préférence | 4.4, 11 |
| non-hypothèses | décision, classement, vérité, logiciel, version, famille, doublon, suppression | 12 |

**Ce que ce document ne fait volontairement pas** : instancier une seule hypothèse réelle, composer des relations, former des groupes, assigner des niveaux de certitude, arbitrer une contradiction par priorité, totaliser l'ordre de préférence. La couche suivante — les conventions d'interprétation effectives et la résolution des contradictions — est l'objet du document 004, qui devra se conformer aux invariants I1–I12.
