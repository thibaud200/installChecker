# 000 — Fondements de l'identité

**Statut** : document fondateur de la phase « Identity Resolution Engine ». Référence conceptuelle de toutes les étapes suivantes.
**Périmètre** : définitions et propriétés mathématiques uniquement. Ce document ne contient ni algorithme, ni règle de calcul, ni architecture logicielle — toute proposition de ce type dans un document ultérieur devra être compatible avec les présentes définitions, ou les faire réviser explicitement.
**Question unique traitée** : *qu'est-ce qu'une identité logique déduite d'observations physiques ?*

---

## 1. Position du problème

### 1.1 L'axiome d'aveuglement

Le moteur d'identité ne connaît **ni logiciel, ni nom de produit, ni fichier**. Il ne voit jamais le disque. Son univers entier est le contenu de la base d'observations produite par le pipeline : des lignes dans des tables (`scan_observations`, `file_headers`, `pe_info`, `version_info`, `authenticode`, `msi_properties`, `appx_manifest`, et toute capacité future), append-only, où NULL signifie « rien n'a été observé » — jamais « inconnu » ni « erreur ».

Tout ce que le moteur affirmera devra être **déductible de ces lignes et d'elles seules**. C'est l'axiome fondateur :

> **A0 (aveuglement)** — Le domaine du moteur est l'ensemble des observations persistées. Aucune assertion du moteur ne peut dépendre d'une information absente de ce domaine.

### 1.2 L'exclusion du contexte

Certaines colonnes persistées décrivent le **contexte de l'acte d'observation**, pas l'objet observé : le chemin (`path`), et l'instant du scan (`scanned_at`). Le nom d'un fichier, son dossier, la date à laquelle on l'a regardé ne disent rien de ce qu'il *est* — le même octet-à-octet peut s'appeler `setup.exe` ou `a.bin`, être scanné hier ou demain.

> **A1 (indépendance au contexte)** — L'identité logique est déduite exclusivement des observations de **contenu**. `path`, `scanned_at` et l'ordre d'insertion (`observation_id` en tant qu'ordre) sont hors du domaine identitaire. Formellement : si deux bases ne diffèrent que par ces éléments, toute conclusion identitaire tirée de l'une doit être tirée de l'autre.

Conséquence immédiate : `size` et `sha256` appartiennent au domaine identitaire (ce sont des propriétés du contenu) ; `path` n'y appartient pas, même quand il « aiderait ».

### 1.3 La nature du problème

Le problème n'est pas un problème de recherche d'information (retrouver un nom), ni de classification supervisée (il n'existe pas de vérité terrain dans la base). C'est un problème d'**inférence sous information partielle** : construire, à partir de faits bruts fragmentaires, des hypothèses d'origine commune, avec un degré de confiance explicite et révisable.

---

## 2. Le donné : fichiers, observations, signaux

### 2.1 Fichier

**Définition 1 (fichier)** — Objet physique du monde extérieur : une suite d'octets à un emplacement, à un instant. Le fichier n'appartient **pas** au domaine du moteur. Il n'existe pour lui qu'à travers les observations qui en ont été faites. Deux conséquences :

- le moteur ne peut jamais « retourner voir » un fichier ; si une information manque, elle manque définitivement pour cet acte d'observation ;
- un même fichier peut avoir engendré plusieurs observations (re-scans), et des fichiers distincts peuvent avoir engendré des observations indistinguables (copies).

### 2.2 Observation

**Définition 2 (observation)** — Le résultat complet d'un acte du pipeline sur un fichier : l'uplet, indexé par un `observation_id`, des valeurs produites par **toutes** les capacités, chacune pouvant valoir ⊥ (NULL, absence).

Notation : soit 𝒞 l'ensemble des capacités (aujourd'hui : en-tête, PE, version, signature, MSI, AppX ; extensible). Une observation est

  ω = ( sha256, size, (v_c)\_{c∈𝒞} )  avec v_c ∈ Val_c ∪ {⊥}

et Ω désigne l'ensemble des observations persistées (l'état de la base, contexte exclu par A1).

Propriétés structurelles héritées du pipeline, que le moteur doit tenir pour acquises :

