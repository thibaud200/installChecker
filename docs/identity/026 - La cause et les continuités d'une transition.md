# 026 — La cause et les continuités d'une transition

**Statut** : dixième acte documentaire de la phase v3 de la série `docs/identity/`. S'appuie sur les documents 000→025, figés. Exécute le **report 9** du 016 § 4.2 — « spécifier la dérivation et la vérification de la cause, et le régime des continuités » — par la voie 2 du 016 § 3, **le document précédant le code** : un volet logiciel strictement borné aux porteurs de la cause réalise ensuite ce que le présent acte fixe. Clôt la tension **T1** du 019 § 2.

**Nature** : un document de **contrat**. La voie retenue est le **raffinement** (Cas B) : la cause de τ n'était dérivable ni des entrées déclarées de C6 (démontré au § 2), ni d'aucune autre source contractuelle — sa fourniture par l'appelant, régime constaté par le 016 § 4.2 et mis en quarantaine par le 018 § 3, contredisait des garanties existantes (Cas A et Cas C rejetés au § 5 ; I59 engagé sur la surface de τ, première branche exercée — écart publié, § 5).

**Périmètre — touchers déclarés** (016 § 3, aucune modification par silence) :

1. **complément assumé du 014 § 1 (C6, clause « produit »)** : l'invocation de transition reçoit, avec les deux index et leurs deux ensembles d'actes, **les deux énumérations d'identifiants d'actes** — l'objet même que le 020 a fait entrer dans la clause « reçoit » de C5, au même titre et pour la même raison : une entrée réellement nécessaire, d'identifiants seuls, à usage unique, re-dérivable de Ω ;
2. **raffinement assumé des 014 §§ 2 et 7.5 (la section `cause`)** : la cause devient la **suite des volets du changement d'index** — zéro, un ou deux volets, dans l'ordre Ω puis ℛ —, chaque volet portant son type (« omega » ou « registre ») et son détail ; le détail Ω s'étend des seuls « identifiants d'actes ajoutés » aux identifiants **ajoutés et retirés** ; le détail ℛ est le **delta des couples (identifiant, version)** entre les deux membres ℛ des index (§ 3) ;
3. **raffinement assumé du 018 §§ 3 et 6** : la cause **disparaît de la demande de transition** — du « reçoit » du porteur comme de celui du consommateur. La clause de quarantaine du 018 § 3 (« sa dérivation et sa vérification relèvent du report 9 ») se résout par sa propre lettre : le présent acte est l'exécution du programme qu'elle annonçait ;
4. le **régime des continuités** est fixé (§ 4) : elles sont dérivées par C6, selon le critère que le 006 § 5 énonce — aucun toucher : le critère existait, seule sa mise en œuvre est assignée.

**Exclusions constitutives** : rien n'anticipe la forme canonique **matérielle** de W ni de τ (report 3 — le présent acte fixe le contenu logique de la cause et des continuités, jamais leur sérialisation), ni la vérification de cohérence d'état de C6 (report 4) ; aucun document figé n'est modifié ; W₀, l'artefact d'oracle et le registre sont intacts ; aucun invariant nouveau, aucun invariant affaibli.

---

## 1. Le constat — la tension T1, réalisée dans le code

Le 019 § 2 l'inventoriait : « la cause de τ n'a pas de source contractuelle ». Les trois pièces, à la lettre :

