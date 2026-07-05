# 010 — Contrat d'implémentation du moteur d'identité

**Statut** : premier document de **conception** de la série `docs/identity/`. La série 000→009 est figée et constitue le référentiel théorique ; les invariants I1–I36 sont normatifs. Le pipeline d'observation est figé.
**Nature** : ce document ne crée aucune théorie. Il traduit la théorie en **contraintes opposables** à toute implémentation. Il est rédigé comme une spécification : chaque exigence est numérotée (EXG-nn) et doit pouvoir devenir un critère de validation. Il ne parle d'aucun moteur particulier — il décrit **tous les moteurs possibles**.
**Question unique** : *que devra respecter toute implémentation conforme ?*

---

## 1. La fonction fondamentale

> **EXG-01** — Le moteur d'identité est une fonction :
>
>   **IdentityEngine : (Ω, ℛ) → W**
>
> et rien d'autre. Ω est une base d'observations conforme au pipeline figé ; ℛ est un état identifié et versionné du registre des conventions ; W est l'état du monde prudent associé à cet index (006 § 10, 007 § 3).

> **EXG-02** — Le moteur n'a **aucune entrée cachée**. Toute donnée influençant W doit être un constituant de Ω ou de ℛ. En particulier, sont interdits comme intrants : l'horloge, l'identité de la machine, l'utilisateur, la locale d'exécution, toute source d'aléa, tout réseau, tout fichier du système observé (A0 : le moteur ne voit jamais un fichier), et **toute option de configuration modifiant le résultat** — un réglage qui change W est une convention déguisée, et une convention n'existe que dans ℛ (I13, I33).

> **EXG-03** — Le moteur n'a **aucun état interne persistant entre deux invocations, aucune mémoire, aucune dépendance temporelle**. Deux invocations sur le même couple (Ω, ℛ), à tout moment, dans tout ordre, sur toute machine, produisent le même W (I24).

---

## 2. Responsabilités

### 2.1 Ce que le moteur fait

> **EXG-04** — Le moteur dérive, conformément aux documents 001→008 : les signaux depuis Ω sous les conventions de ℛ (002), les relations et consensus (003), les hypothèses, leurs résidus et l'ordre de préférence (003), les placements de strates (005), les chaînes de résolution (008 § 5), et produit W : les élections licenciées et forcées, les refus motivés (structurels ou normatifs, 008 § 9), les niveaux de certitude portés par les élections (006 § 8), les dettes (006 § 9) et les continuités déclarées à travers les transitions (006 § 5–7).

> **EXG-05** — Le moteur répond, pour tout acte de W, aux questions : *pourquoi ?* (chaîne complète, § 5), *de quelles observations dépends-tu ?* (Obs, provenance), *de quelles conventions dépends-tu ?* (Dep, versions comprises), *qu'as-tu écarté et pourquoi ?* (concurrentes, motifs), *que faudrait-il renier pour que ceci tombe ?* (dette, ensembles minimaux).

### 2.2 Ce qui est interdit au moteur

> **EXG-06** — Le moteur **ne produit jamais Ω** : l'observation appartient au pipeline figé, exclusivement.
> **EXG-07** — Le moteur **ne modifie jamais Ω** : aucune écriture, aucune suppression, aucune reformulation, aucune annotation dans la base d'observations (I1, I15, I18, I22, I26, I32 — la série descendante complète).
> **EXG-08** — Le moteur **ne modifie jamais ℛ** : les adoptions, révisions et retraits sont des actes de gouvernance extérieurs (007 § 9) ; le moteur consomme le registre, il ne le gouverne pas.
> **EXG-09** — Le moteur **ne crée jamais de conventions** — ni explicitement, ni sous forme de valeurs par défaut, de cas particuliers codés, d'heuristiques ou de « bon sens » embarqué (I13, I29 : tout implicite est une violation).
> **EXG-10** — Le moteur **ne choisit jamais hors registre** : toute élection cite au moins une convention de ℛ (I27) ; là où aucune licence ne s'applique, l'unique comportement est le refus motivé (P7, 007 § 3).
> **EXG-11** — Le moteur **ne complète jamais une observation** : ⊥ reste ⊥ ; aucune valeur n'est inférée, déduite ou remplie (001 § 4.2–4.3).
> **EXG-12** — Le moteur **ne corrige jamais une observation** : une valeur artefactuelle ou contradictoire reste telle quelle ; seul son usage est qualifié, avec trace (001 § 4.5, 002 § 5).
> **EXG-13** — Le moteur **ne réinterprète jamais une convention** : il applique T(κ) à la lettre, dans la version indexée ; toute latitude d'interprétation constatée est un défaut de la convention (à réviser dans ℛ), jamais une liberté du moteur.