- **Immuabilité** : une observation ne se corrige jamais, ne se complète jamais (append-only). Le pipeline peut produire une *nouvelle* observation du même contenu ; l'ancienne demeure.
- **Invariant 1:1** : toute observation porte une ligne par capacité, même toute-⊥. L'absence est une donnée de plein droit, pas un trou.
- **Fidélité brute** : les valeurs sont celles retournées par les API, sans normalisation. Une observation est donc un **fait exact sur ce que l'extracteur a vu** — ce qui, on le verra (§ 2.4), n'est pas la même chose qu'un fait exact sur l'origine du contenu.

### 2.3 Signal

**Définition 3 (signal)** — Une valeur (ou une conjonction de valeurs) non-⊥ d'une observation, **considérée en tant qu'indice sur l'origine du contenu**.

La distinction observation/signal est le pivot de tout le document :

- l'**observation** est un fait brut, toujours vrai en tant que fait (« l'extracteur PE a lu `machine = 4b50` ») ;
- le **signal** est la promotion de ce fait au rang d'indice identitaire (« ce contenu serait un exécutable pour telle machine ») — et cette promotion **peut être trompeuse** alors même que l'observation est exacte.

L'exemple n'est pas théorique : la campagne corpus 1 a montré 20 observations où `pe_info.machine = '4b50'` (les octets « PK ») sur des archives ZIP, parce que l'API de lecture PE accepte des flux sans en-tête MZ et relit le début du ZIP comme un en-tête COFF. L'observation est fidèle ; le signal « c'est un PE » serait faux.

Tout signal possède deux qualités théoriques, définies ici sans mesure (aucune formule à ce stade) :

- **pouvoir discriminant** : combien d'origines distinctes le signal est capable de séparer. Un `sha256` sépare (quasi) tout ; un `subsystem = 0002` ne sépare presque rien ; un `UpgradeCode` MSI sépare des lignées de produits entières.
- **fiabilité** : le degré auquel la valeur reflète réellement l'origine du contenu plutôt qu'une déclaration, un accident ou un artefact. Une signature Authenticode est coûteuse à contrefaire ; un `ProductName` de VersionInfo est une chaîne libre écrite par n'importe qui ; un `machine` peut être un artefact de lecture.

Ces deux qualités sont **indépendantes** : un signal peut être très discriminant et peu fiable (un `ProductName` exotique), fiable et peu discriminant (un subsystem). Leur qualification systématique, capacité par capacité, relève d'un document ultérieur.

### 2.4 Typologie des états d'un signal

Pour un contenu donné et un attribut donné, quatre situations exhaustives :

| État | Définition | Exemple mesuré (corpus 1) |
|---|---|---|
| **présent** | valeur non-⊥, cohérente avec les autres | 84,1 % des observations portent un certificat |
| **absent** | ⊥ — rien observé ; ne prouve rien (ni « non signé », ni « pas un PE ») | 88,5 % sans VersionInfo ; les `.msp` sans propriétés MSI |
| **contradictoire** | deux signaux du même contenu pointent vers des origines incompatibles | possible entre VersionInfo et Authenticode ; non rencontré massivement au corpus 1 |
| **artefactuel** | observation exacte, indice faux | les 20 lignes `machine='4b50'` |

Le moteur doit raisonner sur les quatre états — en particulier, il doit pouvoir dire « cet indice est probablement artefactuel » **sans jamais corriger l'observation** (elle est immuable et fidèle ; c'est l'interprétation qui est en cause).

---

## 3. L'identité : jamais observée, toujours inférée

### 3.1 Deux identités de nature différente

**Définition 4 (identité matérielle)** — Deux observations sont matériellement identiques si leurs contenus sont les mêmes octets. Dans le domaine du moteur : même `sha256` (et même `size`, redondant sauf collision). Notée ≡ₘ.

≡ₘ est une **relation d'équivalence effective** : réflexive, symétrique, transitive, et *décidable par simple lecture de la base*. C'est la seule identité **observée**. Le corpus 1 en montre la réalité : 497 observations, 381 contenus distincts — 23,4 % de redondance purement matérielle.

**Définition 5 (identité logique)** — Hypothèse selon laquelle un ensemble de contenus sont des **matérialisations d'un même artefact édité** — « le même logiciel », au sens où son éditeur le présenterait. Notée ≡ₗ (relation hypothétique).

L'identité logique est plus grossière que la matérielle (≡ₘ ⊆ ≡ₗ : mêmes octets ⟹ même logiciel, à un cas limite près, cf. L5) — mais elle n'est **jamais une donnée**. Aucune colonne de la base ne la contient.

### 3.2 Pourquoi l'identité logique n'est pas observable

Trois raisons structurelles, indépendantes de la qualité du pipeline :

