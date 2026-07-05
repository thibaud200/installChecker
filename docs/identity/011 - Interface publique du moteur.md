# 011 — Interface publique et contrat logiciel du moteur

**Statut** : deuxième document de conception de la série `docs/identity/`. S'appuie sur les documents 000→010 ; les invariants I1–I36 et les exigences EXG-01→39 sont normatifs.
**Nature** : le contrat logiciel du moteur — **le moteur reste une boîte noire** : ce document ne décrit que ce qui entre, ce qui sort, et ce qui est garanti. Aucun code, aucune implémentation, aucun algorithme, aucune structure mémoire, aucune technologie imposée, aucune représentation choisie.
**Portée remarquable** : rien dans ce contrat ne mentionne les installateurs. Le moteur contractualisé ici fonctionne pour **tout producteur d'observations conforme** — le domaine d'application est un paramètre du système, pas du contrat.

---

## 1. Vue générale

Le moteur est un **composant pur** : une fonction sans état, sans mémoire, sans horloge, sans environnement (EXG-01–03), entourée de trois acteurs qu'il ne contrôle pas :

| Acteur | Rôle | Relation au moteur |
|---|---|---|
| **Producteur d'observations** | produit et fait croître Ω (aujourd'hui : le pipeline figé d'InstallChecker ; demain : tout pipeline conforme) | fournit une entrée ; ignore le moteur |
| **Registre** | porte ℛ, gouverné par une autorité extérieure (adoptions, révisions, retraits — 007 § 9) | fournit une entrée ; ignore le moteur |
| **Moteur** | dérive W depuis (Ω, ℛ) et restitue les justifications | boîte noire pure |
| **Consommateurs** | lisent W, τ et les chaînes d'audit | lisent des sorties ; n'influencent jamais rien (EXG-16) |

Les quatre rôles sont **disjoints par contrat** : aucun acteur n'en cumule deux vis-à-vis du même flux (§ 11).

---

## 2. Les entrées

### 2.1 Ω — la base d'observations

- **Nature** : un ensemble d'actes d'observation conforme au contrat public du producteur — actes identifiés, une ligne par capacité (invariant 1:1), valeurs brutes, ⊥ pour l'absence, version de contrat déclarée. Le moteur consomme ce contrat, jamais sa réalisation (EXG-14 : ni extracteurs, ni technologie de stockage, ni formats observés).
- **Cycle de vie** : Ω croît par actes d'observation successifs, en dehors du moteur. **Chaque invocation du moteur porte sur un état identifié de Ω** — une valeur, pas un flux : l'état est figé à l'invocation, et son identification fait partie de l'index de toute sortie.
- **Immutabilité** : vis-à-vis du moteur, Ω est immuable en droit (lecture seule, EXG-33) et en fait (les observations ne se corrigent jamais, I1) ; deux invocations sur le même état identifié voient exactement les mêmes actes.

### 2.2 ℛ — le registre

- **Nature** : un état identifié et versionné du registre des conventions — chaque convention sous sa forme complète (004, Déf. 1 ; 007 § 5), avec les incompatibilités déclarées et l'historique des transitions.
- **Cycle de vie** : le registre évolue par actes de gouvernance tracés (adoption, révision, retrait, remplacement, scission, fusion — 007 § 10), tous extérieurs au moteur (EXG-08).
- **Immutabilité** : chaque état de ℛ est immuable ; une transition produit un état nouveau, l'ancien restant référençable à jamais (004 § 2).

Il n'existe **aucune troisième entrée** (EXG-02). Toute information prétendant influencer W doit devenir un constituant de Ω (par observation) ou de ℛ (par adoption) — ou renoncer à influencer.

---

## 3. Les sorties

- **W — l'état du monde** : l'état prudent de l'index (Ω, ℛ) — les élections (hypothèse, strate, niveau, motif, licences, dette) et les refus motivés (espèce structurelle ou normative, motif exact), couvrant tout domaine-strate à espace d'hypothèses non trivial (006 § 2–4 ; 009 § 5). W porte son index : un W sans index n'est pas une sortie valide.
- **τ — la transition** : pour deux index dont le moteur reçoit les deux membres, la correspondance exhaustive conservé / abandonné / nouveau, avec la cause exacte et les continuités déclarées (006 § 7, EXG-30).
- **Les chaînes d'audit** : pour tout acte de W, la restitution à la demande de sa chaîne complète — observations élémentaires, conventions versionnées, hypothèses écartées, contradictions et leur sort, dette (008 § 5, EXG-23). Les chaînes des refus sont restituables au même titre.