---

## 3. Les frontières

  **Observation Pipeline → Identity Engine → Consommateurs**

> **EXG-14** — Le moteur consomme Ω **exclusivement à travers son contrat public** : le modèle d'observations défini par le pipeline figé (actes identifiés, attributs par capacité, valeurs brutes, ⊥, invariant 1:1, `user_version` du schéma). Il ne dépend **jamais** : des extracteurs (il ne les invoque pas, ne les connaît pas), du moteur de stockage (la technologie de la base est un détail de représentation d'Ω — le moteur doit survivre à son remplacement), des formats de fichiers observés (PE, MSI, ZIP n'existent pour lui que comme valeurs d'attributs).
> **EXG-15** — Le moteur consomme ℛ exclusivement comme un état identifié et versionné de conventions conformes à la forme du 004 (Déf. 1) et du 007 (§ 5) ; il vérifie la cohérence du registre (008 § 4) avant toute dérivation, et **refuse toute élection sous un registre incohérent** (008 § 4 : le motif est « registre incohérent »).
> **EXG-16** — Les consommateurs (rapports, interfaces, outils) lisent W et les chaînes ; **rien de ce qu'un consommateur fait n'influence W** : pas de retour d'usage, pas de correction manuelle injectée, pas de préférence utilisateur — toute influence légitime passe par une transition de ℛ ou un enrichissement de Ω, et par là seulement (EXG-02).
> **EXG-17** — Les colonnes contextuelles d'Ω (`path`, `scanned_at`, l'ordre des identifiants) sont **hors du domaine identitaire** : le moteur peut les restituer aux consommateurs à des fins de présentation, mais aucune dérivation — signal, hypothèse, élection, niveau — ne peut en dépendre (A1, I8, P6).

---

## 4. Le déterminisme, exigence d'implémentation

> **EXG-18** — **Même Ω, même ℛ → même W, au bit près** sur toute représentation canonique de W. Le moteur définit une forme canonique de sa sortie (ordre normalisé, encodage fixé) ; deux exécutions quelconques — machines, dates, charges différentes — produisent des représentations canoniques identiques bit à bit (P2, I11, I24).
> **EXG-19** — Le résultat est **indépendant de l'ordre de parcours** de Ω : permuter l'ordre de lecture des actes, des tables ou des attributs ne change pas W (I31 ; les `observation_id` sont des identifiants, jamais des rangs porteurs de sens).
> **EXG-20** — Le résultat est **indépendant de tout parallélisme interne** : toute exécution concurrente doit être observationnellement équivalente à l'exécution séquentielle de référence (I31 — la confluence rend l'ordre indifférent ; l'implémentation doit préserver cette indifférence, pas s'en remettre à elle).
> **EXG-21** — **Localité** : l'ajout à Ω d'actes sans rapport avec un domaine ne change aucun acte de W sur ce domaine (P3) ; la révision d'une convention ne re-dérive que les actes dont Dep la contient (004 § 9). Un moteur dont une conclusion bouge sans cause tracée dans son index est non conforme.

---

## 5. L'auditabilité

