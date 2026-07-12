# Module Duplicate Files — Plan d'implémentation, révision 2 (conception, sans code)

> **Nature** : révision 2 du plan d'implémentation du module (périmètre v1 = un état Ω désigné, strate contenu — spec D2). **« v1 » désigne ici le périmètre du module, pas la version du plan.** Ce document est la *révision 2 du plan* ; la *révision 1* (`2026-07-11-module-duplicate-files-v1-implementation.md`, avec code TDD) est conservée intacte pour relecture et comparaison.
> **Portée** : décrit les répercussions de la relecture `docs/superpowers/reviews/2026-07-11-relecture-module-duplicate-files.md` (constats P1→P7) sur la conception et sur chaque tâche du plan rev1. **Sans code** : signatures et structures décrites en prose et en tables, pas en C#. La traduction en étapes TDD codées se fera au moment de l'exécution, à partir de ce document et du plan rev1.

**But (inchangé)** : à partir d'un état Ω désigné, regrouper les fichiers identiques en octets (strate contenu, CE-01/EQ-01), suggérer un ordre de conservation par groupe via une politique métier versionnée, produire un rapport **actionnable** — et énoncer honnêtement les capacités non encore disponibles (regroupement de versions).

**Référence de conception** : spec `2026-07-11-module-duplicate-files-design.md` (décisions D1→D6), amendé par les constats P1→P7 ci-dessous.

---

## 1. Ce qui change par rapport à la rev1

Quatre changements de fond (P1→P5), deux notes sans action immédiate (P6, P7). Architecture, direction des dépendances, artefact de politique versionné et démarche TDD : **inchangés**.

| Constat | Décision de la rev2 |
|---|---|
| P1 | Le test « 4 non tranchés » de la rev1 est **faux** : sur le corpus, 0 refus à la strate contenu. Corrigé (voir Task 6 rev2). |
| P2 | `NonTranches` (liste de refus par groupe) est **supprimé**. Remplacé par une **note de capacité** dérivée de W : les strates ayant refusé globalement (variante/version/identité/famille) deviennent une ligne d'information « regroupement de versions indisponible » (lie au spec §8). |
| P3 | Le rapport gagne une **synthèse** : nombre de fichiers redondants, espace récupérable, et un cadrage par groupe « à conserver / candidats à la suppression ». |
| P4 | Le rapport n'expose **aucun type du moteur** : toutes les sorties sont des DTO du module. |
| P5 | Les indicateurs de richesse sont renommés pour dire ce qu'ils mesurent réellement (présence, pas complétude). |
| P6 | Reprojections d'Ω : **noté**, benchmark avant toute optimisation (§10). Aucun changement de code en rev2. |
| P7 | Invariant « W et Ω partagent le même index » : **documenté** en commentaire du composant concerné. Aucun changement de comportement. |

---

## 2. Le rapport, revu comme aide à la décision (P2 + P3 + P4)

Structure cible du rapport produit par le module (tous les champs sont des DTO **du module**, aucun type moteur) :

- **Synthèse de bibliothèque** (nouveau, P3) :
  - nombre de groupes de doublons ;
  - nombre de fichiers redondants = Σ (taille du groupe − 1) sur tous les groupes ;
  - espace récupérable en octets = Σ, sur chaque groupe, de `Taille × (n − 1)` — `Taille` est déjà portée par chaque acte d'observation d'Ω ; les membres d'un groupe étant égaux en octets, leur taille est identique.
- **Note de capacité** (nouveau, P2) : la liste des strates pour lesquelles W porte un refus global de motif « aucune-convention-strate » (et le cas « préalable-absent »). Rendue comme une phrase unique d'information — jamais comme des données par fichier. S'auto-résorbe quand ℛ adopte une convention pour la strate concernée.
- **Groupes de doublons** : pour chaque groupe,
  - le motif court d'identité (référence CE-01 v1, niveau « certaine ») — inchangé vs rev1 (D5) ;
  - la taille unitaire et l'espace récupérable du groupe ;
  - les exemplaires classés, chacun étiqueté **« à conserver »** (rang 1) ou **« candidat à la suppression »** (rang ≥ 2) — reformulation du `Rang` brut de la rev1 (P3).