- le **014 § 7.5** exige une section `cause` dans la représentation de τ : « `cause` (type — « omega » ou « registre » — et détail : identifiants d'actes ajoutés, ou transition de convention) » ;
- la clause « produit » de **C6 (014 § 1)** dérive τ « sur demande **avec deux index et leurs deux ensembles d'actes** » — aucune cause en entrée ;
- l'objet **Transition (014 § 2)** interdit « **tout élément non re-dérivable des deux index** ».

Le régime constaté (016 § 4.2, report 9 : « la cause est fournie par l'appelant **sans vérification** contre les deux index ; les continuités déclarées ne sont **jamais peuplées**, alors que 006 (E5) décrit des continuités triviales ») réalise la tension au lieu de la résoudre, et y ajoute deux violations :

- **une troisième entrée** : la cause fournie par l'appelant coule *verbatim* dans une sortie contractuelle (τ). Or « il n'existe aucune troisième entrée » (011 § 2.2, EXG-02) ; le 018 § 9 la rangeait dans « les demandes de transition et d'audit du 011 §§ 3 et 7 » — une lecture de compromis que le 018 § 3 déclarait lui-même provisoire ;
- **des sorties non fondées** : une cause fausse fournie par l'appelant produit une τ dont la section cause n'est re-dérivable de rien — contre l'interdit du 014 § 2, contre I23 (« la cause est tracée ») et contre I38 (« pas de correspondance non causée dans τ ») ; et la vacuité des continuités contredit le 006 E5 (« les continuités déclarées à travers τ sont triviales ici (les mêmes hypothèses se succèdent à elles-mêmes) » — elles existent, elles sont triviales, elles ne sont pas absentes).

Enfin, la forme même du type (« omega » **ou** « registre », exclusif) ne représente ni le cas où les deux entrées changent, ni le cas où aucune ne change — tous deux constructibles sous EXG-30 : « le moteur doit pouvoir produire τ entre **deux index quelconques** dont il connaît les deux membres ».

---

## 2. La démonstration — ce qui est dérivable, ce qui ne l'est pas

Classons chaque constituant de la cause et des continuités selon les entrées **actuellement déclarées** de l'invocation de τ (deux index, deux ensembles d'actes — 014 § 1) :

| Constituant | Source | Verdict |
|---|---|---|
| type du changement Ω | les deux identités d'états d'Ω, dans les index | **dérivable** — depuis le 025 seulement : avant lui, deux états distincts par renumérotation partageaient une identité, et un changement réel d'Ω était indétectable sur l'index. Le 025 a livré la **condition de détectabilité** de la cause sans anticiper son régime (son en-tête exclut nommément le report 9) |
| type du changement ℛ | les deux membres ℛ des index — « la liste explicite, triée par identifiant, des couples (identifiant, version) en vigueur » (014 § 7.2) | **dérivable** des index seuls |
| détail ℛ (le delta des couples) | les deux mêmes listes | **dérivable** des index seuls |
| détail Ω (les identifiants ajoutés et retirés) | — | **impossible** depuis les entrées actuelles : l'empreinte d'état n'est pas inversible (une fonction d'empreinte ne se remonte pas — 025 § 3), et l'union des domaines des actes de W ne couvre pas, en général, les actes à espace trivial (022 : les singletons de la strate contenu peuvent n'apparaître dans aucun domaine) — la couverture qu'offrent aujourd'hui les refus de domaine maximal de W₀ est contingente, jamais garantie |
| correspondance | les deux ensembles d'actes, sur la référence totale du 024 | **dérivable** (déjà dérivée) |
| continuités | les deux ensembles d'actes, sous le critère du 006 § 5 | **dérivable** (§ 4) |

**Une seule entrée manque** : l'énumération des identifiants d'actes de chaque état. Elle existe déjà dans la machine — C1 la produit, et le 020 l'a fait entrer dans la clause « reçoit » de C5 (« des identifiants seuls, à usage unique, re-dérivables ») ; le complément du présent acte l'achemine à l'invocation de τ, au même titre que les deux ensembles d'actes qu'elle accompagne. Avec elle, **la cause entière est une fonction des entrées** — elle n'a plus d'auteur : elle a une dérivation.

**Le détail profond de ℛ n'a pas besoin de traverser.** Le 006 Déf. 7 veut la cause « datée et justifiée » ; la date, la justification et le type d'acte de gouvernance (adoption / révision / retrait / remplacement / scission / fusion) vivent dans le journal — et « une convention ne parvient jamais à C6 » (014 § 3). La cause **nomme** le changement (le delta des couples) ; sa date et sa justification demeurent **re-dérivables de l'état ℛ que l'index désigne** : le journal est append-only et fait partie de l'état (015 § 6 ; « chaque état de ℛ est immuable ; […] l'ancien restant référençable à jamais », 011 § 2.2). C'est le régime d'I23 — « les deux états sont re-dérivables sous leurs index respectifs » — et la lecture par référence que le 025 § 5 a établie : une exigence est satisfaite par ce qui reste re-dérivable, jamais par ce qu'on embarque. L'interdit du 014 § 2 pousse d'ailleurs à cette représentation minimale.

