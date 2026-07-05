# CLAUDE.md — Contrat d’architecture et règles du projet

---

# 0. RÈGLES POUR LES IA (CONTRAT D’EXÉCUTION)

Tu es un assistant d’architecture logicielle senior.

## Règles strictes

- Ne réécris jamais tout le document inutilement
- Ne modifies que les sections concernées
- Respecte strictement la structure existante
- Évite toute duplication d’information
- Toute décision architecturale doit être justifiée
- Toute évolution majeure doit être tracée via ADR
- Ne supprime jamais une décision sans proposer une alternative argumentée

## Format obligatoire des modifications

1. Résumé des changements
2. Sections modifiées
3. Nouvelles sections
4. Bloc prêt à intégrer

---

# 1. VISION DU PROJET

## Mission

Créer un système intelligent d’analyse d’installateurs Windows capable de :

- identifier un logiciel à partir d’un fichier installateur
- déterminer sa version, son éditeur et son origine
- analyser les relations entre différentes versions
- détecter doublons et obsolescence
- comparer avec des sources externes (WinGet, GitHub, etc.)
- préparer des contributions communautaires (sans publication automatique)

Le système ne doit pas être un simple outil de déduplication de fichiers.

Il doit devenir un moteur de connaissance des logiciels Windows.

---

## Vision long terme

Le système doit pouvoir répondre à des questions comme :

- Quel logiciel représente cet installateur ?
- Quelle version est-ce ?
- Est-ce la dernière version ?
- Existe-t-il une version plus récente ?
- Est-il signé et par qui ?
- Dois-je le conserver ou le supprimer ?
- Est-il présent dans WinGet ?
- Peut-on générer une contribution WinGet ?

---

# 2. PHILOSOPHIE DU PROJET

## Principes fondamentaux

- [RULE] La donnée représente un logiciel, pas un fichier
- [RULE] Toute décision doit être explicable
- [RULE] Aucun comportement opaque (pas de boîte noire)
- [RULE] Les performances sont une exigence fonctionnelle
- [RULE] Les optimisations doivent être mesurées
- [RULE] La reproductibilité des résultats est obligatoire

---

## Ce que le projet n’est PAS

- ❌ antivirus
- ❌ outil de malware analysis
- ❌ simple hash checker
- ❌ simple duplicate finder
- ❌ package manager
- ❌ moteur de recherche internet

---

# 3. OBJECTIFS

## Objectifs principaux

- Identifier automatiquement un logiciel
- Identifier sa version
- Identifier son éditeur
- Détecter doublons exacts
- Détecter versions obsolètes
- Regrouper les variantes d’un même logiciel
- Proposer une stratégie de conservation
- Identifier la dernière version disponible
- Comparer avec WinGet et autres catalogues

---

## Objectifs secondaires

- Construire une base de connaissance locale
- Fonctionnement offline prioritaire
- Amélioration progressive par apprentissage
- Génération de rapports explicables
- Assistance à maintenance de bibliothèque logicielle

---

# 4. HORS PÉRIMÈTRE

## Ce que le projet n’inclut pas

- analyse malware
- exécution d’installateurs
- gestion de paquets système
- moteur de recherche web généraliste
- scraping massif non contrôlé

---

# 5. ARCHITECTURE GLOBALE

## Principes

- Architecture modulaire stricte
- Aucun couplage direct entre modules critiques
- Le core ne dépend d’aucun catalogue externe
- Tous les services externes passent par des connecteurs

---

## Indépendances obligatoires

Le moteur principal doit être indépendant :

- de la base de données
- de l’interface utilisateur
- des sources externes
- des formats d’entrée/sortie

---

## Sources de données (priorité)

1. Métadonnées fichier
2. Signature numérique
3. Catalogue officiel (WinGet, GitHub)
4. Site éditeur
5. Sources secondaires

---

# 6. MODÈLE DE DONNÉES (CONCEPTUEL)

## Entités principales

- Software (logiciel)
- Installer (fichier)
- Version
- Publisher
- Signature
- Source

## Principes

- modèle unique centralisé
- indépendant des formats externes
- versionné
- extensible sans rupture

---

# 7. MODULES (STRUCTURE LOGIQUE)

## Modules obligatoires

- Scanner de fichiers
- Analyse métadonnées Windows
- Analyse PE
- Analyse signature (Authenticode)
- Détection type installateur
- Hash engine
- Moteur d’identification
- Matching engine
- Gestion des versions
- Détection doublons
- Connecteur WinGet
- Connecteur GitHub
- Connecteur autres catalogues
- Base de données
- Cache
- Logging
- Reporting

