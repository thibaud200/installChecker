# Module Duplicate Files — Conception (réflexion métier)

**Statut** : issu d'une session de brainstorming (superpowers:brainstorming), aucune implémentation engagée.
**Date** : 2026-07-11
**Sources consultées** : `docs/projet/VISION.md`, `docs/projet/duplicate-files.md`, `docs/identity/005` (stratification), `docs/identity/011` (interface publique du moteur), `docs/identity/018` (porteur et consommateur), `registre/etat.md`, `registre/conventions/CE-01/v1.md`, `registre/conventions/EQ-01/v1.md`.
**Nature de ce document** : réflexion métier modulaire — définit *ce que fait* le module et *pourquoi*, jamais *comment* (aucun code, aucune classe, aucun nom de fichier source). Sert de mémoire des décisions prises, retenues et écartées, pour reprise ultérieure.

---

## 1. Constat structurant (avant toute décision)

Le moteur d'identité stratifie l'identité logique en 5 strates (005 §3) : **contenu, variante, version, identité, famille**. L'état actuel du registre ℛ₀ (`registre/etat.md`) ne contient que deux conventions — **CE-01** et **EQ-01** — qui licencient exclusivement la **strate contenu** (égalité parfaite d'octets, régime R1 exact, niveau « certaine »). Aucune convention n'existe encore pour les strates variante/version/identité/famille ; le 005 §13 le nomme explicitement comme « conventions futures ».

**Conséquence directe sur le périmètre** : W peut aujourd'hui élire avec certitude « ces fichiers sont des copies binaires exactes », mais ne peut rien élire sur « ces fichiers sont deux versions du même logiciel » — cette question reste un espace d'hypothèses non tranché tant qu'un acte de gouvernance n'ajoute pas la convention correspondante à ℛ. C'est un acte de **gouvernance du registre**, pas une évolution du module ni du moteur (011 §10 : « ajouter une convention dans une famille connue » = une transition de ℛ, une donnée nouvelle, pas un moteur nouveau).

La vision du module (VISION.md §6, duplicate-files.md §8) demande à terme les deux capacités : doublons exacts **et** regroupement de versions d'un même logiciel. Seule la première est consommable dès aujourd'hui.

---

## 2. Journal des décisions

Chaque décision suit le format : Contexte → Décision → Alternatives écartées → Conséquences — même discipline que les ADR du `CLAUDE.md` racine, appliquée ici au niveau métier du module.

### D1 — Découpage en versions du module (v1 / v2 / v3)

