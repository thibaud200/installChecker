Classification : très complexe (décisions d'architecture fondatrices, aucun précédent dans le codebase). Modèle requis : opus ou supérieur — la session tourne déjà sur Fable 5 (au-dessus d'Opus), effort maximum. Aucun changement nécessaire. Je produis l'analyse.

---
Dossier d'analyse architecturale — installChecker

Fait fondateur qui conditionne toute l'analyse : le projet contient 0 ligne de code. Il contient 5 documents (CLAUDE.md, Master_Prompt.md, pipeline_review.md, Prompt_qualité.md, prompt.md). L'"architecture actuelle" est donc une architecture prescrite sur papier, jamais confrontée à la réalité. C'est à la fois une chance (rien à migrer, tout est encore décidable) et le risque n°1 du projet (voir §2.5).

---
1. Analyse critique du système actuel

1.1 Architecture globale (telle que prescrite)

CLAUDE.md décrit un monolithe modulaire à 16+ modules obligatoires (§7), un pipeline séquentiel à 11 étapes (§8), un système de plugins avec versioning strict des interfaces et compatibilité ascendante (§11), des connecteurs externes (WinGet, GitHub, Chocolatey, Scoop), et une base de données non choisie (SQLite/DuckDB "à comparer", §12). Ni langage, ni UI, ni format de stockage ne sont décidés. La section ADR (§17) existe mais est vide : zéro décision structurante n'a été prise.

1.2 Flux de données prescrit

Scan → Métadonnées → PE/Signature → Hash → Classification installateur
→ Identification → Matching base → Analyse version → Lookup externe → Base → Rapport

Ce flux est décrit comme une séquence linéaire. Trois problèmes de fond :

1. Il mélange deux rythmes incompatibles. Les étapes 1–8 sont locales (limitées par le disque et le CPU), l'étape 9 est réseau (limitée par les rate limits GitHub/WinGet, la latence, la disponibilité). Un pipeline qui enchaîne les deux se bloque sur le réseau au milieu d'un scan de 500 000 fichiers. Le lookup externe doit être une phase séparée, asynchrone et optionnelle — le document dit "si nécessaire" mais ne l'architecture pas comme tel.
2. Il implique plusieurs lectures du même fichier. Métadonnées (étape 2), PE (étape 3), hash (étape 4) et classification (étape 5) lisent tous le même binaire. Sur 500 000 fichiers × dizaines de Mo, relire 3–4 fois est le bottleneck I/O que le §10 interdit pourtant. La bonne unité de travail est un fichier traversant tous les analyseurs en un seul passage de lecture, pas une étape traversant tous les fichiers.
3. L'ordre est faux pour la déduplication. Hasher intégralement (étape 4) avant le matching (étape 7) signifie hasher 100 % de la bibliothèque alors qu'un pré-filtre (taille → hash partiel des premiers/derniers 64 Ko) élimine la quasi-totalité des candidats non-doublons sans lecture complète. Master_Prompt.md mentionne "hash partiels, pré-filtrage" comme pistes ; CLAUDE.md ne les intègre pas dans le pipeline.

1.3 Modules et dépendances

Les 16 modules listés sont en réalité de granularités très différentes : "Hash engine" est une fonction, "Moteur d'identification" est le cœur intellectuel du projet, "Connecteur Chocolatey" est un module dont personne n'a besoin en phase 1–4 de la roadmap. Les règles de dépendance ("aucun couplage direct entre modules critiques") sont invérifiables car non définies — quels modules sont "critiques" ? qu'est-ce qu'un couplage "direct" ? Ce genre de règle floue produit de l'indirection gratuite (interfaces à implémentation unique, événements là où un appel de fonction suffit).

1.4 Le vrai problème difficile est sous-spécifié