**Aucune représentation n'est choisie ici** : formats, encodages et supports sont libres (010 § 9), sous une seule contrainte — chaque sortie possède une **forme canonique** définie par l'implémentation, sur laquelle porte l'identité bit à bit (EXG-18).

---

## 4. Le contrat d'exécution

**Préconditions** — à l'invocation :

- Ω est présent, lisible, et conforme au contrat d'observations dans une version que le moteur déclare supporter ;
- ℛ est présent, bien formé (chaque entrée sous la forme normative complète), et **cohérent** au sens du prédicat du 008 § 4.

**Postconditions** — au retour :

- W est **complet** (tout domaine-strate non trivial porte un acte), **cohérent** (006, Déf. 3), **canonique**, et **intégralement justifié** (chaque acte porte ses licences, motifs, provenances — I38) ;
- W est **entier ou absent** : le moteur ne produit jamais d'état partiel. Une invocation échoue ou livre W complet — un demi-état du monde n'existe pas.

**Garanties** :

- déterminisme total (EXG-18–20), localité (EXG-21), lecture seule et absence d'effet de bord (EXG-32–33), auditabilité (EXG-22–25) ;
- **aucune surprise métier** : aucune valeur d'aucune observation ne peut provoquer un comportement non contractuel — tout contenu observé est une donnée légitime (§ 5).

**Échecs possibles** : uniquement ceux du contrat d'erreur (§ 5). Tout autre échec (épuisement de ressources, défaut interne) est une défaillance de l'implémentation — elle doit se signaler comme telle et respecter la postcondition « entier ou absent ».

---

## 5. Le contrat d'erreur

Les **seules** erreurs contractuelles, toutes situées à la frontière des entrées :

| Erreur | Condition |
|---|---|
| **registre absent** | ℛ non fourni ou non identifiable |
| **registre malformé** | une entrée de ℛ ne satisfait pas la forme normative (champ manquant, version non identifiée, dépendance non déclarée) |
| **registre incohérent** | le prédicat du 008 § 4 échoue (incompatibilité en vigueur, dépendance insatisfaite, cycle, non-confluence déclarée non levée) |
| **Ω absent** | base non fournie ou illisible |
| **Ω invalide** | violation du contrat d'observations (invariant 1:1 rompu, acte sans identifiant, structure inattendue) |
| **Ω incompatible** | version du contrat d'observations non supportée par le moteur |

Précisions normatives :

- **le registre incohérent est promu au rang d'échec signalé** — raffinement assumé d'EXG-15 (motivé en fin de document) : un défaut de gouvernance ne doit jamais se présenter aux consommateurs comme une connaissance du monde (« que des refus ») ; il se signale comme ce qu'il est, et se corrige dans ℛ ;
- **aucune erreur métier n'existe** : un type de fichier inconnu, une valeur aberrante, une chaîne hostile, un contenu gigantesque ne sont **jamais** des erreurs — ce sont des observations, et toute observation est une entrée légitime de la dérivation. Un moteur qui échoue sur une *valeur* est non conforme ;
- toute erreur est **explicite, nommée et diagnosticable** : elle identifie l'entrée fautive et la clause violée — jamais un échec silencieux, jamais un résultat dégradé non signalé.

---

## 6. Le contrat de reproductibilité

> **(Ω, ℛ) → W est une fonction pure.**

- **Identité** : deux invocations sur le même index produisent des sorties dont les formes canoniques sont identiques bit à bit — quelles que soient machine, date, charge, version du système hôte (EXG-18) ;
- **Transparence référentielle** : toute occurrence de « le W de (Ω, ℛ) » est substituable par sa valeur ; aucun contexte d'invocation ne peut distinguer un W recalculé d'un W restitué d'un cache valide (EXG-24) ;
- **Idempotence** : invoquer n fois équivaut à invoquer une fois ;
- **Indifférence à l'ordre interne** : parcours, parallélisme et stratégies internes sont inobservables dans la sortie (EXG-19–20, I31) ;
- **Rejouabilité historique** : appliqué à un index passé, le moteur reproduit l'état passé — la reconstruction du passé est une invocation ordinaire (EXG-25).

---

## 7. Le contrat d'audit

Ce qu'un consommateur peut demander — et que toute implémentation doit savoir restituer, **à la demande, unité par unité** (EXG-38) :