---

# 8. PIPELINE DE TRAITEMENT

1. Scan fichiers
2. Extraction métadonnées
3. Analyse binaire (PE / signature)
4. Hash computation
5. Classification installateur
6. Identification logiciel
7. Matching base existante
8. Analyse version
9. Lookup externe (si nécessaire)
10. Stockage en base
11. Génération rapport

---

# 9. QUALITÉ ET ROBUSTESSE

## Règles

- Aucun crash global autorisé
- Isolation des erreurs par fichier
- Reprise après interruption obligatoire
- Logs complets obligatoires
- Mode dégradé accepté

---

# 10. PERFORMANCE

## Contraintes

- Traitement de centaines de milliers de fichiers
- Optimisation I/O disque critique
- CPU scaling multi-thread obligatoire
- Cache agressif mais contrôlé

## Règle absolue

Aucune optimisation sans benchmark.

---

# 11. EXTENSIBILITÉ

## Système de plugins

- Ajout de nouveaux analyseurs sans modification core
- Ajout de nouveaux connecteurs
- Versioning strict des interfaces
- Compatibilité ascendante obligatoire

---

# 12. BASE DE DONNÉES

## Objectifs

- haute performance lecture/écriture
- indexation rapide
- support volume massif
- requêtes de matching efficaces

## Critères d’évaluation

- SQLite / DuckDB / autres à comparer
- gestion concurrence
- scalabilité
- simplicité maintenance

---

# 13. LOGGING & OBSERVABILITÉ

- logs structurés obligatoires
- traçabilité des décisions
- explication des identifications
- auditabilité complète

---

# 14. TESTS & VALIDATION

- tests unitaires obligatoires
- tests d’intégration
- tests de performance
- tests sur dataset massif
- validation de reproductibilité

---

# 15. CE QUE J’ATTENDS DE L’IA

Lorsque tu proposes une solution :

- critique-la
- propose des alternatives
- explique les compromis
- indique les limites
- justifie les choix

Lorsque tu proposes une technologie :

- maturité
- maintenance
- performance
- alternatives

Lorsque tu proposes une architecture :

- risques
- bottlenecks
- complexité
- évolutivité

---

# 16. CE QUE L’IA NE DOIT JAMAIS FAIRE

- inventer des comportements non définis
- modifier une décision sans justification
- supprimer des règles existantes
- ignorer les contraintes de performance
- proposer une solution sans analyse alternative

---

# 17. ADR (ARCHITECTURE DECISION RECORDS)

Toute décision structurante doit être documentée :

- contexte
- décision
- alternatives
- conséquences

## ADR-001 — .NET 10 comme plateforme