Le document consacre l'essentiel de sa surface à la plomberie (scan, hash, DB, plugins) et presque rien au problème central : la résolution d'identité. Décider que setup.exe (sans VersionInfo, non signé) et Firefox Setup 128.0.1.exe sont deux installateurs du même Software est LE moteur d'intelligence promis par la vision ("la donnée représente un logiciel, pas un fichier"). Les signaux existent (VersionInfo, certificat, ressources, icône, chaînes), le scoring de confiance est exigé, mais aucune stratégie de fusion, de conflit, ou de correction manuelle n'est esquissée. C'est l'inversion classique : 80 % de spec sur les 20 % faciles.

1.5 Bottlenecks potentiels (par ordre de gravité réelle)

┌───────────────────────────────────────────────────────────────────────────────────────────┬──────────────────────────────────────┬──────────────────────────────────────────────────────────┐
│                                        Bottleneck                                         │                Nature                │                         Gravité                          │
├───────────────────────────────────────────────────────────────────────────────────────────┼──────────────────────────────────────┼──────────────────────────────────────────────────────────┤
│ Hash intégral de la bibliothèque (25 To lus pour 500k × 50 Mo)                            │ I/O                                  │ Critique — c'est des heures, même sur NVMe               │
├───────────────────────────────────────────────────────────────────────────────────────────┼──────────────────────────────────────┼──────────────────────────────────────────────────────────┤
│ Lectures multiples du même fichier par étapes séparées                                    │ I/O                                  │ Critique (évitable par design)                           │
├───────────────────────────────────────────────────────────────────────────────────────────┼──────────────────────────────────────┼──────────────────────────────────────────────────────────┤
│ Vérification Authenticode avec révocation en ligne (WinVerifyTrust contacte les CRL/OCSP) │ Réseau caché dans une étape "locale" │ Élevée — il faut la désactiver explicitement             │
├───────────────────────────────────────────────────────────────────────────────────────────┼──────────────────────────────────────┼──────────────────────────────────────────────────────────┤
│ Lookup externe dans le pipeline de scan                                                   │ Réseau + rate limits                 │ Élevée                                                   │
├───────────────────────────────────────────────────────────────────────────────────────────┼──────────────────────────────────────┼──────────────────────────────────────────────────────────┤
│ Écritures DB non batchées (une transaction par fichier)                                   │ I/O DB                               │ Élevée — SQLite fait ~50k inserts/s batché, ~50/s sinon  │
├───────────────────────────────────────────────────────────────────────────────────────────┼──────────────────────────────────────┼──────────────────────────────────────────────────────────┤
│ Parsing PE, VersionInfo                                                                   │ CPU                                  │ Faible — en-têtes seulement, quelques Ko par fichier     │
├───────────────────────────────────────────────────────────────────────────────────────────┼──────────────────────────────────────┼──────────────────────────────────────────────────────────┤
│ Lecture de MSI (COM, msi.dll)                                                             │ CPU/threading                        │ Moyenne — API COM à apartments, parallélisation délicate │
└───────────────────────────────────────────────────────────────────────────────────────────┴──────────────────────────────────────┴──────────────────────────────────────────────────────────┘

---
2. Problèmes identifiés

2.1 — Performance : le pipeline prescrit viole ses propres contraintes. (Critique) §10 exige "optimisation I/O disque critique" ; §8 prescrit un ordre d'étapes qui maximise les lectures redondantes et hashe avant de pré-filtrer. Cause racine : le pipeline a été pensé comme une liste conceptuelle d'analyses, pas comme un flux physique de données.

2.2 — Scalabilité : aucun modèle d'exécution. (Élevé) "CPU scaling multi-thread obligatoire" est posé comme règle mais rien ne dit quoi paralléliser. Or la réponse n'est pas triviale : le parallélisme utile est par-fichier (N workers), la DB veut un écrivain unique (SQLite), le COM MSI veut des apartments STA, et le disque HDD veut au contraire peu de lecteurs concurrents (seek thrashing) là où le NVMe en veut beaucoup. Le modèle d'exécution est une décision d'architecture, pas un détail d'implémentation.