**La lecture de « re-dérivable des deux index »** : EXG-30 dit « deux index quelconques **dont il connaît les deux membres** » — l'index désigne un couple (état d'Ω, état de ℛ) ; la re-dérivation est la ré-invocation sur les membres désignés (I23, I39). Sous cette lecture, le delta des identifiants est parfaitement re-dérivable — et un texte libre d'appelant ne l'est jamais.

---

## 3. La décision — la cause dérivée par C6

> **Définition 1 (volet de cause)** — Un *volet* est le constat d'un changement sur **l'un** des deux membres de l'index : son **type** (« omega » ou « registre » — les deux seules causes, 006 Déf. 6) et son **détail** — pour Ω : les identifiants d'actes **ajoutés** et **retirés** (le delta des deux énumérations) ; pour ℛ : les couples (identifiant, version) **adoptés** et **retirés** (le delta des deux listes de l'index).

> **Définition 2 (la cause)** — La section `cause` de τ est la **suite des volets**, dans l'ordre Ω puis ℛ : un volet par membre dont les deux index diffèrent, aucun volet pour un membre inchangé. Entre deux index égaux, la cause est **vide** — τ est alors une pure comparaison (licite : EXG-30 dit « deux index quelconques »), jamais une révision (006 Déf. 6 : une révision est un « passage à un état d'index **différent** » ; sans changement d'index, W est immuable, I24).

Trois conséquences, chacune fondée :

- **le siège du calcul est C6** — c'est sa clause « produit » (014 § 1 : C6 « produit […] τ ») ; dériver la cause est de la même espèce que dériver la correspondance : une comparaison de ce que les entrées énoncent, jamais un jugement. « C6 assemble, il ne dérive pas » (014 § 3) parle de la dérivation des **actes** — la production de τ est, à la lettre, une production de C6 ;
- **le porteur perd le paramètre** — il convoie les énumérations que C1 produit (018 § 2 : « il ne dérive rien », I66 — renforcé : une entrée de moins) ; la demande de transition du consommateur se réduit à **deux désignations d'index** (raffinement du 018 § 6) ;
- **la vérification devient sans objet** — le 016 § 4.2 demandait « la dérivation **et la vérification** » ; une cause qui n'est plus jamais fournie n'a plus à être vérifiée : elle **est** sa dérivation. La branche vérification est résolue par vacuité — aucune erreur nouvelle, aucun siège à créer (la table des sept erreurs du 017 § 6 et le « seul cas » de C7 restent clos).

Le détail Ω s'étend aux **retraits** : EXG-30 n'impose aucune inclusion entre les deux états (« quelconques ») — la forme « identifiants d'actes ajoutés » du 014 § 7.5 décrivait le scénario de croissance (006 Déf. 6 : « actes d'observation nouveaux ») sans couvrir la comparaison générale que EXG-30 exige. Le raffinement couvre les deux sans en retirer aucun.

Un cas limite est couvert sans clause spéciale : deux supports quelconques aux **mêmes identifiants et aux contenus différents** ont des identités distinctes (025) et un delta d'identifiants vide — le volet Ω est alors **présent à détail vide**, constat exact (« Ω a changé ; aucun acte ajouté ni retiré ») et re-dérivable comme le reste.

---

## 4. Le régime des continuités

Le critère existe, mot pour mot, au 006 § 5 : « l'élection e′ ∈ W′ est déclarée *successeur* de e ∈ W lorsque l'état W′ retient que l'origine postulée par h′ est **la même origine** que celle que postulait h — déclaration **justifiée par le recouvrement de leurs domaines et dérivations** ».

Le présent acte en fixe la mise en œuvre :

