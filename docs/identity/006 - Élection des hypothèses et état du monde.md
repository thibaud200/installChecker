# 006 — Élection des hypothèses et état du monde

**Statut** : septième document de la série `docs/identity/`. S'appuie exclusivement sur les documents 000 à 005, validés et figés.
**Périmètre** : définir ce qu'est **l'acte de rétention** d'une hypothèse, l'**état du monde** qui en résulte, sa **continuité** et sa **révision**. Ce document ne décrit **pas comment choisir** une hypothèse : aucun algorithme, aucun score, aucun seuil, aucun mécanisme de choix, aucune convention concrète supplémentaire. Il définit l'objet qui permettra un jour au moteur de dire : *« parmi toutes les hypothèses compatibles, celles-ci sont actuellement retenues »*.

---

## 1. L'acte de rétention

**Définition 1 (élection)** — L'*élection* d'une hypothèse h est un **acte tracé**

  e = ( h, 𝒮, (Ω, K), niveau, motif )

qui confère à h le statut *retenue* : à la strate 𝒮 où elle est placée, sous l'index (Ω, K), avec le niveau de certitude assigné (§ 8) et le **motif** — la justification structurelle de la rétention : position de h dans l'ordre de préférence (maximalité, unicité ou non), concurrentes écartées avec leur trace (003, just), arbitrages mobilisés (dette, § 9).

Trois principes constitutifs, dans l'ordre où ils protègent le système :

- **L'existence précède la rétention.** Une hypothèse existe dans l'espace de sa strate indépendamment de toute élection (005, § 1) ; l'élection est un **acte supplémentaire**, qui ajoute un statut sans rien changer à l'objet (mêmes Dom, Obs, Sig, prov, just — cohérence avec I16 et I20).
- **La rétention n'est pas une promotion ontologique.** Une hypothèse retenue reste une hypothèse : défaisable, concurrençable, révisable (I21, § 11). « Retenue » signifie *actuellement la meilleure explication engagée* — jamais « vraie » (000 § 3.3).
- **La non-rétention n'est pas une destruction.** Une hypothèse non retenue — dominée, incomparable, ou simplement non couverte par un acte d'élection — demeure dans l'espace, dérivable avec sa provenance (I19). Elle peut être élue demain sans avoir jamais cessé d'exister.

---

## 2. L'état du monde

**Définition 2 (état du monde)** — Un *état du monde* est un couple

  W = ( (Ω, K), E ∪ R )

où (Ω, K) est l'index — l'état des observations persistées et le répertoire des conventions — et E ∪ R l'ensemble **cohérent** (§ 3) des actes qui composent l'état : E les élections (Déf. 1) et R les refus de conclure (§ 4), couvrant chacun un domaine et une strate.

**Dépendance exclusive à (Ω, K).** Tout ce qui figure dans W se justifie depuis Ω (les faits) et K (les lectures décidées) — et depuis **rien d'autre** : ni l'heure, ni la machine, ni l'ordre de traitement, ni un opérateur, ni une source externe non observée (A0). Ce principe a une conséquence exigeante, traitée de front au § 10 : lorsque (Ω, K) **ne force pas** une élection (incomparables persistants, sous-détermination), l'état ne peut pas trancher par un intrant caché — le résultat conforme est alors le **refus de conclure**, et tout engagement au-delà exige une convention nouvelle, c'est-à-dire une transition de K, tracée. Le couple (Ω, K) détermine ainsi l'*espace des états compatibles* (§ 10) et, en son sein, un état prudent canonique — jamais un choix arbitraire.

---

## 3. La cohérence globale

**Définition 3 (état cohérent)** — Un état W est *cohérent* lorsqu'il satisfait simultanément :

- **la cohérence propositionnelle** (P1) : aucune paire d'actes de E ne retient des hypothèses concurrentes (003, Déf. 4) sur un même domaine ;
- **les strates et le raffinement** (I17) : les hypothèses co-retenues à des strates différentes respectent l'emboîtement — les origines fines retenues sont incluses dans les grossières retenues ; aucune élection fine ne contredit une élection grossière ;
- **les dépendances** : pour tout h retenu, Obs(h) ⊆ Ω et Dep(h) ne référence que des conventions **en vigueur** dans K (versions comprises, 004 § 10) — un état ne retient jamais une hypothèse dérivée sous un autre index ;
- **les conventions** (I13) : chaque étape de chaque dérivation mobilisée est licenciée par K — aucun implicite ;
- **les préférences** : aucune hypothèse retenue n'est **strictement dominée** (003, Déf. 5) par une hypothèse formulable sous le même (Ω, K) — retenir une explication dominée alors qu'une meilleure existe est une incohérence, pas un choix ;
- **la complétude des refus** : tout domaine-strate non couvert par une élection l'est par un refus explicite (§ 4) — l'état ne laisse pas de zones muettes : il élit ou il refuse, mais il *répond*.