> **EXG-22** — **Toute sortie doit pouvoir être reconstruite** : depuis (Ω, ℛ), le moteur régénère l'intégralité de W — signaux, hypothèses, préférences, actes — sans recours à aucun état antérieur (I5, I10).
> **EXG-23** — **Toute décision doit être retraçable** : pour tout acte (élection ou refus), le moteur restitue la chaîne complète Ω → signal → hypothèse → [contradiction → priorité →] élection → état, avec, à chaque maillon, les `observation_id` et attributs consommés, les conventions appliquées en version exacte, les concurrentes écartées et les motifs (I30, 008 § 5). Les chaînes interrompues (refus) sont restituables au même titre que les chaînes abouties.
> **EXG-24** — **Tout stockage de W est un cache** : invalidé par tout changement d'index, jamais consulté comme source de vérité, et vérifiable — un W stocké qui ne coïncide pas avec le W re-dérivé est invalide et doit être détecté comme tel (I5, I10 : « une hypothèse stockée qui ne serait plus re-dérivable à l'identique est invalide par définition »).
> **EXG-25** — Les états antérieurs restent dérivables sous leurs index (I23) : le moteur appliqué à un couple (Ω, ℛ) historique reproduit l'état historique — la reconstruction du passé est une invocation ordinaire, pas une fonctionnalité spéciale.

---

## 6. La validation

> **EXG-26** — **Le premier oracle de conformité est W₀** (009 § 5, § 10). Toute implémentation, appliquée au couple (Ω_corpus1, ℛ₀), doit produire exactement : **112 élections** de strate contenu (108 classes de 2 actes, 4 classes de 3 actes — vérifiées par oracle indépendant contre la base mesurée), chacune au niveau « certaine », licenciée par CE-01 v1 avec dépendance EQ-01 v1 ; **aucune autre élection** ; des refus motivés sur tout domaine-strate à espace non trivial, avec l'espèce exacte (normatif / structurel) et le motif exact de la carte du 009 § 6. **Sans liberté. Sans approximation. Sans réglage.**
> **EXG-27** — La validation de conformité comprend au minimum, au-delà de W₀ : le test de déterminisme (double exécution → identité bit à bit, EXG-18), le test de permutation (EXG-19), le test de localité (EXG-21), le test de registre incohérent (EXG-15 : registre privé d'EQ-01 → zéro élection partout), et le test de cache (EXG-24 : W stocké falsifié → détection).
> **EXG-28** — Tout écart entre la sortie d'une implémentation et l'oracle est **un défaut de l'implémentation** — jamais une occasion de réviser l'oracle. Si l'oracle lui-même est erroné, sa révision est un acte documentaire de la série théorique (I36), antérieur et étranger à toute correction de code.

---

## 7. L'évolution

> **EXG-29** — Les **seules causes autorisées de changement de sortie** sont : l'augmentation de Ω (actes d'observation nouveaux) et la transition de ℛ (adoption, révision, retrait — 007 § 10). **Rien d'autre** : ni mise à jour du moteur (une version nouvelle du moteur qui change W à index constant est non conforme — ou révèle que l'ancienne l'était), ni recompilation, ni environnement, ni temps qui passe (006 § 6 : sans changement d'index, W est immuable).
> **EXG-30** — Toute transition d'index produit une transition d'état tracée τ (006 § 7) : cause exacte, correspondance conservé / abandonné / nouveau, continuités déclarées. Le moteur doit pouvoir produire τ entre deux index quelconques dont il connaît les deux membres.

---

## 8. Le contrat logiciel minimal

Toute implémentation doit posséder les propriétés suivantes — énoncées comme exigences, sans préjuger d'aucun moyen :

