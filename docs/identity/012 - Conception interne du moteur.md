# 012 — Conception interne du moteur

**Statut** : troisième document de conception de la série `docs/identity/`. S'appuie sur les documents 000→011, figés. L'interface publique (011) est acquise ; ce document ouvre l'intérieur de la boîte noire.
**Nature** : l'**organisation logique interne minimale imposée par la théorie** — quelles responsabilités existent, quelles informations circulent, quelles dépendances sont autorisées, quels invariants tiennent. Aucun code, aucune structure de données, aucun algorithme, aucune technologie, aucun format, aucune API (le 011 les définit déjà).
**Indépendance** : ce document ne mentionne ni le pipeline actuel, ni aucun format observé, ni aucune convention future. Il décrit **la machine abstraite capable de faire fonctionner toute la théorie** — pour tout domaine d'application produisant un Ω conforme.

---

## 1. Les couches logiques internes

### 1.1 Un préalable : les conventions ne sont pas une couche

Les conventions sont des **données** — des objets de ℛ interprétés par les couches — jamais une étape de la dérivation. Il n'existe pas de couche « application des conventions » : chaque couche applique les conventions des familles qui la concernent, reçues en entrée. C'est la condition de la propriété « le système apprend sans changer de moteur » (011 § 10) : si les conventions étaient une couche, en adopter une changerait le moteur.

### 1.2 Les responsabilités

Sept responsabilités internes — les noms sont indicatifs, seuls les contrats font foi :

| # | Responsabilité | Consomme | Produit |
|---|---|---|---|
| **C1** | **Projection de Ω** | le contrat public d'observations (011 § 2.1) | le *modèle d'observations* : actes identifiés, attributs, valeurs, ⊥ — sans contexte identitaire (A1 appliqué à la source) |
| **C2** | **Projection de ℛ** | le contrat public du registre (011 § 2.2) | le *référentiel de conventions* : objets convention versionnés, dépendances, incompatibilités — après vérification de forme et de cohérence (008 § 4) |
| **C3** | **Dérivation des signaux** | modèle d'observations + conventions d'interprétation, d'équivalence, d'attente, de catalogue | l'*étage des signaux* : instances avec régime (R1–R5) et provenance (002) |
| **C4** | **Construction des hypothèses** | étage des signaux + conventions de stratification et de composition | l'*étage des hypothèses* : hypothèses **à la demande** (jamais l'espace exhaustif — EXG-38), relations, consensus, résidus, contradictions, ordre de préférence, placements de strates (003, 005) |
| **C5** | **Décision des actes** | étage des hypothèses + conventions d'élection et de priorité | l'*ensemble des actes* : élections (niveau, motif, licences, dette) et refus (espèce, motif) — chaînes de résolution comprises (006, 007, 008 § 5) |
| **C6** | **Assemblage de l'état** | ensemble des actes + l'index (Ω, ℛ) | **W** : complet, cohérent (006 § 3), indexé, sous forme canonique (§ 5) ; et **τ** entre deux états dont les deux index sont fournis |
| **C7** | **Restitution d'audit** | les objets produits par C1–C6 (en lecture) | les réponses aux sept questions contractuelles (011 § 7) — une **projection**, jamais une source |

### 1.3 Les trois lois des couches

- **Chaque couche ne connaît que ce qu'elle doit connaître** : ses entrées déclarées — rien d'autre. C4 ne sait pas ce qu'est un octet observé ; C5 ne sait pas ce qu'est un attribut ; C6 ne sait pas ce qu'est un signal.
- **Une couche ne saute jamais une autre** : C5 n'atteint les observations qu'à travers les objets de C4, qui les atteint à travers ceux de C3. Le besoin de « voir plus bas » est toujours un défaut de contrat, jamais un droit d'accès.
- **Aucune couche ne peut modifier ce qui précède** : toutes les circulations sont des **valeurs** — produites, jamais retouchées (I41, § 10).

---

## 2. Les contrats entre couches

Forme commune à tout contrat de couche : *la couche reçoit un objet, produit un objet, et ne fait rien d'autre.*

