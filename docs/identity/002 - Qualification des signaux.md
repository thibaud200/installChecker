# 002 — Qualification des signaux

**Statut** : troisième document de la série `docs/identity/`. S'appuie sur les documents 000 (« Fondements de l'identité ») et 001 (« Théorie des observations »), validés et figés.
**Périmètre** : définition de la couche *interprétation* et de la couche *signal* de la chaîne fondamentale (001 § 5). Aucun algorithme, aucune pondération, aucun score, aucune priorité, aucune décision métier. Ce document définit la **nature** des signaux — jamais leur importance.
**Rappel de stratification (001)** : une observation ne peut être contradictoire ou artefactuelle *par elle-même* ; ces statuts n'apparaissent que lorsqu'une couche supérieure interprète les observations. Le présent document **est** cette couche supérieure : c'est ici que ces statuts prennent naissance.

---

## 1. Le signal, objet formel

### 1.1 Définition

**Définition 1 (type de signal)** — Un *type de signal* est un quadruplet documenté

  σ = ( D_σ, P_σ, f_σ, K_σ )

où :

- **D_σ (domaine d'entrée)** — la description des observations élémentaires consommées : quels attributs, de quels types (P/S/D/C au sens de 001 § 3), sur combien d'actes (§ 7.1) ;
- **P_σ (préconditions)** — les conditions que ces observations doivent satisfaire pour que le signal soit *défini* (présence de valeurs, appartenance à un sous-domaine). Si P_σ n'est pas satisfaite, **le signal n'existe pas** — il n'est ni faux, ni vide : il est non défini ;
- **f_σ (fonction)** — une fonction **totale sur P_σ et déterministe** qui produit la sortie ;
- **K_σ (convention)** — la référence explicite à la ou aux conventions d'interprétation utilisées (§ 2), avec leur version.

**Définition 2 (instance de signal)** — s = σ(O), la valeur produite par f_σ sur un ensemble concret d'observations O ⊆ Ω satisfaisant P_σ, munie de sa **provenance complète** : l'ensemble exact des observations élémentaires consommées (leurs `observation_id` et attributs) et la référence K_σ.

### 1.2 Le signal ne crée pas d'information

f_σ étant une fonction de ses seules entrées (plus la convention K_σ, fixe et documentée), un signal **n'ajoute rien** à Ω : il *explicite* une relation déjà contenue dans les observations, sous une forme exploitable. Corollaires, imposés par le prompt fondateur et démontrables depuis la définition :

- deux ensembles d'observations identiques produisent toujours la même instance de signal (déterminisme de f_σ) ;
- un même signal peut être construit à partir d'observations différentes (f_σ n'est pas nécessairement injective : deux contenus distincts peuvent porter le même sujet signataire) ;
- un signal peut **disparaître alors que les observations restent vraies** : il suffit que K_σ soit révisée (la convention change) ou que σ soit retiré du répertoire. Les observations, elles, sont intangibles (I1–I2).

### 1.3 Non-persistance

> **I5 — Reconstructibilité (invariant).** Un signal n'a **aucune existence propre** : l'intégralité des signaux est reconstructible à partir des seules observations persistées et du répertoire des types de signaux. Le stockage d'une instance de signal est une optimisation technique éventuelle (cache), **jamais une nécessité théorique** ; un signal stocké qui ne serait plus reconstructible à l'identique depuis Ω est invalide par définition.

C'est la traduction, à cette couche, de la règle « la vérité reste en bas » (001 § 5) : Ω plus le répertoire des conventions suffisent à régénérer tout l'étage.

---

## 2. L'interprétation : une convention, jamais une découverte