- **entre élections seulement** — la Définition 5 du 006 ne parle que d'« actes d'élection » ; un refus ne postule aucune origine (006 § 9 : un refus ne doit rien) ;
- **e′ est successeur de e** lorsque : même strate, même contenu propositionnel, domaines se recouvrant. À la strate contenu — la seule décidée à l'état de la théorie —, l'origine postulée **est** le contenu commun (l'empreinte partagée) : même contenu ⇔ même origine postulée ; le recouvrement des domaines est la lettre du critère ;
- **les triviales sont déclarées** — une élection conservée à l'identique se succède à elle-même : c'est exactement le cas d'E5 (« les mêmes hypothèses se succèdent à elles-mêmes »), que le 006 donne comme **existant et trivial**, jamais comme absent. Le cas non trivial du même exemple — l'élection dont le domaine s'étend d'un acte, abandonnée et nouvelle sous la correspondance — est précisément ce que la continuité existe pour dire : *même origine, domaine étendu* ;
- **aucun supplément d'engagement** — le 006 § 8 exige une convention pour « tout supplément d'engagement » ; appliquer un critère que la théorie énonce n'est pas un engagement libre : c'est une dérivation. Les continuités restent représentées « comme couples de références » (014 § 7.5), sur la référence totale du 024.

---

## 5. Cas A et Cas C rejetés

**Cas A (confirmation) — rejeté.** La théorie ne dérivait pas la cause : le détail Ω est **impossible** à reconstruire des entrées déclarées de C6 (§ 2 — empreinte non inversible, couverture des domaines contingente par le 022), et la forme exclusive du type ne représente ni le changement double ni le changement nul, tous deux constructibles sous EXG-30. La lettre doit bouger : c'est un raffinement, pas une confirmation.

**Cas C (correction) — rejeté comme qualification de la résolution.** Aucune garantie n'est retirée : le complément ajoute une entrée, les raffinements étendent une représentation et suppriment une entrée que le contrat lui-même tenait en quarantaine — le 018 § 3 déclarait, dans sa propre lettre, que « sa dérivation et sa vérification relèvent du report 9 » : le présent acte est l'exécution d'un programme annoncé, jamais une correction de la théorie. W ne porte pas de cause ; W₀ est intact acte pour acte.

**I59 est néanmoins engagé sur la surface de τ — première branche exercée.** La lettre d'I59 porte sur ce que le moteur *produit* « sur tout index que la v1 connaissait », jamais sur les seules sorties émises. Or, sur la surface des **continuités**, la sortie était déterminée par l'index dans les deux régimes — vide dans l'ancien (contre la lettre du 006 E5), peuplée sous le présent acte — et diffère donc sur des index connus : τ(W₀, W₀) passe de zéro à cent douze continuités ; et la cause de l'ancien régime, écho d'une troisième entrée, violait EXG-02 et l'interdit du 014 § 2. C'est la première exception d'I59, « la correction publiée d'une non-conformité antérieure (011 § 9) » : **l'écart est publié** (`docs/conformite/ecart-publie-cause-continuites.md`) et les versions antérieures déclarées non conformes rétroactivement **sur la surface de τ, et sur elle seule** — aucune τ n'ayant jamais été émise (016 § 5.1 ; réaffirmé par le 024 § 4 : « aucune τ réelle n'a jamais été émise »), la déclaration porte sur la capacité de la version, jamais sur une émission passée.

Reste le **Cas B** : le plus petit raffinement qui rende la cause à sa définition théorique — « le changement d'index » (006 Déf. 7), rien d'autre, dérivé de ce qui change.

---

## 6. Le volet logiciel imposé

Strictement borné aux porteurs de la cause :