| Clause | Contenu exigé pour chaque couche |
|---|---|
| **préconditions** | l'objet d'entrée est bien formé au sens du contrat de la couche précédente ; les conventions reçues appartiennent au référentiel projeté par C2 (jamais d'autre source — EXG-02 décliné par couche) |
| **postconditions** | l'objet de sortie est bien formé, **total sur l'entrée valide** (aucun cas d'entrée valide sans sortie définie — au pire, une sortie « vide mais motivée »), et porte la provenance intégrale de ce qu'il a consommé (I30 décliné par couche) |
| **obligations** | déterminisme (mêmes entrées → même sortie, I43) ; neutralité (aucun poids, aucun score introduit — I9 tient à chaque étage) ; traçabilité (chaque élément de sortie référence les éléments d'entrée et les (κ, ver) qui l'ont produit) |
| **interdictions** | accéder à autre chose que les entrées déclarées ; muter une entrée ; conserver un état entre deux productions ; produire un effet hors de l'objet de sortie ; inventer (tout élément de sortie sans antécédent d'entrée est une fabrication — interdite à tout étage, 002 § 1.2 généralisé) |

Précision par couche des points singuliers :

- **C1** : seule couche à connaître le contrat de Ω ; elle **filtre le contexte** — `chemin`, `date d'observation` et l'ordre d'insertion ne franchissent pas C1 vers la dérivation (A1, EXG-17) ; ils restent disponibles pour C7 à des fins de restitution uniquement, par un canal distinct qui ne traverse aucune couche de dérivation.
- **C2** : seule couche à connaître la représentation du registre ; elle **échoue explicitement** (011 § 5) sur registre absent, malformé ou incohérent — aucune convention ne franchit C2 sans que le prédicat du 008 § 4 soit établi.
- **C3** : ses sorties portent le régime — jamais un jugement ; un signal artefactuel est produit comme tel, avec sa condition de catalogue.
- **C4** : produit **à la demande** — le contrat porte sur la capacité à fournir les hypothèses pertinentes pour un domaine et leurs relations de préférence, pas sur l'énumération d'un espace combinatoire.
- **C5** : ses refus sont des sorties de plein droit, avec espèce et motif (008 § 9) — jamais des absences.
- **C6** : vérifie la cohérence d'état (006 § 3) avant de livrer — la postcondition « entier ou absent » (011 § 4) est la sienne.
- **C7** : lecture seule intégrale ; ses réponses sont re-dérivables (I39) — elle ne stocke aucune vérité.

Ces contrats sont **suffisants pour remplacer une couche sans toucher les autres** (§ 9) : ils spécifient tout le comportement observable de la couche, et rien que lui.

---

## 3. Les dépendances autorisées

Le graphe logique des « a le droit de connaître » :

  contrat de Ω ← **C1** ← **C3** ← **C4** ← **C5** ← **C6**
  contrat de ℛ ← **C2** ← (C3, C4, C5) [les familles de conventions qui les concernent]
  (C1…C6) ← **C7** [lecture seule]

Interdictions structurelles — chacune adossée à la théorie :

- **C4 ne lit jamais Ω directement** : les hypothèses se construisent sur des signaux, jamais sur des observations (001 § 5, règle 1) ;
- **C5 ne relit jamais les observations** — ni les signaux bruts : elle décide sur les objets d'hypothèses, qui portent déjà régimes et provenances ;
- **W n'influence jamais les hypothèses** : aucun arc de C6 vers C4 ou C5 — l'état est une sortie terminale, pas une mémoire (EXG-03) ; il n'existe aucun canal par lequel une élection passée biaiserait une dérivation présente ;
- **une convention ne connaît jamais le moteur** : les conventions sont des données passives interprétées par les couches ; aucune convention n'invoque, ne paramètre ni n'inspecte quoi que ce soit — le sens de l'interprétation va du moteur vers la convention, jamais l'inverse ;
- **C7 n'écrit rien nulle part** : la restitution est une projection sans retour ;
- **aucune couche ne connaît les consommateurs** (EXG-16) ni le producteur d'Ω (C1 connaît le *contrat*, pas le producteur).

Ce graphe est **acyclique par construction** (I44, § 10).

---

## 4. Les objets internes

Existence logique seulement — aucun ne préjuge d'une matérialisation :

| Objet | Rôle | Défini par |
|---|---|---|
| modèle d'observations | Ω projeté : actes, attributs, valeurs, ⊥ — sans contexte | 001 |
| référentiel de conventions | ℛ projeté : conventions versionnées, dépendances, incompatibilités, historique | 004, 007 |
| ensemble de signaux | instances avec régime et provenance ; **jamais persisté par nécessité** (I5) | 002 |
| relations et consensus | relations partielles tenues séparées ; faits de cohérence jointe | 003 |
| ensemble d'hypothèses | quintuplets (Dom, Obs, Sig, prov, just), fournis à la demande | 003 |
| résidus et préférence | ensembles d'inexpliqué comparés par inclusion ; ordre partiel | 003 |
| ensemble de contradictions | les trois types (002 § 9), avec leur sort relatif aux hypothèses | 003 § 5 |
| ensemble des actes | élections (niveau, motif, licences, dette) et refus (espèce, motif) | 006 |
| chaînes de justification | la dérivation complète de chaque acte, maillon par maillon | 008 § 5, § 6 |
| dépendances | Dep par acte, dette identifiée, ensembles minimaux | 004 § 10, 008 § 8 |
| W et τ | l'état indexé canonique ; la transition tracée | 006 |

Tous sont des **valeurs re-dérivables** depuis (Ω, ℛ) (I5, I10, I39) ; aucun n'est une source de vérité ; tout stockage de l'un d'eux est un cache (EXG-24).

---

## 5. La forme canonique de W

Le 011 exige une forme canonique ; en voici la définition — **sans imposer de format** :

**Identité.** Deux W sont identiques si et seulement si : (a) même index — même état identifié de Ω, même état identifié de ℛ ; (b) même ensemble d'actes, où deux actes sont identiques lorsqu'ils coïncident sur : le domaine (l'ensemble des identifiants d'actes d'observation couverts), la strate, le type (élection ou refus), le contenu propositionnel retenu (pour une élection), le niveau, le motif, l'ensemble des licences (κ, ver), la dette, l'espèce et le motif (pour un refus). Rien d'autre n'entre dans l'identité.

**Ordre canonique.** La représentation canonique ordonne les actes selon une clé **totale, déterministe, et dérivée exclusivement du contenu identitaire des actes et des identifiants d'Ω** — jamais de l'ordre de calcul, d'insertion ou de découverte. Toute égalité de contenu se départage par les identifiants, qui font partie de Ω et sont donc identiques à index identique.

**Stabilité.** La forme canonique est invariante par recalcul, permutation des parcours, parallélisme, version du moteur (à conformité égale) — c'est sur elle que porte l'identité bit à bit (EXG-18).

**Contenu obligatoire.** L'index, en clair et en premier ; la totalité des actes ; pour chaque acte, la totalité des champs d'identité ci-dessus. Un W amputé d'un refus, d'un motif ou d'une licence n'est pas canonique — il est invalide.

**Contenu interdit.** Tout élément de contexte d'observation (chemins, dates d'observation) ; toute métadonnée de calcul (durée, machine, date de production, version du moteur — la conformité rend la version indifférente, donc sa présence serait un mensonge de pertinence) ; tout élément non re-dérivable de l'index ; toute valeur d'ordre reflétant le déroulement interne.

---

## 6. Les chaînes d'audit

Composition logique — pas de représentation :

- une chaîne est une **suite finie de maillons**, chacun portant : la couche productrice (C1–C6), les identifiants exacts des objets consommés, les (κ, ver) appliqués, l'objet produit, et son statut (nominal, ou porteur d'un régime / d'une contradiction / d'une dette) ;
- **chaque maillon est identifiable** : il peut être désigné, restitué et re-dérivé isolément (I39) ;
- **chaque rupture est explicite** : une chaîne s'interrompt sur un maillon terminal qui nomme ce qui manque — signal non défini, hypothèse non formulable, contradiction ouverte, licence absente, conflit déclaré — avec l'espèce du refus qui en résulte (008 § 5, § 9) ;
- **une chaîne interrompue est toujours valide** : c'est un objet complet, de même dignité qu'une chaîne aboutie ; l'interruption est une information, pas un échec de la chaîne ;
- une chaîne ne contient **rien qui ne soit dans la dérivation** : pas de commentaire libre, pas d'estimation, pas de reformulation — elle est la dérivation, vue maillon par maillon.

