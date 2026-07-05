# 014 — Contrats internes et modèles logiques du moteur

**Statut** : cinquième document de conception de la série `docs/identity/`. S'appuie sur les documents 000→013, figés. **Dernière étape avant l'écriture du premier code** : après ce document, toute implémentation doit être quasiment mécanique.
**Nature** : les objets logiques, leurs contrats, leurs invariants, leurs relations — jamais une classe, une interface, une méthode, un algorithme ou une structure mémoire. Le lecteur doit pouvoir traduire ces contrats dans n'importe quel langage (I49).

---

## 1. Les contrats C1 → C7

Forme commune : chaque contrat est **total** — toute entrée valide produit une sortie valide ; toute entrée invalide produit une erreur nommée du contrat public (011 § 5). Aucun troisième cas.

### C1 — Projection de Ω

| Clause | Contenu |
|---|---|
| **reçoit** | l'identification d'un support d'observations satisfaisant le contrat logique de Ω (§ 6) |
| **produit** | le *modèle d'observations* : l'ensemble des actes identifiés, avec leurs attributs de contenu — plus, sur un canal séparé destiné exclusivement à C7, les attributs de contexte |
| **garantit** | totalité (chaque acte du support est projeté), fidélité (aucune valeur transformée — ⊥ reste ⊥), filtrage (aucun attribut de contexte ne sort sur le canal de dérivation), déterminisme (même support → même modèle) |
| **refuse** | support absent ou illisible → « Ω absent » ; violation du contrat § 6 (acte sans identifiant, invariant 1:1 rompu) → « Ω invalide » ; version de contrat non supportée → « Ω incompatible » |
| **ignore** | le sens des valeurs (aucune interprétation), l'ordre physique du support, tout ce qui n'est pas décrit au § 6 |

### C2 — Projection de ℛ

| Clause | Contenu |
|---|---|
| **reçoit** | l'identification d'un répertoire de registre (013 § 3) |
| **produit** | le *référentiel de conventions* : les versions en vigueur (objets Convention complets), les arcs de dépendance, les incompatibilités déclarées, l'identité de l'état de ℛ |
| **garantit** | que tout référentiel produit satisfait le prédicat de cohérence (008 § 4) — rien ne franchit C2 sans lui ; fidélité au contenu logique (§ 5) ; déterminisme |
| **refuse** | répertoire absent → « registre absent » ; champ obligatoire manquant, version dupliquée, état citant une version sans fichier → « registre malformé » ; échec du prédicat (dépendance insatisfaite, cycle, incompatibilité en vigueur) → « registre incohérent » |
| **ignore** | la prose des justifications (lue par les humains, pas par le moteur), les conventions candidates non citées par l'état |

### C3 — Dérivation des signaux

| Clause | Contenu |
|---|---|
| **reçoit** | le modèle d'observations (de C1) + les conventions des familles interprétation, équivalence, attente, catalogue d'artefacts (de C2) |
| **produit** | l'*étage des signaux* : les instances de signaux définies, chacune avec type, sortie, régime (R1–R5) et provenance complète |
| **garantit** | qu'aucune instance n'existe sans convention la fondant (I13) ; que les préconditions non satisfaites produisent l'inexistence du signal, jamais une valeur ; que le régime est porté, jamais jugé ; provenance intégrale (I30) |
| **refuse** | rien — C3 n'a pas d'erreur propre : ses entrées sont valides par construction (I51) et toute configuration d'observations est légitime (011 § 5 : aucune erreur métier) |
| **ignore** | les strates, les hypothèses, l'existence des couches supérieures |

### C4 — Construction des hypothèses