- **Pas de section `NonTranches`** (supprimée, P2/P4).

L'audit complet du moteur reste accessible à la demande via la commande `identity audit` existante — non dupliqué dans le rapport (D5, inchangé).

---

## 3. Répercussions composant par composant

Rappel de la chaîne (inchangée) : extraction → enrichissement → classement → synthèse/assemblage → CLI.

| Composant | Rôle | Changement rev2 |
|---|---|---|
| **ExtracteurDeGroupes** | Sépare W en groupes (élections strate contenu, certaine, CE-01) et, accessoirement, refus de strate contenu | Les groupes : **inchangés**. Ajoute la lecture des **refus de strates supérieures** (variante/version/identité/famille) pour alimenter la note de capacité (P2). La sortie « refus de strate contenu » reste un garde défensif, non testée en valeur non nulle. |
| **EnrichisseurDeGroupe** | Relit dans Ω les attributs bruts de classement | **Renommage** des indicateurs (P5) : « présence signature Authenticode », « PE lisible », « présence métadonnées MSI » — au lieu de « …Completes ». Ajoute la lecture de la **taille** de l'acte (déjà dans Ω) pour la synthèse (P3). Documente l'invariant W/Ω même index (P7). Logique inchangée. |
| **PolitiqueRetentionV1** | Classe les exemplaires d'un groupe selon les 4 critères ordonnés (D4) | **Inchangé.** L'ordre des critères et le départage mécanique restent la traduction littérale de `politique-retention/v1.md`. |
| **SynthèseDeBibliothèque** (nouveau) | Calcule les agrégats (fichiers redondants, espace récupérable) à partir des groupes et des tailles | Nouveau composant pur (P3). Aucune dépendance au moteur au-delà des groupes déjà extraits. |
| **GénérateurDeRapport** | Orchestre et assemble le rapport | Produit désormais des **DTO du module** (P4), intègre la **synthèse** (P3) et la **note de capacité** (P2), étiquette les exemplaires « à conserver / candidat » (P3). Ne référence plus `ActeW` dans sa sortie. |
| **RapportDeDoublons** (DTO) | Le rapport restitué | **Nouvelle forme** : synthèse + note de capacité + groupes ; **plus de champ `NonTranches`** typé moteur (P2/P4). |
| **DuplicatesCommand** (CLI) | Câble les adaptateurs et sérialise le rapport | **Inchangé** dans son câblage (même `Porteur.Deriver`, même régime d'erreur). Sérialise le nouveau DTO. |

Types nouveaux ou renommés (tous **du module**, décrits sans code) :
- un DTO de **synthèse** (nb groupes, nb fichiers redondants, espace récupérable en octets) ;
- un DTO d'**exemplaire** portant, en plus du rang, une étiquette « à conserver » / « candidat à la suppression » ;
- un DTO de **groupe** portant taille unitaire et espace récupérable du groupe ;
- un DTO de **note de capacité** (liste des strates indisponibles + phrase explicative) ;
- le DTO **rapport** = synthèse + note de capacité + groupes.

---

## 4. Répercussions sur le découpage en tâches (par rapport à la rev1)

Le découpage en 6 tâches et l'ordre restent valides. Modifications :

- **Task 1 — ExtracteurDeGroupes** : ajouter l'extraction des refus de strates supérieures (pour la note de capacité). Tests : garder les 3 tests sur les groupes ; **remplacer** le test « refus de strate contenu restitué » par un test « les refus de strates supérieures sont collectés pour la note de capacité » et un test « aucun refus de strate contenu sur une entrée sans refus contenu ». Reformuler la sortie (deux flux : groupes ; refus de strates supérieures).
- **Task 2 — EnrichisseurDeGroupe** : renommer les indicateurs (P5) ; ajouter la lecture de la taille de l'acte ; commenter l'invariant W/Ω (P7). Adapter les tests aux nouveaux noms et à la taille.
- **Task 3 — PolitiqueRetentionV1** : **aucun changement.**
- **Task 4 — Registre métier (politique-retention/v1.md)** : **aucun changement** — la politique de classement ne bouge pas. (Ne pas confondre avec la note de capacité, qui n'est pas une convention métier mais une projection de W.)
- **Task 3 bis (nouvelle) — SynthèseDeBibliothèque** : nouveau composant pur + tests (fichiers redondants, espace récupérable, groupe de 2, triplet, cohérence des tailles). À insérer après Task 3.
- **Task 5 — GénérateurDeRapport + DTO** : produire les DTO du module (P4) ; intégrer synthèse (P3) et note de capacité (P2) ; étiqueter les exemplaires (P3) ; supprimer `NonTranches` typé moteur. Adapter le test d'assemblage : vérifier la synthèse, la présence de la note de capacité, l'étiquetage conserver/candidat, l'absence de fuite de type moteur.
- **Task 6 — DuplicatesCommand (CLI)** : câblage **inchangé**. **Corriger** le test corpus (P1) : conserver l'assertion **112 groupes** ; **remplacer** « 4 non tranchés » par (a) présence d'une note de capacité listant variante/version/identité/famille, et (b) une valeur d'espace récupérable strictement positive. Conserver le test de reproductibilité (deux émissions identiques) et le test d'erreur (base absente → erreur du moteur telle quelle).

---

## 5. Répercussions sur les tests (corpus réel)

Attendus corrigés sur `tests/oracle/corpus1-postA1.db` + `registre/` :

- **112 groupes** de doublons (108 paires + 4 triplets) — inchangé, correct.
- **0 refus à la strate contenu** — le contenu ne refuse jamais (003 §9).
- **note de capacité** listant **variante, version, identité, famille** comme indisponibles (dérivée des 4 refus globaux de W) — remplace l'assertion fausse « 4 non tranchés ».
- **espace récupérable > 0** — vérifie que la synthèse agrège réellement les tailles.
- **reproductibilité** : deux émissions du rapport identiques (inchangé).
- **erreur** : base absente → message du moteur restitué tel quel, code 1 (inchangé ; la batterie des sept erreurs reste couverte par `IdentityCommandTests`, non dupliquée).

---

## 6. Ce qui reste strictement identique à la rev1

- Architecture : module → `InstallChecker.Identity` uniquement ; jamais Access, jamais Core, jamais SQLite. Seule la CLI relie les adaptateurs au module.
- Direction des dépendances : `Identity ← DuplicateFiles ← CLI`.
- Politique de rétention en artefact versionné (`modules/duplicate-files/registre-metier/politique-retention/v1.md`) — D6, Approche A. L'ordre des critères de D4 ne bouge pas.
- Démarche : un composant pur par responsabilité, TDD par composant, commits fréquents, un type par fichier, `net10.0`, xUnit, noms de tests en phrases françaises.
- Périmètre : v1 = un état Ω désigné, strate contenu exclusivement. v2 (Ω cumulatif) et v3 (τ) restent hors périmètre (spec D1/§8), de même que le regroupement de versions (bloqué sur une adoption de convention dans ℛ, spec §8).

---

## 7. Notes ouvertes (sans action en rev2)

- **P6 — Performance I/O** : Ω est reprojeté plusieurs fois par exécution et le modèle complet est matérialisé même quand peu d'actes sont en groupe. Acceptable au corpus (497 actes), à mesurer avant tout traitement de « centaines de milliers » (§10 : aucune optimisation sans benchmark). Piste si le benchmark le justifie : ne projeter que les identifiants nécessaires. À ne pas implémenter tant que non mesuré.
- **P7 — Invariant W/Ω** : l'enrichissement suppose que tout identifiant d'un domaine élu de W existe dans Ω. Vrai par construction (la commande dérive W du même Ω). À documenter en commentaire ; ne devient un risque que si un appelant fournit un W et un Ω d'index différents.