---

## 7. La représentation logique du registre

Les objets du registre — leur rôle, pas leur format (ni JSON, ni YAML, ni XML, ni rien) :

| Objet | Rôle |
|---|---|
| **convention** | l'octuple du 004 (Déf. 1), complété des champs d'élection le cas échéant (007 § 5) — l'unité normative |
| **version** | un état daté et immuable du contenu d'une convention ; toute modification est une version nouvelle ; toute version passée reste référençable |
| **dépendances** | les arcs « présuppose » entre conventions, versions comprises — le matériau de l'acyclicité et de la propagation (008 § 7) |
| **historique** | la suite complète des transitions du registre — chaque transition datée, motivée, typée (adoption, révision, retrait, remplacement, scission, fusion — 007 § 10) |
| **adoption** | l'acte qui fait entrer une version en vigueur : (convention, version, date, justification, autorité) — 007 § 9 ; un **événement**, pas une opération du moteur |
| **retrait** | l'acte symétrique, avec les mêmes exigences de trace |
| **état du registre** | l'ensemble des versions en vigueur + les incompatibilités déclarées — la valeur que C2 projette et que l'index référence |

Le registre est **gouverné par événements et lu comme valeur** : le moteur ne voit jamais une opération — il voit des états. La correspondance entre les événements de gouvernance et leur support matériel (fichiers, commits) relève du document 013.