2.3 — Couplage : le remède prescrit est pire que le mal. (Moyen) Le système de plugins (§11) avec chargement dynamique, versioning strict des interfaces et compatibilité ascendante est exigé avant qu'un seul analyseur n'existe. Concevoir un contrat de plugin stable sans avoir écrit trois analyseurs concrets, c'est deviner l'interface — et la compatibilité ascendante obligatoire transformera chaque erreur de devinette en dette permanente. Un plugin n'a de valeur que s'il existe des développeurs tiers ; il n'y en a pas.

2.4 — Complexité : sur-modularisation prescriptive. (Élevé) 16 modules obligatoires + 4 connecteurs + API + CLI + GUI + plugins pour un outil mono-utilisateur dont la phase 1 de la roadmap est "scan + metadata + hash". Le ratio spec/besoin immédiat est d'environ 10:1. Chocolatey et Scoop, notamment, n'apportent presque rien : leurs métadonnées sont moins riches que WinGet et leur couverture recoupe la sienne.

2.5 — Dette technique : la dette est documentaire, et c'est la plus grave. (Critique) Cinq documents de gouvernance, quatre méta-prompts d'analyse, zéro code, zéro ADR rempli, zéro benchmark alors que la règle absolue est "aucune optimisation sans benchmark" — il n'existe même pas de harnais pour en faire un. Le projet est en risque d'architecture astronaut : chaque itération produit plus de spécification à maintenir et repousse le contact avec le réel. Les questions ouvertes de Master_Prompt.md (quel langage ? quelle DB ? quels hash ?) ont des réponses connues de l'état de l'art — les laisser ouvertes est un coût, pas une prudence.

2.6 — Absence d'étude de l'existant. (Élevé) Master_Prompt.md la demande explicitement (lignes 507–510) et elle change le périmètre : une partie substantielle du projet existe déjà sous forme réutilisable — voir §5.4. Ne pas la faire d'abord, c'est risquer de réécrire winget-create.

---
3. Architectures alternatives

Architecture A — Monolithe séquentiel minimal (amélioration incrémentale)

Description. Un exécutable console C# unique. Boucle : énumérer les fichiers, pour chacun exécuter les analyseurs en séquence (une seule ouverture de fichier), écrire en SQLite par lots. Pas de framework de pipeline, pas d'interface d'analyseur — des fonctions.

Principes. Le plus petit programme qui livre la phase 1–2 de la roadmap. Parallélisme limité à un Parallel.ForEach naïf.

Structure logique.
scan.exe
 ├── Walker (énumération FS)
 ├── AnalyzeFile(path) → FileRecord   // metadata + PE + VersionInfo + Authenticode + hash, 1 lecture
 ├── Identify(FileRecord) → SoftwareMatch
 └── Store (SQLite, transactions batchées)