- **Contexte** : le pipeline lit des structures Windows natives (PE, Authenticode, MSI) et doit traiter de gros volumes.
- **Décision** : .NET 10 (C#), BCL en priorité (`PEReader`, `X509CertificateLoader`, `ZipFile`), P/Invoke quand la BCL ne couvre pas (msi.dll). Windows-only assumé.
- **Alternatives** : Python (plus lent, parsing PE via lib tierce), Rust/Go (pas d'accès BCL/Win32 aussi direct, coût de développement supérieur).
- **Conséquences** : dépendance au SDK .NET ; portabilité non recherchée.

## ADR-002 — SQLite append-only, invariant 1:1 par observation

- **Contexte** : reproductibilité et auditabilité exigées ; le même fichier peut être observé plusieurs fois.
- **Décision** : chaque scan insère de nouvelles lignes, jamais d'UPDATE/DELETE ni de dédoublonnage. Chaque capacité écrit exactement une ligne par observation (même toute-NULL), liée par `observation_id` (rowid de `scan_observations`).
- **Alternatives** : upsert par chemin ou par hash (perd l'historique, introduit une identité implicite) ; table unique large (colonnes creuses, capacités couplées).
- **Conséquences** : la base grandit à chaque scan ; toute lecture agrège explicitement ; le sens de « doublon » reste une décision d'analyse, pas de stockage.

## ADR-003 — Capacités d'observation autonomes et indépendantes

- **Contexte** : la fiabilité d'une capacité ne doit jamais dépendre d'une autre ; isolation des erreurs par fichier obligatoire (§9).
- **Décision** : chaque extracteur ouvre lui-même le fichier, décide seul si le format le concerne, et ne consulte jamais le résultat d'une autre capacité (pas de « c'est un PE donc… »).
- **Alternatives** : pipeline conditionnel piloté par le type détecté (moins d'I/O mais couplage et propagation d'erreurs entre capacités).
- **Conséquences** : le fichier est ouvert plusieurs fois par scan — coût accepté tant qu'aucune mesure ne le condamne (§10 : aucune optimisation sans benchmark).

## ADR-004 — Observation pure : NULL = absence, aucune interprétation

- **Contexte** : toute décision doit être explicable (§2) ; le moteur d'identité viendra plus tard, comme consommateur séparé.
- **Décision** : valeurs brutes telles que retournées par l'API, NULL = « rien observé » (jamais « unknown » ni valeur sentinelle). Aucune normalisation, aucune identification, aucune fusion dans le pipeline d'observation.
- **Alternatives** : normaliser à l'écriture (versions, éditeurs) — rejeté : mélange observation et interprétation, rend les erreurs d'interprétation irréversibles.
- **Conséquences** : les données stockées sont rejouables et comparables ; toute interprétation future reste réversible car la source brute est conservée.

## ADR-005 — Authenticode via X509Certificate.CreateFromSignedFile

- **Contexte** : observer le certificat feuille embarqué sans valider la chaîne (pas de réseau, pas de jugement).
- **Décision** : `X509Certificate.CreateFromSignedFile` (marquée SYSLIB0057 en .NET 10 ; avertissement supprimé localement et documenté), champs relus via `X509CertificateLoader`.
- **Alternatives** : P/Invoke `CryptQueryObject` (chemin de sortie documenté si l'API est retirée) ; `Get-AuthenticodeSignature` via PowerShell (coût de processus, hors BCL).
- **Conséquences** : signatures par catalogue non couvertes (NULL — c'est une absence d'observation, pas une conclusion « non signé »).

## ADR-006 — MSI via P/Invoke msi.dll en lecture seule stricte

- **Contexte** : lire la table Property d'un MSI sans moteur d'installation ni COM.
- **Décision** : P/Invoke direct de msi.dll avec `MSIDBOPEN_READONLY` ; jamais de session d'installation, de réparation ni de validation.
- **Alternatives** : COM `WindowsInstaller.Installer` (interop plus lourde, mêmes données) ; parsing OLE-CFB manuel (réinvention fragile du format).
- **Conséquences** : Windows requis (cohérent avec ADR-001) ; fixture de test créée par Windows Installer lui-même.

## ADR-007 — Séparation InstallChecker.Core / ObservationStore / CLI

- **Contexte** : le futur moteur d'identité doit réutiliser le pipeline d'observation sans passer par la CLI (§5 : le core indépendant de l'UI et de la base).
- **Décision** : les extracteurs et leurs records vivent dans la bibliothèque `InstallChecker.Core` (sans dépendance SQLite) ; `ObservationStore` est le propriétaire unique du stockage (DDL, INSERT, projection JSON) ; `ScanCommand` orchestre seulement.
- **Alternatives** : tout laisser dans l'exécutable et référencer l'exe comme bibliothèque (couple le consommateur au packaging CLI et aux dépendances SQLite).
- **Conséquences** : refactoring mécanique sans changement de comportement ; le moteur d'identité sera un second consommateur de Core, dans une phase séparée.

## ADR-008 — Versionnage du schéma par PRAGMA user_version, sans migration

- **Contexte** : le schéma évoluera ; il faut détecter une base d'une autre version sans infrastructure de migration prématurée.
- **Décision** : `PRAGMA user_version = 1` à l'initialisation ; à l'ouverture, 0 (défaut SQLite) = base neuve à initialiser, toute autre valeur que 1 = erreur explicite. Aucune migration, aucune conversion : les bases existantes sont jetables.
- **Alternatives** : table de métadonnées dédiée (plus verbeux pour le même service) ; framework de migrations (YAGNI tant que le schéma n'a qu'une version).
- **Conséquences** : changer le schéma = incrémenter la version et régénérer les bases ; une politique de migration ne sera écrite que si des bases deviennent précieuses.

---

# 18. ROADMAP (ÉVOLUTIF)

- Phase 1 : scan + metadata + hash
- Phase 2 : identification logicielle
- Phase 3 : versioning + duplication
- Phase 4 : connecteurs externes
- Phase 5 : optimisation + scale
- Phase 6 : contribution WinGet

---

# 19. GLOSSAIRE

À compléter progressivement.

---

# 20. ANNEXES

À compléter progressivement.