| Clause | Contenu |
|---|---|
| **reçoit** | l'étage des signaux (de C3) + les conventions de stratification et de composition (de C2) + une *demande* (un domaine et une strate) |
| **produit** | pour la demande : les hypothèses pertinentes (quintuplets complets), leurs résidus, les relations de préférence entre elles (domination, incomparabilité), les contradictions rencontrées avec leur sort |
| **garantit** | fourniture **à la demande** (EXG-38 — jamais d'énumération exhaustive imposée) ; les hypothèses extrêmes toujours disponibles (005 § 11) ; comparaisons par inclusion uniquement (I9 : aucun comptage, aucun poids) ; provenance et just complets |
| **refuse** | rien (mêmes raisons que C3) |
| **ignore** | les licences d'élection, les niveaux de certitude, l'état du monde |

### C5 — Décision des actes

| Clause | Contenu |
|---|---|
| **reçoit** | les productions de C4 + les conventions d'élection et de priorité (de C2) |
| **produit** | l'*ensemble des actes* : pour chaque domaine-strate à espace non trivial, exactement un acte — élection (avec niveau, motif, licences, dépendances, dette) ou refus (avec espèce, motif) — et sa chaîne de résolution |
| **garantit** | I27 (toute élection cite une licence en vigueur) ; P7 (l'absence de licence ou la non-forçure produit un refus, jamais une conjecture) ; le motif canonique = **le premier maillon manquant dans l'ordre de la chaîne** (§ 8) ; complétude (aucun domaine-strate non trivial sans acte) |
| **refuse** | rien |
| **ignore** | la forme canonique, la sérialisation, les transitions |

### C6 — Assemblage de l'état

| Clause | Contenu |
|---|---|
| **reçoit** | l'ensemble des actes (de C5) + l'index (identité de l'état d'Ω, identité de l'état de ℛ) |
| **produit** | **W** sous forme canonique (§ 7) ; sur demande avec deux index et leurs deux ensembles d'actes : **τ** |
| **garantit** | la vérification de cohérence d'état (006 § 3) avant livraison ; « entier ou absent » (011 § 4) ; l'agrégation canonique des refus (§ 7.3) ; l'ordre canonique |
| **refuse** | un ensemble d'actes incohérent — cas qui signale un défaut de C5, jamais une situation d'entrée : c'est une défaillance interne, pas une erreur du contrat public |
| **ignore** | la manière dont les actes ont été dérivés |

### C7 — Restitution d'audit

| Clause | Contenu |
|---|---|
| **reçoit** | une question d'audit (l'une des sept, 011 § 7) portant sur un acte désigné d'un W désigné — plus, en lecture, les objets produits par C1–C6 et le canal de contexte de C1 |
| **produit** | l'objet de réponse correspondant (§ 9), unité par unité |
| **garantit** | que chaque réponse coïncide avec sa re-dérivation (I39) ; lecture seule intégrale ; le contexte n'apparaît que dans les restitutions qui le demandent explicitement, jamais dans une justification |
| **refuse** | une question portant sur un acte inexistant dans le W désigné — seul cas, signalé nommément |
| **ignore** | tout ce qui ne lui est pas demandé |

---

## 2. Les objets logiques

Pour chaque objet : **identité** (ce qui fait que deux exemplaires sont le même), **contenu** (les constituants obligatoires), **interdits**, **invariants**.

| Objet | Identité | Contenu obligatoire | Interdits | Invariants clés |
|---|---|---|---|---|
| **Observation** (élémentaire) | (acte, attribut) | acte propriétaire, attribut, valeur ∈ Val(a) ∪ {⊥} | toute interprétation ; toute pondération | immuable (I1) ; vraie comme rapport (001 § 1.3) |
| **Acte d'observation** | son identifiant (stable, fourni par Ω) | identifiant + la famille complète de ses observations (1:1) + ses attributs de contenu (taille, empreinte) | contexte sur le canal de dérivation (A1) | complet par construction (invariant 1:1) |
| **Signal** (type) | l'identifiant du type | domaine d'entrée, préconditions, fonction, référence de convention (κ, ver) | existence sans convention (I13) | déterministe (I6) |
| **Instance de signal** | (type, observations consommées) | sortie, régime, provenance (actes + attributs), (κ, ver) | poids, score (I9) ; persistance par nécessité (I5) | reconstructible ; régime porté non jugé |
| **Régime** | valeur d'énumération | l'un de : exact, incomplet, ambigu, contradictoire, artefactuel (002 § 5) | tout ordre numérique entre régimes | catégoriel |
| **Hypothèse** | (contenu propositionnel, domaine, strate) | Dom, Obs, Sig (avec régimes), prov, just (003, Déf. 1) | certitude intrinsèque (003 § 1.2) ; « force » | quintuplet complet ; jamais détruite (I19) |
| **Résidu** | l'ensemble lui-même | éléments d'inexpliqué typés (coïncidence, contradiction assumée, artefact supposé) | tout cardinal comparatif | comparé par inclusion seulement (003 § 1.3) |
| **Dépendance** (Dep) | l'ensemble des (κ, ver) | couples convention-version, avec la dette d'arbitrage identifiée en sous-ensemble | annotation manuelle (004 § 10 : Dep se calcule) | reconstructible depuis just |
| **Acte d'élection** | (domaine, strate) dans un W | type=élection, hypothèse retenue (contenu propositionnel), niveau, motif (code), licences, Dep, dette | tout champ de contexte ou de calcul | I21 (reste une hypothèse) ; I27 (licences non vides) |
| **Refus** | (domaine, strate) dans un W | type=refus, espèce (structurel/normatif), motif (code) | dette (006 § 9 : un refus ne doit rien) ; niveau | résultat positif de plein droit (006 § 4) |
| **Transition** (τ) | (index avant, index après) | cause (type + détail), conservé / abandonné / nouveau (références d'actes), continuités | tout élément non re-dérivable des deux index | exhaustive (006 § 7) |
| **État du monde** (W) | son index | index (Ω, ℛ) + la totalité des actes | métadonnées de calcul ; contexte ; chaînes (§ 7.1) | complet, cohérent, canonique (012 § 5) |
| **Convention** | (identifiant, version) | les champs du § 5.1 | score, poids, code exécutable (004 § 12) | version immuable (013 § 3.1) |
| **Maillon de chaîne** | (chaîne, rang) | couche productrice, objets consommés (références), (κ, ver) appliqués, objet produit ou manque nommé, statut | commentaire libre (012 § 6) | identifiable et re-dérivable isolément |

---

## 3. Les frontières : ce qui traverse

| Frontière | Objets qui traversent | Justification |
|---|---|---|
| **C1 → C3** | le modèle d'observations (actes complets) | C3 est le seul consommateur de dérivation des observations (012 § 3) |
| **C1 → C7** | les attributs de contexte, par canal séparé | restitution uniquement — jamais la dérivation (A1, EXG-17) |
| **C2 → C3** | les conventions des familles interprétation, équivalence, attente, catalogue | les familles que C3 applique (012 § 1.2) |
| **C2 → C4** | les conventions de stratification et de composition | idem pour C4 |
| **C2 → C5** | les conventions d'élection et de priorité | idem pour C5 |
| **C2 → C6** | l'identité de l'état de ℛ | l'index de W |
| **C3 → C4** | les instances de signaux (avec régimes et provenances) | les hypothèses se construisent sur des signaux, jamais sur des observations (001 § 5) |
| **C4 → C5** | les hypothèses, résidus, relations de préférence, contradictions | C5 décide sur ces objets, sans relire plus bas (012 § 3) |
| **C5 → C6** | les actes et leurs chaînes | C6 assemble, il ne dérive pas |
| **C6 → C7** | W et τ (comme objets de référence des questions) | C7 répond *sur* des actes de W |

**Règle de traversée** : aucun objet ne franchit une frontière qui n'est pas la sienne — un acte d'observation ne parvient jamais à C5, une convention ne parvient jamais à C6 (seule l'identité de l'état de ℛ y va). Toute traversée hors tableau est un défaut de conception (012 § 2 : le besoin de « voir plus bas » se corrige au contrat, jamais par un accès).

---

## 4. Immutabilité et statuts des objets

| Statut | Objets | Justification |
|---|---|---|
| **immuables** | tous — chaque objet du § 2 est une valeur : produit, jamais modifié | I41 ; Ω intangible (I1), versions de conventions immuables (013 § 3.1), tout le reste est re-dérivé, pas retouché |
| **reconstruisibles** | tout sauf Ω et ℛ : signaux, hypothèses, résidus, actes, chaînes, W, τ, réponses d'audit | I5, I10, I39 — la re-dérivation depuis l'index est la définition de leur validité |
| **calculables** | les mêmes : ils n'existent qu'en tant que fonctions de (Ω, ℛ) | I43 — chaque couche est une fonction ; la composition aussi |
| **persistables** | Ω et ℛ (sources, seules persistances **premières**) ; W, chaînes et réponses d'audit (persistance **de cache** uniquement) | I47 : toute représentation persistée produite par le moteur est dérivable ; EXG-24 : un cache se vérifie, ne se croit pas |

La chaîne de justification : I1 rend Ω immuable → I5/I10 rendent les dérivés reconstructibles → I41 interdit la mutation inter-couches → I43 rend tout calculable → I47/EXG-24 bornent la persistance. Chaque statut est un théorème, pas une préférence.

---

## 5. Les contrats de lecture du registre

Contenu **logique** des trois types de documents (la syntaxe Markdown exacte relève du 015).

### 5.1 Fichier de version d'une convention (`<ID>/v<n>.md`)

**Champs obligatoires** — tous, sans exception (I13 ; 004 Déf. 1 ; 007 § 5) :

| Champ | Contenu | Lu par le moteur ? |
|---|---|---|
| identifiant | l'identifiant pérenne, égal au nom du répertoire | oui |
| version | l'entier de version, égal au nom du fichier | oui |
| famille | l'une des familles connues : interprétation, équivalence, priorité, attente, catalogue, stratification, composition, élection | oui |
| domaine d'application | attributs / types de signaux / strates / contextes concernés | oui |
| transformation | l'énoncé normatif unique de T(κ) | oui (selon la famille) |
| dépendances | la liste des (identifiant, version) présupposés — éventuellement vide | oui |
| régimes admis | pour la famille élection : les régimes admissibles du soutien (007 § 7) | oui |
| portée | ce que la convention autorise au maximum (strate, plafond de niveau) | oui |
| justification | prose — pourquoi cette convention | non (humains) |
| justification empirique | prose + références aux mesures (I35) | non |
| limites | prose — les cas connus où elle égare | non |
| conditions de révision | prose | non |
| date, autorité | l'acte d'adoption de cette version | non (le journal fait foi) |

**Champs interdits** : tout score, poids, probabilité, seuil, priorité numérique (004 § 12) ; tout élément exécutable.

**Causes exactes de rejet par C2** (→ « registre malformé ») : champ obligatoire absent ou vide ; famille inconnue ; identifiant ou version incohérents avec le chemin ; version dupliquée ; dépendance citant un couple (id, version) inexistant dans le répertoire.

### 5.2 Journal (`historique.md`)

Append-only. **Chaque entrée** : date, type de transition (adoption / révision / retrait / remplacement / scission / fusion — 007 § 10), identifiant et version concernés, justification de l'acte, autorité. **Rejet** si : entrée sans l'un de ces éléments ; version adoptée dont le fichier n'existe pas ; retrait d'une version jamais adoptée.

### 5.3 État (`etat.md`)

**Contenu** : la liste des couples (identifiant, version) en vigueur ; les incompatibilités déclarées (couple de conventions, étendue, date). **Rejet** si : version citée sans fichier ; version citée jamais adoptée au journal ; version retirée au journal encore citée. **Cohérence** (→ « registre incohérent » si échec, après bonne formation) : le prédicat du 008 § 4 sur les versions en vigueur.

---

## 6. Le contrat logique de Ω

Ce que C1 exige — **tout support qui le satisfait est acceptable** ; aucun mot sur la persistance :

- **des actes** : un ensemble fini d'actes d'observation, chacun porteur d'un **identifiant unique, stable et totalement ordonné** (l'ordre ne porte aucune sémantique — il sert de clé canonique, 012 § 5) ;
- **des attributs** : pour chaque acte, une valeur par attribut de chaque capacité du répertoire déclaré par le support — la valeur étant un texte, un entier, ou ⊥ ; le répertoire des capacités est ouvert (001 § 1.1) ;
- **l'invariant 1:1** : chaque acte porte une valeur (éventuellement ⊥) pour **chaque** attribut du répertoire — aucun trou ;
- **les attributs de contenu** : chaque acte porte au moins une taille et une **empreinte de contenu** dont le support garantit la sémantique : deux actes de même empreinte ont des contenus parfaitement égaux (la relation qu'EQ-01 fonde) ;
- **les attributs de contexte** : identifiés comme tels par le support (localisation, datation de l'acte), livrables séparément ;
- **la provenance** : toute valeur est rattachée à son acte et à son attribut — rien d'anonyme ;
- **la version du contrat** : le support déclare la version de contrat qu'il honore ; C1 déclare celles qu'il supporte.

---

## 7. La forme canonique complète de W

### 7.1 Structure

Un document unique, trois sections dans cet ordre : **`index`**, **`actes`**, et rien d'autre. Les chaînes d'audit **ne figurent pas dans W** — elles sont restituables à la demande par C7 (décision : W reste l'état, l'audit reste une projection ; 011 § 3 les distingue, 012 § 5 réserve le contenu de W aux actes).

### 7.2 La section `index`

- **`omega`** : la version du contrat d'observations ; le nombre d'actes ; l'**empreinte d'état** — obtenue en appliquant la fonction d'empreinte du support (celle-là même qui produit les empreintes de contenu des actes) à la concaténation, dans l'ordre canonique des identifiants d'actes, des empreintes de contenu. Aucune fonction nouvelle n'est introduite : l'identité d'Ω est dérivée de ce que Ω contient déjà ;
- **`registre`** : la liste explicite, triée par identifiant, des couples (identifiant, version) en vigueur — l'état de ℛ est petit, il s'énumère (pas d'empreinte nécessaire).

### 7.3 La section `actes`

Un tableau, **trié** par : rang de strate (contenu < variante < version < identité < famille), puis type (élection < refus), puis plus petit identifiant d'acte du domaine. Chaque élément porte, dans cet ordre de champs :

| Champ | Élection | Refus |
|---|---|---|
| `type` | « élection » | « refus » |
| `strate` | la strate | la strate |
| `domaine` | la liste **explicitement énumérée et triée** des identifiants d'actes couverts (une seule représentation — jamais d'abréviation « tous ») | idem |
| `contenu` | le contenu propositionnel retenu — pour la strate contenu : l'empreinte partagée de la classe | — (absent) |
| `niveau` | l'un de : « certaine », « probable », « possible » (chaînes normalisées ; « impossible » ne s'élit pas) | — |
| `motif` | un **code de motif** du vocabulaire normalisé (§ 7.4) | un code de motif |
| `espèce` | — | « normatif » ou « structurel » |
| `licences` | la liste triée des (identifiant, version) cités (I27 : non vide) | — |
| `dependances` | Dep complet, trié | — (un refus ne doit rien) |
| `dette` | le sous-ensemble d'arbitrage de Dep, trié (éventuellement vide) | — |

**Règle d'agrégation canonique des refus** : les refus de même (strate, espèce, motif) portant sur des domaines qu'aucun acte de l'état ne distingue sont **fusionnés en un seul refus de domaine maximal**. C'est ce qui rend W fini et canonique là où « tout domaine-strate » serait combinatoire.

**Règle du motif canonique** : le motif d'un refus est **le premier maillon manquant dans l'ordre de la chaîne** (008 § 5) — pas le manque le plus profond ni le plus définitif. (Raffinement du 009, E3 : voir le rapport de livraison.)

### 7.4 Le vocabulaire des motifs

Un registre normalisé de codes, extensible par les documents futurs, initialisé avec : `unique-maximale` (élection : maximale unique licenciée) ; `aucune-convention-strate` (refus normatif : aucune convention en vigueur ne fonde de signal ni de licence à cette strate) ; `préalable-absent` (refus normatif : la strate exige des rétentions préalables inexistantes — la famille sans identités) ; plus, réservés pour les états futurs : `incomparables`, `silence`, `contradiction-ouverte`, `licenciable-non-licencié`, `conflit-déclaré`, `sous-détermination`. Tout code nouveau est introduit par un document, jamais par le moteur (I29 par analogie).

### 7.5 La représentation de τ

Quatre sections dans cet ordre : `index-avant`, `index-apres` (chacun au format § 7.2), `cause` (type — « omega » ou « registre » — et détail : identifiants d'actes ajoutés, ou transition de convention), `correspondance` (trois listes de références d'actes — conservés, abandonnés, nouveaux — chaque référence étant le couple (strate, plus petit identifiant du domaine) ; puis les continuités déclarées, comme couples de références).

---

## 8. La définition complète de W₀

L'oracle est fixé — **un test d'or peut être écrit avant le moteur**, depuis la base archivée `tests/oracle/corpus1-postA1.db` et le présent paragraphe :

- **index** : `omega` = contrat v1, **497 actes**, empreinte d'état calculée comme au § 7.2 ; `registre` = [(CE-01, 1), (EQ-01, 1)] ;
- **112 élections**, toutes de strate contenu, type élection, niveau « certaine », motif `unique-maximale`, licences [(CE-01, 1)], dépendances [(CE-01, 1), (EQ-01, 1)], dette vide — une par classe de contenu multi-actes : **108 domaines de 2 actes et 4 domaines de 3 actes** (dénombrement vérifié par oracle indépendant sur la base archivée), `contenu` = l'empreinte partagée de la classe ;
- **4 refus**, un par strate supérieure : (variante, normatif, `aucune-convention-strate`), (version, normatif, `aucune-convention-strate`), (identité, normatif, `aucune-convention-strate`), (famille, normatif, `préalable-absent`) — chacun de domaine maximal : les 497 actes, énumérés ;
- **total : 116 actes**, dans l'ordre canonique du § 7.3 (les 112 élections de contenu triées par plus petit identifiant de domaine, puis les 4 refus dans l'ordre des strates) ;
- les 269 classes singletons ne produisent **aucun acte** (009 § 5 : espace trivial) ;
- rien d'autre : pas de chaînes (§ 7.1), pas de τ (un seul index), pas de contexte, pas de métadonnées.

---

## 9. Les contrats d'audit

Les objets retournés par C7 — pour chacun : contenu, identité, invariants. Tous sont **entièrement reconstructibles** : chaque réponse est une fonction (question, acte, index) — c'est I39, et c'est testable en re-posant la question après recalcul complet.

| Question (011 § 7) | Objet retourné | Identité |
|---|---|---|
| pourquoi cette élection ? | la **chaîne** : suite de maillons (§ 2) jusqu'à l'acte | (acte, index) |
| pourquoi ce refus ? | la **chaîne interrompue** : maillons + le manque nommé + espèce | (acte, index) |
| de quelles conventions dépend ? | le **Dep trié**, dette identifiée | (acte, index) |
| de quelles observations dépend ? | l'**Obs trié** : couples (identifiant d'acte, attribut) | (acte, index) |
| qu'a-t-on écarté ? | la liste des **hypothèses écartées** (contenu propositionnel, motif d'écartement : dominée / incomparable-non-licenciée) | (acte, index) |
| que faudrait-il renier ? | les **ensembles minimaux** de conventions (008 § 8), triés | (acte, index) |
| qu'est-ce qui a changé ? | **τ** (§ 7.5) | (index, index′) |