- **Contexte** : la vision complète (doublons exacts + regroupement de versions + obsolescence) dépasse ce que ℛ licencie aujourd'hui (§1), et l'accumulation d'Ω à travers plusieurs scans pose une question de périmètre séparée.
- **Décision** : construire par étapes explicites, chacune livrable indépendamment :
  - **v1** — doublons exacts (strate contenu) sur un **état Ω désigné** (un scan donné, figé).
  - **v2** — même capacité, mais sur le **dernier état cumulatif d'Ω** (tous les scans passés confondus).
  - **v3** — comparaison de deux états via **τ** (transition du moteur) pour un usage récurrent (« qu'est-ce qui a changé depuis le dernier scan »).
  - Le regroupement de versions d'un même logiciel (strates version/identité) est **hors périmètre des trois versions ci-dessus** — voir §8.
- **Alternatives écartées** : construire directement le périmètre complet (rejeté — mélange deux natures de dépendance très différentes, l'une purement applicative (v2/v3), l'autre bloquée sur une gouvernance externe du registre (§8)).
- **Conséquences** : le present document couvre **exclusivement v1**. v2 et v3 sont notées comme évolutions connues, non conçues en détail.

### D2 — Périmètre Ω du v1

- **Contexte** : ADR-002 (racine) — Ω est append-only ; chaque scan ajoute des lignes, rien n'est mis à jour ni supprimé.
- **Décision** : le module v1 invoque le porteur sur **un état Ω désigné** (l'index d'un scan précis, figé) — cohérent avec le contrat du moteur (« chaque invocation porte sur un état identifié », 011 §2.1).
- **Alternatives écartées** : raisonner sur le dernier état cumulatif (reporté en v2) ; comparer deux états via τ (reporté en v3).
- **Conséquences** : simple, reproductible ; si la bibliothèque a été scannée plusieurs fois, seul le scan désigné compte pour le v1 — les fichiers ajoutés/déplacés depuis ne sont pas vus.

### D3 — Nature de la politique de conservation

- **Contexte** : VISION.md §6/§8 pose comme principe que « la suppression n'est jamais automatique ».
- **Décision** : le module **propose un classement** au sein de chaque groupe de doublons (quel exemplaire garder en priorité), avec le raisonnement transparent, mais **ne décide jamais seul** — la décision finale reste à l'utilisateur.
- **Alternatives écartées** :
  - lister les groupes sans aucune suggestion (écarté — trop minimal par rapport à l'intention affichée d'assistant à la maintenance de bibliothèque, VISION.md §2) ;
  - politique de conservation configurable par règles utilisateur, façon moteur de règles (écarté pour le v1 — jugé non pertinent par rapport à l'intention de départ ; laissé comme évolution possible, non priorisée).
- **Conséquences** : nécessite un classeur avec critères explicites et ordonnés (D4) et une restitution du motif de classement (D5).

### D4 — Critères de classement et leur ordre

- **Contexte** : suite de D3, il faut définir ce qui rend un exemplaire « préférable » à un autre au sein d'un groupe de doublons.
- **Décision** : ordre retenu, du plus déterminant au moins déterminant :
  1. **Richesse des observations** — signature Authenticode présente/valide, complétude des métadonnées PE/MSI (dérivable directement des capacités déjà produites par `InstallChecker.Core`) ;
  2. **Qualité du nom de fichier** — nom d'apparence originale préféré à un nom manifestement issu d'une copie du système de fichiers (ex. suffixes `(1)`, `- copie`) ;
  3. **Ancienneté de l'observation** — le fichier **le plus récemment observé** est préféré (pas le plus ancien) ;
  4. **Emplacement sur disque** — critère de dernier recours ; **pas de notion de dossier préféré/canonique en v1** (pas de configuration utilisateur de dossiers « canoniques » pour cette version).
- **Alternatives écartées** : notion de dossier préféré configurable par l'utilisateur (écartée pour le v1, cf. D3) ; classement uniquement par emplacement ou uniquement par ancienneté (écartés — jugés moins fiables seuls que la richesse d'observations).
- **Conséquences** : si un critère ne départage pas (égalité, ou absence d'observation ⊥ sur ce critère pour un ou plusieurs fichiers), le classeur passe au critère suivant dans cet ordre (voir §6).

### D5 — Niveau d'exposition de l'audit du moteur dans le rapport

- **Contexte** : le moteur restitue une chaîne d'audit complète (observations → signal → convention → élection) à la demande, unité par unité (011 §7) ; le projet exige que « toute décision doit être explicable » (CLAUDE.md racine §2).
- **Décision** : chaque groupe affiche par défaut un **motif court** (ex. « CE-01 v1, contenu identique, certaine »), avec un moyen explicite de récupérer la **chaîne d'audit complète** du moteur pour un groupe donné, à la demande.
- **Alternatives écartées** : chaîne d'audit complète toujours incluse dans le rapport (écartée — rend le rapport illisible sur une bibliothèque de plusieurs milliers de fichiers) ; aucune justification dans le rapport standard, audit accessible uniquement via la commande `identity` séparée (écartée — s'éloigne du principe d'explicabilité porté directement par le rapport).
- **Conséquences** : le restituteur de rapport doit savoir formuler un motif court **et** relayer une demande d'audit complet vers le porteur, sans jamais reformuler ou interpréter la réponse du moteur (cohérent avec 018 §6 : « restitue toute erreur nommée telle quelle »).

### D6 — Structuration des règles métier du module (politique de rétention)

- **Contexte** : le projet impose au moteur qu'« aucune heuristique » ne vive dans le code — toute interprétation est une donnée versionnée du registre ℛ (CLAUDE.md racine, ADR de fond ; 011 §10). Se pose la question de savoir si cette discipline s'étend aux règles métier propres au module (D4).
- **Décision (Approche A retenue)** : la politique de rétention (les 4 critères de D4, leur ordre, leurs règles de départage) est documentée comme un **artefact versionné**, à l'image de `registre/conventions/CE-01/v1.md`, mais dans un **registre métier propre au module**, structurellement distinct de `registre/` qui reste la propriété exclusive du moteur d'identité (aucun module ne modifie ni n'étend ℛ). Une révision de la politique (ex. inverser le sens du critère « ancienneté ») produit une nouvelle version documentée et tracée, jamais un patch silencieux du code.
- **Alternatives écartées** :
  - **Approche B** — critères encodés comme configuration/constantes du module, documentées par un simple commentaire ou ADR ponctuel, sans être un artefact versionné à part (écartée — casse la cohérence avec le reste du dépôt où « toute heuristique documentée = donnée, jamais du code caché » ; une révision de politique deviendrait un diff de code plutôt qu'un acte de gouvernance tracé) ;
  - **Approche C** — moteur de règles générique pluggable (écartée — over-engineering par rapport à l'intention de départ, cf. D3).
- **Conséquences** : le module a son propre cycle de gouvernance métier (versions de politique, historique), séparé mais parallèle à celui de ℛ — deux registres distincts, deux autorités distinctes, jamais confondus.

---

## 3. Architecture

Le module Duplicate Files est un **nouveau consommateur** du moteur, au même statut contractuel que la commande `identity` existante (018 §6) — mais avec un domaine métier propre. Il consomme **deux contrats publics distincts**, ce qui est explicitement autorisé par 011 §11 : le contrat public du **moteur** (W, τ, audit) pour répondre à « ces fichiers sont-ils identiques ? », et le contrat public d'**Ω** directement, pour ses « propres besoins de présentation », afin de répondre à « lequel garder ? » — cette seconde question est strictement une question du module, jamais du moteur.

Le module ne modifie ni Ω, ni ℛ, ni le moteur. Il porte son propre registre métier versionné (D6), structurellement distinct de `registre/`.

## 4. Composants métier

Découpe conceptuelle en cinq responsabilités, chacune avec un rôle unique :

1. **Sélecteur d'index** — désigne l'état Ω à analyser (v1 : un scan donné) et invoque le porteur avec ℛ courant → obtient W.
2. **Extracteur de groupes** — filtre les actes de W à la strate contenu, niveau « certaine », licenciés par CE-01 → partitionne en classes ≡ₘ (groupes de doublons). Tout refus rencontré à cette strate est mis de côté, jamais éliminé silencieusement (§6).
3. **Enrichisseur de groupe** — pour chaque fichier d'un groupe, relit dans Ω (pas dans W) les attributs bruts nécessaires au classement : signature Authenticode, complétude PE/MSI, nom de fichier, date d'observation, chemin.
4. **Classeur** — applique la politique de rétention versionnée (D6) aux attributs enrichis, produit un ordre suggéré et le motif de chaque comparaison (quel critère a tranché, à quel rang).
5. **Restituteur de rapport** — assemble groupes, exemplaire suggéré, motif court (D5), et une clé pour demander la chaîne d'audit complète du moteur à la demande.

## 5. Flux de données

```
Ω(index désigné) + ℛ(état courant) → [porteur] → W
W (élections strate contenu, certaine, CE-01) → [extracteur de groupes] → groupes bruts (partitions ≡ₘ)
groupes bruts + Ω (attributs bruts) → [enrichisseur] → groupes enrichis
groupes enrichis + politique de rétention v1 → [classeur] → groupes classés + motifs
groupes classés → [restituteur] → rapport (motif court par défaut ; chaîne d'audit complète à la demande)
```

## 6. Gestion des refus / absences

- Un fichier singleton (aucun autre fichier ≡ₘ) n'entre dans aucun groupe — hors périmètre du rapport, il n'y a rien à décider.
- Si W porte un **refus motivé** à la strate contenu pour un domaine d'actes (rare vu le caractère mathématique R1 de CE-01/EQ-01, mais le contrat ne garantit jamais son absence) — jamais éliminé silencieusement : il apparaît dans une section séparée du rapport (« non tranché »), motif restitué **tel quel**, jamais traduit ni interprété (018 §6).
- Si l'invocation du porteur échoue (une des 6 erreurs contractuelles, 011 §5) — **aucun rapport partiel** n'est produit : l'erreur remonte telle quelle, héritage direct de la postcondition « entier ou absent » du moteur.
- Si un attribut Ω nécessaire au classement est absent pour un fichier (ex. signature non observée, ⊥) — ce n'est **pas une erreur**, c'est une observation légitime : le critère concerné est simplement non discriminant pour ce fichier, le classeur passe au critère suivant de la politique (traité comme une égalité sur ce critère). Ce comportement fait partie intégrante de la politique de rétention versionnée (D6), pas laissé implicite dans le code.

## 7. Tests / validation (conceptuel)

- Le module est testable indépendamment du moteur : il consomme un contrat stable (W, Ω) — un W et un Ω fixtures suffisent, pas besoin de réinvoquer le moteur à chaque test (même logique de rejouabilité que 011 §6).
- Cas à couvrir : groupe de 2 ; groupe de 3+ (le corpus 1 réel contient déjà 108 paires + 4 triplets, cf. EQ-01 v1) ; égalité totale sur tous les critères (aucun critère ne départage → ordre arbitraire mais **documenté et stable**, jamais aléatoire) ; absence de signal (⊥) sur un ou plusieurs critères pour un ou plusieurs fichiers du groupe ; refus à la strate contenu (si le corpus en produit un jour) ; chacune des 6 erreurs contractuelles du porteur.
- **Reproductibilité** : même Ω + même ℛ + même version de politique de rétention ⇒ même rapport, bit à bit — hérite de la garantie de déterminisme du moteur (011 §6), étendue à la politique du module (aucun aléatoire toléré dans le classeur).

## 8. Hors périmètre v1 — évolutions futures connues

Notées ici pour mémoire, non conçues en détail :

- **v2** — raisonner sur le dernier état cumulatif d'Ω (tous les scans confondus) plutôt qu'un état désigné (D1/D2). Nécessitera de distinguer « observé historiquement » de « présent sur disque aujourd'hui ».
- **v3** — comparaison de deux états via τ, pour un usage récurrent (nouveaux doublons apparus / résolus depuis le dernier scan). Probablement la version la plus complexe, dépend de v1/v2.
- **Regroupement de versions d'un même logiciel** (strates variante/version/identité, §1) — **bloqué sur un acte de gouvernance du registre ℛ**, pas sur le module ni sur le moteur. Avant toute conception détaillée d'un module v-quelconque exploitant ces strates, il faut : (a) qu'une ou plusieurs conventions nouvelles soient rédigées, justifiées et adoptées dans `registre/conventions/` pour les strates concernées (011 §10, deuxième voie : « introduire une famille de conventions nouvelle » = révision documentaire de la théorie d'abord, puis évolution du moteur, revalidée intégralement) ; (b) que cette adoption soit tracée dans `registre/historique.md` et `registre/etat.md`. Ce travail est **antérieur** et **indépendant** du module Duplicate Files — c'est un jalon de gouvernance du moteur, pas une tâche du module.
- **Politique de rétention configurable par règles utilisateur** (D3, Approche C de D6) — explicitement écartée pour le moment, non priorisée.
- **Notion de dossier « canonique »/préféré** dans le critère d'emplacement (D4) — explicitement absente du v1, à réévaluer si le besoin se confirme à l'usage.

## 9. Dépendances et risques

- **Dépendance dure au moteur** : le module ne peut produire aucun rapport sans une invocation valide du porteur — si ℛ₀ devient incohérent ou Ω invalide, le module hérite intégralement du contrat d'erreur du moteur (§6).
- **Dépendance de gouvernance** (hors module) : toute extension du périmètre au-delà de la strate contenu dépend d'une adoption de convention dans ℛ, événement externe au calendrier du module (§8).
- **Risque de dérive de la politique de rétention** : si la discipline de versionnement (D6, Approche A) n'est pas respectée dans le temps, le module retombe silencieusement dans l'Approche B écartée — à surveiller lors des futures révisions.

---

## Récapitulatif des décisions

| # | Décision | Alternatives écartées |
|---|---|---|
| D1 | Découpage v1 (état désigné) / v2 (cumulatif) / v3 (τ) ; regroupement de versions hors périmètre | Périmètre complet dès le départ |
| D2 | v1 : un état Ω désigné (un scan figé) | Dernier état cumulatif ; comparaison via τ |
| D3 | Classement suggéré, décision finale à l'utilisateur | Liste brute sans suggestion ; règles utilisateur configurables |
| D4 | Ordre : richesse d'observations > nom de fichier > ancienneté (plus récent préféré) > emplacement (sans dossier canonique) | Emplacement en premier ; ancienneté « plus ancien préféré » |
| D5 | Motif court par défaut, chaîne d'audit complète à la demande | Chaîne complète toujours incluse ; aucune justification dans le rapport |
| D6 | Politique de rétention = registre métier versionné propre au module (Approche A) | Constantes de configuration documentées (B) ; moteur de règles pluggable (C) |