Avantages. Livrable en semaines ; débogage trivial ; confronte immédiatement le modèle de données au réel ; presque tout le code écrit survivra dans l'architecture B.
Inconvénients. Pas de reprise fine ; parallélisme naïf qui saturera mal (workers bloqués sur l'écrivain DB, pas de backpressure) ; pas de couture propre pour ajouter des analyseurs ; le lookup externe n'a pas de place naturelle.
Impact performance. Correct sur 10–50k fichiers ; plafond net vers 100k+ (contention DB, pas de priorisation, reprise = tout relancer).
Impact maintenance. Excellent à court terme, refactor obligatoire à moyen terme — mais un refactor informé par le réel.

Architecture B — Monolithe modulaire, pipeline producteur/consommateur borné (refactor modéré)

Description. Un seul processus .NET. Les étages sont reliés par des channels bornés (System.Threading.Channels) : un walker produit des chemins, un pool de N workers exécute tous les analyseurs locaux sur un fichier en un seul passage de lecture, un écrivain SQLite unique consomme les résultats par lots. Les analyseurs implémentent une seule petite interface compilée dans le binaire (extensibilité = ajouter une classe, pas charger une DLL). Le lookup externe et l'enrichissement catalogue sont une phase 2 distincte, pilotée par des requêtes sur la base, jamais dans le flux de scan. Reprise : la table files porte un statut + (taille, mtime) ; relancer le scan ne retraite que le nouveau/modifié.

Principes. Une frontière par différence de rythme (FS / CPU / DB / réseau), pas par différence de sujet. Backpressure par construction (channels bornés = mémoire plafonnée). Un seul écrivain DB = zéro contention SQLite. Pré-filtrage avant hash intégral : taille → hash partiel (xxHash 128 Ko tête+queue) → SHA-256 complet seulement pour les groupes candidats et les fichiers destinés au matching WinGet.

Structure logique.

mermaid
flowchart LR
    subgraph "Phase scan (local, offline)"
        W[Walker FS] -->|"channel borné (chemins)"| P
        subgraph P["Pool workers ×N (1 lecture/fichier)"]
            A1[Metadata] --> A2[PE + VersionInfo] --> A3[Authenticode offline] --> A4[Détection installeur] --> A5[Hash partiel]
        end
        P -->|"channel borné (FileRecord)"| WR["Écrivain SQLite unique (batch)"]
    end
    subgraph "Phase enrichissement (réseau, à la demande)"
        Q[Requêtes DB] --> ID[Résolution identité + scoring]
        ID --> CW[Connecteur WinGet]
        ID --> CG[Connecteur GitHub]
        CW & CG --> WR2[Écrivain SQLite]
    end
    WR -.->|base commune| Q
    R[Rapports / CLI] --> Q

Avantages. Atteint les contraintes de performance de CLAUDE.md par construction (1 lecture/fichier, backpressure, batch DB, N ajustable HDD vs SSD) ; reprise native ; couture d'extensibilité réelle (l'interface analyseur) sans le coût des plugins dynamiques ; les deux rythmes local/réseau sont découplés ; testable étage par étage.
Inconvénients. Plus de code d'infrastructure que A (channels, statuts, annulation) ; l'extensibilité exige une recompilation (pas de plugins tiers) ; un seul processus = pas de GUI découplée gratuite (mais une CLI + rapports couvre les phases 1–5 de la roadmap).
Impact performance. Proche de l'optimal pour ce problème : le débit est borné par le disque, qui est saturé par le pool ; SHA-256 est accéléré matériellement (extensions SHA-NI, plusieurs Go/s par cœur) donc le CPU ne sera jamais le goulot.
Impact maintenance. Bon : une dizaine de projets/namespaces, un seul binaire, un seul langage, dépendances comptées sur les doigts d'une main.

Architecture C — Microkernel événementiel à plugins dynamiques (redesign maximal, ce que §11 de CLAUDE.md implique pleinement réalisé)

