# 013 — Architecture d'implémentation et plan de réalisation

**Statut** : quatrième document de conception de la série `docs/identity/`. S'appuie sur les documents 000→012, figés. Premier document autorisé à parler de logiciel concret : assemblies, projets, fichiers, formats, organisation de la solution.
**Nature** : transformer la machine abstraite (012) en architecture d'implémentation, en démontrant que **chaque décision technique est une conséquence de la théorie** — jamais un choix arbitraire. Toujours aucun code, aucun pseudo-code, aucun algorithme, aucune structure interne, aucune optimisation. On décrit l'architecture, jamais son fonctionnement interne.

---

## 1. Traduction des couches C1→C7 en composants

### 1.1 Le découpage

Trois assemblies et un projet de tests — le minimum qui matérialise les frontières théoriques, et rien de plus :

| Composant | Contenu | Frontière théorique matérialisée |
|---|---|---|
| **`InstallChecker.Identity`** | le moteur pur : les modèles logiques (observations, conventions, signaux, hypothèses, actes, W, τ, chaînes) et les couches **C3→C7** en espaces de noms internes (`Signals`, `Hypotheses`, `Acts`, `State`, `Audit`), plus les **ports** — les interfaces logiques « source d'observations » (consommée par C1) et « source de registre » (consommée par C2) | la boîte noire du 011 ; **zéro dépendance de paquet** — c'est vérifiable dans le fichier projet, et c'est le test le plus simple d'EXG-14 |
| **`InstallChecker.Identity.Access`** | les adaptateurs : **C1** (projection de la base d'observations SQLite vers le modèle logique) et **C2** (projection du répertoire `registre/` vers le référentiel de conventions) | la frontière modèle logique / représentation physique (I46) ; seul composant à référencer un pilote de stockage |
| **`InstallChecker.Identity.Tests`** | les tests des cinq niveaux de validation (§ 6) et les actifs de test (registres cassés, mini-bases fabriquées) | l'architecture de validation (§ 6) ; consomme l'oracle archivé (§ 11) |
| *(ultérieur)* commande `identity` dans la CLI existante | un consommateur (011 § 1) : invoque le moteur, restitue W et l'audit | l'acteur « consommateur » — jamais fusionné avec le moteur |

### 1.2 Dépendances entre composants

  `Identity` ← `Identity.Access` ← CLI (plus tard)
  `Identity.Tests` → `Identity`, `Identity.Access`

- `Identity` ne référence **rien** : ni `Identity.Access` (inversion : les adaptateurs implémentent les ports du moteur), ni `InstallChecker.Core` (§ 2), ni aucun paquet.
- `Identity.Access` référence `Identity` (pour implémenter ses ports) et le pilote SQLite.
- Ce sens unique des références est la traduction directe du graphe du 012 § 3 : les couches hautes définissent ce dont elles ont besoin ; les représentations physiques s'y conforment.

### 1.3 Pourquoi ce découpage respecte I41→I44

- **I41** (aucune modification amont) : les objets circulant entre couches sont des modèles du moteur pur, conçus comme valeurs ; les adaptateurs produisent ces valeurs et ne les revoient jamais — aucun canal de retour n'existe dans les références (§ 1.2) ;
- **I42** (remplaçabilité) : chaque couche C1–C7 vit derrière un contrat interne (les ports pour C1/C2, les frontières d'espaces de noms pour C3–C7) ; remplacer l'adaptateur SQLite par un autre support est un nouveau projet d'accès, sans toucher `Identity` — la démonstration du 012 § 9 s'applique telle quelle ;
- **I43** (déterminisme par entrées) : le moteur pur n'ayant aucune référence, il n'a **matériellement accès** ni à l'horloge système via un paquet, ni au réseau, ni au disque — les seules entrées possibles passent par les ports ; l'architecture rend EXG-02 difficile à violer par accident ;
- **I44** (acyclicité) : le graphe des références d'assemblies est acyclique par construction (l'outillage .NET l'impose), et il est l'image du graphe logique du 012 § 3.

---

## 2. Place dans la solution existante

La solution actuelle contient `src/InstallChecker` (CLI), `src/InstallChecker.Core` (extracteurs), `tests/InstallChecker.Tests`. Le moteur s'y insère ainsi :

- `src/InstallChecker.Identity/` et `src/InstallChecker.Identity.Access/` — nouveaux projets, ajoutés à la solution ;
- `tests/InstallChecker.Identity.Tests/` — nouveau projet de tests, distinct des tests du pipeline (deux systèmes, deux suites) ;
- `registre/` — le registre matérialisé (§ 3), à la racine du dépôt ;
- `tests/oracle/` — l'artefact d'oracle (§ 11).

**La frontière avec `InstallChecker.Core` est une frontière de données, pas de code** : le moteur et le pipeline ne partagent **aucune référence d'assembly, dans aucun sens**. Leur seul point de contact est la base d'observations — le pipeline l'écrit, le moteur la lit, et le contrat entre eux est le schéma documenté (`user_version = 1`) et sa sémantique (001). Conséquences :

- **Core reste uniquement producteur d'Ω** : rien de l'identité n'y entre, aucune de ses signatures ne change, le pipeline figé reste figé — c'est la séparation des rôles du 011 § 1 rendue matérielle ;
- **le moteur ne dépend jamais des extracteurs** (EXG-14) : il ne peut pas — il ne référence pas l'assembly qui les contient ;
- les deux systèmes évoluent, se testent et se livrent indépendamment.

---

## 3. Matérialisation concrète de ℛ

### 3.1 Emplacement et organisation

```
registre/
  conventions/
    EQ-01/ v1.md
    CE-01/ v1.md
  historique.md
  etat.md
```

- **une convention = un répertoire** (l'identifiant pérenne, 004 § 1.1) ; **une version = un fichier immuable** — jamais édité après adoption ; réviser = ajouter `v2.md` (le miroir exact de l'append-only d'Ω : les versions de conventions ne se corrigent pas plus que les observations, I1 transposé par la symétrie du 007 § 8) ;
- **`historique.md`** — le journal append-only des transitions : une entrée par acte de gouvernance (date, type — adoption/révision/retrait/remplacement/scission/fusion —, convention, version, justification, autorité), conformément au 012 § 7 ;
- **`etat.md`** — l'état courant : la liste des versions en vigueur et les incompatibilités déclarées ; c'est **la valeur que C2 projette** ; l'identité d'un état de ℛ est le contenu du répertoire `registre/`, pas un numéro externe.

### 3.2 Format : texte structuré lisible (Markdown à champs fixes)

Choix : chaque fichier de convention est un document Markdown à **sections de champs imposées** (les champs du 004 Déf. 1 + 007 § 5). Justification par les invariants — pas par préférence :

- **I13 (explicitation)** : chaque champ obligatoire est une section nommée ; un champ manquant est visible à l'œil et détectable par C2 (« registre malformé », 011 § 5) ;
- **traçabilité (P5) et gouvernance humaine (007 § 9)** : une convention est un acte normatif dont la justification est de la prose relue par un humain ; un format de données (JSON/YAML/XML) enchâsserait la prose dans des chaînes échappées, dégraderait les diffs git — l'outil central de revue des transitions — et ajouterait une dépendance de parseur à `Identity.Access` sans bénéfice (I46 : moins la représentation physique impose, mieux c'est) ;
- **cohérence du dépôt** : la série 000→013 est déjà l'espace normatif du projet, en Markdown ; le registre est de la même nature documentaire.

La spécification exacte des champs (le contrat de lecture de C2) relève du document 014.

### 3.3 Lien entre adoption et commit Git

- **un acte de gouvernance = un commit** touchant exclusivement `registre/` : l'ajout du fichier de version, l'entrée d'`historique.md`, la mise à jour d'`etat.md` — trois écritures, un seul acte, un seul commit (l'unitarité du 007 § 9) ;
- le commit **est** l'événement daté ; le journal porte la justification et l'autorité ; l'historique git fournit l'immuabilité de la trace — sans que l'identité de ℛ dépende de git (c'est le contenu du répertoire qui identifie l'état, I46 : git est un support, pas un constituant) ;
- **ℛ₀** sera matérialisé par le premier commit de gouvernance (jalon É1, § 8) : `EQ-01/v1.md`, `CE-01/v1.md`, les deux entrées d'adoption au journal (date logique 2026-07-05, autorité : propriétaire du projet, conformément au 009 § 1), l'état initial ;
- **les transitions futures** suivent le même protocole, sans exception — y compris l'adoption de A-01 et de toute la feuille de route du 009 § 7.

---

## 4. Matérialisation de W

- **Format concret** : un document JSON unique par invocation. Justification : la forme canonique exige un encodage intégralement spécifiable (EXG-18) et les consommateurs sont des outils ; JSON est structurellement adapté aux actes réguliers de W, déjà présent dans l'écosystème du projet (sortie `--json` de la CLI), et sa canonicalisation est spécifiable sans dépendance ;
- **Encodage canonique** : UTF-8 sans BOM, fins de ligne uniformes, culture invariante, aucun espace optionnel variable, ordre des champs fixé par la spécification (014), nombres et chaînes sous forme normalisée — de sorte que l'identité bit à bit du 012 § 5 soit un simple test d'égalité de fichiers ;
- **Ordre canonique** : celui du 012 § 5 — clé totale dérivée du seul contenu identitaire (strate, puis domaine par identifiants d'actes croissants) ; jamais l'ordre de calcul ;
- **Contenu** : l'index en tête (identification de l'état d'Ω et de l'état de ℛ), puis la totalité des actes — et rien du contenu interdit (012 § 5 : aucune métadonnée de calcul, aucun contexte d'observation) ;
- **Stockage et cache** : W n'est **jamais stocké par le moteur** — il est émis. Si un consommateur conserve le fichier, c'est un cache au sens d'EXG-24 : re-dérivable, vérifiable par re-émission et comparaison bit à bit, invalide dès que l'index change (I5, I47). **Aucun mécanisme de cache n'est construit en v1** : le moteur recalcule toujours — un cache est une optimisation, et aucune optimisation n'existe avant benchmark (§ 10, CLAUDE.md § 10).

---

## 5. Contrat concret avec Ω

- **Le modèle logique** vit dans `Identity` : actes identifiés, attributs par capacité, valeurs brutes, ⊥ — le modèle du 001, sans contexte (le filtrage d'A1 est une responsabilité de C1, 012 § 2) ;
- **Le port** vit dans `Identity` : l'interface logique « source d'observations » que le moteur consomme — définie par le besoin du moteur, pas par la forme de la base ;
- **L'adaptateur** vit dans `Identity.Access` : il ouvre la base SQLite produite par le pipeline, vérifie `user_version = 1` (sinon : erreur « Ω incompatible », 011 § 5), vérifie l'invariant 1:1 (sinon : « Ω invalide »), et projette les lignes vers le modèle logique. **Lui seul** connaît les noms de tables, les colonnes, le pilote ;
- **La frontière exacte** : tout ce qui mentionne SQLite, un nom de table, un type de colonne est dans `Identity.Access` ; tout ce qui mentionne un acte, un attribut, ⊥ est dans `Identity`. Le moteur ne connaît jamais la persistance — matériellement : l'assembly `Identity` n'a pas la capacité technique d'ouvrir un fichier de base ;
- **Remplacement** : un futur support (autre moteur de stockage, flux réseau, base de test en mémoire) est un nouvel adaptateur du même port — le moteur est inchangé, sa conformité est préservée (I42, 012 § 9). Les tests utiliseront d'ailleurs un adaptateur de test comme preuve vivante de cette substituabilité.

---

## 6. Architecture de validation

Cinq niveaux, chacun avec son oracle :

| Niveau | Porte sur | Oracle |
|---|---|---|
| **composant** | un contrat de couche isolé (012 § 8) : bonne formation des signaux, clôture de provenance d'une hypothèse, canonicité d'un W | micro-cas dérivables à la main, construits dans les tests |
| **couche** | la totalité d'un contrat : substitution d'un adaptateur de test au vrai adaptateur → mêmes objets en sortie (012 § 9 rendu exécutable) | l'égalité des sorties entre implémentations du même port |
| **registre** | le prédicat de cohérence (008 § 4) et le contrat d'erreur | ℛ₀ (valide) + une collection de registres délibérément cassés (malformés, dépendance manquante, incompatibilité en vigueur) versionnée dans les actifs de test |
| **état** | la cohérence structurelle de W (006 § 3), la forme canonique, le déterminisme (double invocation → bit à bit) | W₀ sous sa forme canonique, versionné comme fichier attendu dès le jalon É6 |
| **complète** | la conformité du 011 § 8 : oracle + batterie + audit + erreurs | le quadruplet EXG-39 : documents 000→009, ℛ₀ matérialisé, l'artefact corpus (§ 11), W₀ |

Chaque niveau est exécutable **sans les niveaux supérieurs** (012 § 8 : validation par morceaux) — c'est ce qui permet au plan (§ 8) de livrer des étapes indépendamment validables.

---

## 7. W₀ comme premier objectif logiciel

**Le premier jalon de conformité est la production de W₀** — exactement lui, bit à bit sous la forme canonique. Capacités **minimales nécessaires** :

- C2 lisant `registre/` et validant ℛ₀ (deux conventions, un arc de dépendance) ;
- C1 lisant l'artefact corpus (§ 11) — pour W₀, seuls les identifiants d'actes et les attributs de contenu sont consommés ;
- C3 réduit au **seul signal fondé par EQ-01** (« contenu identique ») ;
- C4 réduit aux hypothèses de strate contenu et à l'unique comparaison de domination (003 § 9) ;
- C5 réduit à la licence CE-01 et aux refus motivés des strates non couvertes ;
- C6 assemblant le W canonique complet (112 élections + les refus, indexé).

**Tout le reste est différable** — et doit l'être : les autres familles de conventions (aucune n'est en vigueur), τ (exige deux index — différé au premier scénario de transition), C7 au-delà des chaînes triviales de contenu, tout cache, toute performance. La discipline du moindre engagement (P7) s'applique au plan de réalisation lui-même : on construit ce que ℛ₀ exige, rien de ce que ℛ₁ exigera peut-être.

---

## 8. Plan de réalisation incrémental

Conforme à la méthode du projet : **une fonctionnalité par étape ; chaque étape livre un dépôt propre, compilable, entièrement testé, validable indépendamment, commité** ; validation du propriétaire entre les étapes.

| Étape | Livre | Testable par |
|---|---|---|
| **É1** | `registre/` matérialisé : ℛ₀ (EQ-01 v1, CE-01 v1, journal, état) + spécification du format des champs | relecture (niveau registre, § 6) — aucun code |
| **É2** | projets `Identity` / `Identity.Access` / `Identity.Tests` vides mais câblés dans la solution + modèles logiques d'observations et de conventions | compilation, tests de modèle (valeurs, égalités) |
| **É3** | C2 : lecture du registre + prédicat de cohérence + contrat d'erreur registre | ℛ₀ accepté ; registres cassés → erreurs nommées |
| **É4** | C1 : adaptateur SQLite (validation `user_version`, 1:1, projection) + adaptateur de test en mémoire | l'artefact corpus (§ 11) + mini-bases fabriquées ; erreurs « Ω invalide / incompatible » |
| **É5** | C3 réduit : le signal « contenu identique » sous EQ-01 | 381 classes retrouvées sur l'artefact (oracle : les annexes de campagne) |
| **É6** | C4+C5+C6 réduits : hypothèses de contenu, domination, licence CE-01, refus, W canonique | forme canonique stable ; double invocation bit à bit |
| **É7** | **conformité W₀** : le test d'or complet + la batterie EXG-27 (déterminisme, permutation, localité, registre amputé, erreurs) | le niveau « complète » du § 6 — le moteur v1 est déclaré conforme |
| **É8** | C7 : les sept questions d'audit sur les actes de W₀ | chaque acte de W₀ répond aux sept questions ; réponses re-dérivables |
| **É9** | la commande `identity` de la CLI (consommateur : invocation, émission de W, restitution d'audit) | bout-en-bout sur l'artefact ; la CLI du pipeline reste intacte |

Chaque étape ultérieure (adoption de A-01, premières interprétations, τ, corpus 2…) suivra le même régime : une transition de ℛ est un commit de gouvernance (§ 3.3) ; une capacité nouvelle du moteur est une étape de ce tableau — jamais les deux ensemble.

---

## 9. Stratégie de tests

Catégories et rôles — pas d'implémentation :

| Catégorie | Rôle |
|---|---|
| **conformité** | le test d'or : (artefact, ℛ₀) → W₀ bit à bit ; s'étend à chaque nouvel état de ℛ par de nouveaux W attendus versionnés |
| **reproductibilité** | double invocation, permutations de parcours, exécutions répétées → identité bit à bit (EXG-18–20) |
| **audit** | chaque acte répond aux sept questions ; chaque réponse coïncide avec sa re-dérivation (I39) |
| **localité** | ajout d'actes sans rapport → les actes existants de W inchangés (EXG-21) |
| **non-régression** | l'intégralité des W attendus passés rejouée à chaque modification du moteur (011 § 9 : sorties identiques sur index passés) |
| **registres incohérents** | la collection de registres cassés → chaque erreur nommée du contrat (011 § 5) |
| **caches** | différée avec le cache lui-même ; le jour venu : falsification → détection (EXG-24) |
| **transitions Ω** | scénario mesuré du 006 E5 (l'acte ajouté) rejoué : τ correct, conservation hors du domaine touché |
| **transitions ℛ** | retrait simulé d'EQ-01 → « registre incohérent » ; adoption simulée d'une convention de test → refus normatifs convertis, le reste intact |

Les actifs de test (mini-bases, registres cassés, W attendus) sont versionnés avec les tests : la validation est **rejouable par un tiers depuis le dépôt seul** — la définition de la reproductibilité que le projet applique depuis la première campagne.

---

## 10. Performance

Première apparition officielle — et son cadre est strict :

- **la performance devient un critère à partir du jalon É7 inclus-exclu** : *aucun* travail de performance (mesure comprise) avant que le moteur soit déclaré conforme. Justification par EXG-39 : l'oracle définit la correction, et « un moteur rapide et faux est faux » — optimiser avant la conformité, c'est optimiser une fonction dont on ignore si c'est la bonne ;
- **après É7** : premier benchmark = la dérivation complète du corpus 1 par le moteur conforme — la baseline, au sens du CLAUDE.md § 10 (« aucune optimisation sans benchmark »), établie sur l'artefact versionné donc rejouable ;
- **la performance ne peut jamais amender la conformité** : toute optimisation (cache, incrémental, parallélisme — les libertés du 010 § 9) doit repasser l'intégralité des tests de conformité et de non-régression bit à bit ; une optimisation qui change W n'est pas une optimisation, c'est un bug ;
- l'objectif d'échelle du projet (centaines de milliers de fichiers) entre au calendrier avec le corpus 2 — après É9, jamais avant.

---

## 11. Archivage de l'oracle — décision

**Décision d'architecture** (exécutoire dès la validation de ce document) :

- le fichier `corpus1-postA1.db` — la base d'observations de la campagne corpus 1 post-A1 (497 actes, 430 080 octets, produite par le pipeline figé au commit `e14b575`) — **devient un artefact officiel du dépôt**, versionné sous `tests/oracle/corpus1-postA1.db` ;
- il constitue **le support permanent de l'oracle** défini au 010 (EXG-39) : c'est le Ω du couple (Ω_corpus1, ℛ₀) dont tout moteur conforme doit dériver W₀ ;
- **toute validation future repose sur cet artefact versionné** — les tests de conformité le référencent par son chemin de dépôt, jamais par un chemin externe ;
- notes d'intégrité : la base contient les colonnes contextuelles (`path`, `scanned_at`) — hors domaine identitaire (A1), leur présence est sans effet sur W₀ et les chemins qu'elles révèlent sont ceux d'un environnement de mesure local, sans sensibilité ; la taille (0,41 Mo) est négligeable pour le dépôt ; le fichier est immuable — toute campagne future produit un artefact *nouveau*, jamais une mise à jour de celui-ci (append-only au niveau des artefacts, cohérent avec tout le reste).

---

## 12. Nouveaux invariants — démontrés

> **I45 — Toute frontière logicielle correspond à une frontière théorique.**
> *Démonstration par énumération exhaustive des frontières introduites* : `Identity` ↔ le reste = la boîte noire du 011 ; `Identity` ↔ `Identity.Access` = la frontière modèle logique / représentation physique (I46, EXG-14) ; `Identity` ↔ `Core` (absence de référence) = la séparation producteur/moteur (011 § 1) ; `registre/` = ℛ (007–009) ; `tests/oracle/` = EXG-39 ; les espaces de noms internes = C1–C7 (012). Aucune frontière sans antécédent théorique — et la règle est prospective : toute frontière future doit nommer le sien, sinon elle est refusée. ∎

> **I46 — Aucun composant ne dépend d'une représentation physique.**
> *Démonstration* : les représentations physiques du système sont la base d'observations, les fichiers du registre et les fichiers émis de W. La première n'est connue que de l'adaptateur C1, la deuxième que de l'adaptateur C2 — tous deux dans `Identity.Access`, derrière les ports du moteur ; la troisième est une sortie, dont aucun composant ne dépend (le cache éventuel est vérifié contre la re-dérivation, jamais cru sur parole). Le moteur pur n'ayant aucune référence de paquet, sa non-dépendance est vérifiable mécaniquement. ∎

> **I47 — Toute représentation persistée est dérivable.**
> *Portée* : les représentations **produites par le moteur** (W émis, caches futurs, réponses d'audit conservées) — Ω et ℛ, les deux sources, sont persistées mais premières, par définition (I34, EXG-01). *Démonstration* : tout objet produit est une valeur re-dérivable de l'index (I5, I10, I39) ; sa représentation persistée est la forme canonique d'une valeur re-dérivable, donc re-productible bit à bit à la demande (EXG-18) ; un persisté non re-dérivable est détectable (comparaison) et invalide par définition (EXG-24). ∎

> **I48 — Toute évolution de l'architecture préserve les contrats publics.**
> *Démonstration* : le contrat public (011) est indépendant de l'implémentation (I40) ; l'architecture de validation (§ 6) rejoue l'intégralité de la conformité et de la non-régression bit à bit à chaque modification (§ 9) ; une évolution qui altère un contrat public échoue donc mécaniquement à la validation complète et ne peut être livrée conforme. La préservation n'est pas une promesse : c'est une barrière exécutable. ∎

---

## Conclusion

L'architecture est posée : trois assemblies dont un moteur pur sans référence, une frontière de données avec le pipeline (aucun lien de code), un registre en texte structuré gouverné par commits, un W canonique en JSON spécifié au bit près, cinq niveaux de validation avec leurs oracles, un plan en neuf étapes conformes à la méthode du projet, la performance rigoureusement subordonnée à la conformité, et l'oracle définitivement ancré dans le dépôt. Chaque choix est adossé à un invariant ou à une exigence — le document 014 peut entrer dans la conception détaillée des composants.

**Ce que ce document ne fait volontairement pas** : écrire du code, spécifier les champs exacts des formats (014), détailler les contrats internes des composants (014), planifier au-delà de É9.