| Question | Réponse contractuelle |
|---|---|
| *pourquoi cette élection ?* | la chaîne complète (008 § 5) : observations → signaux → hypothèse → [contradiction → priorité →] élection, avec licences et motif |
| *pourquoi ce refus ?* | la chaîne interrompue : le maillon manquant, l'espèce (structurel / normatif), le motif exact |
| *de quelles conventions dépend cet acte ?* | Dep complet, versions comprises, dette d'arbitrage identifiée en son sein (004 § 10) |
| *de quelles observations dépend-il ?* | Obs complet : les identifiants d'actes et attributs consommés (I4, I7) |
| *qu'a-t-on écarté ?* | les hypothèses concurrentes écartées et le motif de chaque écartement (P5) |
| *que faudrait-il renier pour que ceci tombe ?* | les ensembles minimaux de conventions de l'acte (008 § 8) et sa dette (006 § 9) |
| *qu'est-ce qui a changé entre deux états ?* | τ : cause, correspondance, continuités (006 § 7) |

Toute réponse d'audit est elle-même **déterministe, canonique et dérivée du seul index** — l'audit est une projection de la dérivation, jamais une source d'information nouvelle. Les questions hypothétiques (« que se passerait-il si… ») n'ont pas d'API dédiée : elles se posent en invoquant le moteur sur l'index hypothétique — une invocation ordinaire.

---

## 8. Le contrat de validation

Un moteur est **déclaré conforme** lorsque, et seulement lorsque :

1. **il retrouve W₀** : appliqué à (Ω_corpus1, ℛ₀), il produit exactement les 112 élections de contenu (108 paires, 4 triplets), au niveau « certaine », licenciées CE-01 v1 / EQ-01 v1, et les refus motivés de la carte du 009 § 6 — sans liberté, sans approximation, sans réglage (EXG-26) ;
2. **il passe la batterie minimale** (EXG-27) : déterminisme (double exécution bit à bit), permutation d'ordre, localité, registre amputé d'EQ-01 → erreur « registre incohérent » (§ 5), cache falsifié → détection ;
3. **il honore le contrat d'audit** (§ 7) sur chaque acte de W₀ ;
4. **il signale correctement chaque erreur** du § 5 sur des entrées construites pour les provoquer.

La conformité est déclarée **par version de moteur** : toute modification du moteur invalide la déclaration et exige la revalidation complète (EXG-29 : une version nouvelle qui change W à index constant est non conforme — ou prouve que l'ancienne l'était). Tout écart à l'oracle est un défaut du moteur, jamais de l'oracle (EXG-28).

---

## 9. La compatibilité

- **Nouvelle version du moteur** : doit produire des sorties identiques bit à bit sur **tous les index passés** — sauf correction documentée d'une non-conformité antérieure, auquel cas l'écart est publié, justifié acte par acte, et l'ancienne version est déclarée non conforme rétroactivement ;
- **Nouvelle version du contrat de Ω** (évolution du producteur — capacités nouvelles, schéma étendu) : le moteur déclare les versions qu'il supporte ; une version inconnue produit l'erreur « Ω incompatible », jamais une lecture partielle silencieuse. Des capacités nouvelles dans une version supportée sont des observations ordinaires (l'extension de 𝒜 est prévue par la théorie, 001 § 1.1) ;
- **Nouvelle version de ℛ** : une transition ordinaire — aucune compatibilité à négocier : le moteur applique l'état fourni, quel qu'il soit, pourvu qu'il soit cohérent ;
- **Ancienne sortie W** : reste valide **sous son index, à jamais** (I23) ; un W conservé n'est interprétable qu'accompagné de son index ; comparer deux W d'index différents ne se fait que par τ — toute comparaison directe par un consommateur est un contresens dont le contrat ne répond pas.

---

## 10. L'extension

Deux voies, aux statuts irréductiblement distincts :

- **Ajouter une convention dans une famille connue** (une interprétation, une équivalence, une entrée de catalogue, une licence…) : une **transition de ℛ** — c'est-à-dire une donnée nouvelle, pas un moteur nouveau. Le contrat public est intact ; les sorties changent par la seule voie autorisée (EXG-29). C'est le régime de croissance normal du système : **le moteur n'a pas besoin de changer pour que le système apprenne** ;
- **Introduire une famille de conventions nouvelle** (une forme de transformation que la théorie ne définit pas) : une **révision documentaire de la théorie d'abord** (I36 — l'instanciation ne modifie jamais la théorie, donc la théorie se révise en propre), puis une évolution du moteur, revalidée intégralement (§ 8–9). Le contrat public de ce document n'est étendu que par ce chemin.

Dans les deux cas, rien de ce que les consommateurs utilisent — la forme des questions du § 7, la sémantique de W et de τ — ne se rétracte : les extensions ajoutent, elles ne retirent pas.

---

## 11. Principes d'architecture

Sans architecture détaillée — les seules contraintes de structure opposables :

