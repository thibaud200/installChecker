# 015 — Registre normatif — première instanciation

**Statut** : sixième et dernier document de la phase de conception de la série `docs/identity/`. S'appuie sur les documents 000→014, tous figés. **Clôt définitivement la phase documentaire fondatrice.**
**Nature** : ce document ne décrit plus la théorie — il **rédige le registre**. Il fixe la syntaxe Markdown exacte que 014 § 5 renvoyait ici, et produit intégralement le contenu logique des quatre fichiers de ℛ₀ dont 009 a décrit les actes d'adoption sans les matérialiser. Après validation : aucun code, aucun pseudo-code, aucun parseur, aucune structure C# — la norme se lit, elle ne s'exécute pas encore.
**Portée** : après ce document, É1 (013 § 8, 014 § 10) se réalise sans aucune décision restante — la matérialisation de `registre/` est une copie conforme des §§ 4 à 7 ci-dessous.

---

## 1. Le registre comme document normatif

Le registre ℛ n'est pas un format de données : c'est un **corpus documentaire gouverné**, au même titre que la série `docs/identity/` elle-même (013 § 3.2). Un fichier du registre est un acte normatif dont la lecture engage le moteur (par C2) et dont la rédaction engage une autorité humaine (007 § 9).

**Ce qu'il contient** : exactement les objets que 007 § 8 (Déf. 3) et 013 § 3.1 énumèrent — des conventions en vigueur (chacune : sa version, ses champs obligatoires du 014 § 5.1), un journal append-only des transitions, un état courant qui en est la projection. Rien d'autre n'est un objet du registre : ni un score, ni un exemple d'application, ni une donnée de corpus (celles-ci vivent dans `docs/mesures/`, jamais dans `registre/`).

**Ce qu'il ne contient jamais** :

- du code ou du pseudo-code (I36 : l'instanciation d'un registre ne modifie jamais la théorie, et la théorie ne s'exprime jamais en code) ;
- un score, un poids, un seuil, une probabilité (004 § 12, repris au 014 § 5.1 « champs interdits ») ;
- une décision spécifique à un fichier, un acte d'observation ou un corpus nommé — une convention est générale par construction (004 Déf. 1 : App(κ) est un domaine, jamais une énumération d'actes) ;
- une référence à une implémentation, un nom de classe, un chemin de code — la frontière I46 s'applique au registre comme au moteur : aucune de ses deux faces ne connaît l'autre.

**Ce qui constitue une modification** : toute variation de contenu logique d'une version déjà adoptée. **Interdite absolument** — une version est immuable dès l'adoption (013 § 3.1, symétrie exacte de l'intangibilité d'Ω, I1 transposé). Ce que le langage courant appelle « modifier une convention » est toujours, au sens du registre, l'une des cinq opérations du 007 § 10 (révision, retrait, remplacement, scission, fusion) — jamais une édition du fichier existant.

**Ce qui constitue une nouvelle version** : l'ajout d'un fichier `v<n+1>.md` dans le répertoire de l'identifiant, à la suite d'une révision (007 § 10), avec une entrée d'adoption propre au journal. La version n'est jamais renumérotée, jamais réutilisée, même après retrait (I28 : le retrait laisse une trace, jamais un trou comblé).

**Ce qui constitue une nouvelle convention** : l'ouverture d'un répertoire `<ID>/` inédit, avec son premier fichier `v1.md`. Un identifiant, une fois pris, n'est jamais réattribué à un objet différent — pas même après retrait complet de toutes ses versions (corollaire de I33 : une convention retirée reste identifiable dans le journal, elle ne libère pas son nom).

**Ce qui constitue une erreur documentaire** : toute violation de la grammaire du § 3 ou des contrats du § 5.1/5.2/5.3 du 014 — champ absent, section dupliquée, référence à une version inexistante, date antérieure à celle d'une entrée précédente du journal. Une erreur documentaire n'est **jamais silencieusement corrigée** : elle est un fait constaté (par relecture humaine à l'adoption, par C2 après l'implémentation), signalé, et corrigé par un nouveau commit de gouvernance qui remplace l'objet fautif avant son adoption effective — ou, si l'erreur n'est découverte qu'après adoption, elle est traitée comme un retrait suivi d'une adoption correcte, jamais comme une réécriture de l'historique.

**Indépendance du moteur** — Le registre existe, se rédige, se discute et se gouverne indépendamment de toute implémentation du moteur d'identité (013 § 3.2 : « la même nature documentaire » que 000→014). ℛ₀ est un objet valide au sens du présent document **avant** qu'aucune ligne de `Identity.Access` n'existe — sa validité se contrôle par relecture humaine (§ 8), pas par exécution. Le moteur est un lecteur de ℛ, jamais son auteur ni sa condition d'existence (009 § 1 : « ℛ₀ est un objet théorique avant d'être un fichier »).

---

## 2. Structure logique définitive

Quatre documents, chacun avec son rôle, son contenu logique, son identité, ses contraintes, son cycle de vie — définitifs, au sens où toute famille de convention future (§ 10) s'y conforme sans les étendre.

### 2.1 Fichier de version d'une convention (`<ID>/v<n>.md`)

- **Rôle** : porter le contenu logique intégral d'une version d'une convention — le seul objet que C2 traduit en un élément du référentiel (014 C2).
- **Contenu logique** : les quatorze champs du § 3.2, dans l'ordre imposé.
- **Identité** : le couple (identifiant du répertoire parent, numéro du fichier) — redondant avec les champs internes `identifiant`/`version` par construction (§ 3.4 : la concordance est vérifiée, jamais supposée).
- **Contraintes** : immuable dès l'adoption (§ 1) ; un seul fichier par version ; aucun champ optionnel — les quatorze sont tous obligatoires, y compris lorsque leur contenu est « aucune » ou « aucun » (§ 3.2 : l'absence explicite d'une dépendance, par exemple, s'écrit, elle ne s'omet pas).
- **Cycle de vie** : créé à l'adoption (§ 9.1) ; jamais édité ; potentiellement retiré (le fichier reste dans le dépôt — l'historique Git et `historique.md` en portent la trace — seul `etat.md` cesse de le citer, § 7).

