# Relecture — Module Duplicate Files (spec + plan v1)

**Date** : 2026-07-11
**Objet** : relecture critique du spec `docs/superpowers/specs/2026-07-11-module-duplicate-files-design.md` et du plan `docs/superpowers/plans/2026-07-11-module-duplicate-files-v1-implementation.md`, à la lumière des deux documents de vision `docs/projet/VISION.md` et `docs/projet/duplicate-files.md`.
**Nature** : rapport de delta — identifie les écarts utiles à corriger. Ne modifie ni le spec ni le plan v1 (conservés pour comparaison). Les répercussions sont portées par la révision 2 du plan : `docs/superpowers/plans/2026-07-11-module-duplicate-files-v1-implementation-rev2.md`.
**Méthode** : relecture croisée + une vérification factuelle sur l'oracle (`tests/oracle/W0-attendu.json`).

---

## 1. Le fait qui change le cadrage

Inspection des 4 refus du W de référence (`tests/oracle/W0-attendu.json`, corpus 1) : ils ne sont **pas** à la strate contenu, contrairement à ce que le spec §6 et le plan supposaient. Ils sont :

| strate | espèce | motif | domaine |
|---|---|---|---|
| variante | normatif | `aucune-convention-strate` | les 497 actes |
| version | normatif | `aucune-convention-strate` | les 497 actes |
| identité | normatif | `aucune-convention-strate` | les 497 actes |
| famille | normatif | `préalable-absent` | les 497 actes |

Ce sont **quatre refus globaux**, un par strate supérieure, chacun couvrant l'intégralité du corpus. Ils énoncent une seule chose : ℛ n'a adopté aucune convention au-delà du contenu, donc le moteur refuse de conclure sur variante/version/identité/famille — exactement le constat du spec §1. Le refus « famille » porte le motif `préalable-absent` : famille présuppose identité (005 §8), qui a elle-même refusé.

La strate contenu ne produit **aucun** refus : 112 élections certaines, zéro refus. C'est structurel (003 §9 : les singletons dégénèrent et n'émettent aucun acte ; les classes multi-actes sont dominées immédiatement par l'égalité mathématique) — le contenu ne refuse jamais.

---

## 2. Constats

### P1 — Bug de correction dans le plan (bloquant)

`ExtracteurDeGroupes` (plan Task 1) filtre les refus par strate contenu. Sur le corpus réel, ce filtre renvoie **0 refus**, pas 4. Le test de Task 6 `Le_corpus_reel_produit_112_groupes_et_4_non_tranches` asserte `NonTranches == 4` : il **échouera** (valeur obtenue : 0). Les 112 groupes sont corrects ; le « 4 » est faux.

### P2 — Le concept `NonTranches` vise la mauvaise cible (métier)

Le spec §6 prévoyait de restituer les refus *de strate contenu* comme « non tranchés ». Ils n'existent pas. Les vrais refus signifient « le regroupement de versions n'est pas disponible » — c'est la frontière de périmètre du spec §8, pas une indécision par groupe de doublons. Restituer 4 enregistrements portant chacun un domaine de 497 entiers est du bruit, pas une donnée.

