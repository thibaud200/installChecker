# Module Duplicate Files — Plan d'implémentation, révision 3 (définitif, sans code)

> **Nature** : révision 3 du plan d'implémentation du module (périmètre v1 = un état Ω désigné, strate contenu — spec D2). **« v1 » désigne ici le périmètre du module, pas la version du plan.** Cette révision 3 est le **plan définitif avant implémentation** ; elle remplace la révision 2 (`2026-07-11-module-duplicate-files-v1-implementation-rev2.md`). La révision 1 (`2026-07-11-module-duplicate-files-v1-implementation.md`, avec les étapes TDD codées) reste conservée : elle sert de source pour la traduction en code au moment de l'exécution.
> **Portée** : intègre les corrections validées par la relecture contradictoire `docs/superpowers/reviews/2026-07-11-relecture-module-duplicate-files.md` (constats P1→P7) et clôt le périmètre v1 par une section *Décisions différées*. **Sans code** : signatures et structures décrites en prose et en tables, jamais en C#. La traduction en étapes TDD codées se fait à l'exécution, à partir de ce document et du plan rev1.
> **Ce que la rev3 ne fait pas** : elle ne repense pas l'architecture, ne modifie pas le découpage en tâches, n'introduit aucune fonctionnalité nouvelle, n'élargit pas le périmètre. Les décisions métier D1→D6 de la conception sont conservées, sauf là où une correction validée l'impose (P2 : la restitution des refus ; P5 : le nom des indicateurs de richesse).

**But (inchangé)** : à partir d'un état Ω désigné, regrouper les fichiers identiques en octets (strate contenu, CE-01/EQ-01), suggérer un ordre de conservation par groupe via une politique métier versionnée, et produire un rapport **actionnable** — qui dit combien d'espace peut être récupéré et quels exemplaires supprimer — tout en énonçant honnêtement que le regroupement de versions n'est pas encore outillé par ℛ.

**Référence de conception** : spec `2026-07-11-module-duplicate-files-design.md` (décisions D1→D6), amendée par les corrections P1→P7 ci-dessous. Cohérence tenue avec `docs/projet/VISION.md` (§2 « assister la maintenance », §6/§8 « produire un rapport avant toute suppression ») et `docs/projet/duplicate-files.md` (§8 objectifs, §12 « Existe-t-il des copies inutiles ? Quelle copie conserver ? »).

---

## 1. Corrections intégrées de la relecture

Quatre corrections de fond (P1→P5), deux notes documentaires (P6, P7). Architecture, direction des dépendances, artefact de politique versionné, démarche TDD, découpage en tâches et niveau de détail : **inchangés**.

| Constat | Correction intégrée en rev3 |
|---|---|
| **P1** (bloquant) | La strate contenu produit **112 élections et aucun refus** ; les quatre refus du corpus sont **globaux** et portent sur les strates supérieures (variante, version, identité, famille). Plus aucun test, attendu, exemple ou explication du plan n'attend « quatre refus de contenu ». |
| **P2** | Le concept `NonTranches` est **supprimé**. Il est remplacé par une **note de capacité** dérivée des refus globaux des strates supérieures. Ces refus ne sont **jamais** exposés directement comme éléments du rapport utilisateur. |
| **P3** | Le rapport porte désormais une **synthèse d'aide à la décision** en tête (cinq agrégats métier), avant le détail des groupes. Le classement par groupe est **conservé** tel quel. |
| **P4** | Le rapport n'expose **aucun type du moteur**. Tout objet issu du moteur est transformé en **DTO du module** avant exposition. |
| **P5** | Les indicateurs `MetadonneesPeCompletes` / `MetadonneesMsiCompletes` sont **renommés** pour refléter exactement ce qui est testé (présence, pas complétude). La logique de test est **inchangée**. |
| **P6** | La remarque sur les reprojections d'Ω est **conservée**. Aucune optimisation n'est proposée ; une note indique que ce point devra être **benchmarké avant toute évolution** (CLAUDE.md racine §10). |
| **P7** | L'invariant « W et Ω proviennent de la même dérivation » est **documenté explicitement**. Aucune logique supplémentaire. |

