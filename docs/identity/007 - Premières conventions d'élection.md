# 007 — Premières conventions d'élection

**Statut** : huitième document de la série `docs/identity/`. S'appuie exclusivement sur les documents 000 à 006, validés et figés.
**Périmètre** : définir la **forme** des conventions qui autoriseront un jour une élection — la première catégorie de conventions capable de transformer un état prudent (006 § 10) en état engagé. Ce document définit ces conventions ; il ne les applique pas. Aucun code, aucun algorithme, aucun score, aucun seuil, aucune heuristique, aucune décision spécifique aux installateurs, aucune statistique nouvelle.
**Raffinement assumé** : ce document durcit le 006 sur un point précis (l'élection « forcée » exige désormais une licence normative, I27) — l'écart est motivé au § 3 et récapitulé en fin de document.

---

## 1. Les conventions d'élection : une famille nouvelle de K

**Définition 1 (convention d'élection)** — Une convention κ ∈ K (forme complète du 004, Déf. 1) dont la transformation T(κ) est une **licence de rétention** : elle décrit une configuration structurelle — une hypothèse, sa position dans l'ordre de préférence, la nature et le régime de son soutien — et **autorise**, lorsque cette configuration est réalisée dans un état (Ω, K), l'acte d'élection correspondant (006, Déf. 1), y compris le niveau de certitude que cet acte peut assigner.

Ce que cette famille **ne fait jamais** — et qui la distingue de toutes les familles précédentes :

- elle **ne produit aucune observation** (I26 ; rien de nouveau sous A0) ;
- elle **ne produit aucun signal** (la couche 2 appartient aux conventions d'interprétation et d'équivalence, 004 § 3–4) ;
- elle **ne crée aucune hypothèse** (I25 ; l'espace des hypothèses est engendré par les couches 3 et 5 — 003, 005) ;
- elle **autorise uniquement la rétention** d'une hypothèse existante.

La famille prend place dans la typologie de K aux côtés des conventions d'interprétation, d'équivalence, de priorité, d'attente, de catalogue d'artefacts (004), de stratification et de composition (005). Elle est la **dernière-née et la plus haute** : elle opère sur la couche terminale (l'état du monde), là où toutes les autres opèrent sur les couches de construction. C'est pourquoi elle n'apparaît qu'au 007 : sa définition présuppose tout l'édifice.

---

## 2. Domaine d'application

Une convention d'élection agit :

- **uniquement sur les hypothèses existantes** — celles que l'espace de leur strate contient déjà sous l'index courant. Si l'hypothèse souhaitée n'est pas formulable (signaux manquants, composition non licenciée), la convention d'élection est sans objet : la réponse est en dessous (capacités nouvelles pour Ω, conventions de construction pour K), jamais dans une licence qui fabriquerait son propre candidat (I25) ;
- **uniquement dans un état (Ω, K)** — la licence s'évalue sur la configuration réalisée sous l'index : position de préférence *sous cet Ω*, régimes des signaux *sous ce K*. Elle ne dit jamais rien dans l'absolu ; la même convention peut licencier une élection sous un index et rien sous un autre ;
- **jamais ailleurs** : pas sur les observations (I26), pas sur les signaux, pas sur les espaces de strates, pas sur les conventions des autres familles — et pas sur les états passés (l'historicité de K, 004 § 2 : une adoption ne réécrit aucun état antérieur, I23).

---

## 3. Le principe de moindre engagement, complété

Le principe P7 (000 § 6) reçoit ici sa forme achevée, comme **asymétrie de la charge de justification** :

> **La charge de la justification pèse toujours sur l'engagement, jamais sur le refus.** Une convention d'élection doit expliquer *pourquoi elle autorise un engagement supplémentaire* — quelle configuration structurelle rend la rétention défendable, et pourquoi le refus serait, dans cette configuration, une prudence sans objet. L'inverse n'existe pas : **le refus de conclure n'a jamais besoin de convention** — il reste toujours valide, dans tout état, sur tout domaine, à toute strate (006 § 4). On n'adopte pas de convention pour refuser ; refuser est l'état de nature du système.

Conséquence — et c'est le durcissement annoncé du 006 :

> **I27 (anticipé ici, énoncé au § 11)** — Toute élection est justifiée par au moins une convention adoptée. La « rétention forcée » du 006 (maximale unique, aucune incomparable) décrit la configuration où une élection est *licenciable sans risque structurel* — elle ne constitue pas la licence elle-même. Avant toute adoption, l'état prudent est fait **exclusivement de refus**, y compris sur les configurations forcées, avec pour motif : *« élection licenciable, aucune convention adoptée »*. Le système ne s'engage jamais par défaut, pas même quand l'engagement serait sûr : la sûreté se constate, la licence se décide.

Cette asymétrie a un corollaire de dette : les refus ne doivent rien (006 § 9) ; toute élection doit au moins sa convention d'élection — la dépendance minimale de tout engagement.

---

## 4. La convention-plafond : CE-01 (candidate)

Première convention d'élection formalisée — celle qu'annoncent le 000 (§ 5.1), le 004 (E6) et le 006 (E1) :

**CE-01 — Élection par identité de contenu** *(candidate, non adoptée)*

- **T(CE-01)** : lorsque, sur un domaine d'actes, l'hypothèse « même contenu » (strate contenu) est soutenue par le signal relationnel d'égalité parfaite de contenu en régime exact, et qu'elle domine strictement toute concurrente formulable, la rétention de cette hypothèse est autorisée, **au niveau maximal de certitude** (« certaine », au sens conventionnel du 000 § 5.1).
- **Portée exacte** : la strate contenu, et elle seule. CE-01 ne dit rien des strates supérieures — la certitude du contenu ne remonte pas la chaîne (005 § 10) ; élire « même contenu » n'élit ni variante, ni version, ni identité.
- **Ce que CE-01 établit conceptuellement** : qu'une **égalité parfaite de contenu peut constituer une convention donnant le niveau maximal** — c'est-à-dire que le plafond « certain » du système est lui-même un objet normatif ordinaire (004, E6), adopté et révocable comme les autres, et non une vérité du monde. Le présent document formalise la convention et s'arrête là : son **adoption** serait l'acte inaugural du registre (§ 8–9) — un acte réel, hors du périmètre de ce document.

Le document s'interdit, conformément au prompt, toute discussion de collisions, de fonctions de hachage et d'implémentation : CE-01 est énoncée au niveau de l'égalité de contenu comme objet logique ; la manière dont un système constate cette égalité relève des couches d'observation, déjà figées.

---

## 5. Conditions minimales de toute convention d'élection

Au-delà de la forme générale du 004 (Déf. 1), une convention d'élection doit obligatoirement préciser :

| Champ | Contenu exigé |
|---|---|
| **domaine** | la strate concernée et la configuration structurelle exacte (position de préférence requise, nature du soutien) — jamais « partout » |
| **justification** | pourquoi l'engagement est défendable dans cette configuration — la réponse à la charge du § 3 |
| **limites** | les cas connus où la licence pourrait égarer (configurations frontières, dépendance à la qualité du corpus) — une convention sans limites déclarées est suspecte par construction |
| **dépendances** | les conventions des autres familles qu'elle présuppose (interprétations, équivalences, compositions, entrées de catalogue) — CE-01, par exemple, présuppose la convention d'égalité de contenu (004, E6) |
| **régime des signaux utilisés** | quels régimes (R1–R5) sont admis dans le soutien de l'hypothèse licenciée, selon les contraintes du § 7 |
| **portée** | ce que l'élection autorisée peut assigner : quelle strate, quel plafond de niveau — jamais plus que ce que la configuration soutient |
| **conditions de révision** | ce qui justifierait son retrait ou sa révision (limite franchie en pratique, dépendance révoquée, configuration reconnue trop permissive) |

**Aucune autre convention d'élection n'est instanciée** dans ce document : CE-01 est la seule formalisée, et elle reste candidate.

---

## 6. Compatibilité des conventions d'élection

**Définition 2 (compatibilité)** — Deux conventions d'élection sont *compatibles* lorsque, pour tout index (Ω, K) et tout domaine partagé, les élections qu'elles licencient peuvent co-figurer dans un état cohérent (006, Déf. 3). Elles sont *incompatibles* lorsqu'il existe une configuration où leurs licences produisent des rétentions concurrentes (P1), ou violant la cohérence verticale (I17), ou assignant des niveaux inconciliables aux mêmes actes.

Représentation, sans mécanisme de résolution :

- l'incompatibilité est **déclarée dans le registre** (§ 8) comme un objet propre — au même titre que les contradictions entre signaux sont représentées sans être tranchées (002 § 9) : le couple de conventions, la configuration qui les oppose, la date du constat ;
- deux conventions déclarées incompatibles **ne peuvent être simultanément en vigueur** sur le domaine du conflit : l'adoption de la seconde exige le retrait ou la restriction de la première — c'est une contrainte de cohérence du registre, pas un arbitrage automatique ;
- l'incompatibilité peut être *partielle* (limitée à une strate ou à une configuration) : la déclaration précise son étendue, et les conventions coexistent hors du conflit.

---

## 7. Régimes des signaux et admissibilité

Le 002 (§ 5) définit les régimes R1–R5. Une convention d'élection doit déclarer les régimes admis dans le soutien des hypothèses qu'elle licencie, sous les contraintes théoriques suivantes — catégorielles, sans seuil, sans niveau de confiance :

- **R1 (exact)** — admissible sans restriction : le régime nominal est le terrain naturel des licences ;
- **R2 (incomplet)** — un soutien ne peut jamais **citer une absence comme appui** (principe du silence, 003 § 6) ; une convention d'élection ne peut donc admettre R2 que pour des signaux explicitement définis sur l'absence (écarts d'attente, 004 § 7), et uniquement comme *faits à charge du résidu des concurrentes* — jamais comme corroboration directe de l'hypothèse licenciée ;
- **R3 (ambigu)** — admissible seulement si la convention **démontre dans sa justification que l'ambiguïté est sans incidence** sur le contenu propositionnel licencié (l'ambiguïté MSI/MSP du conteneur est sans incidence sur une hypothèse de strate contenu ; elle est rédhibitoire pour une hypothèse qui distinguerait les deux formats) ;
- **R4 (contradictoire)** — aucune élection ne peut être licenciée **par-dessus une contradiction ouverte portant sur le contenu même** de l'hypothèse ; si la contradiction est résolue explicativement, le soutien est admissible ; si elle est résolue conventionnellement, l'élection porte la dette d'arbitrage correspondante (004 § 6, 006 § 9) et la convention d'élection doit déclarer qu'elle admet des soutiens endettés ;
- **R5 (artefactuel)** — **jamais admissible comme soutien** : un signal en régime artefactuel ne fonde aucune élection. En revanche, une élection peut s'appuyer sur l'hypothèse de statut artefactuel elle-même (dont le soutien — la signature d'une entrée de catalogue — est en régime exact) : on élit « la lecture est artefactuelle », jamais « l'architecture est PK » ;
- **héritage** — le soutien composite hérite du pire régime de ses composantes (002 § 7.2) : une chaîne de soutien vaut son maillon le plus fragile, et c'est ce maillon que la convention doit déclarer admissible ou non.

---

## 8. Le registre des conventions

**Définition 3 (registre ℛ)** — L'objet représentant l'ensemble des conventions **réellement adoptées**, avec pour chacune : sa version en vigueur, sa justification, son historique complet (adoptions, révisions, retraits, datés et motivés), et les incompatibilités déclarées (§ 6).

Distinction à poser une fois pour toutes :

> **K théorique** est l'espace des conventions *formulables* — tout objet conforme à la forme du 004 (Déf. 1) en fait partie, y compris les candidates jamais adoptées et les variantes incompatibles entre elles. **Le registre ℛ** est la partie *en vigueur* : ce que les documents 004 à 006 appelaient « l'état de K » est exactement un état du registre. La symétrie avec l'étage supérieur est complète et voulue : **les conventions sont au registre ce que les hypothèses sont à l'état du monde** — l'espace contient tout ; l'acte retient peu ; rien n'est détruit.

Une convention candidate (telle CE-01 aujourd'hui) existe dans K théorique, peut être discutée, comparée, déclarée incompatible avec d'autres candidates — et **n'a aucun effet** sur aucun état du monde tant qu'elle n'est pas adoptée.

---

## 9. L'adoption

**Définition 4 (adoption)** — L'acte tracé

  a = ( κ, ver, date, justification d'adoption )

qui fait entrer une convention dans le registre. Principe constitutif :

> **Une convention n'existe pas parce qu'elle est imaginable, mais parce qu'elle est adoptée.** L'imaginable peuple K théorique ; seul l'adopté gouverne.

Propriétés de l'acte :

- **explicite et daté** — il n'y a pas d'adoption tacite, d'usage qui « vaudrait » adoption (I13 : l'implicite n'existe pas), ni d'adoption rétroactive ;
- **justifié en propre** — la justification d'adoption s'ajoute à la justification interne de la convention (§ 5) : pourquoi *maintenant*, pourquoi *celle-ci* parmi les candidates, au vu de quel ancrage empirique ;
- **unitaire** — un acte adopte une convention (minimalité I14 transposée aux actes : pas d'adoption en bloc qui rendrait les retraits grossiers) ;
- **générateur de transition** — toute adoption est une transition du registre, donc un changement d'index (Ω, K) pour les états du monde, donc une révision potentielle (006 § 6–7), propagée le long des dépendances réelles et d'elles seules (004 § 9) ;
- **porté par une autorité extérieure au moteur** — l'adoption est une décision de gouvernance du système, pas un acte que le moteur s'accorde à lui-même ; le cadre exige la trace, il ne désigne pas le décideur (c'est un fait d'organisation, hors périmètre théorique).

---

## 10. La révision du registre

Cinq opérations, toutes des transitions tracées du registre, toutes propagées vers les états du monde par le mécanisme unique du 006 (§ 6–7) — conservé / abandonné / nouveau, le long des dépendances :

- **Ajout** — l'adoption (§ 9). Effet : des refus peuvent devenir des élections (des licences nouvelles s'appliquent à des configurations déjà réalisées) ; rien d'existant ne tombe (les licences ne se retirent pas mutuellement, sauf incompatibilité déclarée, § 6) ;
- **Retrait** — la sortie du registre d'une convention en vigueur, datée et motivée (limite franchie, dépendance révoquée). Effet : les élections dont la dépendance contient la convention retirée **retombent en refus** — sauf si une autre convention en vigueur les licencie indépendamment ; l'état prudent regagne le terrain. Rien d'autre ne bouge (localité) ;
- **Remplacement** — un retrait et un ajout couplés en une transition unique, tracée comme telle, avec correspondance explicite entre l'ancienne et la nouvelle licence. Effet : les élections migrent de dépendance là où la nouvelle licence couvre, tombent là où elle ne couvre plus ;
- **Scission** — la division d'une convention reconnue non minimale (violation constatée de I14 : deux transformations révisables indépendamment sous un même identifiant) en conventions minimales. Effet théorique nul sur les états au moment de la scission (les licences conjointes équivalent à la licence d'origine) ; effet réel ensuite : les révisions deviennent fines ;
- **Fusion** — l'opération inverse, admissible dans le seul cas où deux conventions se révèlent être les fragments d'une **unique** transformation conceptuelle (leur révision indépendante n'a jamais de sens) — sinon la fusion violerait I14. Rare par construction.

Dans tous les cas : les états antérieurs restent dérivables sous leurs index (I23), et les observations sont hors d'atteinte (I26, I28).

---

## 11. Invariants

> **I25 — Une convention d'élection ne crée jamais une hypothèse.** Elle sélectionne dans l'existant ; si le candidat souhaité manque, la réponse appartient aux couches de construction (capacités, interprétations, compositions) — jamais à la licence. Une licence qui fabriquerait son candidat déciderait des identités, ce que K ne contient jamais (004 § 12).

> **I26 — Une convention d'élection ne modifie jamais une observation.** Clôture terminale de la chaîne I1–I15–I18–I22 : la famille la plus haute de K est soumise à la même interdiction absolue que toutes les autres.

> **I27 — Toute élection est justifiée par au moins une convention adoptée.** Il n'existe pas d'élection de plein droit, pas même sur les configurations structurellement forcées : la sûreté d'un engagement se constate, sa licence se décide (§ 3). Corollaire : un registre vide de conventions d'élection produit des états faits exclusivement de refus — tous motivés, tous cohérents.

> **I28 — Toute convention peut être retirée sans perte des observations.** Le retrait de n'importe quelle convention — d'élection ou d'une autre famille — laisse Ω strictement intact et l'intégralité des états passés re-dérivable sous leurs index. Le système survit à n'importe quel repli normatif : au pire, il refuse davantage. C'est la garantie ultime de réversibilité (P4) : aucune décision de lecture, si ancienne soit-elle, n'est devenue constitutive des faits.

---

## 12. Exemples — corpus 1 exclusivement, sans décision réelle

**E1 — Convention applicable** : CE-01 sur les 381 classes de contenu (dont la classe à 3 actes). La configuration exigée est réalisée : signal relationnel en régime exact, domination stricte de l'unique concurrente, strate contenu. Si CE-01 était adoptée, tout état cohérent contiendrait ces élections au niveau plafond. Elle ne l'est pas : l'exemple constate l'applicabilité, il n'applique rien.

**E2 — Convention inapplicable** : une candidate CE-x licenciant des élections de strate variante sur soutien d'architecture interprétée. Sur les 20 actes en condition A-01, le soutien serait en régime R5 — inadmissible par § 7, quelle que soit la rédaction de CE-x : la configuration n'est jamais réalisée sur ce domaine. Sur les 61 actes PE concordants, la même candidate resterait inapplicable tant que les conventions d'interprétation qu'elle présuppose (table des codes machine) ne figurent pas au registre — dépendances non satisfaites (§ 5).

**E3 — Convention retirée** (théorique) : le retrait de CE-01 après adoption. Les élections de strate contenu retombent en refus motivés (« licence retirée ») ; les 497 actes, les 381 classes, tous les signaux : strictement intacts (I28) ; les états antérieurs restent dérivables sous leur index. Le système n'a rien perdu — il affirme moins.

**E4 — Convention révisée** (théorique) : une version 2 de CE-01 restreignant sa portée (niveau assigné « probable » au lieu de « certaine » — révision du plafond conventionnel, 000 § 5.1). Transition du registre, re-dérivation des seules élections dépendantes : mêmes hypothèses retenues (I20), niveaux réassignés, motifs mis à jour. Aucune observation, aucun signal, aucune hypothèse n'a changé — seule la hauteur de l'engagement.

**E5 — Conventions concurrentes** (théorique) : deux candidates de strate version sur le domaine des 59 actes au sujet signataire Python Software Foundation — l'une licenciant des élections fondées sur les déclarations de lignée interprétées, l'autre sur les seules successions composées conventionnellement (005 § 9). Sur les configurations où leurs hypothèses licenciées sont concurrentes (003, Déf. 4), leur incompatibilité serait déclarée au registre avec son étendue exacte (§ 6) ; en vigueur simultanément hors du conflit, jamais dedans. Aucun mécanisme ne les départage : adopter l'une, l'autre, ou aucune est un acte de gouvernance — pas un théorème.

---

## Conclusion

Les documents 000 → 007 définissent désormais complètement : **les faits** (observations, 001), **leur interprétation** (signaux, 002), **les hypothèses** (003), **les conventions** (004), **les strates** (005), **les états du monde** (006), et **les conventions d'élection** (007) — la famille qui, une fois le registre ouvert et les premières adoptions faites, transformera des refus en engagements tracés, révocables, justifiés.

**Le moteur de décision n'existe toujours pas.** Aucune convention n'est adoptée, aucun registre n'est ouvert, aucune élection réelle n'a eu lieu. Il ne manque plus que la **théorie des règles de résolution** — comment les conventions en vigueur s'appliquent ensemble, dans quel ordre conceptuel, avec quelles garanties de cohérence de l'ensemble — qui constituera le document 008.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| convention d'élection | licence de rétention : autorise l'acte d'élection d'une hypothèse existante dans un état ; ne produit ni observation, ni signal, ni hypothèse | 1 |
| domaine d'action | hypothèses existantes, dans un état (Ω, K) — jamais ailleurs, jamais rétroactif | 2 |
| moindre engagement complété | la charge de justification pèse sur l'engagement ; le refus n'a jamais besoin de convention | 3 |
| CE-01 (candidate) | l'égalité parfaite de contenu licencie l'élection « même contenu » au niveau maximal ; strate contenu uniquement ; non adoptée | 4 |
| conditions minimales | domaine, justification, limites, dépendances, régimes admis, portée, conditions de révision | 5 |
| compatibilité | co-figurabilité dans un état cohérent ; incompatibilité déclarée au registre, jamais résolue d'office | 6 |
| admissibilité par régime | R1 libre ; R2 jamais comme appui ; R3 si incidence nulle démontrée ; R4 si résolue (dette déclarée) ; R5 jamais ; héritage du pire | 7 |
| registre ℛ | les conventions réellement adoptées (versions, justifications, historique, incompatibilités) ; distinct de K théorique — l'espace du formulable | 8 |
| adoption | acte tracé, explicite, daté, justifié, unitaire, générateur de transition ; porté par une autorité extérieure au moteur | 9 |
| révision du registre | ajout, retrait, remplacement, scission, fusion — transitions tracées, propagées par les dépendances | 10 |
| invariants | I25 aucune hypothèse créée, I26 aucune observation modifiée, I27 toute élection licenciée par une convention adoptée, I28 tout retrait sans perte | 11 |

**Ce que ce document ne fait volontairement pas** : adopter une convention (CE-01 reste candidate), ouvrir le registre, effectuer une élection réelle, définir l'ordre de co-application des conventions en vigueur (théorie des règles de résolution, document 008).