### 2.2 Journal (`historique.md`)

- **Rôle** : la trace append-only de tous les actes de gouvernance (007 § 10) — la seule source qui fait foi de « ce qui s'est passé, quand, et pourquoi » (§ 6).
- **Contenu logique** : une suite ordonnée d'entrées, chacune avec les cinq champs du § 6.2.
- **Identité** : le fichier entier — il n'a pas d'identité partielle ; une entrée s'identifie par sa position (rang) et par le couple (identifiant, version) qu'elle concerne, jamais par un numéro propre (aucun identifiant d'entrée n'est introduit : ce serait un objet de plus sans rôle, contraire à la minimalité I14).
- **Contraintes** : append-only strict — aucune entrée existante n'est jamais modifiée ni supprimée, y compris pour corriger une erreur documentaire (§ 1 : l'erreur se répare par une entrée nouvelle, jamais par une réécriture) ; ordre chronologique strict (§ 6.3).
- **Cycle de vie** : ouvert au premier acte de gouvernance (l'adoption de ℛ₀, § 9) ; ne se clôt jamais ; grandit d'une entrée par acte, pour la durée de vie du projet.

### 2.3 État (`etat.md`)

- **Rôle** : la projection courante et unique que C2 lit pour établir le référentiel de conventions en vigueur (014 C2) — le seul document du registre qui change de contenu sans grandir en taille de façon monotone (une révision de format n'ajoute pas de ligne, elle en remplace).
- **Contenu logique** : les six sections du § 7.
- **Identité** : le fichier entier, à un instant du dépôt — au sens de 009 § 1, « l'identité d'un état de ℛ est le contenu du répertoire `registre/`, pas un numéro externe » ; `etat.md` en est le résumé projeté, jamais la source (la source est l'ensemble des actes du journal, § 6.1).
- **Contraintes** : à tout instant, cohérent avec `historique.md` (chaque couple cité y a une entrée d'adoption non suivie d'un retrait, § 7.4) et avec les fichiers de version existants (aucune citation sans fichier, § 7.4).
- **Cycle de vie** : réécrit intégralement à chaque acte de gouvernance (ce n'est pas un journal — la version précédente d'`etat.md` ne survit que dans l'historique Git, jamais dans `registre/`) ; sa validité s'établit à nouveau à chaque réécriture, jamais par différence avec la précédente.

### 2.4 Le registre entier

- **Rôle** : le seul objet que C2 projette (013 § 3.1) — les trois types de documents ci-dessus, ensemble, sous `registre/`.
- **Identité** : le contenu du répertoire `registre/` (§ 1, 009 § 1) — jamais un numéro de version externe au registre lui-même (ℛ n'a pas de « version 3 » globale ; ce qui a une version, ce sont ses conventions, une à une).
- **Contraintes** : cohérence documentaire (§ 8) avant toute prétention de validité.
- **Cycle de vie** : ouvert par le premier commit de gouvernance (§ 9.1, jalon É1) ; évolue par les cinq opérations du 007 § 10, chacune un commit distinct (013 § 3.3) ; ne se clôt jamais tant que le moteur existe.

---

## 3. Grammaire normative

Contenu logique (014 § 5) ⟶ **matérialisation Markdown exacte** (le présent paragraphe). Principe directeur : **le moteur ne lit jamais la prose** — seuls les champs normatifs, structurellement repérables sans compréhension du français, sont consommés par C2 ; la prose (justification, justification empirique, limites, conditions de révision) est produite pour l'humain qui gouverne le registre et n'entre dans aucune dérivation (014 § 5.1, colonne « lu par le moteur ? »).

### 3.1 Sections et ordre obligatoire

Un fichier de version est un document Markdown composé **exclusivement** de quatorze sections de niveau 2 (`## `), dans cet ordre strict, sans section supplémentaire, sans section manquante, sans réordonnancement :

1. `## identifiant`
2. `## version`
3. `## famille`
4. `## domaine d'application`
5. `## transformation`
6. `## dépendances`
7. `## régimes admis`
8. `## portée`
9. `## justification`
10. `## justification empirique`
11. `## limites`
12. `## conditions de révision`
13. `## date`
14. `## autorité`

Un titre de niveau 1 (`# `) ouvre le fichier — le nom de la convention et sa version, en toutes lettres (`# EQ-01 — Égalité parfaite de contenu (v1)`) ; il est **documentaire uniquement**, jamais lu par C2 (l'identité vérifiée est celle du § 3.1, champs 1–2, contre le chemin du fichier). Aucun autre niveau de titre n'apparaît dans un fichier de version.

### 3.2 Titres : forme exacte

Les quatorze titres ci-dessus sont **littéraux** — orthographe, accents, apostrophe typographique exclus (apostrophe droite `'`), absence de ponctuation finale. Un titre reformulé, abrégé, ou traduit est une section absente au sens du 014 § 5.1 (« champ obligatoire absent ») : cause de rejet par C2 (« registre malformé »).

### 3.3 Unicité des champs

Chaque section apparaît **exactement une fois**. Une section dupliquée, y compris avec un contenu identique, est une cause de rejet à part entière (ambiguïté de lecture : quelle occurrence ferait foi ne se décide jamais par convention implicite, I13). Une section dont le contenu est vide n'est pas absente au sens strict, mais **invalide** : chaque champ obligatoire du 014 § 5.1 exige un contenu non vide — y compris `dépendances`, où l'absence de dépendance s'écrit explicitement (§ 3.5).

### 3.4 Représentation de l'identité et des versions

- **`identifiant`** : une seule ligne, la chaîne exacte servant de nom au répertoire parent (`EQ-01`, `CE-01`, et pour les identifiants futurs : lettres majuscules, tiret, deux chiffres — le gabarit du § 10) ;
- **`version`** : une seule ligne, un entier positif sans zéro non significatif (`1`, jamais `01` ni `1.0`), égal au nom du fichier sans l'extension (`v1.md` ⟹ `1`) ;
- **concordance obligatoire** : les valeurs des sections `identifiant` et `version` doivent coïncider avec le chemin du fichier (`<identifiant>/v<version>.md`) — toute divergence est une cause de rejet nommée par 014 § 5.1 (« identifiant ou version incohérents avec le chemin »).

### 3.5 Représentation des dépendances

La section `dépendances` contient soit la ligne unique `Aucune.` (dépendance vide, réservée aux racines de fondation, 008 § 7), soit une liste à puces, une dépendance par ligne, chaque ligne au format exact :

```
- <IDENTIFIANT>, version <n>
```

triée par ordre alphabétique de `<IDENTIFIANT>`. Une dépendance citant un couple (identifiant, version) absent du répertoire `registre/conventions/` est une cause de rejet nommée par C2 (014 § 5.1 : « dépendance citant un couple (id, version) inexistant dans le répertoire »).

### 3.6 Représentation des références documentaires

Toute référence, dans la prose (§§ 9–12 des champs), à un document de la série `docs/identity/` suit la grammaire déjà en usage dans 000→014, reconduite ici comme norme définitive : `<numéro à trois chiffres> § <section>` (`007 § 3`), ou `<numéro> Déf. <n>` pour une définition numérotée, ou `000, L<n>` pour une limite actée. Ces références sont **exclusivement documentaires** : elles n'apparaissent jamais dans les huit champs lus par le moteur (§ 3, colonne 014 § 5.1) — seulement dans `justification`, `justification empirique`, `limites`, `conditions de révision`, `date` et `autorité`, qui sont tous des champs non lus par C2 à l'exception de `date` et `autorité`, dont le contenu (respectivement une date ISO et un nom d'autorité) ne comporte jamais de référence documentaire.

### 3.7 Contenu des champs lus par le moteur

Pour lever toute ambiguïté sur ce que « lu par le moteur » signifie structurellement (014 § 5.1, dernière colonne) : les sections `identifiant`, `version`, `famille`, `domaine d'application`, `transformation`, `dépendances`, `régimes admis`, `portée` sont rédigées de sorte que leur premier niveau de contenu (une valeur unique, une liste, ou une phrase normative unique et non ambiguë) soit mécaniquement extractible sans analyse sémantique du français — **le 015 rédige des textes qui satisfont cette contrainte** (§§ 4–5) ; la spécification d'un analyseur capable de l'extraire relève du code, hors du présent document (§ 1).

---

## 4. Texte normatif complet de EQ-01/v1.md

Contenu intégral du fichier `registre/conventions/EQ-01/v1.md`, tel qu'il existera après le commit d'adoption (§ 9.1) :

```markdown
# EQ-01 — Égalité parfaite de contenu (v1)

## identifiant

EQ-01

## version

1

## famille

interprétation

## domaine d'application

Tout couple d'actes d'observation, dans toute base conforme au contrat logique de Ω (014 § 6). Le signal relationnel fondé par cette convention est défini sur la totalité des actes du domaine — cent pour cent, sans précondition ni exception : tout acte porte une empreinte de contenu (014 § 6), donc toute paire d'actes est comparable.

## transformation

Deux actes d'observation dont les contenus observés sont parfaitement égaux — la même suite d'octets — sont liés par la relation « contenu identique », qui est l'identité matérielle ≡ₘ (000, Déf. 4). Cette convention fonde le signal relationnel « contenu identique » à partir de cette relation, exclusivement en régime exact.

## dépendances

Aucune.

## régimes admis

R1 (exact) exclusivement. Aucune autre valeur de régime n'existe pour ce signal : ni absence (⊥), ni ambiguïté, ni contradiction, ni artefact — l'égalité parfaite de contenu est établie ou ne l'est pas, sans degré intermédiaire.

## portée

Fonder la relation ≡ₘ et le signal relationnel « contenu identique ». Rien au-delà : cette convention ne fonde aucun signal de strate variante, version, identité ou famille.

## justification

L'égalité parfaite de contenu est la seule relation du système garantie par les mathématiques — une relation de type P (001 § 3) : discriminance maximale, fiabilité maximale (002 § 4.1). Elle constitue le socle de toute la chaîne d'interprétation : aucune convention de strate supérieure ne peut se fonder sans qu'une relation de cette nature existe quelque part sous elle.

## justification empirique

Corpus 1 (497 actes, base archivée `tests/oracle/corpus1-postA1.db`, 013 § 11) : le signal est défini sur la totalité des actes, en régime exact (R1), reproductible à l'identique sur double scan du corpus gelé (rapport de campagne, contrôle C6). Dénombrement : 381 classes de contenu distinctes, dont 112 classes multi-actes (108 paires, 4 triplets) et 269 classes singletons.

## limites

Les limites de cette convention sont exactement celles, actées, de son socle : le niveau « certain » du système demeure conventionnel en dernière analyse (000, L7) ; le cas d'un contenu identique porté par des identités logiques distinctes reste indiscernable de l'intérieur du système (000, L5) — un risque résiduel assumé, jamais résorbé par cette convention. Cette convention n'affirme rien au-delà de la strate contenu.

## conditions de révision

Une remise en cause du socle de la couche d'observation — un événement du type décrit en 000, L7. Toute révision produit une version nouvelle (`v2.md`), tracée dans `historique.md` ; le présent fichier n'est jamais modifié après son adoption.

## date

2026-07-05

## autorité

Propriétaire du projet.
```

---

## 5. Texte normatif complet de CE-01/v1.md

Contenu intégral du fichier `registre/conventions/CE-01/v1.md`, tel qu'il existera après le commit d'adoption (§ 9.1) :

```markdown
# CE-01 — Élection par identité de contenu (v1)

## identifiant

CE-01

## version

1

## famille

élection

## domaine d'application

La strate contenu, et elle seule, sur tout domaine d'actes où l'hypothèse « même contenu » est formulable sous l'index courant (007 § 2). Cette convention est sans objet — ni licence ni refus qu'elle motiverait — sur toute autre strate.

## transformation

Lorsque, sur un domaine d'actes, l'hypothèse « même contenu » (strate contenu) est soutenue par le signal fondé par EQ-01 en régime exact et domine strictement toute concurrente formulable (003 § 9), son élection est autorisée au niveau maximal de certitude — « certaine », au sens conventionnel du 000 § 5.1.

## dépendances

- EQ-01, version 1

## régimes admis

R1 (exact) exclusivement — hérité du signal qu'elle mobilise : EQ-01 n'admet elle-même que R1 (§ 4). Aucun soutien en régime incomplet, ambigu, contradictoire ou artefactuel n'est admissible pour cette convention.

## portée

Assigner le niveau « certaine » à l'hypothèse « même contenu », strate contenu exclusivement. Cette convention ne dit rien des strates variante, version, identité ou famille : le niveau « certaine » n'existe, dans tout le registre en vigueur, que pour l'identité matérielle.

## justification

La configuration licenciée par cette convention est la seule du système où la maximale de préférence est structurellement unique (consensus dégénéré, domination immédiate, 003 § 9) et où le soutien est de type P en régime exact. La charge de justification de l'engagement (007 § 3) est acquittée par la nature mathématique du garant : aucune convention supplémentaire ne pourrait renforcer une égalité déjà certaine par construction.

## justification empirique

Corpus 1 : les 112 classes multi-actes (108 paires, 4 triplets) réalisent intégralement la configuration licenciée par cette convention, sans exception ni cas limite observé (rapport de campagne).

## limites

Héritées d'EQ-01 (000, L5, L7) — voir `EQ-01/v1.md`. La certitude de contenu ne remonte jamais la chaîne des strates (005 § 10) : élire « même contenu » n'élit ni variante, ni version, ni identité.

## conditions de révision

Réviser le niveau assigné par cette convention (le plafond conventionnel du 000 § 5.1) constituerait une version nouvelle (`v2.md`), tracée dans `historique.md` ; le présent fichier n'est jamais modifié après son adoption.

## date

2026-07-05

## autorité

Propriétaire du projet.
```

---

## 6. `historique.md`

### 6.1 Rôle et forme générale

Un document Markdown, une section de niveau 1 (`# Historique du registre`), puis une suite d'entrées de niveau 2, dans l'ordre chronologique strict de leur ajout — jamais réordonnées, jamais retirées.

### 6.2 Grammaire d'une entrée

Chaque entrée est un titre de niveau 2 suivi de cinq sous-sections de niveau 3, dans cet ordre :

```markdown
## <date ISO> — <type de transition> — <identifiant> v<version>

### type

<adoption | révision | retrait | remplacement | scission | fusion>

### convention

<identifiant>, version <n>

### justification de l'acte

<prose>

### autorité

<autorité>
```

Le titre de niveau 2 est une redite documentaire des sous-sections `type` et `convention` (lisible en un coup d'œil dans un historique long) ; en cas de désaccord entre le titre et les sous-sections, **les sous-sections font foi** — le titre n'est jamais lu par C2 (§ 3.7 : seuls les champs structurés le sont), qui ne consulte `historique.md` que pour vérifier, entrée par entrée, l'existence d'une adoption pour chaque couple cité par `etat.md` (014 § 5.3).

### 6.3 Contraintes

- **append-only** : ajouter une entrée est la seule opération permise sur ce fichier ;
- **ordre chronologique non décroissant** : la date d'une entrée n'est jamais antérieure à celle de l'entrée qui la précède ;
- **rejet** (014 § 5.2) si : une entrée omet l'un des cinq éléments ; une entrée d'adoption ou de révision cite une version dont le fichier `v<n>.md` n'existe pas dans `registre/conventions/` ; une entrée de retrait cite une version jamais adoptée par une entrée antérieure.

### 6.4 Les deux premières entrées

Le journal s'ouvre par exactement deux entrées, dans cet ordre — la structure de fondation du 008 § 7 rendue traçable :

```markdown
# Historique du registre

## 2026-07-05 — Adoption — EQ-01 v1

### type

adoption

### convention

EQ-01, version 1

### justification de l'acte

Première convention adoptée du registre — racine de la structure de fondation (008 § 7). Fonde le signal relationnel « contenu identique », socle de toute la chaîne d'interprétation. Justifiée théoriquement en 007 § 4 (comme candidate) et empiriquement en 009 § 3, sur la base du corpus 1 mesuré (497 actes, contrôle C6).

### autorité

Propriétaire du projet.

## 2026-07-05 — Adoption — CE-01 v1

### type

adoption

### convention

CE-01, version 1

### justification de l'acte

Deuxième convention adoptée du registre — première licence d'élection, dépendante d'EQ-01 (arc de fondation, 008 § 7, E4). Autorise l'élection au niveau maximal de l'hypothèse « même contenu » lorsque la configuration est structurellement forcée (maximale unique). Candidate depuis 007 § 4, adoptée en 009 § 4 sur la base des 112 classes multi-actes du corpus 1.

### autorité

Propriétaire du projet.
```

**Identité et lien avec les commits** : ces deux entrées, avec les deux fichiers de version (§§ 4–5) et l'état initial (§ 7.5), forment un seul acte de gouvernance — un seul commit (013 § 3.3, jalon É1, § 9.1). Le commit est la **trace** de l'événement, jamais son identité : conformément à 009 § 1 et au principe rappelé au § 1 ci-dessus, l'identité de l'acte est le contenu documentaire lui-même (les cinq champs de chaque entrée) — un `git rebase` qui changerait le hash du commit sans toucher `historique.md` ne changerait rien à l'identité de l'adoption ; à l'inverse, un journal réécrit sous un commit identique serait un registre invalide, quel que soit son hash Git.

---

## 7. `etat.md`

### 7.1 Rôle

La projection courante de ℛ que C2 lit (014 § 5.3) — dérivée d'`historique.md`, jamais une source indépendante (§ 2.3).

### 7.2 Structure : six sections, ordre obligatoire

```markdown
# État du registre

## ℛ₀

<identité de l'état>

## conventions en vigueur

<liste>

## conventions retirées

<liste ou « Aucune. »>

## version du registre

<entier>

## date logique

<date ISO>

## index documentaire

<liste>
```

### 7.3 Contenu exact de chaque section

- **`ℛ₀`** : le nom de l'état (au sens du 009 § 1 : « registre des conventions d'identité, InstallChecker », identifiant `ℛ-IC`, état `ℛ₀`) — une ligne : `ℛ-IC, état ℛ₀`. Les états futurs porteront leur propre nom sur ce même schéma (`ℛ₁`, etc., § 7.6) ;
- **`conventions en vigueur`** : une liste à puces, une convention par ligne, triée par ordre alphabétique d'identifiant, format `- <IDENTIFIANT>, version <n>` — pour ℛ₀ : `EQ-01, version 1` puis `CE-01, version 1` ;
- **`conventions retirées`** : la même grammaire de liste, réservée aux identifiants dont **toutes** les versions ont été retirées sans remplacement en vigueur ; pour ℛ₀, aucune n'existe encore : la ligne unique `Aucune.` ;
- **`version du registre`** : un entier, incrémenté à chaque acte de gouvernance (distinct de la version d'une convention individuelle, § 3.4) — `0` pour l'état initial, cohérent avec la numérotation ℛ₀ du 009 § 1 ;
- **`date logique`** : la date de l'acte de gouvernance le plus récent reflété par ce fichier — `2026-07-05` pour ℛ₀, identique à la date des deux entrées du § 6.4 ;
- **`index documentaire`** : une liste à puces des chemins relatifs des fichiers de version actuellement en vigueur — `registre/conventions/EQ-01/v1.md` puis `registre/conventions/CE-01/v1.md` — la table de correspondance qui permet à un lecteur humain de vérifier § 7.4 sans outil.

### 7.4 Cohérence exigée — causes de rejet (014 § 5.3)

- une version citée dans `conventions en vigueur` sans fichier `v<n>.md` correspondant ;
- une version citée sans entrée d'adoption dans `historique.md` ;
- une version retirée au journal encore citée en vigueur ;
- une incohérence entre `version du registre` et le nombre d'actes de gouvernance recensés dans `historique.md` ;
- une convention citée dans `conventions en vigueur` sans que ses dépendances (§ 3.5 de son fichier de version) soient elles-mêmes citées en vigueur — la vérification du prédicat de cohérence, 008 § 4, appliquée à l'état courant.

### 7.5 Contenu exact d'`etat.md` pour ℛ₀

```markdown
# État du registre

## ℛ₀

ℛ-IC, état ℛ₀

## conventions en vigueur

- CE-01, version 1
- EQ-01, version 1

## conventions retirées

Aucune.

## version du registre

0

## date logique

2026-07-05

## index documentaire

- registre/conventions/CE-01/v1.md
- registre/conventions/EQ-01/v1.md
```

### 7.6 Évolutions futures

Un nouvel acte de gouvernance (adoption d'EQ-02, retrait de CE-01, etc.) produit un `etat.md` **entièrement réécrit** : `version du registre` passe à 1, `date logique` prend la date du nouvel acte, les listes reflètent le nouveau contenu. Le fichier précédent ne survit que dans l'historique Git — jamais comme second fichier dans `registre/` (§ 2.3 : `etat.md` n'est pas un journal).

---

## 8. Validation documentaire — la check-list complète d'É1

Avant qu'un registre puisse être considéré valide, **tout** ce qui suit doit être vérifié — chaque cause de rejet nommée par 014 §§ 5.1–5.3 y figure, sans exception :

**Sur chaque fichier de version (`<ID>/v<n>.md`)** :

1. les quatorze sections du § 3.1 sont présentes, dans l'ordre exact, avec les titres littéraux du § 3.2 ;
2. aucune section n'est dupliquée (§ 3.3) ;
3. aucune section n'est vide (§ 3.3) ;
4. `identifiant` et `version` coïncident avec le chemin du fichier (§ 3.4) ;
5. `famille` est l'une des huit familles connues (014 § 5.1) ;
6. chaque dépendance listée en `dépendances` désigne un couple (identifiant, version) dont le fichier existe dans `registre/conventions/` (§ 3.5) ;
7. aucun champ interdit n'apparaît (score, poids, probabilité, seuil, priorité numérique, élément exécutable — 004 § 12, 014 § 5.1) ;
8. la structure de dépendance forme un graphe acyclique sur l'ensemble du répertoire (008 § 4 — vérifiable pour ℛ₀ : un arc unique, CE-01 → EQ-01, 009 § 8) ;

**Sur `historique.md`** :

9. chaque entrée porte les cinq éléments du § 6.2 ;
10. l'ordre chronologique est non décroissant (§ 6.3) ;
11. chaque entrée d'adoption ou de révision cite une version dont le fichier existe ;
12. aucune entrée de retrait ne cite une version jamais adoptée ;

**Sur `etat.md`** :

13. les six sections du § 7.2 sont présentes, dans l'ordre, avec les titres littéraux ;
14. chaque version citée dans `conventions en vigueur` a un fichier existant (§ 7.4) ;
15. chaque version citée dans `conventions en vigueur` a une entrée d'adoption dans `historique.md`, non suivie d'une entrée de retrait ;
16. aucune version retirée au journal n'est encore citée en vigueur ;
17. `version du registre` coïncide avec le nombre d'actes de gouvernance recensés dans `historique.md` ;
18. `date logique` coïncide avec la date de l'acte de gouvernance le plus récent ;
19. `index documentaire` énumère exactement les fichiers cités par `conventions en vigueur`, sans omission ni surplus ;
20. chaque dépendance d'une convention en vigueur est elle-même en vigueur (008 § 4, prédicat de cohérence) ;

**Sur l'ensemble du registre** :

21. aucune incompatibilité déclarée n'oppose deux conventions simultanément en vigueur (007 § 6) — pour ℛ₀, vide par construction (deux conventions, un arc de dépendance, aucune concurrence, 009 § 8) ;
22. minimalité : retirer toute convention en vigueur fait tomber une élection ou une licence d'un état dérivé — aucune convention superflue (009 § 8, vérifié pour ℛ₀ : l'ensemble minimal de chaque élection de W₀ est {EQ-01, CE-01}, unique) ;
23. chaque convention est justifiée empiriquement (I35) — la section `justification empirique` cite des faits mesurés et référencés, jamais des principes seuls ;
24. le registre entier est relisible et vérifiable par un humain sans outil, en suivant exactement les vingt points ci-dessus — la définition opérationnelle de « humainement vérifiable » du 013 § 8, jalon É1.

**Verdict** : le registre est valide si et seulement si les vingt-quatre points sont satisfaits. Un seul point en échec produit un registre invalide dans son ensemble — il n'existe pas de validité partielle (symétrie avec « entier ou absent », 011 § 4, appliquée ici au registre plutôt qu'à W).

---

## 9. Gouvernance

### 9.1 Qui peut proposer

Toute proposition de convention (nouvelle ou révisée) peut être rédigée par quiconque contribue au projet — la rédaction n'est pas un acte de gouvernance (007 § 9 : l'adoption l'est). Une proposition non adoptée demeure dans K théorique (007 § 8) : elle peut être discutée, versionnée en dehors de `registre/` (par exemple dans un brouillon de document de conception), sans aucun effet sur ℛ tant qu'elle n'a pas franchi le § 9.2.

### 9.2 Qui adopte

**L'autorité d'adoption est le propriétaire du projet**, conformément à 007 § 9 (« l'adoption est une décision de gouvernance du système, pas un acte que le moteur s'accorde à lui-même ») et à 009 § 1 (pour ℛ₀ précisément : « la validation du présent document par cette autorité constitue l'acte d'adoption »). Le cadre théorique exige la trace (une autorité nommée, une date) ; il ne prescrit pas d'organe collégial, de vote ou de procédure de délégation — matière d'organisation, hors périmètre théorique (007 § 9, dernière puce). Toute évolution future de l'autorité (délégation, collège) est elle-même un fait d'organisation à documenter dans les entrées concernées d'`historique.md` (champ `autorité`), jamais une modification du présent document.

### 9.3 Qui retire

La même autorité — le retrait est une transition du registre au même titre que l'adoption (007 § 10), tracée par la même grammaire d'entrée (§ 6.2, avec `type = retrait`). Aucune procédure distincte n'est introduite : retirer une convention n'est pas moins engageant qu'en adopter une (007 § 3 : la charge de justification pèse sur l'engagement — le retrait d'une licence en vigueur *retire* un engagement, il ne charge donc rien de nouveau et n'exige pas de justification renforcée au-delà de celle exigée par 007 § 10 : « datée et motivée »).

### 9.4 Comment une justification évolue

Une justification (les champs `justification`, `justification empirique`, `limites`, `conditions de révision` d'un fichier de version) **n'évolue jamais dans le fichier existant** (§ 1, § 2.1 : immuabilité dès adoption). Faire évoluer une justification — par exemple, l'enrichir d'une mesure nouvelle du corpus 2 — est une révision (007 § 10) : un nouveau fichier `v<n+1>.md` portant la justification mise à jour, une entrée `révision` dans `historique.md` motivant l'écart avec la version précédente, et une mise à jour d'`etat.md`.

### 9.5 Comment une convention devient obsolète

Une convention devient obsolète par **retrait** (§ 9.3), jamais par désuétude tacite (I13 : rien d'implicite). Le motif du retrait — limite franchie en pratique, dépendance révoquée, configuration reconnue trop permissive (007 § 10) — est consigné dans le champ `justification de l'acte` de l'entrée de retrait. Une convention retirée reste lisible indéfiniment (son fichier `v<n>.md` n'est jamais supprimé du dépôt) : l'obsolescence est un fait du présent (`etat.md` ne la cite plus), jamais un effacement du passé (`historique.md` et le fichier de version subsistent, I28).

---

## 10. Compatibilité future

**EQ-02** (une future convention d'interprétation, famille déjà connue) : un nouveau répertoire `registre/conventions/EQ-02/v1.md`, satisfaisant la grammaire du § 3 sans aucune extension — les quatorze sections suffisent à toute convention de la famille interprétation, quel que soit son objet. Une entrée d'adoption s'ajoute à `historique.md` (append) ; `etat.md` est réécrit (§ 7.6) pour la citer. **Aucun document existant (EQ-01, CE-01, historique passé) n'est modifié.** ∎

**CE-02** (une future convention d'élection, sur une strate supérieure) : de même — un répertoire nouveau, une entrée d'adoption, un `etat.md` réécrit. Sa section `dépendances` pourra citer EQ-01, EQ-02, ou toute combinaison en vigueur — la grammaire du § 3.5 n'impose aucune limite au nombre de dépendances. **Aucun document existant n'est modifié.** ∎

**A-01** (la première convention de catalogue d'artefacts, 009 § 7) : sa famille (`catalogue`) est déjà l'une des huit connues (014 § 5.1) — aucune extension de la grammaire n'est nécessaire ; A-01 suit exactement le même gabarit que EQ-01 et CE-01, avec un contenu de domaine différent (une entrée de catalogue plutôt qu'une relation ou une licence). ∎

**Futures conventions de familles encore non instanciées** (priorité, attente, stratification, composition — 009 § 7) : la liste des huit familles est déjà close par 014 § 5.1 ; toute convention de ces familles suit la même grammaire, sans qu'aucune section supplémentaire ne soit requise — le champ `transformation` porte, pour chacune, l'énoncé propre à sa famille (004 §§ 3–7, 005), sans que la structure du fichier n'en soit affectée. ∎

**Une neuvième famille** (hypothèse la plus extensive) : par 011 § 10, une famille nouvelle est d'abord une décision théorique (un document de la série, postérieur au 015) ; au niveau du registre, elle s'ajoute à l'énumération admissible du champ `famille` (§ 3.1, point 5 de la check-list) — une extension du **vocabulaire** d'un champ existant, jamais une nouvelle section, jamais une nouvelle grammaire. La grammaire du § 3 est donc démontrée stable même sous ce cas limite. ∎

Dans les cinq cas : **la grammaire normative du § 3 et les quatre documents du § 2 ne sont jamais modifiés — seul leur contenu grandit**, exactement comme 013 § 11 le démontrait déjà pour les contrats internes du moteur. Le registre est extensible par construction.

---

## 11. Clôture documentaire

**La phase documentaire fondatrice de la série `docs/identity/` est officiellement close.**

Les documents 000 à 015 forment un corpus théorique complet, cohérent et instancié :

- **la théorie** (000→008) : les objets, leurs propriétés, les invariants I1 à I36 ;
- **l'instanciation** (009) : ℛ₀, W₀, les invariants I33 à I36 ;
- **le contrat d'implémentation** (010→014) : l'interface publique, la conception interne, l'architecture, les contrats internes, les invariants I37 à I52 ;
- **le registre matérialisé** (015, le présent document) : la grammaire exacte, le texte intégral d'EQ-01/v1 et CE-01/v1, historique.md et etat.md complets, la check-list de validation, la gouvernance, les invariants I53 à I56.

**Aucune décision théorique ou normative ne fait plus obstacle à l'implémentation.** Toute évolution future du système passera exclusivement par l'une de ces trois voies, jamais une quatrième :

- une **évolution de registre** (§ 9 : adoption, révision, retrait, remplacement, scission, fusion — un commit de gouvernance sous `registre/`) ;
- une **évolution de théorie** (une nouvelle série documentaire, postérieure au 015, pour toute question que 000→015 ne couvrent pas — une nouvelle strate, une nouvelle famille de conventions) ;
- une **implémentation** (le code lui-même, conforme au contrat des documents 010→015, sans aucune liberté normative résiduelle).

**Le moteur n'a plus aucune liberté normative.** Toute question qu'un futur développeur du moteur pourrait se poser sur « comment interpréter ceci » a sa réponse écrite dans 000→015 ; s'il ne la trouve pas, la réponse n'est pas à inventer dans le code — elle appartient à l'une des trois voies ci-dessus, et son absence est un signal qu'un document (registre ou théorie) doit être complété **avant** que le code ne soit écrit (I36 appliqué prospectivement).

**Le projet entre dans la phase É1 d'implémentation** : la matérialisation de ℛ₀ (013 § 8, 014 § 10) — une copie conforme des §§ 4, 5, 6.4 et 7.5 du présent document dans `registre/`, suivie du plan É1→É9 défini au 013 § 8 et détaillé au 014 § 10.

---

## 12. Nouveaux invariants — démontrés

> **I53 — Toute convention possède une représentation documentaire complète.**
> *Démonstration* : le § 3.1 impose quatorze sections, toutes obligatoires (§ 3.3), couvrant exhaustivement les champs du 014 § 5.1 ; la check-list du § 8 (points 1–8) vérifie mécaniquement leur présence et leur non-vacuité pour tout fichier de version ; les §§ 4 et 5 exhibent deux instances satisfaisant intégralement cette contrainte. Une convention sans l'une de ces quatorze sections n'est pas une convention incomplète : elle est rejetée dans son entier (014 § 5.1, « registre malformé ») — il n'existe donc aucune convention partiellement représentée dans un registre valide. ∎

> **I54 — Toute évolution du registre est traçable sans ambiguïté.**
> *Démonstration* : les cinq opérations du 007 § 10 sont chacune une entrée d'`historique.md` (§ 6.2), append-only (§ 6.3) — jamais réécrite, jamais réordonnée ; chaque entrée porte son type, la convention et la version concernées, sa justification, son autorité. Deux entrées ne peuvent désigner le même acte (§ 6.1 : l'ordre chronologique est total, la position dans le journal est l'identité de rang) ; la check-list (§ 8, points 9–12) exclut toute entrée incomplète. La traçabilité n'est donc jamais partielle : soit l'entrée existe et porte ses cinq éléments, soit le registre est invalide. ∎

> **I55 — Toute lecture normative est indépendante de la prose justificative.**
> *Démonstration* : le § 3.7 identifie huit champs structurés (identifiant, version, famille, domaine d'application, transformation, dépendances, régimes admis, portée) dont le contenu est mécaniquement extractible ; les six champs restants (justification, justification empirique, limites, conditions de révision, date, autorité) sont de la prose ou des valeurs atomiques non normatives, jamais consultés par C2 (014 § 5.1, colonne « lu par le moteur »). La séparation est structurelle — deux ensembles de sections disjoints, positionnellement fixes (§ 3.1) — et non une discipline de lecture qu'un lecteur pourrait enfreindre par inadvertance : un moteur conforme ne peut matériellement pas faire dépendre une dérivation de la section `justification`, puisque son contrat (C2, 014 § 1) ne définit aucune sortie qui en proviendrait. ∎

> **I56 — Toute convention future est exprimable sans modifier la structure du registre.**
> *Démonstration* : le § 10 énumère cinq cas de compatibilité future, du plus étroit (une convention de plus dans une famille déjà connue) au plus large (une neuvième famille) ; dans chacun, la conclusion est identique — un répertoire nouveau ou une entrée nouvelle utilisant la grammaire du § 3 telle quelle, au pire une extension du vocabulaire admissible d'un champ existant (`famille`), jamais une section supplémentaire ni un document d'un cinquième type. La grammaire du § 3 est donc une fonction du contenu logique des conventions (014 § 5.1) et non de leur nombre, leur famille ou leur date d'introduction — elle est démontrée stable par induction sur les cas du § 10. ∎

---

## Conclusion

Le registre est désormais un document normatif complet : son statut documentaire est défini (§ 1), ses quatre objets logiques sont définitifs (§ 2), sa grammaire Markdown est fixée section par section (§ 3), le texte intégral d'EQ-01/v1 et de CE-01/v1 est rédigé sans aucune partie implicite (§§ 4–5), `historique.md` et `etat.md` sont spécifiés et instanciés pour ℛ₀ (§§ 6–7), la validation d'É1 dispose d'une check-list exhaustive de vingt-quatre points (§ 8), la gouvernance est définie sans jamais toucher à l'architecture logicielle (§ 9), l'extensibilité est démontrée sur cinq cas (§ 10), et quatre nouveaux invariants (I53–I56) closent la série au même niveau de rigueur que 010→014.

**La phase documentaire fondatrice — 000 à 015 — est officiellement close.** Le projet entre dans la phase É1 d'implémentation : la matérialisation de ℛ₀, suivie de l'implémentation incrémentale du moteur d'identité conformément au plan É1→É9 défini au document 013 et détaillé au document 014.

**Ce que ce document ne fait volontairement pas** : écrire une ligne de code, définir un parseur, adopter une convention au-delà d'EQ-01 et de CE-01 (déjà adoptées par 009 — ce document les matérialise, il ne les adopte pas une seconde fois), planifier au-delà d'É9, réviser un invariant antérieur.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| registre comme document normatif | corpus documentaire gouverné ; contenu/interdits/modification/version/convention/erreur définis ; indépendant du moteur | 1 |
| structure logique définitive | `<ID>/v<n>.md`, `historique.md`, `etat.md`, le registre entier — rôle, contenu, identité, contraintes, cycle de vie de chacun | 2 |
| grammaire normative | 14 sections de niveau 2, ordre strict, titres littéraux, unicité, format des versions/dépendances/références documentaires | 3 |
| EQ-01/v1.md | texte intégral, 14 champs, aucune partie implicite | 4 |
| CE-01/v1.md | texte intégral, 14 champs, dépend d'EQ-01 | 5 |
| historique.md | append-only, 5 champs par entrée, 2 premières entrées (adoption EQ-01, adoption CE-01, 2026-07-05) | 6 |
| etat.md | 6 sections définitives ; contenu exact pour ℛ₀ (2 conventions en vigueur, 0 retirée, version 0) | 7 |
| validation documentaire | check-list de 24 points, verdict binaire (tout ou rien) | 8 |
| gouvernance | propose : quiconque ; adopte/retire : le propriétaire du projet ; obsolescence = retrait tracé, jamais désuétude tacite | 9 |
| compatibilité future | EQ-02, CE-02, A-01, familles restantes, neuvième famille — cinq cas, aucune modification des documents existants | 10 |
| clôture documentaire | 000→015 close ; trois voies d'évolution seulement : registre, théorie, implémentation | 11 |
| invariants | I53 représentation complète, I54 traçabilité sans ambiguïté, I55 indépendance à la prose, I56 extensibilité sans refonte | 12 |