---

## 8. La validation interne

Objets vérifiables **indépendamment, sans moteur complet** — chaque vérification n'exige que l'objet et ses entrées déclarées :

| Vérification | Porte sur | N'exige que |
|---|---|---|
| cohérence du registre (008 § 4) | l'état projeté de ℛ | C2 seul |
| bonne formation d'un signal (préconditions, régime, provenance) | une instance de signal | l'instance + les observations et conventions qu'elle cite |
| bonne formation d'une hypothèse (quintuplet, clôture de provenance) | une hypothèse | l'hypothèse + les signaux qu'elle cite |
| cohérence d'une comparaison de préférence (inclusion des résidus) | une paire d'hypothèses | les deux hypothèses |
| cohérence structurelle de W (complétude, absence de conflit, cohérence verticale, canonicité) | W seul | W |
| cohérence d'une chaîne (chaque maillon résout, chaque (κ, ver) existe dans l'index) | une chaîne | la chaîne + l'index |
| validité d'un cache (coïncidence avec la re-dérivation) | tout objet stocké | l'objet + l'index |

Cette vérifiabilité par morceaux est ce qui rend la conformité **testable couche par couche** avant tout assemblage — et ce qui rend les défauts localisables : un W incohérent désigne C6, une chaîne qui ne résout pas désigne la couche du maillon fautif.

---

## 9. Le remplacement d'une couche

> **Proposition.** Si une couche est remplacée par une autre qui honore le même contrat (§ 2), le moteur entier reste conforme.

**Démonstration** (conséquence du découpage, pas propriété empirique) : chaque contrat de couche spécifie **la totalité du comportement observable** de la couche — l'objet de sortie comme fonction de l'objet d'entrée (I43), sans état, sans effet, sans autre accès. Deux couches honorant le même contrat produisent donc, sur toute entrée valide, des objets de sortie identiques. Les couches aval ne consommant que ces objets (§ 3 : aucun accès sous-jacent, aucun canal caché), leurs propres sorties sont inchangées, de proche en proche jusqu'à W. La forme canonique (§ 5) étant fonction des seuls actes, W est identique bit à bit — et la conformité (011 § 8) porte sur W et les restitutions, elles-mêmes re-dérivées des mêmes objets. ∎

La condition est exigeante et assumée : elle tient parce que les contrats du § 2 sont **totaux** (ils ne laissent aucun comportement observable non spécifié). Un contrat partiel ruinerait la proposition — c'est pourquoi toute zone d'ombre découverte dans un contrat de couche se traite comme un défaut de conception (le contrat est complété), jamais comme une liberté d'implémentation.

---

## 10. Les invariants nouveaux — démontrés