1. **Le lien contenu → produit est une convention externe.** « Firefox », « la version 2.3 », « l'édition Pro » sont des actes de nomination de l'éditeur, qui vivent hors du fichier (sites, catalogues, communication). Le fichier ne *porte* pas son identité ; il porte au mieux des inscriptions qui la *déclarent*.
2. **Les inscriptions internes sont déclaratives.** VersionInfo, propriétés MSI, manifeste AppX sont des chaînes écrites librement au moment du build : absentes (88,5 % des cas au corpus 1), erronées, mensongères, ou simplement non mises à jour.
3. **Même la preuve cryptographique ne prouve pas l'identité.** Un certificat Authenticode valide prouve *qui a signé ce contenu* — pas *ce qu'est* ce contenu. Microsoft signe des milliers de produits distincts avec le même sujet (192 contenus sous le même signataire au corpus 1). La signature borne l'origine, elle ne nomme pas le produit.

### 3.3 L'identité comme meilleure explication

Puisqu'elle n'est pas observable, l'identité logique a un statut épistémologique précis : c'est une **hypothèse abductive** — une explication postulée pour rendre compte des observations.

> **Définition 6 (identité comme hypothèse)** — Une identité logique h est une hypothèse d'origine commune portant sur un sous-ensemble d'observations. On dit que h **explique** ω si l'existence d'un artefact édité conforme à h rend attendues les valeurs (et les absences) de ω. Une identité est *retenue* quand elle explique les observations concernées **mieux que toute hypothèse concurrente** — c'est-à-dire en postulant moins de coïncidences, de falsifications ou d'accidents.

Trois conséquences définitoires :

- Une identité n'est jamais « vraie » : elle est *compatible*, *préférable*, ou *réfutée*.
- Une identité est toujours **relative à Ω** : elle est la meilleure explication *des observations actuelles*. Toute assertion identitaire est indexée par l'état de la base qui la fonde.
- Une identité est **défaisable** : une observation nouvelle (ou une capacité future) peut la détrôner. Ce n'est pas un défaut du moteur, c'est la nature de l'objet.

---

## 4. La hiérarchie identitaire

L'expérience du monde logiciel impose de distinguer plusieurs strates. Toutes sont des hypothèses (seule ≡ₘ est observée) ; elles diffèrent par leur **granularité**.

**Définition 7 (version)** — Sous-hypothèse d'une identité : un **état édité** de l'artefact, tel que l'éditeur l'a figé et diffusé à un moment de sa vie. Deux contenus « de même version » sont supposés fonctionnellement équivalents du point de vue de l'éditeur.

**Définition 8 (variante)** — Sous-hypothèse d'une version : des **matérialisations distinctes du même état édité**, destinées à des contextes de déploiement différents — architecture (x86 / x64 / arm64), langue, canal (stable / beta), format d'empaquetage (EXE / MSI / MSIX). Des variantes ont des `sha256` différents mais la même position dans la vie du produit.

**Définition 9 (branche)** — Une **lignée ordonnée de versions** d'une même identité, munie d'une relation de succession. L'ensemble des versions d'une identité n'est pas totalement ordonné : plusieurs branches coexistent (maintenance longue durée et courant principal, majeures parallèles). Mathématiquement : les versions d'une identité forment un **ordre partiel** ; une branche en est une **chaîne** maximale ou distinguée.

**Définition 10 (famille)** — Un ensemble d'**identités distinctes mais éditorialement apparentées** : suites (produit + runtime + outils), éditions (Community / Pro), déclinaisons. La parenté de famille n'est pas une équivalence fonctionnelle : c'est une **relation de parenté** (réflexive, symétrique, non nécessairement transitive) entre identités.

### 4.1 Structure d'ensemble

Les strates d'équivalence s'emboîtent en partitions de plus en plus grossières des contenus :

  ≡ₘ (contenu) ⊆ variante ⊆ version ⊆ identité

tandis que *branche* n'est pas une partition mais une **structure d'ordre à l'intérieur** d'une identité, et *famille* une **relation entre** identités.

| Strate | Nature mathématique | Observable ? |
|---|---|---|
| contenu (≡ₘ) | équivalence effective | **oui** (sha256) |
| variante | équivalence hypothétique, raffine la version | non |
| version | équivalence hypothétique, raffine l'identité | non |
| identité | équivalence hypothétique (classe maximale « même logiciel ») | non |
| branche | chaîne dans l'ordre partiel des versions d'une identité | non |
| famille | relation de parenté entre identités | non |