- **l'objet cause** devient la suite des volets (Définitions 1–2) — le type exclusif disparaît ;
- **C6** dérive la cause (des deux index et des deux énumérations) et les continuités (des deux ensembles d'actes, § 4) — la vacuité codée disparaît ;
- **le porteur** convoie les deux énumérations et perd le paramètre cause ; la restitution de C7 (« qu'est-ce qui a changé ? ») reste un passe-plat ;
- **les scénarios de transition du 013 § 9 sont joués pour la première fois** : transition Ω (l'acte ajouté — le scénario d'E5), transition ℛ (l'adoption simulée d'une convention de test), transition double, comparaison d'index égaux ;
- **rien d'autre** : la CLI est intacte — la demande de transition du consommateur demeure « le cas échéant » (018 § 6), non réalisée en v2, non exigée ici.

---

## 7. Compatibilité

Vérifiée contre chaque document de la série — toute compatibilité non listée est une compatibilité par silence qu'il faudrait déclarer : la liste est exhaustive.

| Document / clause | Ce qui pouvait frotter | Résolution |
|---|---|---|
| 006 §§ 5–7 (Déf. 5–7, E5) | la cause « datée et justifiée » ; les continuités déclarées | la cause nomme le changement, date et justification re-dérivables de l'état ℛ désigné (§ 2, régime I23) ; le critère du § 5 est appliqué à la lettre, E5 enfin honoré (§ 4). ∎ |
| 006 § 8 (« tout supplément d'engagement exige une convention ») | dériver des continuités semble un engagement | appliquer un critère que la théorie énonce n'est pas un engagement libre (§ 4). ∎ |
| 010 EXG-02 ; 011 § 2.2 (« aucune troisième entrée ») | la cause-paramètre était une troisième entrée | elle disparaît — la garantie est restaurée, pas amendée. ∎ |
| 010 EXG-29, EXG-30 | « deux index quelconques » | représentés : volets composables, cause vide pour les index égaux, détail Ω ajoutés et retirés (§ 3). ∎ |
| 011 §§ 3, 9, 10 ; I37, I38 | τ sortie contractuelle ; « seule comparaison » ; extension sans retrait | τ reste l'une des trois sorties et la seule comparaison ; le contrat s'étend par ajout (une entrée d'invocation, une représentation plus riche), rien n'est retiré. ∎ |
| I23, I24 | re-dérivabilité de la cause | I23 **renforcé** : la cause devient une fonction des deux couples d'entrées — elle n'est plus jamais autre chose que sa re-dérivation. ∎ |
| 012 § 1.2 (C6 : « τ entre deux états dont les deux index sont fournis ») ; § 3 | les entrées de l'invocation s'enrichissent | vrai et inchangé : les deux index restent fournis — l'énumération exhaustive des entrées de l'invocation vit au 014 § 1, que le complément amende ; le graphe du 012 § 3 est intact (les entrées d'une invocation ne sont pas un arc). ∎ |
| 013 §§ 4, 9 | « différé au premier scénario de transition » | les scénarios du § 9 sont joués pour la première fois (§ 6) — le différé prend fin par où il avait été annoncé. ∎ |
| 014 § 1 (C6) | l'invocation de τ sans cause en entrée | complément déclaré en en-tête : les deux énumérations entrent, la cause n'entre jamais — la clause « produit » devient dérivable de bout en bout. ∎ |
| 014 § 2 (objet Transition) | « cause (type + détail) » ; l'interdit | raffinement déclaré : la cause = suite de volets, chacun (type, détail) ; l'interdit est enfin **satisfiable** — tout constituant de τ est re-dérivable des deux index et de leurs membres. ∎ |
| 014 § 3 (la table ; « une convention ne parvient jamais à C6 ») | les énumérations vers C6 | aucune ligne nouvelle : les entrées de l'invocation de τ sont les productions de deux dérivations, fournies au titre de la clause « produit » de C6 — la lecture du 018 § 5 (honorer une clause déclarée n'est pas traverser hors table), consolidée par le 025 § 4 ; aucune convention ne traverse — seuls des identifiants et des couples (identifiant, version) déjà présents dans l'index. ∎ |
| 014 § 7.5 | le type exclusif ; « identifiants d'actes ajoutés » | raffinement déclaré : volets (§ 3) ; ajoutés et retirés ; les continuités gardent leur représentation (couples de références). ∎ |
| 015 (le journal) | le détail ℛ semble exiger le journal | il ne l'exige pas : le delta des couples se lit dans l'index ; le reste demeure re-dérivable de l'état désigné (§ 2). ∎ |
| 016 §§ 3, 4.2 (report 9) | « dérivation **et vérification** » | la dérivation est spécifiée ; la vérification est résolue par vacuité — plus rien à vérifier (§ 3). ∎ |
| 016 I59 | la sortie de τ change entre les régimes sur des index connus (les continuités : vides puis peuplées) | engagé et résolu par sa première exception : correction publiée d'une non-conformité antérieure — écart publié (§ 5) ; la surface de W est rigoureusement identique. ∎ |
| 017 (couverture, sept erreurs) | une erreur nouvelle (« cause fausse ») aurait été nécessaire sous d'autres voies | aucune : la table des sept erreurs reste close, le « seul cas » de C7 aussi (§ 3). ∎ |
| 018 §§ 3, 4, 6, 9 | la quarantaine ; l'ordre membre par membre ; la démonstration EXG-02 | raffinement déclaré des §§ 3 et 6 (la cause disparaît des « reçoit ») ; l'ordre du § 4 (index avant puis index après) inchangé ; la démonstration du § 9 devient exacte sans lecture de compromis. ∎ |
| 019 § 2 (T1) | — | close : la cause a désormais une source contractuelle — les entrées de C6. ∎ |
| 020 (l'énumération vers C5) | voisine du complément | même patron, même objet, seconde destination : l'énumération est « re-dérivable de Ω » (020) — le complément la fournit à l'invocation de τ. ∎ |
| 021, 022 | — | consommés tels quels : la carte des refus intacte ; le 022 fonde l'impossibilité du § 2 (les actes à espace trivial hors de tout domaine). ∎ |
| 024 (la référence totale) | les couples de continuités | portés par la référence totale du 024, comme la correspondance. ∎ |
| 025 (l'identité d'un état d'Ω) | la détection du changement Ω repose sur l'identité | c'est sa condition de détectabilité (§ 2) : le 025 la garantit (deux états distincts ont deux identités) sans avoir anticipé le régime de la cause (son en-tête l'exclut nommément). ∎ |

**W₀ est inchangé** : la cause et les continuités ne vivent que dans τ ; aucun acte, aucun index, aucune sortie de dérivation n'est touché.

---

## Récapitulatif

| Objet | Contenu | § |
|---|---|---|
| le constat | T1 réalisée : cause exigée (014 § 7.5), jamais en entrée de C6 (014 § 1), interdit du 014 § 2 violable par toute cause fausse ; troisième entrée (EXG-02) ; continuités vides contre E5 ; type exclusif contre EXG-30 | 1 |
| la démonstration | tout est dérivable sauf le détail Ω — impossible depuis les entrées actuelles (empreinte non inversible, couverture contingente par le 022) ; une seule entrée manque : les énumérations d'identifiants (patron du 020) ; le détail profond de ℛ re-dérivable par référence (régime I23, lecture du 025 § 5) | 2 |
| la décision (Cas B) | la cause = suite des volets (Ω puis ℛ, possiblement vide), dérivée par C6 ; détail Ω = ajoutés et retirés ; détail ℛ = delta des couples ; le porteur perd le paramètre ; la vérification résolue par vacuité | 3 |
| les continuités | dérivées par C6 entre élections : même strate, même contenu propositionnel, domaines se recouvrant (006 § 5 à la lettre) ; les triviales déclarées (E5) | 4 |
| les rejets et I59 | Cas A : la lettre doit bouger (détail Ω impossible, type non représentatif) ; Cas C rejeté comme qualification (raffinement, aucune garantie retirée) ; I59 engagé sur la surface de τ — première branche exercée, écart publié, déclaration rétroactive bornée à τ | 5 |
| le volet logiciel | l'objet cause en volets ; C6 dérive cause et continuités ; le porteur convoie les énumérations ; les scénarios du 013 § 9 joués ; CLI intacte | 6 |

**Ce que ce document ne fait volontairement pas** : modifier un document figé, changer un acte ou l'index de W₀, spécifier la forme canonique matérielle de W ou de τ (report 3), toucher à la vérification de cohérence d'état de C6 (report 4), ajouter une erreur contractuelle ou un cas de refus à C7, créer une convention, créer un invariant.