**Correctif** : remplacer `NonTranches` (liste de refus par groupe) par une **note de capacité** en tête de rapport, dérivée des strates ayant refusé globalement — p. ex. « Regroupement de versions indisponible : aucune convention adoptée au-delà de la strate contenu (variante, version, identité, famille en attente — voir spec §8) ». On peut conserver un filtre défensif sur les refus de strate contenu (le contrat ne l'interdit pas), mais sans tester une valeur qui est 0 par construction. La note se dérive de W et s'auto-résorbe : le jour où ℛ adopte une convention de strate version, ce refus devient une élection et disparaît de la note sans toucher au code.

### P3 — Absence d'agrégats actionnables (le delta métier le plus utile)

Relu contre VISION.md §2 (« assister la maintenance ») et §12 (« Existe-t-il des copies inutiles ? Quelle copie conserver ? »), le rapport actuel est un vidage classé, pas une aide à la décision. Manquent, alors que les données sont déjà dans Ω :

- **nombre de fichiers redondants** = Σ (taille du groupe − 1) ;
- **espace récupérable** = Σ sur chaque groupe de `Taille × (n−1)` — `ActeObservation.Taille` est déjà porté par Ω, coût quasi nul ; les fichiers d'un groupe sont égaux en octets donc de taille identique ;
- **cadrage « à conserver / candidats à la suppression »** : le rapport ne livre aujourd'hui qu'un entier `Rang`. Le reformuler (rang 1 = à conserver, rang ≥ 2 = candidats) transforme une liste triée en la décision réelle de l'utilisateur (VISION §8 : « produire un rapport avant toute suppression »).

C'est le point où le module cesse d'être un « duplicate finder » (que le CLAUDE.md racine interdit d'être) pour devenir une aide à la maintenance.

### P4 — Le rapport laisse fuir un type du moteur

`RapportDeDoublons` (plan Task 5) expose `IReadOnlyList<ActeW> NonTranches` — un type interne du moteur sérialisé brut en JSON. Le format de sortie du module devient couplé à la forme de `ActeW` : une évolution du moteur changerait silencieusement la sortie du module, en contradiction avec D6 (« le module possède ses rapports »). P2 supprime `NonTranches`, mais le principe vaut pour toute sortie : le module n'expose que **ses** types (DTO), jamais ceux du moteur.

### P5 — Noms qui surpromettent

`MetadonneesPeCompletes` / `MetadonneesMsiCompletes` (plan Task 2) testent en réalité la présence d'**un seul** attribut (`pe_info.machine`, `msi_properties.product_name`) : cela mesure « est un PE/MSI lisible », pas « métadonnées complètes » comme l'annonçait D4. Correctif : renommer (p. ex. `EstUnPeLisible` / `PresenceMetadonneesMsi`). Mesurer une vraie complétude serait de la sur-ingénierie tant qu'aucun besoin ne le réclame.

### P6 — I/O (à signaler, ne pas corriger maintenant)

Le module reprojette Ω (`ProjeterModele` + `ProjeterContexte`) après le porteur, et matérialise un dictionnaire de tous les actes même si peu sont en groupe. Négligeable sur 497 fichiers, réel sur les « centaines de milliers » du §10. Le §10 impose « aucune optimisation sans benchmark » : **signaler, mesurer avant d'agir**. Déclencheur : un benchmark sur un corpus de grande taille.

### P7 — Robustesse (mineur)

`EnrichisseurDeGroupe` indexe `actes[id]` / `contextes[id]` directement ; un W et un Ω d'index différents lèveraient un `KeyNotFoundException` non capturé par `DuplicatesCommand`. Sûr aujourd'hui (la commande dérive W du même Ω) : invariant à documenter, pas un bug.

---

## 3. Ce qui est sain et reste inchangé

L'ossature n'est pas remise en cause : module → `Identity` seul (jamais Access ni SQLite), politique de rétention en artefact versionné (D6), un composant pur par responsabilité, TDD par composant, délégation au même `Porteur.Deriver` que la commande `identity`. Les correctifs touchent le **contenu du rapport** et **un test faux**, pas l'architecture.

---

## 4. Synthèse

| # | Gravité | Où | Correctif |
|---|---|---|---|
| P1 | Bloquant | Plan Task 6, test `…_4_non_tranches` | Les 4 refus sont hors strate contenu → le test échoue. Corriger l'attendu et le concept. |
| P2 | Fort (métier) | Spec §6, `NonTranches` | Remplacer par une note de capacité dérivée de W (lie au §8). |
| P3 | Fort (valeur) | Spec §5, `RapportDeDoublons` | Agrégats : fichiers redondants, espace récupérable, cadrage conserver/supprimer. |
| P4 | Moyen | Plan Task 5 | Ne pas exposer `ActeW` ; mapper vers un DTO du module. |
| P5 | Faible | Plan Task 2 | Renommer `…Completes` → `EstUnPeLisible` / `PresenceMetadonneesMsi`. |
| P6 | À surveiller | Plan Task 5 | Reprojections d'Ω : signaler, benchmark avant d'optimiser (§10). |
| P7 | Faible | Plan Task 2 | Documenter l'invariant W/Ω même index. |

Cœur du rapport : **P1 est un test faux à corriger ; P2 + P3 sont le vrai gain métier** — passer d'un rapport qui liste des groupes à un rapport qui dit combien d'espace on récupère et quoi supprimer, tout en énonçant honnêtement que le regroupement de versions n'est pas encore outillé par ℛ.
