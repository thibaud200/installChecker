# Déclaration de conformité — moteur v3

**Nature** : une **consignation** — jamais une norme. La conformité est déclarée par version de moteur (011 § 8) et sa déclaration est consignée (014, É7) ; depuis le 017 § 3, la couverture déclarée en est une composante. Le présent document consigne l'exercice des clauses figées (011 § 8, 017, 018 et les raffinements des actes 019→026) par la version décrite ; il ne définit, ne raffine ni ne reformule aucune d'elles.

**Version déclarée** : le moteur v3 — `InstallChecker.Identity` et `InstallChecker.Identity.Access`, état du dépôt aux jalons V3-1 → V3-10 (2026-07-09 → 2026-07-11), la clôture V3-11 portant la présente consignation. Le tag de version relève du propriétaire, comme `identity-v1.0` pour la v1.

---

## 1. La couverture déclarée (017 §§ 2–3)

La présente version couvre **exactement deux familles** — la couverture est **inchangée depuis la v2** :

- interprétation
- élection

Les six autres familles du 014 § 5.1 — équivalence, priorité, attente, catalogue, stratification, composition — sont **hors couverture** : tout registre dont une convention en vigueur en relève produit l'erreur « registre non couvert » (017 § 6), vérifiée par C2 après bonne formation et cohérence (017 § 8).