### 4.2 Frontières conventionnelles

Les frontières entre strates ne sont **pas découvrables** dans le cas général : la différence entre « nouvelle version » et « nouvelle variante », entre « nouvelle majeure » et « nouveau produit », relève de conventions d'éditeur non normalisées (numérotations incompatibles entre éditeurs, renommages commerciaux). Le moteur devra donc, à terme, expliciter ses propres conventions de découpage — ce seront des **décisions documentées**, pas des découvertes. Ce document se borne à l'acter.

---

## 5. Les niveaux de certitude

Toute assertion identitaire du moteur devra porter un niveau de certitude explicite. On définit ici l'échelle et ses propriétés — **aucune formule, aucun seuil, aucune méthode de calcul** : uniquement la sémantique des niveaux.

### 5.1 L'échelle

Pour une hypothèse h et l'état d'observations Ω :

- **Impossible** — Ω contient au moins une observation **incompatible** avec h : retenir h obligerait à nier un fait persisté. C'est le seul niveau **démontrable** : la réfutation est un constat, pas un jugement.
- **Possible** — h est compatible avec Ω, mais des hypothèses concurrentes le sont tout autant, et rien dans Ω ne les départage. h est une explication parmi d'autres.
- **Probable** — h est compatible avec Ω et **toute hypothèse concurrente exige de postuler des coïncidences, falsifications ou accidents dont Ω ne porte aucune trace**. h est la meilleure explication disponible, sans être hors d'atteinte.
- **Certaine** — niveau limite. Au sens strict, aucune identité *logique* n'est certaine (cf. § 5.2) ; le niveau existe dans l'échelle pour deux usages : l'identité **matérielle** (même sha256), et les cas où une **convention explicite** du moteur décidera d'assimiler « probable au-delà de tout doute raisonnable » à « certain ». Cette convention sera une décision future, tracée ; elle n'est pas définie ici.

L'échelle est **totalement ordonnée** : impossible < possible < probable < certaine.

### 5.2 Propriétés de l'échelle