Invariants communs : lecture seule (aucune question ne modifie rien) ; déterminisme (même question, même index → même réponse, canonique) ; complétude de provenance (I30) ; le contexte (`path`, `scanned_at`) n'apparaît que si la question le demande explicitement — aucune des sept ne le fait ; sa restitution est un service de présentation séparé, hors justification.

---

## 10. Le plan détaillé É1 → É7

| | **É1 — Registre matérialisé** |
|---|---|
| objectif | ℛ₀ existe dans le dépôt et est humainement vérifiable |
| composants | aucun code — `registre/` seul |
| nouveaux objets | EQ-01/v1, CE-01/v1, journal (2 adoptions), état |
| tests / oracle | relecture contre § 5 (chaque champ obligatoire présent) et contre le 009 §§ 3–4 |
| validation | tous les champs du § 5.1 présents ; dépendance CE-01 → EQ-01 déclarée ; état cohérent à la main |
| livrable | un commit de gouvernance (013 § 3.3) |

| | **É2 — Squelette et modèles** |
|---|---|
| objectif | la solution accueille le moteur ; les objets logiques existent |
| composants | `Identity`, `Identity.Access`, `Identity.Tests` (créés, câblés) |
| nouveaux objets | les modèles du § 2 (observations, conventions, actes, W) et les deux ports |
| tests / oracle | build 0 avertissement ; tests d'égalité/identité des modèles (les identités du § 2) |
| validation | `Identity` sans référence de paquet (vérifié dans le projet — EXG-14) |
| livrable | commit ; les suites existantes du pipeline restent vertes et intactes |