La déclaration est matérialisée dans le moteur (`DeclarationDeCouverture` — constante par version, aucun point d'injection : I62, I63) ; sa coïncidence avec la présente consignation est gardée par test (`ConformiteV2Tests` : le compte exact — deux — et chacune des deux familles).

## 2. Les quatre points du 011 § 8, exercés à la frontière du porteur (018 § 8)

| Point | Exercice | Preuve |
|---|---|---|
| **1 — W₀ retrouvé** | (Ω_corpus1, ℛ₀) → W₀ par `Porteur.Deriver` : 116 actes, la caractérisation du 014 § 8 à l'acte près — et, **nouveauté v3**, le **test d'or bit à bit** : l'émission canonique du moteur est identique octet pour octet au fichier attendu `tests/oracle/W0-attendu.json`, produit **hors moteur** par l'oracle indépendant `tests/oracle/oracle-w0.py` (« script hors moteur, dans l'esprit des campagnes », 014 § 10, É7 ; « dans un autre langage, depuis les mêmes définitions », 013 § 12) — EXG-26 et le membre 4 d'EXG-39 exercés à la lettre, pour la première fois | `FormeCanoniqueTests`, `IdentityCommandTests`, `ConformiteV2Tests`, `PorteurTests` |
| **2 — batterie minimale (EXG-27)** | déterminisme : double dérivation **et double émission — identité bit à bit sur les octets émis** (EXG-18, enfin sur son objet propre) ; permutation d'ordre et localité : suites de couches (012 § 8) ; registre amputé d'EQ-01 → « registre incohérent », par le porteur ; test de cache **sans objet** : aucun cache construit (013 § 4 : le moteur recalcule toujours) — même relativité qu'en v1 et v2 | `FormeCanoniqueTests`, `AssemblageDeLetatTests`, `ConformiteV2Tests`, suites C1–C6 |
| **3 — contrat d'audit** | les sept questions du 011 § 7 exercées sur les actes de W₀ (les cinq questions par acte sur les 116 actes) ; ré-exercées par la frontière publique sur actes désignés — la désignation abrégée du 024 § 3 —, chaque réponse re-dérivée de l'index (I39) | `RestitutionDAuditTests`, `PorteurTests` |
| **4 — chaque erreur provoquée** | les **sept** erreurs de la table du 017 § 6, provoquées à la frontière du porteur sur des entrées construites, dans l'ordre total du 018 § 4, au premier échec (I67) ; et — **nouveauté v3** — la **défaillance interne** (014 C6 : « un défaut de C5, jamais une situation d'entrée ») provoquée sur ensembles d'actes forgés : signalée **comme telle** (011 § 4), hors des deux hiérarchies d'erreurs nommées — la table des sept erreurs reste close | `PorteurTests`, `ConformiteV2Tests`, `AssemblageDeLetatTests` |

## 3. Les sorties de transition (EXG-30, 013 § 9)

Fait nouveau de la v3, consigné : la cause de τ est **dérivée par C6** des entrées de l'invocation (026 § 3 — la suite des volets, possiblement vide entre index égaux) et les continuités sont **dérivées** selon le critère du 006 § 5 (026 § 4) ; plus aucune entrée d'appelant. Les **scénarios de transition du 013 § 9 sont joués** : transition Ω (le scénario du 006 E5, avec la continuité « même origine, domaine étendu »), adoption simulée d'une convention (volet ℛ), transition double, comparaison d'index égaux — « le moteur doit pouvoir produire τ entre deux index quelconques dont il connaît les deux membres » (EXG-30) est exercé. Preuves : `PorteurTests`, `AssemblageDeLetatTests`, `FormeCanoniqueTests` (l'émission canonique de τ). Aucune τ de production n'a été émise — les scénarios sont des tests.

## 4. La borne du 017 § 10 — sans objet pour cette version

L'exigence de provocation porte sur chaque erreur dont la condition est **satisfiable** pour la version validée (017 § 10). La couverture de la présente version laissant six familles hors d'elle, la condition de « registre non couvert » est satisfiable — et l'erreur est provoquée (§ 2, point 4). **Aucune vacuité à démontrer ni à consigner.**

## 5. La vérification de cohérence d'état — le partage consigné (report 4)

C6 vérifie, avant toute livraison de W (014 C6 : « garantit la vérification de cohérence d'état (006 § 3) avant livraison »), les clauses de la Définition 3 **décidables sur ses entrées déclarées** (l'ensemble des actes et l'index, 014 § 1) :

| Clause du 006 § 3 | Sort | Fondement |
|---|---|---|
| cohérence propositionnelle (P1) | **vérifiée**, par sa projection sur les actes : au plus un acte par identité (domaine, strate) — « exactement un acte » (014 C5) | l'identité de l'acte, 014 § 2 |
| dépendances « en vigueur dans K (versions comprises) » | **vérifiée** — l'index énumère les couples en vigueur (014 § 7.2) | 006 § 3 |
| conventions (I13) / I27 | **vérifiée** par sa trace sur les actes : licences non vides et en vigueur | I27, 014 C5 |
| emboîtement (I17), Obs ⊆ Ω, préférences (dominance), complétude des refus | **garanties par C5** (014 C5) — les vérifier exigerait de « voir plus bas », interdit (012 § 2 ; la clause « ignore » de C6) | 012 § 2 ; 014 §§ 1, 3 |

Tout échec est une **défaillance interne** — jamais une huitième erreur contractuelle. Preuves : `AssemblageDeLetatTests` ; la conformité de W₀ démontre que la vérification passe sans effet sur toute entrée réelle.

## 6. La forme canonique matérielle (report 3)

La forme canonique des sorties est **définie par le moteur** (EXG-18 : « le moteur définit une forme canonique de sa sortie ») et **consignée** dans `docs/conformite/forme-canonique-materielle.md` (version 1) — la présente déclaration la référence sans la redéfinir. Le consommateur émet W « tel que produit, sous la forme canonique du 013 § 4 » (018 § 6 — l'obligation honorée). **Procédure d'identité à trois voies, rejouable par un tiers depuis le dépôt seul** : (1) `python tests/oracle/oracle-w0.py` régénère le fichier attendu ; (2) `installchecker identity derive tests/oracle/corpus1-postA1.db registre` émet W₀ par la commande réelle ; (3) l'égalité des trois productions est **octet pour octet** — exercée en continu par les tests d'or (`FormeCanoniqueTests`, `IdentityCommandTests`). Le fichier attendu est protégé des conversions de fin de ligne (`.gitattributes`).

## 7. Les exceptions d'I59 exercées pendant la campagne

I59 (« la conformité v2 contient la conformité v1 ») a été exercé trois fois pendant la campagne — au titre que les actes 021, 025 et 026 lui donnent chacun expressément —, sous ses deux seules exceptions prévues :

- **branche 2** (révision documentaire préalable de l'oracle, EXG-28/I36) : une fois — l'acte **021** (la carte des refus de W₀ réconciliée, **sans changement de sortie**) ; le 025 a démontré qu'elle demeure **le seul cas** de la série ;
- **branche 1** (correction publiée d'une non-conformité antérieure, 011 § 9) : deux fois — l'acte **025** (l'identité d'un état d'Ω : `docs/conformite/ecart-publie-identite-etat-omega.md`) et l'acte **026** (la cause et les continuités de τ : `docs/conformite/ecart-publie-cause-continuites.md`) ;
- le jalon **V3-10** n'a exercé **aucune** exception : aucun objet de sortie ne change, la forme matérielle naît sans prédécesseur (démontré au rapport du jalon).

L'écart publié de la v2 (`docs/conformite/ecart-publie-v1.md`, 017 § 9) demeure en vigueur, inchangé. Les trois écarts constituent l'ensemble complet des déclarations rétroactives ; chacune est bornée à sa surface et laisse « la validité des sorties passées sous leur index » entière (I23, I57).

## 8. La résorption des inventaires de relativité v1 et v2

Les déclarations v1 et v2 **ne sont pas réécrites** (le patron de la v1 § 3 : « cette déclaration postérieure ne réécrit rien ») — leurs inventaires, datés, sont résorbés comme suit :

| Ligne d'inventaire | Source | Résorption |
|---|---|---|
| la forme canonique matérielle et le fichier W₀ attendu « n'ont pas été livrés » (report 3) | v1 § 2 ; v2 § 5 | **V3-10** — consignation de forme, `FormeCanonique`, oracle indépendant, test d'or |
| l'identité d'un état d'Ω sous le régime du 014 § 7.2 (report 5) | v2 § 5 | acte **025** (V3-7) + écart publié |
| la cause de τ fournie par l'appelant (report 9) | v2 § 5 | acte **026** (V3-8) + écart publié |
| la vérification de cohérence d'état de C6 inchangée (report 4) | v2 § 5 | **V3-9** (§ 5 ci-dessus) |
| « le consommateur É9 n'est pas livré » | v2 § 5 | **V2-7** (la déclaration v2, datée V2-1 → V2-6, précédait sa livraison) |

## 9. Inventaire de relativité v3 (le patron du 016 § 1.3)

La présente déclaration est **relative à l'inventaire suivant, connu, consigné et daté — jamais absolue** :

- les **réponses d'audit** n'ont pas de forme canonique matérielle consignée (011 § 3 : « chaque sortie possède une forme canonique ») — le manque, signalé par la consignation de forme (§ 4), est consigné ici ; la conformité de l'audit est déclarée au niveau du contenu des réponses ;
- la **demande de transition n'est pas exposée par la CLI** : le porteur la sert (`Transitionner`), la commande ne l'offre pas — conforme à la lettre du 018 § 6 (« le cas échéant ») ;
- une **élection au domaine vide** (défaut interne d'une espèce qu'aucune lettre ne nomme) échouerait sans être signalée *comme telle* — une dette d'hygiène, pas une anomalie contractuelle.

---

**Validation** : la présente déclaration prend effet à la validation du propriétaire (011 § 8 ; méthode du projet). Elle est rejouable par un tiers depuis le dépôt seul : chaque rubrique cite les tests et artefacts versionnés qui l'exercent — 148 tests Identity et 50 tests CLI/pipeline, tous verts, 0 avertissement, à la date de la présente consignation.
