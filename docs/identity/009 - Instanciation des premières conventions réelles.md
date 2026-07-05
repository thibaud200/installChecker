# 009 — Instanciation des premières conventions réelles

**Statut** : dixième et dernier document de la phase fondatrice de la série `docs/identity/`. S'appuie exclusivement sur les documents 000 à 008, les mesures du corpus 1 (`docs/mesures/`), les ADR existantes (CLAUDE.md § 17) et les faits effectivement observés. Aucune hypothèse supplémentaire.
**Périmètre** : ce document ne définit plus ce qui est possible — il définit **ce qui entre effectivement dans le premier registre réel ℛ₀**. C'est le premier document de la série où des conventions deviennent réellement adoptées. Il ne construit toujours aucun moteur. Aucun code, aucun algorithme, aucun score, aucun seuil, aucune convention non justifiée.

---

## 1. Ouverture du premier registre : ℛ₀

**Acte d'ouverture** — Le registre des conventions du système d'identité d'InstallChecker est ouvert. Son état initial est :

| Champ | Valeur |
|---|---|
| **identifiant** | ℛ-IC (registre des conventions d'identité, InstallChecker) |
| **version** | 0 — l'état initial est noté **ℛ₀** |
| **date logique** | 2026-07-05 (date de validation du présent document) |
| **autorité d'adoption** | le propriétaire du projet — conformément au 007 § 9, l'autorité est extérieure au moteur ; conformément à la méthode du projet, **la validation du présent document par cette autorité constitue l'acte d'adoption** de son contenu |
| **domaine** | toute base d'observations produite par le pipeline figé (schéma `user_version = 1`), à commencer par le corpus 1 mesuré |

**ℛ₀ est un objet théorique avant d'être un fichier.** Sa réalité est celle de ses actes : deux adoptions (§ 3–4), leurs justifications, leurs dépendances, leur historique naissant. Toute matérialisation future (fichier versionné dans le dépôt, section dédiée) sera une *représentation* de ℛ₀ — commode, auditable, mais dérivée : si la représentation et les actes divergent, les actes font foi. C'est la même relation que entre Ω et la base SQLite : l'objet précède son support.

**Contenu de ℛ₀** : exactement **deux conventions** — EQ-01 et CE-01 — dans l'ordre de fondation (008 § 7). Rien d'autre. En particulier : A-01 (catalogue d'artefacts) n'y figure pas — voir § 7 et l'écart documenté en fin de document.

---

## 2. Critères d'adoption

Rappel des exigences, toutes issues des documents précédents, **sans exception** :

- **justification complète** (004, Déf. 1 ; 007 § 5) : domaine, justification interne, limites déclarées, dépendances, régimes admis, portée, conditions de révision — plus la justification d'adoption propre (007 § 9 : pourquoi maintenant, pourquoi celle-ci) ;
- **cohérence** : l'entrée ne rend pas ℛ₀ incohérent (008 § 4 : compatibilité, dépendances satisfaites, acyclicité, minimalité, confluence) ;
- **compatibilité** : aucune incompatibilité déclarée avec une convention en vigueur (007 § 6) ;
- **satisfaction de I13 à I32** : explicite et versionnée (I13), minimale (I14), sans effet sur les observations (I15, I26, I32), sans création d'hypothèse (I25), sans dépendance à l'ordre d'application (I31), provenance intégrale (I30) ;
- **justification empirique** (I35, § 11) : l'adoption s'appuie sur des faits mesurés, référencés — pas sur des principes seuls.

---

## 3. Première convention adoptée : EQ-01

**EQ-01 — Égalité parfaite de contenu** *(adoptée, version 1)*

| Champ | Contenu |
|---|---|
| **famille** | convention d'interprétation (004 § 3) — elle fonde le signal relationnel « contenu identique » |
| **transformation T** | deux actes d'observation dont les contenus observés sont **parfaitement égaux** — la même suite d'octets — sont liés par la relation « contenu identique » ; cette relation est l'identité matérielle ≡ₘ (000, Déf. 4) |
| **domaine** | tout couple d'actes de toute base conforme ; le signal est défini sur 100 % des actes (la couche d'observation établit l'égalité de contenu pour tout fichier lu — c'est un invariant du pipeline figé, la manière dont elle l'établit relevant des ADR de la couche d'observation, hors du présent document) |
| **justification** | l'égalité parfaite de contenu est la seule relation du système garantie par les mathématiques (type P, 001 § 3) : discriminance maximale, fiabilité maximale (002 § 4.1) ; c'est le socle de toute la chaîne — sans elle, rien au-dessus n'est fondé |
| **justification empirique** | corpus 1 : signal défini sur les 497 actes, en régime exact (R1), reproductible à l'identique sur double scan du corpus gelé (rapport de campagne, contrôle C6) ; 381 classes de contenu, 112 classes multi-actes |
| **limites** | les limites de la relation sont celles, actées, de son socle (000, L7 : le « certain » du système est conventionnel en dernière analyse) et du cas L5 (contenu identique, identités logiques distinctes — indiscernable de l'intérieur, risque résiduel accepté) ; EQ-01 n'affirme rien au-delà de la strate contenu |
| **dépendances** | aucune — EQ-01 est une racine de la structure de fondation (008 § 7) |
| **régime** | consomme le signal d'égalité en R1 exclusivement ; aucun autre régime n'existe pour ce signal (jamais ⊥, jamais ambigu, jamais artefactuel) |
| **portée** | fonder la relation ≡ₘ et le signal relationnel « contenu identique » ; rien d'autre |
| **conditions de révision** | remise en cause du socle de la couche d'observation (événement de type L7) ; toute révision serait une version nouvelle, tracée |

Conformément au prompt et au 007 § 4 : rien n'est dit ici des mécanismes par lesquels la couche d'observation établit l'égalité — ni fonctions de hachage, ni implémentation. EQ-01 porte sur l'égalité parfaite de contenu comme objet logique, et sur elle seule.

---

## 4. Deuxième convention adoptée : CE-01, le premier niveau de certitude

**CE-01 — Élection par identité de contenu** *(adoptée, version 1 — la candidate du 007 § 4, désormais en vigueur)*

- **T(CE-01)** : lorsque, sur un domaine d'actes, l'hypothèse « même contenu » (strate contenu) est soutenue par le signal fondé par EQ-01 en régime exact et domine strictement toute concurrente formulable, son élection est autorisée, **au niveau maximal de certitude** (« certaine », au sens conventionnel du 000 § 5.1).
- **Portée exacte** : **la seule strate contenu**. CE-01 ne dit rien — et ℛ₀ ne dit rien — des strates variante, version, identité, famille. Le niveau « certain » n'existe, dans tout le système, que pour l'identité matérielle.
- **Dépendances** : EQ-01 (satisfaite dans ℛ₀ — l'arc de fondation du 008, E4, est désormais réel).
- **Justification d'adoption** : la configuration licenciée est la seule du système où la maximale est structurellement unique (003 § 9 : consensus dégénéré, domination immédiate) et où le soutien est de type P en R1 ; la charge de justification de l'engagement (007 § 3) est acquittée par la nature mathématique du garant. Empiriquement : les 112 classes multi-actes du corpus 1 réalisent la configuration, sans exception ni cas limite observé.
- **Limites, régimes, conditions de révision** : héritées du 007 § 4 et alignées sur EQ-01 ; réviser le niveau assigné (le plafond conventionnel) serait une version nouvelle de CE-01 (le scénario du 007, E4).

---

## 5. Le premier état prudent : W₀

**Définition (W₀)** — Le premier état du monde du système : l'état prudent (006 § 10) associé à l'index (Ω_corpus1, ℛ₀), où Ω_corpus1 est la base mesurée de la campagne (497 actes, 381 classes de contenu).

Caractéristiques — toutes trois voulues :

- **uniquement des élections de contenu** : une élection par classe multi-actes — **112 élections** (108 classes de 2 actes, 4 classes de 3 actes, chiffres mesurés R10), chacune retenant l'hypothèse « même contenu » au niveau « certaine », avec pour motif la domination stricte (unique concurrente : la coïncidence de contenu, dominée par inclusion de résidus) et pour licence CE-01, dépendance EQ-01. Les 269 classes singletons ne portent aucune hypothèse non triviale à la strate contenu : rien à élire, rien à refuser — leur couverture est le constat de vacuité de l'espace (précision au 006 § 3 : la complétude se lit sur les domaines où l'espace d'hypothèses est non trivial) ;
- **refus partout ailleurs** : toutes les strates supérieures, sur tous les domaines, sont en refus motivé — la carte en est dressée au § 6 ;
- **aucune identité logique encore retenue** : W₀ ne contient aucune élection de strate variante, version, identité ou famille. Le moteur d'identité, à sa naissance, saura dire « ces contenus sont les mêmes octets » — et rien d'autre. **Cet état est volontairement minimal** : c'est l'application exacte du moindre engagement (P7, 007 § 3) — le système ne s'engage que là où la licence existe et la configuration est forcée ; tout le reste attend des adoptions justifiées, pas de l'audace.

---

## 6. La carte des refus

Sous (Ω_corpus1, ℛ₀), tout refus de W₀ est de l'une des deux espèces du 008 § 9 :

**Refus normatifs** — le monde permettrait d'aller plus loin ; ℛ₀ ne le permet pas. Ils **disparaîtront par enrichissement de ℛ** :

- les 20 actes en condition A-01 : la contradiction intra-acte reste ouverte (A-01 n'est pas en vigueur) — l'adoption de l'entrée de catalogue rendrait l'hypothèse artefactuelle formulable et dominante ;
- toute lecture d'architecture, de sous-système, de forme de version, de signataire : **aucun signal au-dessus du contenu n'existe sous ℛ₀** (I13 : sans convention d'interprétation adoptée, aucune étape interprétative n'est licite) — chaque interprétation candidate adoptée ouvrirait son étage de signaux, de relations et d'hypothèses ;
- toute élection au-dessus du contenu : même une fois les signaux ouverts, il faudra des licences de strate (des CE-xx justifiées).

**Refus structurels** — le monde lui-même ne départage pas. Ils ne céderont que par **enrichissement de Ω** (corpus plus riche, capacités nouvelles) — et certains jamais :

- les domaines de silence (L2) : les actes sans aucun signal au-delà du contenu (12 % du corpus sans certificat ni déclaration ni structure exploitable ; le fichier géant : contenu et conteneur, tout le reste ⊥) — aucune adoption n'y créera de prise ;
- les sous-déterminations (L1) : là où plusieurs découpages resteront également soutenus quels que soient les signaux ouverts (les 59 actes au sujet signataire commun en sont le prototype mesuré) ;
- les indiscernables par construction (L5) : contenu identique, identités logiques distinctes — refus définitif, acté depuis le 000.

La carte des refus normatifs **localise exactement les adoptions candidates** (008 § 9) ; celle des refus structurels borne ce qu'aucun registre n'obtiendra jamais.

---

## 7. Feuille de route des futures conventions

Familles candidates, **listées sans en adopter ni en définir aucune** — l'ordre reflète la structure de fondation (008 § 7), pas un calendrier :

1. **interprétations** — tables de lecture des codes structurels (architecture, sous-système), syntaxe des noms de signataires, formes canoniques des déclarations de version ;
2. **équivalences** — casse, normalisations, espaces, sur les domaines déclaratifs, chacune avec sa perte documentée ;
3. **artefacts** — l'adoption formelle de A-01 (candidate depuis le 002, requalifiée comme telle par I33 — voir l'écart en fin de document) et la gouvernance du catalogue ;
4. **attentes** — les attentes explicites (table Property d'une lecture nominale MSI, ressource de version d'un exécutable nominal), avec leur seul effet licite : l'écart à expliquer ;
5. **compositions** — les compositions licenciées de relations (signataire × lignée déclarée) préparant les strates hautes ;
6. **versions** — conventions d'ordre et de stratification version/variante ;
7. **familles** — conventions de parenté entre identités.

Les familles 5 à 7, et une partie de 1 et 4, **exigent l'ancrage du corpus 2** avant toute justification d'adoption honnête (I35) : les distributions du corpus 1 sont trop biaisées (75,8 % de MSI, 0 manifeste AppX) pour justifier des conventions de strates hautes.

---

## 8. Validation de ℛ₀

Propriétés à contrôler — par relecture, sans algorithme :

- **cohérence** (008 § 4) : deux conventions, aucune incompatibilité déclarée ni déclarable (leurs domaines s'emboîtent sans conflit) ; l'unique dépendance (CE-01 → EQ-01) est satisfaite ; la structure de fondation est un arc unique — acyclique ; chaque convention est une transformation unique (I14 : fonder la relation / licencier l'élection — deux actes, deux conventions, précisément la scission qu'exigeait la minimalité) ;
- **confluence** (008 § 3) : une seule licence, un seul signal fondé, aucune interaction entre conventions susceptible de dépendre d'un ordre — la confluence est triviale par construction ;
- **minimalité** : ℛ₀ est minimal au sens fort — retirer l'une ou l'autre convention fait tomber toute élection (l'ensemble minimal de chaque élection de W₀ est {EQ-01, CE-01}, unique) ;
- **complétude relative** (008 § 9) : ℛ₀ est complet pour la strate contenu sur Ω_corpus1 (aucun refus normatif n'y subsiste : toute classe multi-actes est élue, toute classe singleton est vide) — et volontairement incomplet partout au-dessus, où tous les refus normatifs du § 6 subsistent.

---

## 9. Validation de W₀

Propriétés attendues — sans moteur, sans calcul :

- **complétude** : chaque domaine-strate à espace d'hypothèses non trivial porte un acte — élection (112, strate contenu) ou refus motivé (tout le reste) ;
- **licences** : chaque élection cite CE-01 (I27) et sa chaîne restitue EQ-01, le signal, l'hypothèse, la concurrente écartée (I30) ; aucune élection ne cite rien d'autre — il n'y a rien d'autre à citer ;
- **prudence** : aucune élection n'existe hors des configurations forcées (maximale unique) et licenciées ; les niveaux assignés ne dépassent nulle part la portée de CE-01 (« certaine », strate contenu uniquement) ;
- **motifs des refus** : chaque refus porte son espèce (normatif / structurel) et son motif exact — la carte du § 6 est intégralement restituable depuis W₀ ;
- **cohérence d'état** (006 § 3) : trivialement satisfaite — les élections de contenu ne peuvent entrer en conflit ni entre elles (classes disjointes) ni verticalement (aucune élection au-dessus).

---

## 10. Conséquences : la conformité de tout moteur futur

> **Tout moteur conforme devra produire exactement W₀ à partir du couple (Ω_corpus1, ℛ₀). Sans liberté d'interprétation.**

C'est la conséquence conjointe de I11 (déterminisme de l'étage), I24 (relativité au seul index), I27 (toute élection licenciée) et I31 (indépendance à l'ordre d'application) : l'index détermine l'espace des hypothèses, ℛ₀ détermine les licences, la confluence interdit toute variation d'ordonnancement, et la prudence (P7, 007 § 3) fixe l'unique comportement en l'absence de licence. Il n'existe donc **aucun degré de liberté** entre (Ω_corpus1, ℛ₀) et W₀ — deux implémentations correctes produiront le même état, élection pour élection, motif pour motif.

W₀ devient ainsi le **premier oracle de conformité** du futur moteur : un critère de validation complet, défini avant qu'une seule ligne du moteur n'existe, vérifiable contre la base mesurée et versionnée du corpus 1.

---

## 11. Invariants

> **I33 — Toute convention adoptée appartient à un registre identifié.** Il n'existe pas d'adoption hors registre : une convention est en vigueur si et seulement si un registre identifié et versionné la contient. Corollaire rétroactif : avant l'ouverture de ℛ₀, **rien n'était adopté** — les conventions évoquées comme « actées » dans les documents antérieurs étaient des candidates formalisées (voir l'écart documenté en fin de document).

> **I34 — Tout état retenu dépend exclusivement du registre adopté.** Aucun état du monde ne mobilise une convention candidate, une intention, un usage ou une anticipation : seul le contenu de ℛ à la version indexée gouverne (renforcement terminal de I24 : le K de l'index est le registre, rien d'autre).

> **I35 — Le premier registre ne contient que des conventions justifiées empiriquement.** Toute entrée de ℛ₀ — et de ses états futurs — cite des faits mesurés et référencés à l'appui de son adoption. La théorie seule ne suffit pas à adopter : elle rend adoptable ; la mesure rend adopté.

> **I36 — L'instanciation d'un registre ne modifie jamais la théorie.** Les documents 000 à 008 sont indépendants du contenu de ℛ : aucune adoption, révision ou retrait ne peut exiger la réécriture d'une définition, d'une propriété ou d'un invariant. Si une instanciation révèle un défaut de la théorie, la théorie est révisée par un acte documentaire propre — jamais silencieusement par le registre (symétrie exacte de I16 : les conventions ne changent pas les propositions ; les registres ne changent pas la théorie).

---

## 12. Exemples — corpus 1, sous (Ω_corpus1, ℛ₀), aucune décision nouvelle

**E1 — Une élection permise.** L'une des 4 classes de contenu à 3 actes (mesurée, R10) : configuration forcée (unique maximale), soutien EQ-01 en R1, licence CE-01 → élection au niveau « certaine », chaîne complète restituable (3 `observation_id`, EQ-01 v1, CE-01 v1, la concurrente « coïncidence » écartée par inclusion de résidus). C'est l'une des 112 élections de W₀ — le contenu exact de ce que le premier moteur devra produire.

**E2 — Un refus normatif.** Les 20 actes en condition A-01 : la contradiction intra-acte est ouverte sous ℛ₀ (aucune entrée de catalogue en vigueur), la chaîne s'interrompt — refus motivé « contradiction ouverte ; entrée de catalogue candidate non adoptée ». Une transition de ℛ le lèverait ; aucune observation nouvelle n'est requise.

**E3 — Un refus structurel.** Le fichier géant (observation quasi vide : contenu et conteneur, tout le reste ⊥) : à toute strate au-dessus du contenu, l'espace d'hypothèses ne contient que les extrêmes indiscernables — refus « silence (L2) ». Aucun enrichissement de ℛ n'y changera rien ; seul un Ω plus riche (capacité nouvelle) pourrait donner prise.

**E4 — Une dépendance satisfaite.** Chaque élection de W₀ a pour ensemble minimal {EQ-01, CE-01} — unique. L'arc CE-01 → EQ-01 est satisfait dans ℛ₀ ; le retrait de EQ-01 seul rendrait ℛ₀ incohérent (dépendance insatisfaite, 008 § 4) et interdirait toute élection — la sévérité du 008 E4, désormais mesurable sur un registre réel.

**E5 — Une hypothèse volontairement non retenue.** Sur la même classe à 3 actes : l'hypothèse « ces trois actes relèvent d'une même variante » (strate variante). Elle est formulable (l'emboîtement ≡ₘ ⊆ variante la rend candidate naturelle — 005, E1) ; elle n'est soutenue par aucun signal de strate (aucune interprétation en vigueur) et licenciée par aucune convention → **non retenue**, refus normatif — et **non détruite** (I19) : elle attend, dans l'espace, les adoptions qui la concerneront. W₀ dit « mêmes octets » ; il ne dit pas encore « même variante » — c'est exactement la différence entre ce que le système sait et ce qu'il oserait.

---

## Conclusion — la phase documentaire fondatrice est achevée

Les documents 000 → 009 forment désormais un **cadre théorique complet et instancié** :

- **la théorie est stabilisée** — objets, propriétés, invariants I1–I36, limites ;
- **le premier registre existe** — ℛ₀, deux conventions adoptées, justifiées empiriquement, dans l'ordre de fondation ;
- **le premier état du monde est défini** — W₀, 112 élections de contenu au niveau maximal, refus motivés partout ailleurs, cartographiés ;
- **toute implémentation future devra être une traduction fidèle de cette théorie** — et son premier critère de conformité est fixé : produire exactement W₀ depuis (Ω_corpus1, ℛ₀), sans liberté d'interprétation.

**La phase documentaire fondatrice est achevée.** Ce qui suit — la conception du moteur d'identité — n'est plus de la théorie : c'est le contrat entre cette théorie et son implémentation.

---

## Récapitulatif des objets instanciés

| Objet | Instance | § |
|---|---|---|
| registre | ℛ-IC, état initial ℛ₀ (version 0, date logique 2026-07-05, autorité : propriétaire du projet) | 1 |
| convention adoptée | **EQ-01 v1** — égalité parfaite de contenu (fonde ≡ₘ et le signal « contenu identique » ; racine de fondation) | 3 |
| convention adoptée | **CE-01 v1** — élection par identité de contenu (niveau « certaine », strate contenu uniquement ; dépend de EQ-01) | 4 |
| état du monde | **W₀** — état prudent de (Ω_corpus1, ℛ₀) : 112 élections de contenu (108 paires, 4 triplets), refus motivés partout ailleurs, aucune identité logique retenue | 5 |
| carte des refus | normatifs (cèdent à ℛ) / structurels (cèdent à Ω ou jamais) | 6 |
| oracle de conformité | tout moteur conforme produit exactement W₀ depuis (Ω_corpus1, ℛ₀) | 10 |
| invariants | I33 adoption dans un registre identifié, I34 dépendance exclusive au registre, I35 justification empirique, I36 l'instanciation ne modifie jamais la théorie | 11 |

**Ce que ce document ne fait volontairement pas** : adopter quoi que ce soit au-dessus de la strate contenu, définir une seule convention de la feuille de route, construire le moteur, matérialiser ℛ₀ en fichier (représentation à définir en phase de conception).