| | **É3 — C2 : lecture du registre** |
|---|---|
| objectif | ℛ₀ est projeté ; tout registre invalide est rejeté nommément |
| composants | `Identity.Access` (lecteur), `Identity` (référentiel, prédicat de cohérence) |
| nouveaux objets | le référentiel de conventions ; les trois erreurs registre |
| tests / oracle | ℛ₀ accepté (référentiel = 2 conventions, 1 arc) ; collection de registres cassés versionnée dans les tests → chaque cause de rejet du § 5 produit son erreur |
| validation | chaque cause de rejet couverte par au moins un cas ; aucune élection possible sous registre incohérent n'est encore testable (pas de moteur) — différé à É7 |
| livrable | commit |

| | **É4 — C1 : lecture de Ω** |
|---|---|
| objectif | la base archivée est projetée vers le modèle logique |
| composants | `Identity.Access` (adaptateur du support actuel), `Identity` (modèle), + un adaptateur de test en mémoire |
| nouveaux objets | le modèle d'observations projeté ; les trois erreurs Ω |
| tests / oracle | l'artefact `tests/oracle/corpus1-postA1.db` : 497 actes, attributs de contenu présents, 1:1 vérifié ; mini-bases fabriquées → « Ω invalide » ; version étrangère → « Ω incompatible » ; l'adaptateur mémoire produit les mêmes objets sur les mêmes données (preuve de substituabilité, I42) |
| validation | le contexte ne sort que sur le canal C7 ; le modèle ne contient ni chemin ni date |
| livrable | commit |