---

## 2. Le rapport, aide à la décision (P2 + P3 + P4)

Structure cible du rapport produit par le module. **Tous les champs sont des DTO du module** — aucun type moteur n'apparaît en sortie (P4). L'ordre de présentation est imposé : **synthèse d'abord, puis note de capacité, puis détail des groupes** (P3).

### 2.1 Synthèse de bibliothèque (nouveau, P3)

Cinq agrégats métier, mis en avant **avant** le détail des groupes :

1. **Nombre de groupes** de doublons.
2. **Nombre total de fichiers redondants** = Σ, sur chaque groupe, de `(n − 1)`.
3. **Espace récupérable** en octets = Σ, sur chaque groupe, de `Taille × (n − 1)`. `Taille` est déjà portée par chaque acte d'observation d'Ω ; les membres d'un groupe étant égaux en octets, leur taille est identique.
4. **Nombre de fichiers recommandés à conserver** = un exemplaire (rang 1) par groupe.
5. **Nombre de candidats à la suppression** = Σ, sur chaque groupe, des exemplaires de rang ≥ 2.

**Invariant de cardinal** : le classeur restitue exactement un exemplaire par fichier du groupe ; il ne filtre ni ne supprime aucun membre. Le cardinal du groupe est donc préservé — c'est ce qui garantit que les agrégats 4 et 5 se somment bien à `n` par groupe.

> Note d'honnêteté (explicabilité, CLAUDE.md racine §2) : en v1, par construction, l'agrégat 4 égale l'agrégat 1 (un seul exemplaire conservé par groupe) et l'agrégat 5 égale l'agrégat 2. Les cinq champs restent néanmoins distincts dans le DTO : ils répondent à deux questions différentes de l'utilisateur — l'histoire de l'espace (2, 3) et l'histoire de l'action (4, 5) — et pourraient diverger si une politique future recommandait de conserver plus d'un exemplaire par groupe. Aucun de ces champs n'est calculé par une logique nouvelle : ce sont des sommes sur les groupes déjà extraits et classés.

### 2.2 Note de capacité (nouveau, P2)

Une **phrase d'information unique**, dérivée de W : la liste des strates pour lesquelles W porte un refus global (variante, version, identité, famille dans le corpus actuel). Le module la formule comme une note de capacité, jamais comme des données par fichier, et la note indique que :

- le **regroupement de versions n'est actuellement pas disponible** ;
- cela provient de **l'absence de conventions dans ℛ** au-delà de la strate contenu ;
- cette note **disparaîtra naturellement** dès que de nouvelles conventions seront adoptées dans ℛ pour les strates concernées — sans aucun changement de code (spec §8).

Le module **n'expose jamais** les refus globaux eux-mêmes (leur espèce, leur motif, leur domaine de 497 entiers) comme éléments du rapport utilisateur : ils sont du bruit, pas une donnée métier. Ils ne servent qu'à dériver la présence et le contenu de la note.

### 2.3 Détail des groupes (classement conservé)

Pour chaque groupe de doublons, après la synthèse et la note :

- le **motif court d'identité** (référence CE-01 v1, niveau « certaine ») — inchangé (D5) ;
- la **taille unitaire** et l'**espace récupérable** du groupe ;
- les **exemplaires classés**, chacun étiqueté **« à conserver »** (rang 1) ou **« candidat à la suppression »** (rang ≥ 2) — reformulation lisible du `Rang` brut, l'ordre de classement lui-même restant strictement celui de la politique de rétention v1 (D4, inchangé).

Il n'y a **pas** de section `NonTranches` (supprimée, P2/P4). L'audit complet du moteur reste accessible à la demande via la commande `identity audit` existante — non dupliqué dans le rapport (D5, inchangé).

---

## 3. Composants du module

Chaîne (inchangée dans son principe) : extraction → enrichissement → classement → synthèse → assemblage → CLI. Un composant pur par responsabilité.