Description. Un noyau minimal (bus d'événements, registre de plugins, cycle de vie). Chaque analyseur, connecteur, et même le stockage sont des plugins chargés dynamiquement (AssemblyLoadContext), communiquant par messages. API REST locale ; CLI et GUI sont des clients de l'API. Interfaces de plugins versionnées avec compatibilité ascendante garantie.

Structure logique.
mermaid
flowchart TD
    K["Noyau : bus d'événements + registre plugins"]
    K --- P1[Plugin PE] & P2[Plugin Authenticode] & P3[Plugin MSI] & P4[Plugin Hash]
    K --- C1[Plugin WinGet] & C2[Plugin GitHub] & C3[Plugin Chocolatey]
    K --- S[Plugin Stockage]
    API[API REST locale] --> K
    CLI[CLI] & GUI[GUI] --> API

Avantages. Extensibilité maximale, plugins tiers possibles, isolation des pannes par plugin, UI totalement découplée. C'est l'architecture qui satisfait littéralement toutes les exigences d'extensibilité de CLAUDE.md §11.
Inconvénients. Coût de développement multiplié par 3–5 ; le versioning d'interfaces avec compatibilité ascendante devient un travail permanent ; le débogage traverse un bus de messages ; la sérialisation inter-plugins et l'indirection ajoutent un overhead précisément sur le chemin chaud (des millions d'événements par scan) ; tout cela pour un écosystème de plugins tiers qui n'existe pas et n'existera vraisemblablement jamais pour un outil de niche mono-utilisateur.
Impact performance. Négatif net : le message-passing sur le chemin par-fichier coûte plus que l'architecture B, pour zéro gain de débit disque.
Impact maintenance. Le pire des trois : la surface de contrats à maintenir dépasse la surface de logique métier pendant les deux premières années.

Variante écartée d'emblée — cœur Rust + shell C#. Master_Prompt.md la suggère. Rejetée : le goulot est l'I/O disque et SHA-256 accéléré matériellement — un cœur Rust n'accélère rien de mesurable ici, tandis que les API indispensables (COM msi.dll, WinVerifyTrust, WinRT pour MSIX/APPX) sont nettement plus ergonomiques depuis .NET. Deux langages = deux toolchains, une frontière FFI, et une violation directe de "aucune dépendance inutile". La règle de CLAUDE.md "aucune optimisation sans benchmark" s'applique aussi au choix du langage : Rust serait une optimisation non mesurée.

---
4. Comparaison

Convention : 10 = meilleur. "Complexité" : 10 = simple.

┌────────────────────────────────────────────────────┬────────────────────────┬────────────────────────┬─────────────────────────┐
│                      Critère                       │ A — Séquentiel minimal │ B — Pipeline P/C borné │ C — Microkernel plugins │
├────────────────────────────────────────────────────┼────────────────────────┼────────────────────────┼─────────────────────────┤
│ Performance (CPU/I/O)                              │ 4                      │ 9                      │ 6                       │
├────────────────────────────────────────────────────┼────────────────────────┼────────────────────────┼─────────────────────────┤
│ Scalabilité (→ centaines de milliers de fichiers)  │ 3                      │ 9                      │ 7                       │
├────────────────────────────────────────────────────┼────────────────────────┼────────────────────────┼─────────────────────────┤
│ Maintenabilité                                     │ 7                      │ 8                      │ 3                       │
├────────────────────────────────────────────────────┼────────────────────────┼────────────────────────┼─────────────────────────┤
│ Complexité (simplicité)                            │ 10                     │ 7                      │ 2                       │
├────────────────────────────────────────────────────┼────────────────────────┼────────────────────────┼─────────────────────────┤
│ Extensibilité                                      │ 3                      │ 7                      │ 10                      │
├────────────────────────────────────────────────────┼────────────────────────┼────────────────────────┼─────────────────────────┤
│ Robustesse / reprise                               │ 3                      │ 9                      │ 7                       │
├────────────────────────────────────────────────────┼────────────────────────┼────────────────────────┼─────────────────────────┤
│ Coût de développement (10 = faible)                │ 10                     │ 7                      │ 2                       │
├────────────────────────────────────────────────────┼────────────────────────┼────────────────────────┼─────────────────────────┤
│ Globale (pondérée par les exigences de CLAUDE.md)* │ 5,1                    │ 8,2                    │ 4,9                     │
└────────────────────────────────────────────────────┴────────────────────────┴────────────────────────┴─────────────────────────┘

* Pondération : performance et robustesse sont des exigences fonctionnelles dans CLAUDE.md (§9, §10), l'extensibilité tiers est spéculative (aucun utilisateur) — je pondère donc perf/scalabilité/robustesse ×2, extensibilité ×0,5. Sans cette pondération : A=5,7 ; B=8,0 ; C=5,3 — le classement ne change pas.

---
5. Recommandation finale

Architecture retenue : B — monolithe modulaire à pipeline producteur/consommateur borné, construite en démarrant par A comme squelette

Une seule architecture cible : B. Mais le chemin y menant passe par A, car ~90 % du code de A (les analyseurs, le modèle de données, le stockage) est réutilisé tel quel dans B — seul l'orchestrateur change. Concrètement : livrer A sur 10 000 fichiers réels en milestone 1, mesurer (le harnais de benchmark exigé par §10 naît ici), puis introduire channels + reprise en milestone 2.

Décisions structurantes qui en découlent (à tracer en ADR, la section §17 de CLAUDE.md est vide et c'est le vrai blocage du projet) :

1. Langage : C# / .NET 8+. Justification : accès natif idiomatique à VersionInfo, Authenticode (WinVerifyTrust), MSI (COM), MSIX (WinRT) ; SHA-256 matériel via SHA256.HashData ; perf suffisante car le goulot est le disque ; un seul runtime à maintenir. Alternatives rejetées : Rust (voir §3, optimisation non mesurée à coût élevé), Go (interop COM/Win32 pénible, pas d'écosystème Windows-internals), C++ (coût de maintenance sans gain).
2. Base : SQLite en mode WAL, écrivain unique, transactions batchées. Justification : des centaines de milliers de lignes est un volume trivial pour SQLite ; zéro administration ; le fichier DB est le journal de reprise. Rejets : DuckDB (moteur OLAP, mauvais en upserts incrémentaux qui sont exactement notre charge), PostgreSQL (un serveur à administrer pour un outil desktop viole "aucune dépendance inutile"), RocksDB (clé-valeur sans requêtes de matching), LiteDB (moins mûr, moins rapide, aucun avantage).
3. Hash : xxHash3 partiel (pré-filtre) + SHA-256 complet (identité et interop). SHA-256 est non négociable : c'est le hash des manifests WinGet, donc la clé de jointure avec le monde extérieur. BLAKE3 est plus rapide que SHA-256 logiciel mais pas que SHA-256 matériel, et ne joint avec rien. CRC32 et SHA-512 : aucun usage, rejetés.
4. Extensibilité : interface d'analyseur compilée, pas de plugins dynamiques. Le §11 de CLAUDE.md est amendé, pas supprimé (règle : jamais de suppression sans alternative argumentée) : la couture est posée dès le départ (une interface, un registre), le chargement dynamique est différé jusqu'à preuve d'un besoin tiers. Le chemin B→C reste ouvert à coût modéré via AssemblyLoadContext le jour où il existe un deuxième contributeur.
5. Lookup externe hors du pipeline de scan, en phase séparée pilotée par la base — et Authenticode vérifié avec révocation hors-ligne pendant le scan (sinon chaque fichier signé déclenche des requêtes CRL/OCSP silencieuses).

Compromis acceptés. Pas de plugins tiers sans recompilation ; un seul processus (la GUI, si elle advient, sera un front séparé lisant la même base — décision reportable sans coût) ; SQLite impose l'écrivain unique (c'est ici une vertu, pas une limite) ; la version A intermédiaire sera jetée en partie (l'orchestrateur, ~10 % du code).

Pourquoi les autres sont rejetées. A comme cible : plafonne exactement sur les deux exigences fonctionnelles du projet (volume et reprise) et n'offre aucune couture d'extension — elle ne survit pas à la phase 3 de la roadmap. C : paie immédiatement et en permanence (contrats versionnés, bus, débogage distribué) pour un bénéfice — l'écosystème de plugins tiers — dont l'existence est spéculative ; elle viole KISS et YAGNI que Prompt_qualité.md impose d'auditer, et dégrade même la performance sur le chemin chaud.

5.4 Étude de l'existant (exigée par Master_Prompt.md) — briques à réutiliser au lieu de réécrire

Aucun outil existant ne couvre la vision complète (bibliothèque locale + résolution d'identité + cycle de vie des versions + préparation WinGet) : la niche est réelle. Mais plusieurs briques majeures existent :

┌───────────────────────────────────────────────┬─────────────────────────────────────────────────────────┬───────────────────────────────────────────────────────────────────────────────────────────────┐
│                    Brique                     │                    Ce qu'elle couvre                    │                                         Réutilisation                                         │
├───────────────────────────────────────────────┼─────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────┤
│                                               │ Parse EXE/MSI/MSIX, extrait métadonnées, détecte le     │ La brique la plus précieuse : couvre une grande partie des modules "détection type            │
│ microsoft/winget-create (C#, MIT)             │ type d'installateur, génère et valide des manifests     │ installateur" et toute la phase 6 (contribution WinGet). À réutiliser comme bibliothèque ou à │
│                                               │ WinGet                                                  │  défaut comme référence d'implémentation                                                      │
├───────────────────────────────────────────────┼─────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────┤
│ Index source WinGet (source.msix = une base   │ Matching local hors-ligne contre l'intégralité de       │ Élimine le besoin de scraper microsoft/winget-pkgs ; le "connecteur WinGet" devient :         │
│ SQLite précompilée de tout le catalogue)      │ WinGet                                                  │ télécharger un fichier, l'attacher en SQLite                                                  │
├───────────────────────────────────────────────┼─────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────┤
│ Detect It Easy (base de signatures)           │ Détection NSIS / Inno / InstallShield / Burn /          │ Réutiliser la base de signatures (formats documentés) plutôt qu'écrire des heuristiques       │
│                                               │ Squirrel, etc.                                          │ maison                                                                                        │
├───────────────────────────────────────────────┼─────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────┤
│ PeNet ou AsmResolver (NuGet)                  │ Parsing PE, ressources, VersionInfo, imports            │ Évite d'écrire un parseur PE                                                                  │
├───────────────────────────────────────────────┼─────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────┤
│ System.IO.Hashing (Microsoft)                 │ xxHash3/xxHash128                                       │ Le pré-filtre en une dépendance officielle                                                    │
├───────────────────────────────────────────────┼─────────────────────────────────────────────────────────┼───────────────────────────────────────────────────────────────────────────────────────────────┤
│ API Windows natives                           │ WinVerifyTrust (Authenticode), COM msi.dll (MSI), WinRT │ Zéro dépendance externe pour toute l'analyse Windows                                          │
│                                               │  Windows.ApplicationModel (MSIX/APPX)                   │                                                                                               │
└───────────────────────────────────────────────┴─────────────────────────────────────────────────────────┴───────────────────────────────────────────────────────────────────────────────────────────────┘

Point de vérification à faire avant l'ADR final : confirmer la licence et l'API publique actuelles de winget-create et le format actuel de l'index source WinGet (mes données datent de début 2026 ; à revalider au moment de l'implémentation).

---
6. Évolutions long terme

Améliorations possibles. Watcher temps réel (USN journal NTFS plutôt que FileSystemWatcher — fiable à grande échelle et donne le diff depuis le dernier scan presque gratuitement) ; GUI en processus séparé lisant la même SQLite ; rapport HTML statique généré (aucun serveur) ; apprentissage progressif du moteur d'identité (les corrections manuelles de l'utilisateur deviennent des règles pondérées — c'est la version raisonnable du "IA si pertinente" de Master_Prompt.md, un classifieur ML n'étant justifié qu'après avoir constaté les limites des heuristiques mesurées).

Risques futurs. (1) Dérive du format des manifests/index WinGet — mitigé par l'isolation en connecteur, exigence déjà bonne dans CLAUDE.md. (2) Rate limits GitHub sur l'enrichissement massif — mitigé par la phase réseau séparée, le cache et un budget de requêtes. (3) Croissance du moteur d'identité en sac de règles illisible — mitigé par l'exigence d'explicabilité déjà présente (chaque score doit tracer ses signaux ; c'est la meilleure règle de tout CLAUDE.md, à protéger). (4) Le risque principal reste méta : que le projet continue de produire de la gouvernance plutôt que du code. La contre-mesure est dans le plan : milestone 1 = binaire A qui scanne 10 000 fichiers réels, pas un document de plus.

Extensions possibles. Contribution WinGet semi-automatisée (génération + validation locale via winget-create, publication toujours manuelle — conforme à la contrainte) ; archivage/rollback des installateurs ; export SBOM-like de la bibliothèque ; connecteurs additionnels si et seulement si un manque de couverture WinGet+GitHub est constaté sur la bibliothèque réelle.

---
Prochaine étape proposée

Le blocage réel du projet est l'absence de décisions tracées. Je peux rédiger les 4 ADR fondateurs (langage C#/.NET, SQLite WAL, architecture B avec chemin A→B, analyseurs compilés vs plugins différés) au format de CLAUDE.md §17 (contexte / décision / alternatives / conséquences), prêts à intégrer — dites-moi si je les écris.