| | **É5 — C3 réduit : le signal de contenu** |
|---|---|
| objectif | le signal « contenu identique » sous EQ-01, et lui seul |
| composants | `Identity` (couche signaux, pilotée par la famille interprétation du référentiel) |
| nouveaux objets | instances de signal relationnel, régime, provenance |
| tests / oracle | sur l'artefact : **381 classes** dont **112 multi-actes (108 paires, 4 triplets)** — chiffres des annexes de campagne ; provenance de chaque instance restituable ; sans EQ-01 dans le référentiel : aucun signal (I13) |
| validation | aucune instance sans (EQ-01, 1) dans sa provenance |
| livrable | commit |

| | **É6 — C4+C5+C6 réduits : les actes et W canonique** |
|---|---|
| objectif | produire un W canonique complet sur tout index (Ω conforme, ℛ₀) |
| composants | `Identity` (hypothèses de contenu, domination, licence CE-01, refus, assemblage, sérialisation canonique § 7) |
| nouveaux objets | hypothèses, actes, W, la forme canonique |
| tests / oracle | sur des mini-bases fabriquées aux W attendus écrits à la main ; double invocation → bit à bit ; permutation de l'ordre de lecture → bit à bit ; forme canonique conforme au § 7 champ par champ |
| validation | « entier ou absent » ; cohérence d'état vérifiée avant livraison |
| livrable | commit |