**Définition 3 (convention d'interprétation)** — Une application documentée et versionnée qui associe à des valeurs brutes d'un attribut une propriété conventionnelle exploitable. Exemples canoniques :

| Observation (fait brut) | Convention appliquée | Propriété conventionnelle |
|---|---|---|
| `machine = '8664'` | table des codes machine du format COFF/PE | « architecture cible x86-64 » |
| `magic_hex` commençant par `4d5a` | spécification du format PE (« MZ ») | « conteneur PE » |
| `subject = 'CN=…, O=…'` | syntaxe des noms distingués X.500 | « éditeur **déclaré** au certificat » |
| `product_version = '17.7.40001'` | convention de numérotation (à définir) | « déclaration de version, structure a.b.c.d » |

Trois propriétés définitoires :

1. **Extériorité.** La convention n'est pas dans l'observation : elle vit dans une spécification de format, un registre public, un usage — c'est-à-dire hors de Ω. La table des codes machine appartient à la spécification PE, pas au fichier observé. En conséquence, une convention peut être **erronée, incomplète ou périmée** indépendamment de toute observation.
2. **Décision, pas découverte.** Adopter une convention est un choix de conception, documenté et révisable — jamais un fait constaté. Deux conventions raisonnables peuvent différer (formats de version, notamment) ; le choix entre elles est un arbitrage, hors du périmètre de ce document.
3. **Versionnement.** Toute convention est identifiée et versionnée (composante K_σ). La révision d'une convention est l'un des deux événements pouvant réviser tout l'étage des signaux — l'autre étant l'arrivée d'observations nouvelles (non-monotonie, 000 § 5.2).

La couche interprétation est **le premier acte faillible** du système (001 § 5, règle 4). L'erreur d'interprétation existe ; elle se corrige en révisant la convention, jamais en touchant aux observations.

---

## 3. Les qualités intrinsèques d'un signal

Le document 000 (§ 2.3) a posé deux axes. On les définit ici mathématiquement — c'est-à-dire par leurs structures, **sans métrique, sans pondération, sans classement**.

### 3.1 Pouvoir discriminant

Tout type de signal σ induit sur les actes d'observation qui satisfont P_σ une **relation d'équivalence** : deux actes sont σ-équivalents si f_σ y produit la même sortie. Cette relation induit une **partition** des actes concernés.

**Définition 4 (pouvoir discriminant)** — Le pouvoir discriminant de σ est la **finesse de la partition induite**, rapportée à la partition visée (celle des identités logiques). Il se compare, il ne se mesure pas : σ est *plus discriminant* que σ′ si sa partition raffine celle de σ′ sur le même domaine.

- Partition la plus fine du système : celle du contenu (`sha256`) — chaque classe est (quasi) un singleton de contenus.
- Partitions grossières : `container` (trois classes au corpus 1), `subsystem` (deux classes non-⊥).
- Aucune mesure n'est définie ici ; seul l'ordre partiel « raffine » est posé.

### 3.2 Fiabilité

**Définition 5 (fiabilité)** — La fiabilité de σ est le degré de **conformité entre la sémantique annoncée par sa convention et les mécanismes réels de production des valeurs consommées** : un signal est fiable dans la mesure où, quand il est défini, sa sortie reflète la propriété qu'il prétend expliciter — plutôt qu'une déclaration libre (type D), un accident, ou une lecture hors domaine nominal (artefact, § 8).

La fiabilité est une propriété **qualitative et comparative**, ancrée dans la typologie de 001 § 3 : elle découle du *garant* des observations consommées (mathématiques > engagement cryptographique > convention de format > sincérité du producteur), sans que cet ordre constitue un score — c'est un ordre de nature, pas de valeur numérique.

### 3.3 Indépendance des deux axes

Les axes sont indépendants : aucune des quatre combinaisons n'est vide.

| | **Fiable** | **Peu fiable** |
|---|---|---|
| **Discriminant** | condensat de contenu (`sha256`) : sépare tout, garanti par les mathématiques | déclaration de nom de produit exotique (`product_name` rare) : sépare beaucoup, ne garantit rien |
| **Peu discriminant** | « conteneur PE » (magic `4d5a`) : fait structurel robuste, presque tout un parc en relève | déclaration d'entreprise générique (`company_name = 'Microsoft Corporation'`) : chaîne libre partagée par des milliers de contenus |

La **qualification** d'un signal, au sens du présent document, est la donnée de sa position qualitative sur ces deux axes, de ses préconditions, de son régime face à l'absence, et de ses conditions d'artefact connues. Rien de plus.

---

## 4. Qualification des familles de signaux du pipeline actuel

Pour chaque capacité existante : la nature des observations consommées, les signaux constructibles, leur position qualitative, et le régime mesuré au corpus 1. **Aucun score, aucun ordre entre familles.**

### 4.1 Contenu (`sha256`, `size`) — type P

- Signaux constructibles : « contenu identique » (relationnel, § 7.1) ; « taille identique » (relationnel, faible).
- Nature : discriminance maximale du système, fiabilité maximale (garant mathématique). Seule famille dont l'interprétation est quasi transparente (le condensat *est* la propriété).
- Régime : jamais absent (le pipeline échoue le fichier entier sinon). Corpus 1 : 497 actes, 381 classes de contenu.

### 4.2 En-tête (`magic_hex`, `container`) — type S

- Signaux constructibles : « conteneur déclaré par les octets » (PE / OLE-CFB / ZIP).
- Nature : fiabilité élevée en tant que fait structurel (les octets sont là), discriminance très faible (trois classes). **Ambiguïté constitutive** : un même conteneur porte plusieurs familles logiques — OLE-CFB recouvre MSI *et* MSP (mesuré : 376 ole-cfb = 366 + 10) ; ZIP recouvre archives, paquets AppX, formats bureautiques. Le signal dit ce que les octets *sont*, pas ce que le contenu *représente* (philosophie du pipeline, reprise à cette couche).
- Régime : `container` peut être ⊥ (aucun des trois motifs) — 0 cas au corpus 1, corpus biaisé installateurs.

### 4.3 Structure PE (`machine`, `subsystem`, `characteristics`, `timestamp`, `optional_header_magic`) — type S

- Signaux constructibles : « architecture cible » (via table des codes machine), « sous-système cible », « en-tête optionnel présent ».
- Nature : discriminance faible à moyenne (quelques classes) ; fiabilité **conditionnelle** — élevée dans le domaine nominal (le contenu est effectivement un PE), nulle hors domaine : c'est la famille porteuse du premier artefact catalogué (§ 8.3, `machine='4b50'`). La précondition « domaine nominal » n'est **pas observable directement** ; elle est elle-même une interprétation (co-lecture avec d'autres observations) — le signal doit déclarer cette dépendance dans P_σ.
- Régime corpus 1 : 81 actes avec `machine` non-⊥, dont 61 concordants avec un conteneur PE et 20 artefactuels.

### 4.4 VersionInfo (`product_name`, `company_name`, `product_version`, `file_version`) — type D

- Signaux constructibles : « nom de produit déclaré », « entreprise déclarée », « version déclarée ».
- Nature : discriminance potentiellement forte (chaînes riches), fiabilité structurellement faible (garant : la sincérité du producteur — c'est-à-dire rien, 001 § 3). Toute montée en confiance viendra de la **corroboration** par d'autres signaux, objet des couches supérieures.
- Régime : l'absence est l'état majoritaire (88,5 % au corpus 1) ; quand la ressource existe, les quatre attributs sont présents ensemble (fait mesuré — régularité empirique, pas garantie de format).

### 4.5 Authenticode (`subject`, `issuer`, `serial_number`, `thumbprint`, `not_before`, `not_after`) — type C

- Signaux constructibles : « signataire déclaré » (sujet interprété en nom distingué), « même signataire » (relationnel), « autorité émettrice », « fenêtre de validité déclarée », « même certificat exact » (`thumbprint`, relationnel — discriminance forte : il identifie *le certificat*, pas le produit).
- Nature : fiabilité élevée *sur ce qu'il prouve réellement* — l'acte de signature de ce contenu par le détenteur de la clé — et c'est tout : la discriminance vers le **produit** est faible (mesuré : 192 contenus sous un même sujet Microsoft, 59 sous Python Software Foundation, couvrant produits, versions et variantes multiples). Confondre « même signataire » et « même logiciel » serait une erreur d'interprétation type.
- Régime : présent à 84,1 % au corpus 1 (biais installateurs) ; l'absence ne signifie jamais « non signé » (catalogues — 001 § 4.2).

### 4.6 Propriétés MSI (`product_name`, `product_version`, `manufacturer`, `product_code`, `upgrade_code`, `product_language`) — type D

- Signaux constructibles : « déclarations d'installateur » ; deux méritent mention à part — `product_code` (déclaré unique par *version empaquetée*) et `upgrade_code` (déclaré stable par *lignée de produit*) : conventions du format Windows Installer, donc discriminance déclarée forte et **structurée** (le format lui-même prétend distinguer version et lignée — matière première évidente pour les strates du 000 § 4, sans qu'on en décide rien ici).
- Nature : type D malgré cette structure — la garantie reste la discipline du producteur ; les GUID peuvent être réutilisés, dupliqués, régénérés à tort.
- Régime corpus 1 : tout-ou-rien (366 actes avec les six propriétés, 130 sans aucune) ; les 10 `.msp` illustrent l'ambiguïté du conteneur (§ 4.2), pas une contradiction.

### 4.7 Manifeste AppX (`name`, `publisher`, `version`, `processor_architecture`) — type D

- Signaux constructibles : « identité de paquet déclarée » (le format impose la présence des attributs dans le manifeste).
- Nature : type D à garant renforcé *dans l'écosystème de distribution* (les stores vérifient la concordance publisher/signature) — mais cette vérification est extérieure à Ω : au niveau du signal, cela reste une déclaration.
- Régime : **empiriquement non ancré** — 0 manifeste au corpus 1 (000, L8). Toute qualification fine est reportée à un corpus qui en contient.

---

## 5. Les régimes d'un signal

Pour une instance donnée (un type σ appliqué à des observations concrètes), cinq régimes exhaustifs. Chacun est relié aux états des observations (001 § 4).

**R1 — Exact.** P_σ satisfaite, toutes les observations consommées *présentes* (001 § 4.1), convention appliquée dans son domaine nominal, sortie définie. Le régime nominal.

**R2 — Incomplet.** Au moins une observation du domaine D_σ vaut ⊥. Deux sous-cas hérités de 001 § 4.3 : l'absence réelle et l'illisible projeté — **indiscernables à jamais** dans Ω. Chaque type de signal doit déclarer dans P_σ son comportement face à ⊥ : soit le signal est *non défini* (cas général), soit il se définit explicitement sur l'absence (« aucune déclaration de version ») — auquel cas sa sortie assume l'ambiguïté absent/illisible et ne peut jamais valoir preuve d'inexistence.

**R3 — Ambigu.** P_σ satisfaite, sortie définie, mais la convention K_σ associe à cette sortie **plusieurs propriétés candidates sans moyen interne de choisir**. Cas mesuré : `container='ole-cfb'` → { base MSI, patch MSP, autres formats OLE } ; les 10 `.msp` du corpus 1 rendent cette ambiguïté concrète. L'ambiguïté est une propriété *de la convention* (non-injectivité sémantique), pas des observations.

**R4 — Contradictoire.** Les observations consommées (ou ce signal et un autre — § 9) violent l'attente d'origine unique portée par la convention. Statut **relationnel et assigné** (001 § 4.4) : aucun rapport de lecture n'est faux ; la conjonction est inattendue. La résolution est explicitement hors périmètre (conventions de priorité, 000 L3).

**R5 — Artefactuel.** Le signal est produit conformément à sa méthode et à sa convention, mais la méthode a opéré **hors de son domaine nominal** : la sortie décrit le comportement du mécanisme de lecture, pas l'objet logique recherché (§ 8).

Un signal en régime R2–R5 **n'est pas supprimé ni corrigé** : son régime fait partie de son instance (provenance comprise) et sera une donnée d'entrée des couches supérieures.

---

## 6. Les équivalences interprétatives

### 6.1 Définition

Le document 001 (§ 2) fixe que les observations ne connaissent que l'égalité **byte-à-byte**. Toute comparaison plus souple est un acte d'interprétation :

**Définition 6 (équivalence interprétative)** — Une relation ≈ sur un domaine de valeurs Val(a) (ou sur les sorties d'un type de signal), **définie par une convention documentée et versionnée**, qui est une véritable relation d'équivalence — réflexive, symétrique, **transitive** — et qui est plus grossière que l'égalité byte-à-byte (x = y ⟹ x ≈ y).

Exemples de conventions candidates (données comme objets possibles, **sans en retenir aucune**) : insensibilité à la casse ; normalisation Unicode (NFC/NFD) ; neutralisation des espaces ; égalité de structure sur des formats de version (a.b.c.d) ; unification d'encodages.

### 6.2 Propriétés exigées

- **Transitivité obligatoire.** Une « similarité » non transitive (distance d'édition sous seuil, ressemblance approximative) **n'est pas une équivalence** et n'a pas sa place à cette couche : elle induirait des classes mal définies. Si de telles relations servent un jour, ce sera comme objets d'une couche supérieure, sous un autre nom et un autre statut.
- **Grossièreté contrôlée.** Une équivalence ne peut que fusionner des classes (jamais distinguer davantage que le byte-à-byte) ; chaque fusion est une **perte de discriminance assumée**, qui doit être documentée dans K_σ.
- **Déterminisme et traçabilité.** ≈ est calculable de façon déterministe ; toute instance de signal qui l'emploie référence sa convention et sa version.
- **Localité.** Une équivalence est définie par attribut ou par domaine — il n'existe pas d'équivalence « universelle » des chaînes.
- **Aucune décision ici.** Le présent document ne retient, n'écarte ni ne recommande aucune équivalence : il définit l'objet et ses lois. Le choix effectif sera une décision documentée d'une couche ultérieure.

---

## 7. Les signaux composites

### 7.1 Arité et portée

**Définition 7 (signal unaire / composite / relationnel)** —

- *Unaire simple* : consomme une seule observation élémentaire (« conteneur déclaré »).
- *Composite* : consomme plusieurs observations élémentaires **d'un même acte** (« architecture cible » consommant `machine` *et* la co-lecture de `container` et `optional_header_magic` pour établir son domaine nominal).
- *Relationnel* : consomme des observations de **plusieurs actes** et explicite une relation entre eux (« contenu identique » entre deux actes ; « même signataire » ; « même `upgrade_code` déclaré »). Les signaux relationnels sont la matière première du futur travail de regroupement (003) — ici, seul l'objet est défini.

### 7.2 Lois des composites

- **Provenance** : la provenance d'un composite est l'**union exacte** des provenances élémentaires. Un signal composite **ne masque jamais ses observations d'origine** (extension de I4) : depuis toute instance, la liste complète des `observation_id` et attributs consommés est restituable.
- **Traçabilité** : la justification d'un composite inclut celle de chacune de ses composantes plus la convention de composition.
- **Stabilité** : un composite est stable si et seulement si toutes ses composantes le sont ; il hérite du **pire régime** de ses composantes (un composite dont une entrée est incomplète est incomplet ; dont une entrée est artefactuelle est suspect d'artefact). Aucun composite ne peut être « plus sûr » que sa composante la plus fragile — écho de la non-composabilité naïve (000 § 5.2).
- **Réversibilité** : composer n'est pas fusionner. Les observations composées restent distinctes et intangibles (I1) ; le composite est décomposable, recalculable, jetable (I5).

---

## 8. Les artefacts

### 8.1 Définition

**Définition 8 (artefact)** — Un signal produit **conformément à sa méthode et à sa convention**, mais dont la méthode d'extraction a opéré hors de son domaine nominal, de sorte que la sortie **ne décrit pas correctement l'objet logique recherché**. Un artefact n'est pas une erreur : chaque maillon a fonctionné comme spécifié ; c'est la *rencontre* entre un mécanisme permissif et un contenu hors domaine qui produit un indice sans valeur.

### 8.2 Distinction des quatre situations voisines

| Situation | Où est le défaut | Ce qui est faux | Remède (nature, pas procédure) |
|---|---|---|---|
| **Erreur d'observation** | la méthode d'extraction viole sa propre spécification (bug) | le rapport de lecture lui-même | bug réel du pipeline — seul cas autorisant une modification du code (précédent : A1) |
| **Erreur d'interprétation** | la convention K_σ est fausse ou mal appliquée (mauvaise table de codes, équivalence trop grossière) | la propriété conventionnelle déduite | révision de la convention, re-dérivation des signaux (I5) |
| **Artefact** | la méthode a opéré hors domaine nominal ; méthode et convention sont conformes | la **pertinence** du signal (le rapport et la convention restent exacts) | catalogage : la condition d'artefact devient une précondition négative documentée du type de signal |
| **Contradiction** | nulle part — tous les rapports sont fidèles, toutes les conventions conformes | rien ; c'est l'attente d'origine unique qui est violée (fait sur le monde) | aucune à cette couche ; matière des conventions de priorité futures (000 L3) |

### 8.3 Le catalogue des artefacts

Tout type de signal doit documenter ses **conditions d'artefact connues** ; le catalogue est ouvert (000 L4 : rien ne garantit que tous soient connus).

**Entrée A-01 (première entrée, mesurée)** — *Lecture COFF de flux non-PE.* La méthode de lecture PE accepte des flux sans en-tête MZ et interprète les premiers octets comme un en-tête COFF. Signature observationnelle au corpus 1 : `machine='4b50'` (octets « PK »), `characteristics` et `timestamp` porteurs de fragments de structure ZIP, `subsystem` et `optional_header_magic` ⊥, co-présence de `container='zip'` ; 20 actes. Conséquence pour la qualification : tout signal fondé sur la structure PE porte la précondition négative « hors condition A-01 » — et cette précondition, étant elle-même une interprétation (co-lecture d'observations), est faillible et tracée comme telle.

Le statut artefactuel d'une instance reste une **hypothèse** (001 § 4.5) : le catalogue nomme des conditions, il ne certifie pas leur exhaustivité ni leur infaillibilité.

---

## 9. Les contradictions

Trois objets distincts, définis sans aucune résolution :

**Définition 9 (contradiction intra-observation)** — Entre observations élémentaires **d'un même acte** : la conjonction des valeurs viole l'attente d'origine unique au sein d'un même contenu. Exemple type (voisin du mesuré) : un conteneur déclaré ZIP co-présent avec des champs PE renseignés — configuration dont la lecture naturelle est l'artefact A-01, mais qui, avant ce diagnostic, se présente comme une contradiction intra-acte.

**Définition 10 (contradiction inter-observations)** — Entre observations élémentaires **d'actes distincts portant sur le même contenu** (même classe ≡ₘ) : deux lectures du même contenu produisent des valeurs incompatibles. Théoriquement possible si une méthode dépend de l'environnement (réserve de stabilité des déclaratifs, 001 § 2) ; aucun cas mesuré au corpus 1 (reproductibilité PASS sur double run à environnement constant). Ce serait le symptôme d'une instabilité de méthode — à distinguer d'une erreur d'observation.

**Définition 11 (contradiction inter-signaux)** — Entre **instances de signaux** portant sur le même acte ou la même classe de contenu : deux propriétés conventionnelles incompatibles sous origine unique. Cas d'école : « entreprise déclarée » (VersionInfo, type D) désignant A tandis que « signataire » (Authenticode, type C) désigne B. Les deux signaux sont exacts en leur régime ; la contradiction est un fait composé, qui devra être **représenté** (avec provenance des deux côtés) et non résolu — la résolution exigera les conventions de priorité, explicitement reportées.

Propriété commune : une contradiction est toujours **relative à une attente conventionnelle** (« ce qu'une origine unique produirait ») ; elle hérite donc du statut des conventions — décidée, documentée, révisable — et n'invalide jamais aucun de ses constituants.

---

## 10. Propriétés mathématiques des signaux — invariants

Dans la continuité des invariants I1–I4 (001) et I5 (§ 1.3) :

> **I6 — Déterminisme conventionnel.** À observations identiques et répertoire de conventions identique (types, versions), les signaux produits sont identiques — quels que soient l'instant, la machine et l'ordre de reconstruction. La révision d'une convention est le seul événement, avec l'arrivée d'observations, qui change l'étage des signaux ; elle est datée et tracée.

> **I7 — Transparence de provenance.** Aucune instance de signal ne masque ses origines : la chaîne signal → observations élémentaires → actes est intégralement restituable, y compris à travers les composites (§ 7.2).

> **I8 — Indépendance contextuelle.** Aucun type de signal ne consomme d'observation de type X (`path`, `scanned_at`, ordre des actes) à des fins identitaires — reformulation à cette couche de A1/P6 (000).

> **I9 — Neutralité.** Aucun signal ne porte de poids, de score, de probabilité ni de priorité. La qualification (axes du § 3, régimes du § 5, conditions d'artefact du § 8) est une description de nature, jamais une valeur. Toute pondération éventuelle appartiendra aux couches supérieures et n'est définie nulle part à ce jour.

S'y ajoutent, hérités par construction : reproductibilité (conséquence de I6 et de la reproductibilité des observations), réversibilité (I5 : re-dérivation intégrale), et le rappel que les signaux sont **non persistés par nature** (I5).

---

## 11. Ce qui n'est PAS un signal

| Notion | Pourquoi ce n'est pas un signal | Couche |
|---|---|---|
| identité | c'est l'hypothèse retenue — le résultat final | identité (000 § 3) |
| score, probabilité, poids | les signaux sont neutres (I9) ; aucune quantification n'existe à cette couche | non définie à ce jour |
| décision | un signal explicite, il ne choisit jamais | couches supérieures |
| appartenance à une famille | relation entre identités (000 Déf. 10) | construction sur identités |
| version retenue | strate hypothétique arbitrée | hypothèse (003+) |
| doublon | « contenu identique » est un signal ; « doublon » ajoute un jugement de redondance et d'intention | hypothèse / décision |
| consensus, corroboration | combinaisons d'hypothèses concurrentes | 003 |
| « ce fichier est un installateur » | jugement fonctionnel composé, non réductible à une convention de lecture unique | hypothèse |

Règle générale (miroir de 001 § 7) : **tout ce qui choisit, quantifie, regroupe ou conclut n'est pas un signal.** Un signal explicite une relation ; il ne prend jamais parti.

---

## 12. Exemples — corpus 1 exclusivement

Chaque exemple sépare strictement les trois plans : *observations (faits) → signal (interprétation conventionnelle) → ce qui reste interdit ici*.

**E1 — L'artefact `4b50`** (20 actes).
Observations : `machine='4b50'`, `subsystem=⊥`, `optional_header_magic=⊥`, `container='zip'` — toutes exactes. Signal « architecture cible » : précondition « domaine nominal PE » non établie, condition A-01 reconnue → instance en régime **R5 (artefactuel)**, provenance complète conservée. Interdit ici : supprimer les observations, corriger `machine`, ou décider que « les ZIP n'ont pas d'architecture » (règle métier).

**E2 — VersionInfo absent** (439 actes sur 496 hors fichier géant).
Observations : quatre attributs ⊥ après lecture nominale. Signal « version déclarée » : précondition non satisfaite → **non défini** (pas « version inconnue », pas « pas de version ») ; s'il existe un type de signal défini sur l'absence, sa sortie porte l'ambiguïté irréductible absent/illisible (R2). Interdit ici : traiter l'absence comme information négative sur le contenu.

**E3 — Les 10 `.msp` en OLE-CFB.**
Observations : `container='ole-cfb'` présent, six propriétés MSI ⊥. Signal « conteneur déclaré » : exact mais **ambigu (R3)** — OLE-CFB recouvre plusieurs formats ; signal « déclarations d'installateur » : non défini. Aucune contradiction : seule une attente naïve « ole-cfb ⟹ propriétés MSI » en fabriquerait une (001, E7). Interdit ici : conclure « ce sont des patchs » (interprétation fonctionnelle non couverte par une convention de lecture).

**E4 — Certificats sans ProductName** (360 ole-cfb signés, 0 avec VersionInfo).
Observations : `subject` présent, `product_name` (VersionInfo) ⊥. Signaux : « signataire » exact ; « nom de produit déclaré » non défini. Les deux familles sont indépendantes (001 § 2) ; leur co-régime est une configuration normale, pas un défaut. Interdit ici : promouvoir le signataire en « éditeur du produit » — c'est un saut de couche (le signal dit *qui a signé*, l'éditeur est une hypothèse).

**E5 — SHA256 identiques** (381 classes pour 497 actes ; maximum 3 actes pour un même contenu).
Observations : condensats égaux byte-à-byte entre actes. Signal relationnel « contenu identique » : exact, discriminance maximale, fiabilité maximale — l'unique signal dont la couche hypothèse pourra faire un « certain » conventionnel (000 § 5.1). Interdit ici : « doublon » (jugement de redondance), « à supprimer » (décision).

**E6 — Contradiction inter-signaux potentielle** (aucun cas massif mesuré ; construit sur les objets réels).
Observations : `company_name` (type D) déclarant une organisation, `subject` (type C) en désignant une autre — deux rapports exacts. Signaux : « entreprise déclarée » et « signataire », tous deux exacts en régime, formant une **contradiction inter-signaux (Déf. 11)**. Le système la représente avec les deux provenances ; il ne la tranche pas. Interdit ici : toute règle du type « la signature l'emporte » — c'est une convention de priorité, décision future et documentée.

---

## 13. Récapitulatif

| Objet | Définition | § |
|---|---|---|
| type de signal σ = (D_σ, P_σ, f_σ, K_σ) | fonction déterministe et documentée sur des observations | 1.1 |
| instance de signal | sortie + provenance complète + convention versionnée | 1.1 |
| interprétation | convention extérieure, décidée, versionnée — jamais une découverte | 2 |
| pouvoir discriminant | finesse de la partition induite (ordre « raffine », aucune métrique) | 3.1 |
| fiabilité | conformité sémantique annoncée / mécanismes réels (qualitative) | 3.2 |
| régimes | exact, incomplet (⊥ ambigu à jamais), ambigu, contradictoire, artefactuel | 5 |
| équivalence interprétative | relation d'équivalence (transitivité obligatoire) plus grossière que byte-à-byte, décidée et versionnée | 6 |
| composites / relationnels | provenance en union exacte, héritage du pire régime, jamais de masquage | 7 |
| artefact | signal conforme à sa méthode, hors domaine nominal ; catalogue ouvert (A-01) | 8 |
| contradictions | intra-observation, inter-observations, inter-signaux — représentées, jamais résolues | 9 |
| invariants | I5 reconstructibilité, I6 déterminisme conventionnel, I7 transparence de provenance, I8 indépendance contextuelle, I9 neutralité | 1.3, 10 |
| non-signaux | identité, score, probabilité, décision, famille, version retenue, doublon, consensus | 11 |

**Ce que ce document ne fait volontairement pas** : choisir des équivalences, pondérer quoi que ce soit, ordonner les familles de signaux, résoudre une contradiction ou une ambiguïté, regrouper des contenus. La couche suivante — hypothèses et consensus — est l'objet du document 003, qui devra se conformer aux invariants I1–I9.
