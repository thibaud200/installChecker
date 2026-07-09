# Déclaration de conformité — moteur v2

**Nature** : une **consignation** — jamais une norme. La conformité est déclarée par version de moteur (011 § 8) et sa déclaration est consignée (014, É7) ; depuis le 017 § 3, la couverture déclarée en est une composante. Le présent document consigne l'exercice des clauses figées (011 § 8, 017, 018) par la version décrite ; il ne définit, ne raffine ni ne reformule aucune d'elles.

**Version déclarée** : le moteur v2 — `InstallChecker.Identity` et `InstallChecker.Identity.Access`, état du dépôt aux jalons V2-1 → V2-6 (2026-07-09). Le tag de version relève du propriétaire, comme `identity-v1.0` pour la v1.

---

## 1. La couverture déclarée (017 §§ 2–3)

La présente version couvre **exactement deux familles** :

- interprétation
- élection

Les six autres familles du 014 § 5.1 — équivalence, priorité, attente, catalogue, stratification, composition — sont **hors couverture** : tout registre dont une convention en vigueur en relève produit l'erreur « registre non couvert » (017 § 6), vérifiée par C2 après bonne formation et cohérence (017 § 8).

La déclaration est matérialisée dans le moteur (`DeclarationDeCouverture` — constante par version, aucun point d'injection : I62, I63) ; sa coïncidence avec la présente consignation est gardée par test (`ConformiteV2Tests`). Modifier la couverture, c'est changer de version de moteur, revalidée intégralement (017 § 3).

## 2. Les quatre points du 011 § 8, exercés à la frontière du porteur (018 § 8)

| Point | Exercice | Preuve |
|---|---|---|
| **1 — W₀ retrouvé** | (Ω_corpus1, ℛ₀) → W₀ par `Porteur.Deriver` : 116 actes — 112 élections de strate contenu (108 paires, 4 triplets), niveau « certaine », motif `unique-maximale`, licences [(CE-01, 1)], dépendances [(CE-01, 1), (EQ-01, 1)], dette vide ; 4 refus motivés de domaine maximal (497 actes) — la caractérisation du 014 § 8, à l'acte près | `ConformiteV2Tests`, `PorteurTests` |
| **2 — batterie minimale (EXG-27)** | déterminisme : double dérivation par le porteur, identique champ par champ ; permutation d'ordre et localité : suites de couches (012 § 8) ; registre amputé d'EQ-01 → « registre incohérent », **par le porteur** ; test de cache **sans objet** : aucun cache construit (013 § 4 : le moteur recalcule toujours) — même relativité qu'en v1 | `ConformiteV2Tests`, suites C1–C6 |
| **3 — contrat d'audit** | les sept questions du 011 § 7 exercées sur les actes de W₀ par la suite C7 (validation par morceaux, 012 § 8) ; ré-exercées par la frontière publique sur actes désignés, chaque réponse re-dérivée de l'index (I39) | `RestitutionDAuditTests`, `PorteurTests` |
| **4 — chaque erreur provoquée** | les **sept** erreurs de la table du 017 § 6, provoquées **à la frontière du porteur** sur des entrées construites : Ω absent, Ω incompatible, Ω invalide (adaptateur SQLite réel sur mini-bases fabriquées) ; registre absent, malformé, incohérent, non couvert (fixtures réelles versionnées) — dans l'ordre total du 018 § 4, au premier échec (I67) | `ConformiteV2Tests`, `PorteurTests` |

## 3. La borne du 017 § 10 — sans objet pour cette version

L'exigence de provocation porte sur chaque erreur dont la condition est **satisfiable** pour la version validée (017 § 10). La couverture de la présente version laissant six familles hors d'elle, la condition de « registre non couvert » est satisfiable — et l'erreur est provoquée (point 4). **Aucune vacuité à démontrer ni à consigner.**

## 4. L'écart publié (017 § 9)

La présente version exerce l'exception prévue d'I59 — la correction publiée d'une non-conformité antérieure (011 § 9). L'écart est matérialisé dans `docs/conformite/ecart-publie-v1.md` : la classe d'index, la justification uniforme et acte par acte, les **deux sous-classes** du comportement conforme — toutes deux démontrées de bout en bout par test — et la déclaration rétroactive de non-conformité de la v1 sur cette classe seule.

## 5. Inventaire de relativité (le patron du 016 § 1.3)

La présente déclaration est **relative à l'inventaire suivant, connu, consigné et daté — jamais absolue** :

- la forme canonique **matérielle** de W n'est pas livrée (report 3) : la conformité est déclarée au niveau du contenu logique de W, comme l'était celle de la v1 (016 § 1.3) ;
- l'identité d'un état d'Ω suit le régime actuel du 014 § 7.2, dont la révision relève du report 5 ;
- la cause de τ est fournie par l'appelant et transportée telle quelle, sans vérification (régime consigné au 018 § 3 ; report 9) ;
- la vérification de cohérence d'état de C6 est inchangée depuis la v1 (report 4) ;
- le consommateur É9 n'est pas livré (jalon V2-7) — hors du périmètre du 011 § 8, qui porte sur le moteur.

---

**Validation** : la présente déclaration prend effet à la validation du propriétaire (011 § 8 ; méthode du projet). Elle est rejouable par un tiers depuis le dépôt seul : chaque ligne du § 2 cite les tests versionnés qui l'exercent.