Aucune méthode de calcul n'est définie : la cohérence est un **prédicat** sur les états, pas un procédé pour les produire.

---

## 4. Le refus de conclure

Le principe P7 (000 § 6, moindre engagement) reçoit ici sa forme d'objet :

**Définition 4 (refus de conclure)** — Un acte tracé

  r = ( D, 𝒮, (Ω, K), motif structurel )

déclarant qu'**aucune hypothèse n'est retenue** sur le domaine D à la strate 𝒮, avec le motif structurel exact : *incomparabilité persistante* (plusieurs maximales, 003 § 4.3), *silence* (aucune hypothèse non extrême formulable, 000 L2 — la minimale seule non dominée, 005 § 11), *contradiction ouverte* (003, Déf. 7), ou *sous-détermination* (classes d'indiscernabilité, 000 L1).

Statut, sans ambiguïté :

- le refus est un **résultat positif** : un constat structurel sur l'espace des hypothèses sous (Ω, K) — pas une erreur, pas un manque, pas un état d'attente honteux. Le monde peut être réellement sous-déterminé ; l'état qui le dit est plus fidèle que l'état qui invente ;
- il est un **composant de plein droit de W** (Définition 2) : daté par son index, motivé, révisable comme une élection — l'arrivée d'observations ou de conventions peut le convertir en élection, et inversement ;
- il est **le comportement déterministe de l'élection non forcée** (§ 2) : là où la préférence laisse des incomparables, l'état prudent refuse — c'est ainsi que la reproductibilité (P2, I11) s'étend jusqu'aux états du monde sans intrant caché.

---

## 5. La continuité

**Définition 5 (continuité d'une identité)** — Une relation tracée entre actes d'élection d'états successifs : l'élection e′ ∈ W′ est déclarée *successeur* de e ∈ W lorsque l'état W′ retient que l'origine postulée par h′ est **la même origine** que celle que postulait h — déclaration justifiée par le recouvrement de leurs domaines et dérivations, et enregistrée dans la trace de transition (§ 7).

Mise au point impérative :

> **La continuité est une propriété des hypothèses retenues.** Pas des observations — aucun attribut de Ω ne traverse nécessairement la vie d'une identité (005 § 7). Pas des signaux — ils se reconstruisent à chaque index (I5). Pas des fichiers — ils sont hors domaine (A0, 001 Déf. 1).

La continuité est donc elle-même **hypothétique au second degré** : c'est l'hypothèse que deux hypothèses retenues successivement parlent de la même origine. Elle se déclare, se trace, se révise — elle ne se constate jamais. « La même identité qu'hier » est un engagement de l'état, pas un fait ; le cadre l'assume au lieu de le dissimuler.

---

## 6. La révision

**Définition 6 (révision d'état)** — Le passage d'un état W à un état W′ d'index différent. **Deux causes, et deux seulement** (003, Déf. 9–10, élevées aux états) : Ω change (actes d'observation nouveaux) ou K change (transition de conventions — adoption, version nouvelle, retrait). Aucune troisième cause n'existe : un état ne se révise ni par l'usure du temps, ni par caprice — sans changement d'index, W est immuable (I24).

Le point que ce paragraphe doit établir :

> **Une identité peut être remplacée sans jamais avoir été fausse.** L'hypothèse détrônée était, sous son index (Ω, K), la meilleure explication engagée — et elle le **reste sous cet index** : re-dérivable, cohérente, justifiée (I23). Le remplacement n'est pas la correction d'une erreur ; c'est le déplacement de la meilleure explication sous un index nouveau. « Faux » ne s'applique qu'aux hypothèses *réfutées* (niveau impossible — une observation les contredit) ; une hypothèse simplement *dominée après coup* n'a jamais été fausse : elle a été la bonne réponse à une question plus pauvre.

C'est la conséquence directe du statut abductif de l'identité (000 § 3.3) : les élections ne prétendent jamais au vrai, elles ne peuvent donc pas avoir menti.

---

## 7. La dérivation temporelle

**Définition 7 (transition)** — La relation entre deux états successifs est l'objet tracé

  τ = ( W → W′, cause, correspondance )

où *cause* est le changement d'index (quels actes ajoutés à Ω, ou quelle transition de K, datée et justifiée), et *correspondance* le classement exhaustif du contenu de W′ par rapport à W :

- **conservé** — les actes re-dérivés à l'identique sous le nouvel index : mêmes hypothèses (au sens de I20), mêmes niveaux, mêmes motifs. Par localité (P3, 004 § 9), tout ce dont les dépendances ne touchent ni les observations ajoutées ni les conventions modifiées est conservé *par construction* ;
- **abandonné** — les élections que le nouvel index ne soutient plus (hypothèse désormais dominée, dépendance révoquée, contradiction nouvelle) et les refus convertis. Abandonné ≠ détruit : tout reste dérivable sous l'ancien index (I19, I23) ;
- **nouveau** — les élections et refus que l'ancien index ne permettait pas : hypothèses nouvellement formulables, dominations nouvellement établies, refus nouvellement motivés ;
- la **continuité** (§ 5) est déclarée à travers τ : les successions d'identités font partie de la correspondance.

Aucun procédé n'est décrit : τ est la *forme* de toute transition, quelle que soit la manière dont un moteur futur la produira.

---

## 8. Qui porte la certitude

Le 005 (§ 10) a défini la **propagation** des niveaux ; reste à dire **qui les porte**. Par élimination, chaque étage inférieur est disqualifié par ses propres invariants :

- **pas l'observation** : un fait n'a pas de degré — il est (001 § 1.3) ; lui attacher un niveau confondrait le rapport et la chose ;
- **pas le signal** : la neutralité I9 l'interdit — un signal explicite, il ne croit pas ; ses régimes (R1–R5) décrivent sa production, pas une confiance ;
- **pas la strate** : un espace ne croit rien — il contient toutes les hypothèses, y compris incompatibles ;
- **pas même l'hypothèse en soi** : elle n'a aucune certitude intrinsèque (003 § 1.2).

> **Le porteur du niveau de certitude est l'hypothèse retenue dans un état donné** — c'est-à-dire l'acte d'élection e, qui assigne le niveau *relativement à l'index (Ω, K)* et à la position de h face à ses concurrentes sous cet index (000 § 5 : les définitions des niveaux sont exactement relationnelles). Le niveau est une composante de e (Définition 1), pas de h : la même hypothèse peut être retenue probable sous un index et possible sous un autre, sans changer d'objet (I20). Les lois du 005 (propagation ascendante, maillon le plus faible) contraignent les niveaux *entre* actes co-présents dans W — elles opèrent donc, elles aussi, au niveau de l'état.

---

## 9. La dette conventionnelle dans l'état

Le 004 (§ 6, § 10) a défini la dette d'arbitrage — le sous-ensemble identifié de Dep(h) que h doit à des résolutions conventionnelles. Son rôle dans un état du monde :

- chaque acte d'élection **expose** la dette de son hypothèse : quelles contradictions ont été tranchées par décret, sous quelles conventions (κ, ver). C'est une **information attachée**, consultable — jamais un malus, jamais un ordre numérique, jamais une pondération (I9, I12) ;
- la dette est la **carte de fragilité** de l'état face aux révisions de K : révoquer une convention de priorité révise exactement les élections dont la dette la contient — et aucune autre (localité, 004 § 9). L'état peut ainsi répondre, pour toute conclusion : *« que faudrait-il renier pour que ceci tombe ? »* ;
- la **dette totale** d'un état est l'union des dettes de ses élections — même statut : information structurelle, comparée par inclusion si un document futur en a l'usage, jamais comptée ;
- un refus de conclure n'a pas de dette : ne rien engager ne doit rien. C'est l'expression ultime du moindre engagement (P7) — et une raison structurelle de plus pour que le refus soit le comportement par défaut de l'élection non forcée.

---

## 10. Les états concurrents

**Définition 8 (états compatibles)** — Deux états W₁ ≠ W₂ de **même index** (Ω, K) sont *concurrents* lorsque tous deux sont cohérents (Définition 3). L'ensemble des états cohérents d'un index donné est entièrement déterminé par (Ω, K) — mais **son cardinal n'est jamais garanti égal à un**.

Pourquoi l'unicité échappe, structurellement :

- **l'incomparabilité** (003 § 4.2) : là où plusieurs hypothèses maximales coexistent, un état peut en retenir une, un autre l'autre, un troisième refuser — les trois sont cohérents ;
- **la sous-détermination** (000 L1) : des classes d'indiscernabilité entières laissent des découpages multiples également soutenus ;
- **les contradictions ouvertes** (003, Déf. 7) : assumées par des hypothèses différentes dans des états différents.

Structure de cet ensemble, sans mécanisme de choix :

- les états compatibles s'ordonnent partiellement par **engagement** : W₁ ⊑ W₂ lorsque toute élection de W₁ figure dans W₂ (W₂ convertit des refus de W₁ en élections cohérentes) ;
- **l'état prudent** — celui qui n'élit que là où l'élection est forcée (maximale unique, aucune incomparable) et refuse partout ailleurs — existe toujours et est le **minimum** de cet ordre. C'est lui que la dépendance exclusive à (Ω, K) désigne comme canonique (§ 2, § 4) ;
- s'engager au-delà du prudent n'est pas interdit — mais **tout supplément d'engagement exige une convention** (une transition de K, tracée, justifiée, révocable) qui rende l'élection forcée sous le nouvel index. Le choix entre états concurrents n'est jamais un acte libre du moteur : c'est ou bien un refus, ou bien une convention. Il n'y a pas de troisième voie.

---

## 11. Invariants

> **I21 — Une hypothèse retenue reste une hypothèse.** L'élection ne transmute pas : l'hypothèse retenue demeure défaisable, concurrençable, révisable, et n'acquiert aucun statut de vérité. Tout langage futur qui traiterait une élection comme un fait (« ce contenu *est* X ») violerait I21.

> **I22 — Un état du monde ne crée aucune observation.** Rien de ce qu'un état contient — élections, refus, continuités, transitions, dettes — n'entre dans Ω, ne modifie Ω, ni ne prétend au statut d'observation (prolongement terminal de A0, I1, I15, I18).

> **I23 — Toute révision est entièrement reconstructible.** Pour toute transition τ : W → W′, les deux états sont re-dérivables sous leurs index respectifs, la cause est tracée (actes ajoutés ou transition de K), et la correspondance (conservé / abandonné / nouveau / continuités) est restituable exactement. Aucune révision n'efface son passé (extension de I10 et de l'historicité de K aux états).

> **I24 — Toute décision est relative au couple (Ω, K).** Aucun acte d'un état — élection, refus, niveau, continuité — ne dépend d'autre chose que de l'index : ni instant, ni machine, ni ordre de traitement, ni opérateur, ni source non observée. À index égal, l'espace des états cohérents est identique ; et l'état prudent, identique. (P2 et I11 étendus jusqu'au sommet de l'édifice.)

---

## 12. Exemples — corpus 1 exclusivement, sans décision réelle

**E1 — Rétention forcée** (une classe ≡ₘ de 3 actes).
Sur ce domaine, à la strate contenu, l'hypothèse « même contenu » domine strictement sa seule concurrente (collision, 003 § 9) : la maximale est unique, l'élection est **forcée** — tout état cohérent la contient, au niveau « certain » conventionnel si la convention-plafond figure dans K (004, E6). C'est un théorème du cadre, pas une décision effectuée : l'exemple décrit ce que *tout* état cohérent contient, sans en produire aucun.

**E2 — Non-rétention sans destruction** (le même domaine).
L'hypothèse de collision n'est retenue dans aucun état cohérent (dominée, § 3) — et n'est détruite dans aucun (I19) : elle demeure dans l'espace, dérivable, citée dans le motif de l'élection de E1 comme concurrente écartée. Si un jour Ω contenait de quoi la soutenir, elle serait élue sans avoir jamais cessé d'exister.

**E3 — Refus de conclure** (les 59 actes au sujet signataire Python Software Foundation, strate version).
Les découpages en versions restent incomparables sous (Ω, K) actuel (005, E2) : aucune maximale unique. L'état prudent enregistre r = (ces 59 actes, strate version, index, *incomparabilité persistante*) — un résultat positif et daté, qui dit exactement pourquoi il ne dit rien de plus. Aucune erreur, aucun manque : la base ne contient pas de quoi départager, et l'état le déclare.

**E4 — Révision par K, remplacement sans fausseté** (les 20 actes en condition A-01).
Sous un K sans l'entrée A-01 : la contradiction intra-acte est ouverte ; l'hypothèse h₁ qui l'assume peut être la mieux placée. Sous le K qui adopte A-01 (transition réelle, actée au 002) : h₂ (lecture artefactuelle) devient formulable et domine (003, E5). La transition τ trace la cause (adoption de A-01 v1), l'abandon de h₁, l'élection de h₂ — et h₁ **n'a jamais été fausse** : sous l'ancien index, elle était la meilleure explication disponible. Les 20 observations n'ont pas changé d'un octet.

**E5 — Continuité et localité** (transition Ω mesurée : le corpus est passé de 496 à 497 actes).
Entre W(Ω₄₉₆, K) et W(Ω₄₉₇, K) — l'ajout réel du fichier géant, campagne post-A1 : tout acte dont le domaine ne touche pas le nouvel acte est **conservé à l'identique** (P3, § 7) ; le nouveau domaine reçoit ses propres actes (à la strate contenu : une classe ≡ₘ nouvelle ; aux strates supérieures : silence quasi total de l'acte → refus motivés par L2). Les continuités déclarées à travers τ sont triviales ici (les mêmes hypothèses se succèdent à elles-mêmes) — la forme de l'objet, elle, est exactement celle du § 5.

---

## Conclusion

Avec ce document, la série 000 → 006 définit complètement, sans un seul acte de calcul :

- **les faits** (001 : observations, leurs états, leur intangibilité),
- **leur interprétation** (002 : signaux, régimes, artefacts),
- **les hypothèses** (003 : consensus, résidus, préférence),
- **les conventions** (004 : K, priorités, résolutions, attentes),
- **les strates** (005 : espaces de granularité, raffinement, extrêmes),
- **l'état du monde** (006 : élections, refus, continuité, révision, états concurrents).

Il ne manque plus que le cadre permettant, dans les documents suivants, de définir les **règles de décision** — les conventions concrètes qui rendront des élections forcées là où l'état prudent refuse aujourd'hui. Tout ce qui précède les borne par avance : ces règles seront des objets de K, minimales, tracées, révocables, et soumises aux invariants I1–I24.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| élection e = (h, 𝒮, (Ω,K), niveau, motif) | acte tracé conférant le statut « retenue » ; n'altère pas l'objet ; ne transmute pas en vérité | 1 |
| état du monde W = ((Ω,K), E ∪ R) | ensemble cohérent des élections et refus ; dépend de l'index et de rien d'autre | 2 |
| cohérence d'état | prédicat : P1 + I17 + dépendances en vigueur + I13 + aucune retenue dominée + complétude des refus | 3 |
| refus de conclure r = (D, 𝒮, (Ω,K), motif) | résultat positif de plein droit ; comportement déterministe de l'élection non forcée | 4 |
| continuité | relation déclarée entre élections d'états successifs ; propriété des hypothèses retenues, hypothétique au second degré | 5 |
| révision | changement d'index (Ω ou K, seules causes) ; remplacement sans fausseté | 6 |
| transition τ = (W→W′, cause, correspondance) | conservé / abandonné / nouveau / continuités, exhaustif et tracé | 7 |
| porteur de la certitude | l'hypothèse retenue dans un état donné — le niveau est une composante de l'acte d'élection | 8 |
| dette dans l'état | information attachée aux élections ; carte de fragilité face aux révisions de K ; les refus n'ont pas de dette | 9 |
| états concurrents | états cohérents de même index ; ordre partiel par engagement ; l'état prudent en est le minimum ; s'engager davantage = convention | 10 |
| invariants | I21 retenue reste hypothèse, I22 aucune observation créée, I23 révision reconstructible, I24 relativité à (Ω, K) | 11 |

**Ce que ce document ne fait volontairement pas** : produire un état réel, élire une hypothèse réelle, définir une règle d'élection, assigner un niveau effectif, déclarer une continuité effective.