| Composant | Rôle | État en rev3 |
|---|---|---|
| **ExtracteurDeGroupes** | Sépare W en groupes de doublons et collecte les refus globaux des strates supérieures | Les groupes (élections strate contenu, niveau certaine, licence CE-01) : **inchangés**. Ajoute la **collecte des refus globaux des strates supérieures** (variante/version/identité/famille) pour alimenter la note de capacité (P2). Le composant expose exactement **deux flux** : les groupes ; les refus de strates supérieures. |
| **EnrichisseurDeGroupe** | Relit dans Ω (jamais dans W) les attributs bruts de classement, plus la taille | **Renommage des indicateurs** (P5) : `SignatureAuthenticodePresente`, `EstUnPeLisible`, `PresenceMetadonneesMsi` — au lieu des `…Completes` qui sur-promettaient. Ajoute la lecture de la **taille** de l'acte (déjà dans Ω) pour la synthèse (P3). L'invariant W/Ω même dérivation est documenté (P7). Logique de lecture **inchangée**. |
| **PolitiqueRetentionV1** | Classe les exemplaires d'un groupe selon les 4 critères ordonnés (D4) | **Inchangé.** L'ordre des critères et le départage mécanique restent la traduction littérale de `politique-retention/v1.md`. |
| **SynthèseDeBibliothèque** (nouveau) | Calcule les cinq agrégats métier à partir des groupes classés et des tailles | Nouveau composant **pur** (P3). Ne dépend que des groupes déjà extraits, classés et de leurs tailles. Aucune dépendance supplémentaire au moteur. |
| **GénérateurDeRapport** | Orchestre les composants et assemble le rapport | Produit désormais des **DTO du module** (P4), place la **synthèse** en tête (P3), y adjoint la **note de capacité** (P2), étiquette les exemplaires « à conserver / candidat » (P3). Calcule les métriques **par groupe** portées par `GroupeClasse` : taille unitaire du groupe (taille d'un membre) et espace récupérable du groupe = `taille × (n − 1)`. Ne référence plus aucun type moteur dans sa sortie. |
| **RapportDeDoublons** (DTO) | Le rapport restitué | **Nouvelle forme** : synthèse + note de capacité + groupes. **Plus de champ `NonTranches` typé moteur** (P2/P4). |
| **DuplicatesCommand** (CLI) | Câble les adaptateurs et sérialise le rapport | Câblage **inchangé** (même `Porteur.Deriver`, même régime d'erreur que la commande `identity`). Sérialise le nouveau DTO. |

DTO et types du module (tous **du module**, décrits sans code) :

- un DTO de **synthèse** portant les **cinq** agrégats (§2.1) ;
- un DTO de **note de capacité** portant la liste des strates indisponibles et la phrase explicative (§2.2) ;
- un type de travail **fichier enrichi** portant chemin, date d'observation, taille, et les trois indicateurs de présence **renommés** (P5) ;
- un DTO d'**exemplaire** portant le rang **et** l'étiquette « à conserver » / « candidat à la suppression » (P3) ;
- un DTO de **groupe** portant le domaine, le motif court, la taille unitaire, l'espace récupérable du groupe et les exemplaires étiquetés ;
- le DTO **rapport** = synthèse + note de capacité + groupes.

---

## 4. Découpage en tâches

Le découpage en 6 tâches de la rev1 est **conservé**, avec l'ajout du composant de synthèse (Task 3 bis, déjà prévu en rev2). Ordre inchangé. La traduction en étapes TDD codées (test qui échoue → implémentation → test qui passe → commit) se fait à l'exécution, en suivant le plan rev1 amendé par les points ci-dessous.

- **Task 1 — ExtracteurDeGroupes** : conserver les trois tests sur les groupes (une élection certaine CE-01 en strate contenu devient un groupe ; une élection non certaine n'est pas un groupe ; une élection hors strate contenu n'est pas un groupe). **Remplacer** le test « un refus de strate contenu est restitué » par un test « les refus globaux des strates supérieures sont collectés pour la note de capacité ». La sortie du composant expose exactement deux flux : les groupes ; les refus de strates supérieures.
- **Task 2 — EnrichisseurDeGroupe** : **renommer** les indicateurs (P5 → `SignatureAuthenticodePresente`, `EstUnPeLisible`, `PresenceMetadonneesMsi`) ; **ajouter** la lecture de la taille de l'acte ; **documenter** l'invariant W/Ω même dérivation (P7). Adapter les tests aux nouveaux noms et à la taille, sans changer la logique (un attribut présent = vrai ; un attribut absent ou ⊥ = faux, jamais une erreur).
- **Task 3 — PolitiqueRetentionV1** : **aucun changement.** L'ordre des critères (D4) et le classement ne bougent pas.
- **Task 4 — Registre métier (`politique-retention/v1.md`)** : la politique de classement — ses critères, leur ordre, ses règles de départage — est **inchangée** (D4/D6). Seul le **libellé descriptif** des signaux de richesse s'aligne sur le renommage P5 (dire « présence de signature Authenticode / PE lisible / présence de métadonnées MSI » plutôt que « complétude »), sans modifier ni le nombre de critères ni leur ordre. Ne pas confondre cette politique métier avec la note de capacité, qui n'est pas une convention métier mais une projection de W.
- **Task 3 bis — SynthèseDeBibliothèque** (à insérer après Task 3) : nouveau composant pur + tests. Couvrir : les cinq agrégats sur un groupe de 2 ; sur un triplet ; la cohérence des tailles au sein d'un groupe ; la coïncidence attendue en v1 (fichiers à conserver = nombre de groupes ; candidats = fichiers redondants).
- **Task 5 — GénérateurDeRapport + DTO** : produire les **DTO du module** (P4) ; intégrer la **synthèse** en tête (P3) et la **note de capacité** (P2) ; **étiqueter** les exemplaires (P3) ; **supprimer** le champ `NonTranches` typé moteur. Adapter le test d'assemblage : vérifier la présence et l'ordre de la synthèse, la présence de la note de capacité, l'étiquetage « à conserver / candidat », et **l'absence de toute fuite de type moteur** dans la sortie.
- **Task 6 — DuplicatesCommand (CLI)** : câblage **inchangé**. **Corriger** le test corpus (P1) : conserver l'assertion **112 groupes** ; **remplacer** toute assertion « 4 non tranchés » par (a) la présence d'une **note de capacité** listant variante/version/identité/famille comme indisponibles, (b) une valeur d'**espace récupérable strictement positive**, et (c) la présence des cinq agrégats de synthèse. Conserver le test de **reproductibilité** (deux émissions identiques) et le test d'**erreur** (base absente → erreur du moteur restituée telle quelle, code 1 ; la batterie des sept erreurs reste couverte par `IdentityCommandTests`, non dupliquée).

---

## 5. Tests sur le corpus réel

Attendus corrigés sur `tests/oracle/corpus1-postA1.db` + `registre/` :

- **112 groupes** de doublons (108 paires + 4 triplets) — correct, inchangé.
- **Aucun refus à la strate contenu** — le contenu ne refuse jamais (003 §9 : les singletons dégénèrent sans émettre d'acte, les classes multi-actes sont dominées immédiatement par l'égalité mathématique). Le plan n'attend **plus** quatre refus de contenu (P1).
- **Note de capacité** listant **variante, version, identité, famille** comme indisponibles, dérivée des quatre refus globaux de W (le refus « famille » porte le motif `préalable-absent`, les trois autres `aucune-convention-strate`). Cette note **remplace** l'assertion fausse « 4 non tranchés » (P1/P2).
- **Espace récupérable strictement positif** — vérifie que la synthèse agrège réellement les tailles (P3).
- **Cinq agrégats présents** dans la synthèse (P3).
- **Reproductibilité** : deux émissions du rapport identiques bit à bit (inchangé — hérite du déterminisme du moteur, 011 §6).
- **Erreur** : base absente → message du moteur restitué tel quel, code 1 (inchangé).

---

## 6. Invariant W et Ω (P7)

L'enrichissement suppose que tout identifiant d'un domaine élu dans W existe dans Ω. **C'est vrai par construction : la commande dérive W du même Ω qu'elle reprojette ensuite pour l'enrichissement — W et Ω proviennent de la même dérivation, dans la même invocation.** L'invariant est **documenté explicitement** (commentaire du composant `EnrichisseurDeGroupe` et de la commande). Aucune logique supplémentaire n'est ajoutée : l'invariant ne devient un risque que si un appelant fournissait un W et un Ω d'index différents, ce que le module ne fait jamais.

---

## 7. Note de performance (P6)

Ω est reprojeté plusieurs fois par exécution (`ProjeterModele` + `ProjeterContexte`) et le modèle est matérialisé même quand peu d'actes sont en groupe. Ce point est **conservé comme remarque** ; **aucune optimisation n'est proposée en v1**. Conformément à CLAUDE.md racine §10 (« aucune optimisation sans benchmark »), **ce point devra être benchmarké avant toute évolution** : négligeable sur le corpus (497 actes), il n'est à mesurer et éventuellement traiter que face aux « centaines de milliers » de fichiers du §10. Rien n'est à implémenter tant que la mesure n'a pas été faite.

---

## 8. Ce qui reste strictement inchangé

- **Architecture** : module → `InstallChecker.Identity` uniquement ; jamais Access, jamais Core, jamais SQLite. Seule la CLI relie les adaptateurs au module.
- **Direction des dépendances** : `Identity ← DuplicateFiles ← CLI`.
- **Politique de rétention** en artefact versionné (`modules/duplicate-files/registre-metier/politique-retention/v1.md`) — D6, Approche A. L'ordre des critères de D4 ne bouge pas.
- **Démarche** : un composant pur par responsabilité, TDD par composant, commits fréquents, un type par fichier, `net10.0`, xUnit, noms de tests en phrases françaises.
- **Périmètre** : v1 = un état Ω désigné, strate contenu exclusivement. Le regroupement de versions reste bloqué sur une adoption de convention dans ℛ (spec §8) — d'où la note de capacité, non une fonctionnalité.

---

## 9. Décisions différées

Section **informative uniquement**. Elle recense les sujets identifiés au fil de la conception et de la relecture, mais **volontairement exclus du périmètre de la v1**. Aucun n'est planifié ici ; ils sont consignés pour mémoire, sans engagement de calendrier.

- **Optimisation des reprojections d'Ω** — la matérialisation répétée d'Ω (P6) ; à benchmarker avant toute évolution (CLAUDE.md racine §10), jamais à optimiser à l'aveugle.
- **Notion de dossiers préférés / canoniques** — absente du critère d'emplacement en v1 (D4) ; à réévaluer si le besoin se confirme à l'usage.
- **Politique de conservation configurable par l'utilisateur** — écartée pour la v1 (D3, Approche C de D6) ; non priorisée.
- **Suppression automatisée** — hors périmètre par principe : la suppression n'est jamais automatique, elle reste une décision de l'utilisateur (VISION.md §6, duplicate-files.md §8). Le module produit un rapport, jamais une action destructrice.
- **Exploitation des strates version / identité / famille** — le regroupement de versions d'un même logiciel ; **bloqué sur un acte de gouvernance du registre ℛ** (adoption de conventions nouvelles), antérieur et indépendant du module (spec §8). C'est ce blocage que la note de capacité (§2.2) énonce honnêtement.
- **Périmètres v2 (état cumulatif d'Ω) et v3 (comparaison via τ)** — évolutions applicatives connues du module, notées en D1/§8, non conçues en détail.
- **Autres évolutions déjà identifiées mais reportées** — tout sujet apparu en relecture et jugé hors v1 sera ajouté ici plutôt qu'implémenté silencieusement, afin de préserver la frontière de périmètre.