> **EXG-31 — Pureté** : la sortie est fonction des seules entrées (EXG-01–03).
> **EXG-32 — Absence d'effet de bord** : l'invocation du moteur ne modifie rien d'observable hors de la production de W et de ses chaînes — ni Ω, ni ℛ, ni le système hôte.
> **EXG-33 — Lecture seule** : l'accès à Ω et à ℛ est exclusivement en lecture, vérifiable (le moteur doit pouvoir fonctionner sous un accès matériellement non inscriptible).
> **EXG-34 — Rejouabilité** : toute exécution passée est reproductible à l'identique depuis son index (EXG-25).
> **EXG-35 — Reproductibilité** : indépendance à la machine, à l'instant, à l'ordre, au parallélisme (EXG-18–20).
> **EXG-36 — Auditabilité** : chaînes restituables intégralement (EXG-22–23).
> **EXG-37 — Testabilité** : la conformité est vérifiable par les oracles du § 6 sans instrumenter le moteur — les exigences portent sur les sorties observables, et les sorties observables suffisent à les vérifier.
> **EXG-38 — Modularité de validation** : les productions intermédiaires prescrites par la théorie (signaux d'un acte, chaîne d'un acte, Dep d'une élection) sont restituables **à la demande, unité par unité** — sans exiger l'énumération exhaustive d'espaces combinatoires (le moteur justifie tout acte ; il n'a pas à matérialiser tout l'espace des hypothèses).

---

## 9. Ce qui est volontairement laissé libre

La théorie impose le résultat. **Jamais la manière.** Sont explicitement hors du contrat — libres, pourvu que toutes les EXG tiennent :

- le **langage** d'implémentation, les bibliothèques, l'outillage ;
- l'**architecture interne** (couches, modules, processus) — la chaîne théorique n'impose pas une chaîne logicielle isomorphe ;
- les **structures de mémoire** et représentations internes ;
- les **algorithmes** — tout algorithme produisant exactement W est conforme, quelle que soit sa stratégie ;
- l'**ordre interne de calcul** — rendu indifférent par la confluence (I31), donc libre ;
- les **optimisations** et le **parallélisme** (sous EXG-20) ;
- les **caches** à tous les étages (sous EXG-24 : invalidables, vérifiables, jamais sources de vérité) ;
- le **calcul incrémental** — admissible si et seulement si son résultat est observationnellement identique à la re-dérivation complète (EXG-18 en fait foi) ;
- les **index**, la **base de données** ou tout autre support de lecture d'Ω (sous EXG-14) ;
- l'**API**, les formats d'exposition de W et des chaînes aux consommateurs (sous EXG-16–17) ;
- la **représentation matérielle de ℛ** (fichiers, formats — sous EXG-15 et la forme normative du 004/007) ;
- les performances — la théorie n'impose aucun délai ; le projet, lui, en imposera par mesure (CLAUDE.md § 10 : aucune optimisation sans benchmark), mais c'est un contrat distinct.

---

## 10. L'oracle officiel de conformité

> **EXG-39** — Constituent ensemble, à compter de ce document, **l'oracle officiel de conformité** de tout moteur d'identité :
>
> 1. **les documents 000→009** (le référentiel théorique, invariants I1–I36) ;
> 2. **le registre ℛ** dans son état courant identifié — aujourd'hui ℛ₀ (EQ-01 v1, CE-01 v1) ;
> 3. **le corpus 1** — la base d'observations mesurée, gelée et archivée de la campagne (497 actes, 381 classes de contenu), avec ses annexes versionnées (`docs/mesures/`) ;
> 4. **W₀** — l'état du monde prudent de (Ω_corpus1, ℛ₀), caractérisé au 009 § 5 et vérifié par oracle indépendant.
>
> **Aucune implémentation future ne pourra être considérée correcte si elle produit un autre résultat.** L'utilité, l'élégance ou la performance d'un moteur ne compensent jamais un écart à l'oracle : un moteur rapide et faux est faux.

---

## Récapitulatif des exigences

| Bloc | Exigences | Objet |
|---|---|---|
| Fonction | EXG-01–03 | (Ω, ℛ) → W ; aucune entrée cachée ; aucun état interne |
| Responsabilités | EXG-04–13 | ce que le moteur fait ; les huit interdictions |
| Frontières | EXG-14–17 | contrats publics seulement ; consommateurs sans influence ; contexte hors domaine |
| Déterminisme | EXG-18–21 | identité bit à bit ; indépendance à l'ordre et au parallélisme ; localité |
| Auditabilité | EXG-22–25 | reconstruction totale ; chaînes restituables ; W = cache ; passé re-dérivable |
| Validation | EXG-26–28 | W₀ exact (112 élections) ; batterie minimale ; l'oracle ne se négocie pas |
| Évolution | EXG-29–30 | deux causes de changement ; transitions tracées |
| Contrat logiciel | EXG-31–38 | pureté, lecture seule, rejouabilité, reproductibilité, auditabilité, testabilité, modularité de validation |
| Liberté | § 9 | tout le reste — le résultat est imposé, jamais la manière |
| Oracle | EXG-39 | 000→009 + ℛ + corpus 1 + W₀ |

**Ce que ce document ne fait volontairement pas** : proposer une architecture, choisir une technologie, décrire un algorithme, définir une API, parler d'un moteur particulier.