> **I41 — Aucune couche ne peut modifier un objet provenant d'une couche antérieure.**
> *Démonstration.* Les objets inter-couches sont des valeurs re-dérivables (I5, I10). Supposons qu'une couche modifie un objet amont : l'objet modifié diffère alors de sa re-dérivation depuis l'index — il est invalide par définition (EXG-24, I39), et la vérification de cache (§ 8) le détecte. De plus, la source ultime est soit Ω (intangible, I1, EXG-07), soit ℛ (immuable par état, 011 § 2.2) : la modification n'a nulle part où s'ancrer sans violer un invariant antérieur. ∎

> **I42 — Toute couche est remplaçable si son contrat est respecté.**
> *Démonstration.* C'est la proposition du § 9, qui repose uniquement sur la totalité des contrats (§ 2) et l'absence de canal hors contrat (§ 3). ∎

> **I43 — Toute sortie est entièrement déterminée par les entrées de la couche.**
> *Démonstration.* Par les interdictions du § 2 (aucun accès non déclaré, aucun état conservé, aucun aléa — sinon EXG-02/03 seraient violés au niveau moteur, le moteur étant la composition des couches), chaque couche est une fonction de ses entrées. La composition de fonctions est une fonction : le déterminisme de bout en bout (EXG-18) est obtenu par construction, pas par discipline. ∎

> **I44 — Aucune dépendance circulaire n'existe entre couches.**
> *Démonstration.* Supposons un cycle dans le graphe du § 3 : un objet serait alors requis, directement ou transitivement, pour sa propre dérivation. Or tout objet interne doit être re-dérivable depuis (Ω, ℛ) en un nombre fini d'étapes (I5, I10, I39) — une dérivation bien fondée. Un objet nécessaire à lui-même n'a pas de dérivation bien fondée : contradiction. Le graphe est donc acyclique, et l'ordre C1/C2 → C3 → C4 → C5 → C6 (C7 en lecture) en est un tri topologique. ∎

---

## Conclusion

La machine abstraite est définie : sept responsabilités aux contrats totaux, un graphe de dépendances acyclique démontré, des objets internes qui sont tous des valeurs re-dérivables, une forme canonique qui rend l'identité bit à bit opératoire, des chaînes d'audit dont les ruptures sont des objets de plein droit, un registre gouverné par événements et lu comme valeur, une validation par morceaux qui localise les défauts, et la remplaçabilité des couches obtenue par démonstration. Rien de tout cela ne nomme un langage, un format, un algorithme ni un domaine d'application : **toute la théorie peut fonctionner sur cette machine, et cette machine peut être construite dans n'importe quelle technologie.** Le document 013 pourra choisir.

---

## Récapitulatif

| Objet | Contenu | § |
|---|---|---|
| les sept responsabilités | C1 projection de Ω, C2 projection de ℛ, C3 signaux, C4 hypothèses, C5 actes, C6 état, C7 audit | 1 |
| les conventions comme données | pas de couche « application des conventions » — condition de « apprendre sans changer de moteur » | 1.1 |
| contrats de couche | préconditions, postconditions, obligations, interdictions — totaux, remplaçabilité à la clé | 2 |
| graphe des dépendances | chaîne stricte + C2 transversal en données + C7 en lecture ; interdictions structurelles ; acyclique | 3 |
| objets internes | tous valeurs re-dérivables ; tout stockage est un cache | 4 |
| forme canonique de W | identité définie champ par champ ; ordre dérivé du seul contenu identitaire ; contenus obligatoire et interdit | 5 |
| chaînes d'audit | maillons identifiables, ruptures explicites, chaîne interrompue toujours valide | 6 |
| registre logique | convention, version, dépendances, historique, adoption, retrait — événements de gouvernance, valeur pour le moteur | 7 |
| validation interne | sept vérifications indépendantes, sans moteur complet ; défauts localisables | 8 |
| remplaçabilité | démontrée depuis la totalité des contrats | 9 |
| invariants | I41–I44, chacun démontré | 10 |

**Ce que ce document ne fait volontairement pas** : nommer une technologie, un format ou un algorithme ; découper en modules logiciels ; matérialiser ℛ ; définir un plan de réalisation.
