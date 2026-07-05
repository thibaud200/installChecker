# 004 — Conventions d'interprétation et résolution des contradictions

**Statut** : cinquième document de la série `docs/identity/`. S'appuie sur les documents 000 à 003, validés et figés.
**Périmètre** : la couche **normative** du système. Les documents précédents définissaient ce qui existe (observations), comment les objets se construisent (signaux) et se combinent (relations, consensus, hypothèses) ; celui-ci définit comment les **conventions** gouvernent ces constructions. Il ne décide toujours d'aucune identité : il définit les règles permettant de produire des interprétations cohérentes. Aucun algorithme, aucun score, aucune pondération, aucune règle métier spécifique, aucune décision de regroupement.
**Rappel structurel** : les observations sont indépendantes des conventions (couche 1, intangible) ; les signaux dépendent des conventions (002, composante K_σ) ; les hypothèses dépendent des signaux (003). Toute évolution de convention est donc une cause de révision potentielle des hypothèses (003, Déf. 10) — jamais des observations.

---

## 1. La convention, objet formel

### 1.1 Définition

**Définition 1 (convention)** — Un objet

  κ = ( id, ver, App(κ), T(κ), just(κ), date, hist(κ), src(κ) )

où :

- **id** — identifiant unique et pérenne (une convention ne change jamais d'identifiant) ;
- **ver** — version ; toute modification du contenu de κ produit une version nouvelle, l'ancienne restant référençable à jamais ;
- **App(κ) — domaine d'application** : sur quels attributs, quels types de signaux, quels contextes (contradictions, absences) la convention opère — et *seulement* eux ;
- **T(κ) — la transformation conceptuelle** : ce que la convention fait, en une seule opération (§ 11, minimalité) ;
- **just(κ) — justification** : pourquoi cette convention plutôt qu'une autre — spécification de format invoquée, usage documenté, arbitrage assumé ; une convention sans justification est invalide ;
- **date** — date d'introduction de la version ;
- **hist(κ)** — l'historique des versions (contenus successifs, motifs de révision) ;
- **src(κ) — provenance documentaire** : les sources externes sur lesquelles la convention s'appuie (spécification PE, RFC, documentation Windows Installer, rapport de campagne).

### 1.2 Jamais implicite

> **I13 — Explicitation (invariant).** Aucune convention n'est implicite. Toute étape interprétative de toute dérivation (signal, relation, consensus, hypothèse) référence une convention de K par identifiant **et** version. Une dérivation contenant une étape interprétative non référencée est invalide par définition — c'est ainsi que le système se protège des « évidences » non tracées (l'attente naïve « ole-cfb ⟹ propriétés MSI » du 001, E7, est exactement le genre d'implicite que I13 interdit).

---

## 2. Le système K

**Définition 2 (état de K)** — Un *état* de K est un ensemble fini de couples (κ, ver) — les conventions en vigueur, chacune en une version précise. K évolue par **transitions discrètes, datées et justifiées** : adoption d'une convention, révision (nouvelle version), retrait. Chaque état de K est identifiable et reconstructible.

Propriétés :

- **Indexation des hypothèses.** Toute hypothèse est indexée par (Ω, K) (003 § 10). **Deux hypothèses construites sous deux états de K différents sont deux objets différents**, même si leurs quintuplets semblent coïncider : leurs dérivations, leurs résidus et leurs dépendances (§ 10) diffèrent en droit.
- **Historicité.** Les états passés de K restent référençables : une conclusion ancienne demeure re-dérivable sous son index d'origine (P4, I10).
- **K est une décision, pas une découverte.** Aucun état de K ne se déduit de Ω (002 § 2) ; K se justifie, se documente, se révise — il ne se démontre pas.

---

## 3. Les conventions d'interprétation

**Définition 3 (convention d'interprétation)** — Une convention dont la transformation T(κ) associe aux valeurs brutes d'un attribut une propriété conventionnelle (002 § 2), utilisable par un type de signal. Exemples de forme (aucun n'est adopté ici) :

| Observation (intangible) | Convention candidate | Propriété conventionnelle |
|---|---|---|
| `machine = '8664'` | table des codes machine COFF/PE, version datée de la spécification | « architecture cible x86-64 » |
| `product_version = '1.0'` | mise sous forme canonique d'une déclaration de version (structure, complétion, casse) | « version canonique 1.0.0.0 » (forme, pas vérité) |
| chaîne déclarative quelconque | normalisation Unicode (NFC), version de l'annexe Unicode | forme normalisée comparable |

Loi fondamentale de la couche :

> **Une convention transforme une observation en signal. Jamais l'observation elle-même.** La chaîne `'17.7.40001'` reste éternellement `'17.7.40001'` dans Ω ; toute forme canonique est une sortie de signal, reconstructible et jetable (I5). Réviser la convention change la forme canonique — pas la chaîne.

---

## 4. Les conventions d'équivalence

**Définition 4 (convention d'équivalence)** — Une convention dont T(κ) définit une équivalence interprétative ≈ (002 § 6 : relation d'équivalence véritable, plus grossière que le byte-à-byte). Sa forme obligatoire précise :

- **domaine** : l'attribut ou le domaine de valeurs concerné (localité, 002 § 6.2 — pas d'équivalence universelle) ;
- **perte d'information** : la description explicite des distinctions effacées — le quotient opéré (quelles classes byte-à-byte sont fusionnées : casses, formes Unicode, espaces) ; toute équivalence est une perte de discriminance **assumée et documentée** ;
- **justification** : pourquoi ces distinctions sont conventionnellement non significatives sur ce domaine (usage du format, pratique des producteurs) ;
- **limites** : les cas connus où la fusion est abusive (deux produits distincts ne différant que par la casse : la convention doit citer ce risque, même jugé acceptable).

Le présent document **ne choisit toujours aucune équivalence** : il fixe la forme que devra avoir chacune pour être admissible dans K.

---

## 5. Les conventions de priorité

Premier document où cet objet apparaît (000 L3, reporté jusqu'ici).

**Définition 5 (convention de priorité)** — Une convention dont T(κ) est une **relation** ▷ entre familles de signaux (types, éventuellement restreints par régime), **conditionnée à un contexte de contradiction déclaré** : σ ▷ σ′ *dans le contexte c* signifie que lorsqu'une contradiction du type c oppose une instance de σ à une instance de σ′, la lecture de σ est conventionnellement retenue pour la construction des hypothèses, et celle de σ′ conventionnellement écartée — **écartée, pas effacée** : l'instance de σ′ demeure dans Sig(h), marquée supplantée, avec la référence (κ, ver) de l'arbitrage.

Propriétés définitoires, qui résolvent la tension avec I9 et I12 :

- **une priorité n'est pas un poids** : ▷ est une relation ordinale, locale à un contexte de contradiction. Elle ne produit aucun score, ne s'additionne pas, ne se compose pas transitivement d'office entre contextes (σ ▷ σ′ dans c et σ′ ▷ σ″ dans c′ n'impliquent rien dans c″) ;
- **partialité assumée** : là où aucune priorité n'est déclarée, la contradiction reste ouverte ou assumée (003, Déf. 7) — le système ne fabrique jamais d'arbitrage par défaut ;
- **elle ne dit pas le vrai** : σ ▷ σ′ ne signifie pas « σ est plus fiable » (cela, c'est la qualification, 002 § 3.2, descriptive) mais « en cas de conflit de type c, *nous décidons* de construire sur σ ». C'est un décret, daté, justifié, révocable ;
- **neutralité préservée** : la priorité opère sur la *construction* des hypothèses, jamais sur leur *comparaison* — l'ordre de préférence (003 § 4) reste fondé sur l'inclusion des résidus, et I12 est intact.

Le présent document définit l'objet ; **aucune priorité concrète n'est instanciée**.

---

## 6. Les conventions de résolution

Le 003 (Déf. 7) définit la résolution **explicative** : une hypothèse rend la contradiction attendue par son contenu même. Le présent document définit l'acte **conventionnel** :

**Définition 6 (résolution conventionnelle)** — L'application, tracée, d'une convention de priorité à une instance de contradiction : le triplet (contradiction, (κ, ver), lecture retenue) est enregistré dans la dérivation just(h) de toute hypothèse qui en bénéficie. La contradiction quitte alors Res(h) — c'est la fonction de l'acte — mais entre dans la **dette conventionnelle** de h (§ 10) : h doit désormais sa position à un décret.

Distinction structurante entre les deux résolutions :

| | Résolution explicative (003) | Résolution conventionnelle (004) |
|---|---|---|
| mécanisme | le contenu de h rend la conjonction attendue | un décret de K retient une lecture |
| robustesse | tient sous **tout** état de K | tient sous les seuls états de K contenant (κ, ver) |
| trace | dans le contenu propositionnel et just(h) | dans just(h) **et** Dep(h), avec (κ, ver) |
| statut du perdant | il n'y a pas de perdant : tout est expliqué | l'instance supplantée reste présente, marquée |
| révocation | par domination d'une meilleure explication | par simple révision de κ |

Les deux peuvent coexister sur la même contradiction ; le cadre n'énonce aucune règle de précédence entre elles — il constate seulement que la résolution explicative est **structurellement plus robuste** (elle ne figure pas dans la dette), et que la comparaison des dettes se fait, comme les résidus, **par inclusion et jamais par comptage** (003 § 1.3). L'usage de cette comparaison dans la préférence est reporté aux documents suivants.

---

## 7. Les attentes conventionnelles

Le 003 (§ 6) interdit toute attente **implicite**. Le présent document définit l'attente **explicite** :

**Définition 7 (attente conventionnelle)** — Une convention dont T(κ) déclare : *« sous la lecture nominale N, la présence de l'attribut (ou du signal) X est attendue »*. Exemple de forme (non adopté) : « un contenu lu comme base d'installation Windows nominale possède normalement une table Property ».

Effets licites — et eux seuls :

- lorsque X est **présent**, l'attente est satisfaite ; aucun effet ;
- lorsque X est **⊥**, l'attente autorise la construction d'un signal d'**écart à l'attente** : « la configuration s'écarte de la lecture nominale N selon (κ, ver) ». C'est exactement le chemin que le 002 (R2) avait réservé : un signal défini sur l'absence, dont la sortie **assume l'ambiguïté irréductible absent/illisible** ;
- l'écart est un **fait à expliquer**, jamais une réfutation : il peut entrer dans le domaine des hypothèses (une hypothèse qui l'explique — « format distinct partageant le conteneur » — en rend compte ; une autre l'assume dans son résidu). **En aucun cas** un écart ne rend une hypothèse *impossible* : le niveau « impossible » (000 § 5.1) exige une observation incompatible, et une absence n'est jamais incompatible avec rien (principe du silence, 003 § 6).

Coexistence avec les états du 001 : l'attente ne modifie ni « présent », ni « absent », ni « illisible » — elle ajoute une lecture *de la configuration* relative à une norme choisie. **Une attente est une convention, pas une vérité** : la réviser fait disparaître les écarts sans qu'aucune observation ait changé.

---

## 8. Les conventions sur les artefacts : le catalogue

Le 002 (§ 8.3) a ouvert le catalogue avec A-01. Le présent document en fait un **sous-ensemble versionné de K** et fixe la forme d'une entrée :

**Définition 8 (entrée de catalogue d'artefacts)** — Une convention A-xx dont T(κ) déclare une **condition d'artefact** : « la méthode M, appliquée hors de son domaine nominal selon la signature observationnelle S, produit des valeurs artefactuelles pour les attributs {a…} ». Forme obligatoire :

- **critères d'entrée** : une *signature observationnelle* S — conditions exprimées uniquement sur des observations (valeurs, co-présences, absences) — reproductible et vérifiable sur Ω ;
- **justification mécaniste** : pourquoi la méthode quitte son domaine nominal (comportement documenté de l'API, format permissif) — sans mécanisme identifié, l'entrée reste candidate, pas admise ;
- **preuve** : les instances mesurées, référencées (corpus, rapport, effectifs) ;
- **version** : la signature S peut être resserrée ou élargie ; chaque modification est une version nouvelle ;
- **retrait éventuel** : une entrée est retirée si sa justification mécaniste est invalidée ou sa signature reconnue trop large (elle capturait des cas légitimes) ; le retrait est une transition de K, datée et justifiée — les hypothèses qui s'appuyaient sur l'entrée sont révisées par la voie normale (§ 9).

Statut : une entrée de catalogue **ne certifie pas** — le caractère artefactuel d'une instance reste une hypothèse (001 § 4.5) ; l'entrée fournit la condition conventionnelle sous laquelle cette hypothèse est constructible et sa signature vérifiable. Le catalogue est **ouvert** (000 L4) : l'absence d'entrée ne prouve pas l'absence d'artefact.

**Entrée A-01 mise en forme** (première instance, adoptée au 002) : M = lecture PE ; S = { `machine` non-⊥ ∧ `container='zip'` ∧ `optional_header_magic=⊥` ∧ `subsystem=⊥` } ; justification mécaniste : l'API accepte les flux sans en-tête MZ et lit les premiers octets comme en-tête COFF ; preuve : 20 actes, campagne corpus 1 (`docs/mesures/`) ; version 1.

---

## 9. Les révisions de convention

Le 003 (Déf. 9–10) distingue révision par observation (Ω) et par convention (K). Le présent document formalise la seconde :

- **Révision locale** — Une transition de K ne touchant qu'une convention κ. Grâce à la minimalité (§ 11, I14) et aux dépendances (§ 10), l'impact est **circonscrit par construction** : seuls les signaux dont K_σ référence κ, et les hypothèses dont Dep(h) contient κ, sont à re-dériver. Tout le reste est stable.
- **Révision globale** — Une transition touchant une convention dont dépendent de larges pans de l'étage (une équivalence de casse utilisée par de nombreux types de signaux). Ce n'est pas une autre espèce : c'est une révision locale à grande zone de propagation — la distinction est de degré, mesurée par la relation de dépendance, pas de nature.
- **Propagation** — La re-dérivation suit exactement la relation de dépendance (§ 10) : κ → types de signaux qui la référencent → instances → relations, consensus, hypothèses. Rien d'autre ne bouge (P3, localité, propagée à la couche normative).
- **Stabilité** — Toute conclusion dont Dep(h) ne contient pas κ est **inchangée à l'identique** par la révision de κ. C'est le dividende du principe de minimalité : des conventions fines rendent les révisions fines.
- **Borne absolue** — Aucune révision de convention, locale ou globale, ne modifie, ne supprime ni ne reformule une observation (I1, I15). La couche 1 est hors de portée de K par construction.

---

## 10. Les dépendances

**Définition 9 (dépendance conventionnelle)** — Pour toute hypothèse h, Dep(h) est l'ensemble exact des couples (κ, ver) mobilisés n'importe où dans sa dérivation : conventions d'interprétation et d'équivalence (via les K_σ de Sig(h)), attentes (via les signaux d'écart), entrées de catalogue (via les statuts artefactuels), priorités et résolutions conventionnelles (via just(h), § 6).

Exigences :

- **Reconstructibilité totale.** Dep(h) n'est pas une annotation tenue à la main : il **se calcule** depuis just(h) et prov(h), qui tracent chaque étape (I4, I7, I13). À la question *« de quelles conventions dépends-tu ? »*, toute hypothèse répond exactement, versions comprises.
- **Séparation des dettes.** Dans Dep(h), les résolutions conventionnelles (§ 6) sont distinguables des simples conventions de lecture : la *dette d'arbitrage* (ce que h doit à des décrets tranchant des contradictions) est un sous-ensemble identifié de Dep(h), comparé par inclusion si besoin — jamais compté.
- **Symétrie avec Ω.** (Ω, K) indexe les hypothèses (003 § 10) ; Obs(h) donne la dépendance exacte à Ω, Dep(h) la dépendance exacte à K. Les deux questions « de quelles observations » et « de quelles conventions » ont des réponses de même précision.

---

## 11. Propriétés mathématiques des conventions — invariants

> **I14 — Minimalité (invariant).** Une convention = **une seule transformation conceptuelle**. Une convention ne couvre jamais plusieurs décisions indépendantes : une table de codes machine et une normalisation Unicode sont deux conventions ; une équivalence de casse et une équivalence d'espaces sont deux conventions ; une priorité dans un contexte de contradiction et une attente de présence sont deux conventions. Critère opératoire : si deux parties de T(κ) peuvent être révisées indépendamment sans se contredire, elles doivent être deux conventions. La minimalité est ce qui rend les révisions de K **fines** (§ 9) : elle borne la propagation au strict nécessaire.

> **I15 — Innocuité descendante (invariant).** Les conventions n'opèrent qu'à partir de la couche interprétation. Aucune convention ne peut modifier, supprimer, reformuler ou conditionner une observation — ni exiger du pipeline qu'il observe autrement (I3 : la pertinence appartient au consommateur). K et Ω sont **mutuellement indépendants** : Ω ne détermine aucune convention (K se décide), K n'altère aucune observation (Ω s'observe).

> **I16 — Invariance propositionnelle (invariant).** Une convention ne modifie jamais le **domaine explicatif** d'une hypothèse. Le contenu propositionnel — « ces actes procèdent d'une origine commune à telle strate » — est un objet indépendant de K ; ce que K détermine, c'est la **manière de construire** l'hypothèse : quels signaux existent pour la soutenir, ce que contient son résidu, quels arbitrages elle mobilise, donc sa position dans l'ordre de préférence. Réviser K peut rendre h mieux ou moins bien soutenue, constructible ou inconstructible — jamais changer *ce que h affirme*. (En termes du 003 : K agit sur Sig(h), Res(h), just(h), Dep(h) ; jamais sur Dom(h) ni sur l'origine postulée.)

S'y ajoutent, hérités ou reformulés à cette couche :

| Propriété | Contenu |
|---|---|
| déterminisme | T(κ) est une transformation déterministe ; à (Ω, K) fixé, tout l'édifice est identique (I6, I11) |
| traçabilité | chaque usage de κ est référencé par (id, ver) dans les dérivations (I13) |
| versionnement | toute modification = version nouvelle ; les versions passées restent référençables (§ 1.1, § 2) |
| réversibilité | revenir à une version antérieure de K est une transition comme une autre ; rien n'est perdu, tout se re-dérive (I5, I10) |
| composabilité | les dérivations peuvent chaîner plusieurs conventions (normalisation puis équivalence puis interprétation), chaque maillon référencé séparément — la composition est tracée, jamais implicite |
| stabilité | une révision ne propage que le long des dépendances réelles (§ 9) |
| indépendance des observations | I15 |

---

## 12. Ce qui n'est PAS une convention

| Notion | Pourquoi ce n'est pas une convention |
|---|---|
| un score, une pondération | une convention est une **transformation ou une relation**, jamais une valuation ; introduire un nombre à prétention comparative violerait I9/I12 quelle que soit la couche qui l'abrite |
| une probabilité | une probabilité prétend décrire le monde (fréquences, croyances) ; une convention ne décrit pas, elle **décide une lecture** — et s'assume comme telle |
| une décision (d'identité) | la convention gouverne la *construction* des hypothèses ; élire une hypothèse, agir, nommer sont des actes des couches supérieures (003 § 12) |
| une identité, un logiciel | ce sont des origines postulées par les hypothèses — le *résultat* du système, que K ne contient jamais (sinon K déciderait des identités, ce que ce document s'interdit dès son préambule) |
| un consensus | le consensus est un **fait structurel** constaté sur des signaux (003 § 2) ; il se produit ou non — on ne le décrète pas |
| une observation | I15 : les observations sont indépendantes de K par construction |
| une « bonne pratique » implicite | tant qu'elle n'est pas un objet κ complet (id, version, justification…), une habitude de lecture n'existe pas pour le système (I13) |

Règle générale : **une convention décide comment lire — jamais ce qui est, ce qui est vrai, ni ce qui doit être retenu.**

---

## 13. Exemples — corpus 1 exclusivement

Chaque exemple montre des conventions **coexistantes**, opérant sur les mêmes observations sans jamais les modifier.

**E1 — A-01 sous sa forme normative** (20 actes).
L'entrée du § 8 : identifiant A-01, version 1, signature S vérifiable sur Ω (les 20 actes la satisfont, aucun autre), justification mécaniste, preuve référencée. Coexistence : sous un état de K sans A-01, la configuration `zip` + `machine='4b50'` est une contradiction intra-acte ouverte (002, Déf. 9) ; sous un état avec A-01, l'hypothèse artefactuelle est constructible et domine (003, E5). **Les 20 observations sont identiques dans les deux mondes** — seule la lecture a changé. Dep(h₂) contient (A-01, v1) ; l'hypothèse répond exactement de sa dépendance.

**E2 — `product_version = '17.7.40001'`** (366 actes MSI avec déclarations).
Deux conventions d'interprétation candidates peuvent coexister dans des dérivations distinctes : κ_a « forme canonique à quatre composantes numériques » et κ_b « chaîne opaque comparée par égalité stricte ». Elles produisent des signaux différents à partir de la même observation ; ni l'une ni l'autre ne touche la chaîne persistée. Choisir entre elles (ou les faire coexister durablement) est une décision future ; le cadre exige seulement que chacune soit un objet κ complet et que tout signal cite la sienne (I13).

**E3 — `upgrade_code`** (366 actes).
Convention d'interprétation candidate : « déclaration de lignée au sens du format Windows Installer » (src : documentation du format ; justification : sémantique déclarée du champ). Sa minimalité (I14) impose de la séparer d'une éventuelle attente (« un MSI nominal porte un `upgrade_code` ») et de toute future convention de stratification qui s'en servirait : trois transformations conceptuelles, trois conventions, révisables indépendamment.

**E4 — VersionInfo absent** (439 actes) et l'attente explicite.
Sans attente dans K : silence pur — ni corroboration, ni contradiction, ni écart (003 § 6). Avec une attente candidate « un exécutable PE nominal porte une ressource VersionInfo » : les 4 actes PE sans VersionInfo (61 PE, 57 avec) produiraient un signal d'écart — un fait à expliquer, assumant l'ambiguïté absent/illisible, incapable de rendre quoi que ce soit impossible (§ 7). Les 439 observations ⊥ restent strictement identiques sous les deux états de K : l'écart vit dans la lecture, pas dans la base.

**E5 — Les 10 `.msp` et l'attente « table Property ».**
Sous l'attente candidate du § 7, les 10 actes produisent un écart à la lecture nominale MSI. L'hypothèse « format distinct partageant le conteneur OLE-CFB » **explique** l'écart (résolution explicative, robuste) ; l'hypothèse « base MSI dégradée » l'**assume** dans son résidu. La préférence se joue par inclusion (003, Déf. 5), l'attente n'ayant fait que rendre l'écart *visible et traçable* — elle n'a tranché personne.

**E6 — `sha256` : même le socle est conventionné.**
La comparaison des condensats mobilise une convention triviale (égalité byte-à-byte de chaînes hexadécimales — minimale, justifiée par la définition même de l'attribut) ; et le plafond « certain » que le 000 (§ 5.1) réserve à ≡ₘ est lui-même une **convention candidate de K** (« l'égalité des condensats vaut certitude conventionnelle ») — datée, justifiée par L7 (probabilité de collision), révocable en théorie. Le cadre retrouve ainsi sa propre clef de voûte comme objet normatif ordinaire : rien, pas même ≡ₘ, n'échappe à I13.

---

## 14. Récapitulatif

| Objet | Définition | § |
|---|---|---|
| convention κ = (id, ver, App, T, just, date, hist, src) | transformation conceptuelle unique, explicite, versionnée, justifiée | 1 |
| système K | ensemble versionné de conventions ; transitions datées ; indexe toute hypothèse avec Ω | 2 |
| convention d'interprétation | transforme une observation en signal — jamais l'observation | 3 |
| convention d'équivalence | forme obligatoire : domaine, perte d'information, justification, limites — aucune choisie | 4 |
| convention de priorité | relation ▷ conditionnée à un contexte de contradiction ; décret révocable, pas un poids ; aucune instanciée | 5 |
| résolution conventionnelle | acte tracé (contradiction, (κ,ver), lecture retenue) ; sort du résidu, entre dans la dette ; distincte de la résolution explicative | 6 |
| attente conventionnelle | attente explicite ; l'absence produit au plus un écart à expliquer — jamais une preuve, jamais un impossible | 7 |
| catalogue d'artefacts | sous-ensemble versionné de K ; entrées A-xx (signature, mécanisme, preuve, version, retrait) ; A-01 mise en forme | 8 |
| révision de K | locale par construction (minimalité + dépendances) ; propagation le long des dépendances réelles ; jamais vers les observations | 9 |
| dépendance Dep(h) | ensemble exact des (κ, ver) mobilisés, calculé depuis la dérivation ; dette d'arbitrage identifiée, comparée par inclusion | 10 |
| invariants | I13 explicitation, I14 minimalité, I15 innocuité descendante, I16 invariance propositionnelle | 1.2, 11 |
| non-conventions | score, probabilité, décision, identité, consensus, observation, pratique implicite | 12 |

**Ce que ce document ne fait volontairement pas** : adopter une seule convention concrète (ni équivalence, ni priorité, ni attente — seule A-01, déjà actée au 002, est mise en forme normative), arbitrer une contradiction réelle, intégrer la dette conventionnelle dans l'ordre de préférence, stratifier quoi que ce soit. La couche suivante — la stratification de l'identité logique (variante, version, branche, famille comme constructions effectives) — est l'objet du document 005, qui devra se conformer aux invariants I1–I16.