| | **É7 — Conformité W₀** |
|---|---|
| objectif | le moteur v1 est déclaré conforme |
| composants | `Identity.Tests` (le test d'or et la batterie) |
| nouveaux objets | le fichier W₀ attendu, produit **par un oracle indépendant** (script hors moteur, dans l'esprit des campagnes) depuis la base archivée et le § 8, versionné avec les tests |
| tests / oracle | (artefact, ℛ₀) → W₀ **bit à bit** ; batterie EXG-27 : déterminisme, permutation, localité (mini-base augmentée d'actes sans rapport), registre amputé d'EQ-01 → « registre incohérent », erreurs Ω ; les 116 actes du § 8 exactement |
| validation | déclaration de conformité v1 (011 § 8), consignée |
| livrable | commit ; rapport de validation au propriétaire |

Chaque étape : une fonctionnalité, dépôt propre, tests verts, commit, validation du propriétaire avant la suivante — la méthode du projet, sans exception.

---

## 11. La compatibilité future — démonstration

- **Ajout d'une convention** (dans une famille connue) : le référentiel de C2 gagne un élément ; les contrats C3–C5 reçoivent « les conventions de leurs familles » — leur **domaine** s'étend, leur clause ne change pas d'un mot. Aucun objet du § 2 ne change : une instance de signal nouvelle est une instance de signal. ∎
- **Nouvelle famille de signaux** (famille de conventions nouvelle) : par le 011 § 10, c'est théorie d'abord ; au niveau des contrats internes, la famille s'ajoute à l'énumération du § 5.1 et à la table des frontières (§ 3, ligne C2 → Cn) — extension de domaine d'un champ, pas de refonte : les objets Signal/Instance/Hypothèse sont définis indépendamment des familles qui les fondent. ∎
- **Nouveau producteur d'Ω** : le contrat du § 6 est le seul point de contact ; un support nouveau = un adaptateur nouveau du même port (013 § 5) ; aucun objet, aucun contrat C2–C7 n'est concerné — ils ne savent pas d'où viennent les actes. ∎
- **Nouvelle strate** : révision théorique préalable (005 fixe la chaîne actuelle) ; au niveau des contrats, la strate est une **valeur** d'un vocabulaire ordonné (§ 7.3), pas une structure : le vocabulaire s'étend, l'ordre canonique s'étend, aucun objet ne change de forme. ∎

Dans les quatre cas : **les contrats internes existants ne sont jamais modifiés — seulement leurs domaines.**

---

## 12. Nouveaux invariants — démontrés

> **I49 — Aucun objet logique ne dépend d'un langage de programmation.**
> *Démonstration* : chaque objet du § 2 est défini par identité, contenu, interdits et invariants — des notions ensemblistes et documentaires, sans référence à un système de types, une classe ou une convention d'appel. La preuve opératoire : le test d'or de É7 est produit par un outil **hors moteur**, dans un autre langage, depuis les mêmes définitions — si les objets dépendaient d'un langage, l'oracle indépendant serait impossible. ∎

> **I50 — Toute frontière échange exclusivement des objets complets.**
> *Démonstration* : la table du § 3 énumère les objets traversants ; chacun est défini au § 2 avec son contenu obligatoire intégral ; les contrats du § 1 garantissent en postcondition la complétude (provenance comprise — I30). Un objet partiel (une hypothèse sans résidu, un acte sans motif) viole la postcondition de sa couche productrice et ne peut donc pas atteindre une frontière. ∎

> **I51 — Tout objet consommé est valide par construction.**
> *Démonstration* : les entrées externes sont validées aux deux seuls points d'entrée (C1, C2 — les seules couches avec clauses de refus) ; au-delà, chaque couche ne consomme que les productions d'une couche dont la postcondition garantit la validité (§ 1). Par récurrence sur la chaîne (acyclique, I44), tout objet interne consommé est valide sans re-vérification. C'est pourquoi C3–C5 « ne refusent rien » : leur validation d'entrée serait redondante par construction. ∎

> **I52 — Toute sortie du moteur est exprimable uniquement avec les objets définis par les contrats.**
> *Démonstration* : les sorties contractuelles sont W, τ, les réponses d'audit et les erreurs nommées (011) ; les §§ 7 et 9 les définissent intégralement en termes des objets du § 2 et des vocabulaires normalisés (§ 7.4) ; I37 interdit toute autre manifestation observable. Une sortie inexprimable dans ces objets serait soit un état interne exposé (violation de I37), soit un objet non contractuel (violation de I50) — dans les deux cas, détectable et non conforme. ∎

---

## Conclusion

Les contrats internes sont totaux, les objets logiques complets, les frontières exhaustivement énumérées, la forme canonique de W spécifiée champ par champ, W₀ fixé à l'acte près (116 actes), l'audit contractualisé, le plan É1→É7 exécutable étape par étape avec ses oracles. **Toute implémentation est désormais quasiment mécanique** — il ne reste qu'une décision de contenu : le texte normatif exact des fichiers de ℛ₀, objet du document 015, après quoi É1 s'implémente sans aucune décision restante.

**Ce que ce document ne fait volontairement pas** : écrire le texte des conventions EQ-01/CE-01 (015), fixer la syntaxe Markdown précise des fichiers du registre (015), écrire le moindre code.