- **séparation des responsabilités** : production d'Ω, gouvernance de ℛ, dérivation de W, consommation — quatre rôles, jamais cumulés dans un même composant vis-à-vis du même flux. En particulier : le moteur ne scanne pas, le pipeline ne dérive pas, le registre ne calcule pas, les consommateurs n'écrivent rien ;
- **lecture seule** : le moteur accède à ses deux entrées sous un régime vérifiable de non-écriture (EXG-33) ;
- **composition** : le moteur peut être composé de parties internes — le contrat s'applique à la frontière, et **rien de la composition interne ne fuit** (I37) : aucune partie n'est adressable de l'extérieur, aucun état intermédiaire n'est exposé autrement que par les chaînes d'audit ;
- **dépendances autorisées** : moteur → contrat public de Ω ; moteur → contrat public de ℛ ; consommateurs → contrat public du moteur (et, pour leurs propres besoins de présentation, contrat public de Ω) ;
- **dépendances interdites** : moteur → extracteurs, technologie de stockage, formats de fichiers (EXG-14) ; moteur → consommateurs (EXG-16) ; quiconque → intérieurs du moteur (I40) ; moteur → tout ce qui n'est pas ses deux entrées (EXG-02).

---

## 12. Invariants

> **I37 — Le moteur n'expose jamais d'état interne.** Ses seules manifestations observables sont ses sorties contractuelles (W, τ, chaînes, erreurs nommées). Tout ce qu'un consommateur peut apprendre du moteur passe par le contrat ; ce qui ne passe pas par le contrat n'existe pas pour lui.

> **I38 — Toute sortie possède une justification complète.** Il n'existe pas d'acte non justifié dans W, pas de correspondance non causée dans τ, pas de réponse d'audit sans provenance. Une sortie sans justification est non conforme par définition — quelle que soit son exactitude.

> **I39 — Toute justification est reconstructible.** Les justifications ne sont pas des annotations stockées faisant foi : elles se re-dérivent de l'index, à l'identique, à tout moment (I5, I10, I23 portés au contrat). Une justification restituée qui ne coïncide pas avec sa re-dérivation est invalide.

> **I40 — Le contrat public est indépendant de l'implémentation.** Aucune clause du présent document ne nomme, ne suppose ni ne privilégie une technologie, une architecture ou un algorithme ; réciproquement, aucun choix d'implémentation ne peut restreindre, étendre ou réinterpréter le contrat. Le contrat survit à toutes les implémentations qui l'honorent.

---

## Conclusion

La théorie est désormais **totalement transformée en contrat logiciel** : deux entrées immuables et identifiées, trois sorties justifiées et canoniques, six erreurs nommées et aucune erreur métier, une fonction pure, un contrat d'audit en sept questions, une procédure de conformité adossée à W₀, des règles de compatibilité et d'extension qui protègent les fondements. Le moteur peut maintenant être conçu de l'intérieur sans qu'aucun choix interne ne puisse remettre en cause ce qui précède : **le document 012 pourra commencer la conception interne du moteur.**

---

## Récapitulatif

| Contrat | Contenu | § |
|---|---|---|
| acteurs | producteur d'Ω, registre, moteur (boîte noire pure), consommateurs — quatre rôles disjoints | 1 |
| entrées | Ω (état identifié, immuable, contrat public) et ℛ (état versionné, cohérent) — aucune troisième | 2 |
| sorties | W (complet, cohérent, canonique, indexé), τ, chaînes d'audit à la demande — représentation libre | 3 |
| exécution | préconditions, postconditions (« entier ou absent »), garanties, échecs bornés | 4 |
| erreurs | six erreurs de frontière ; aucune erreur métier ; toujours explicites | 5 |
| reproductibilité | fonction pure : identité bit à bit, transparence référentielle, idempotence, rejouabilité | 6 |
| audit | sept questions contractuelles, réponses déterministes dérivées du seul index | 7 |
| validation | W₀ + batterie + audit + erreurs ; conformité par version de moteur | 8 |
| compatibilité | moteur nouveau ⟹ sorties identiques sur index passés ; W ancien valide sous son index à jamais | 9 |
| extension | convention nouvelle = donnée (ℛ) ; famille nouvelle = théorie d'abord, moteur ensuite | 10 |
| architecture | séparation, lecture seule, composition sans fuite, dépendances autorisées/interdites | 11 |
| invariants | I37 aucun état interne exposé, I38 justification complète, I39 justification reconstructible, I40 contrat indépendant de l'implémentation | 12 |

**Ce que ce document ne fait volontairement pas** : choisir une représentation, une technologie, une architecture interne ; définir la forme canonique concrète de W (conception interne, 012) ; matérialiser ℛ ; écrire la moindre ligne de code.