- **Asymétrie réfutation/confirmation.** On peut *prouver* qu'une hypothèse est impossible (un fait la contredit) ; on ne peut jamais prouver qu'elle est vraie (les faits sont compatibles avec d'autres mondes : collision de hachage, certificat volé, métadonnées contrefaites — improbables, jamais impossibles). L'échelle monte par élimination des concurrents, pas par accumulation de preuves positives.
- **Relativité à Ω.** Un niveau de certitude n'a de sens qu'indexé par l'ensemble d'observations qui le fonde. « h est probable » signifie « h est probable *au vu de Ω* ».
- **Non-monotonie.** L'arrivée d'observations nouvelles peut faire **chuter** un niveau (une hypothèse probable devient impossible si un fait la contredit) comme le faire monter. Le moteur doit être conçu pour la **révision**, pas pour l'accrétion. Aucune conclusion n'est acquise.
- **Non-composabilité naïve.** Le niveau attaché à une conjonction d'hypothèses (identité + version + variante) n'est pas le niveau de son maillon le plus fort ; il ne peut excéder celui de son maillon le plus **faible**. On peut être probable sur l'identité et seulement possible sur la version : l'assertion composée est au mieux possible.
- **Le refus de conclure est une conclusion.** Quand Ω ne distingue aucune hypothèse, le résultat légitime est l'absence d'assignation (rester à « possible » pour toutes, ou ne rien affirmer). Un moteur qui conclut toujours est un moteur qui invente.

---

## 6. Propriétés mathématiques exigées du moteur

Les propriétés suivantes sont des **exigences de conception** : tout mécanisme futur devra les satisfaire, et toute impossibilité devra être documentée comme une révision du présent document.

> **P1 — Cohérence.** Le moteur n'affirme jamais deux assertions contradictoires au même instant sur le même Ω : une observation ne peut être rattachée avec certitude à deux identités disjointes ; une hypothèse ne peut être à la fois retenue et réfutée. Les strates doivent respecter l'emboîtement du § 4.1 (deux contenus de même variante ne peuvent être d'identités différentes).

> **P2 — Reproductibilité (déterminisme).** À Ω identique, conclusions identiques — quels que soient la machine, l'instant, et **l'ordre d'insertion des observations**. Corollaire de A1 : `observation_id` est un identifiant, pas un rang porteur de sens. Deux bases contenant les mêmes observations de contenu produisent les mêmes identités.

> **P3 — Stabilité (localité).** Une conclusion ne change que si une observation **pertinente pour elle** change. L'ajout d'observations sans rapport (un nouveau scan d'autres contenus) ne modifie aucune assertion existante. Pas d'effet global d'une donnée locale.

> **P4 — Réversibilité.** Les identités forment une **couche dérivée** : les observations restent la source de vérité, intactes et suffisantes pour reconstruire (ou réviser) toute conclusion. Aucune opération identitaire n'est destructive — pas de fusion irréversible, pas d'écrasement d'observation, pas de résumé qui remplace ses sources. Le droit à l'erreur est structurel : se tromper d'identité doit être réparable par simple recalcul.

> **P5 — Traçabilité (explicabilité).** Toute assertion identitaire référence explicitement : les observations (`observation_id`) qui la fondent, les signaux retenus, les hypothèses concurrentes écartées et le motif de leur écartement, et son niveau de certitude. « Pourquoi le moteur dit-il cela ? » doit toujours avoir une réponse lisible par un humain, reconstructible depuis la base.

> **P6 — Indépendance au contexte.** Reformulation opérationnelle de A1 : les conclusions sont invariantes par renommage, déplacement, re-scan à une autre date, et permutation de l'ordre des observations. Aucune règle future ne peut invoquer `path` ou `scanned_at` à des fins identitaires.

> **P7 — Moindre engagement.** En présence d'information insuffisante, le moteur choisit l'assertion la plus faible compatible avec Ω (cf. § 5.2, dernier point). Il préfère l'absence de conclusion à une conjecture non traçable. La complétude n'est pas un objectif ; la fausse assignation est le seul échec grave.

Remarque : la **non-monotonie** (§ 5.2) n'est pas une propriété exigée mais une propriété **subie**, conséquence du caractère abductif de l'identité. Les propriétés P1–P7 doivent tenir *à travers* les révisions : après toute révision, le nouvel état satisfait de nouveau P1–P7 relativement au nouveau Ω.

---

## 7. Limites théoriques

Ces limites ne sont pas des défauts corrigeables : elles bornent ce que *tout* moteur respectant A0–A1 peut conclure. Les connaître évite de les combattre.

**L1 — Sous-détermination (indiscernabilité).** Si deux hypothèses distinctes expliquent exactement aussi bien les mêmes observations, **aucun raisonnement interne ne peut les départager**. Pire : si deux contenus distincts présentent exactement les mêmes signaux (mêmes métadonnées, même signataire, tailles voisines), le moteur ne dispose d'aucun moyen de décider s'ils sont deux versions, deux variantes ou deux produits. Les observations induisent des **classes d'indiscernabilité** ; la résolution ne peut jamais être plus fine que ces classes. C'est le plancher absolu du système.

**L2 — L'observation vide.** Un contenu dont tous les signaux sont ⊥ (archive quelconque, fichier de données : 12 % du corpus 1 n'a ni certificat ni VersionInfo ni propriétés d'installateur) n'offre **aucune prise** à l'identité logique. Pour lui, seule l'identité matérielle existe : il est identique à lui-même, et c'est tout ce qu'on saura. Le moteur doit représenter dignement ce cas (« aucune hypothèse formulable ») plutôt que de le forcer dans une identité.

**L3 — Les signaux contradictoires.** Quand deux signaux fiables pointent vers des origines incompatibles (métadonnées déclarant A, signature prouvant B), aucune règle *interne aux observations* ne tranche : il faudra une **convention de priorité entre capacités** — c'est-à-dire une décision de conception, documentée et révisable, pas une découverte. Le présent document acte seulement que le cas existe et qu'il n'a pas de solution neutre.

**L4 — Les artefacts d'observation.** Une observation exacte peut porter un signal faux (les `machine='4b50'` du corpus 1). Le moteur devra pouvoir *expliquer* un signal comme artefactuel — hypothèse parmi d'autres, soumise aux mêmes niveaux de certitude — sans jamais toucher à l'observation. Limite associée : rien ne garantit que tous les artefacts soient connus ; une capacité future peut en révéler de nouveaux.

**L5 — Matériel et logique ne coïncident dans aucun des deux sens.**
- *Contenus distincts, même position logique* : recompilation, re-signature, horodatage interne → deux sha256 pour « la même version ». C'est le cas normal, le moteur existe pour lui.
- *Contenu identique, identités logiques distinctes* : distribution rebadgée (white-label), même binaire vendu sous deux noms. Les observations étant strictement identiques, ce cas est **indiscernable de l'intérieur** (cas particulier de L1) : le moteur conclura légitimement « même identité » et se trompera — irréfutablement au vu de Ω. À documenter comme risque résiduel accepté, pas à résoudre.

**L6 — Les frontières de strates sont conventionnelles.** Aucune observation ne dit si un écart entre deux contenus est un patch, une version, une variante ou un produit nouveau (§ 4.2). Toute stratification effective reposera sur des conventions choisies ; deux conventions raisonnables peuvent produire deux découpages différents des mêmes observations, tous deux cohérents.

**L7 — Le socle est probabiliste.** L'identité matérielle elle-même repose sur SHA-256 (collision de probabilité non nulle) ; la fiabilité d'Authenticode repose sur la non-compromission des clés. « Certain » est donc toujours, en dernière analyse, conventionnel — l'échelle du § 5 l'assume explicitement au lieu de le cacher.

**L8 — Ouverture du domaine.** 𝒞 est extensible : toute capacité future enrichit Ω et peut réviser n'importe quelle conclusion (non-monotonie). Le corpus mesuré borne en outre ce qui est *déjà* observable : au corpus 1, la capacité AppX n'a produit aucun signal (0 manifeste) — toute théorie de l'identité des paquets MSIX reste, à ce jour, empiriquement non ancrée.

---

## 8. Ancrage empirique

Les définitions ci-dessus ne sont pas spéculatives : chacune répond à un fait mesuré (campagne corpus 1, `docs/mesures/`).

| Fait mesuré (497 observations) | Concept fondé |
|---|---|
| 381 contenus distincts (23,4 % de redondance) | ≡ₘ non triviale ; la déduplication matérielle est observable, l'identité logique commence après elle |
| 88,5 % sans VersionInfo ; quand présent, les 4 champs le sont ensemble | l'absence est l'état majoritaire d'un signal déclaratif ; ⊥ est une donnée |
| 84,1 % avec certificat ; 192 contenus sous un même signataire | signal fiable mais peu discriminant seul : borne l'origine, ne nomme pas le produit |
| 10 `.msp` : conteneur ole-cfb présent, propriétés MSI toutes ⊥ | présence d'un conteneur ≠ présence des signaux qu'on y attend |
| 20 lignes `machine='4b50'` sur des ZIP | observation exacte ≠ signal véridique (artefact, L4) |
| 0 manifeste AppX sur 59 ZIP | domaine empiriquement non couvert (L8) |
| fichier de 5,9 Gio : hash + taille + conteneur, tout le reste ⊥ | observation quasi vide : l'identité logique peut n'avoir aucune prise (L2) |

---

## 9. Récapitulatif des objets

| Objet | Statut | Défini en |
|---|---|---|
| fichier | physique, hors domaine | Déf. 1 |
| observation ω, base Ω | fait brut persisté, immuable, contexte exclu | Déf. 2, A0–A1 |
| signal | valeur promue au rang d'indice ; discriminance × fiabilité ; 4 états (présent/absent/contradictoire/artefactuel) | Déf. 3 |
| identité matérielle ≡ₘ | équivalence effective, seule identité observée | Déf. 4 |
| identité logique ≡ₗ | hypothèse abductive : meilleure explication d'origine commune | Déf. 5–6 |
| version / variante | équivalences hypothétiques emboîtées sous l'identité | Déf. 7–8 |
| branche | chaîne dans l'ordre partiel des versions | Déf. 9 |
| famille | relation de parenté entre identités | Déf. 10 |
| niveaux de certitude | échelle ordinale {impossible < possible < probable < certaine}, relative à Ω, non monotone | § 5 |
| exigences du moteur | P1 cohérence, P2 reproductibilité, P3 stabilité, P4 réversibilité, P5 traçabilité, P6 indépendance au contexte, P7 moindre engagement | § 6 |
| limites | L1 sous-détermination … L8 ouverture | § 7 |

**Ce que ce document ne fait volontairement pas** : qualifier un à un les signaux des capacités existantes, définir les conventions de priorité (L3) et de stratification (L6), choisir la convention du niveau « certain » (§ 5.1), et a fortiori tout mécanisme de calcul. Ce sont les objets des documents suivants de la série `docs/identity/`, qui devront se conformer aux définitions et propriétés posées ici.
