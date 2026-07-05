# Rapport de mesure — corpus 1 (validation)

Campagne de validation empirique du pipeline d'observation, exécutée le 2026-07-05 selon la méthodologie validée (`doc/méthodilogie_pipeline.txt`). Aucun code modifié : le pipeline mesuré est l'oracle.

**Synthèse** : le pipeline a **crashé** sur le corpus tel que constitué (fichier ≥ 2 GiB → exception non gérée, anomalie A1, **C1 FAIL**). Sur le corpus ajusté (496 fichiers), toutes les autres mesures ont été produites : intégrité parfaite (C3, C4, C5 PASS), reproductibilité stricte (C6 PASS), baseline de performance archivée (C9 PASS). Conformément à la règle de lecture des critères, **le pipeline n'est pas encore déclaré apte** : A1 doit être traitée comme bug réel (seul cas de modification autorisé du pipeline figé) avant toute nouvelle phase.

---

## 1. Environnement

| Élément | Valeur |
|---|---|
| Commit Git du binaire mesuré | `f91874e6a9df1b53a1a1e21c34a34dd50607e8fd` (arbre `src/` propre au moment du build) |
| Configuration de build | Release, .NET SDK 10.0.301, exécutable lancé directement (pas de `dotnet run`) |
| OS | Windows 11 Home 10.0.26200 |
| CPU / RAM | Intel Core i7-10700KF @ 3.80 GHz / 24 Go |
| Disque (corpus **et** base) | Crucial P3 Plus 2 To NVMe (CT2000P3PSSD8), volume C: |
| État du cache disque | Non contrôlé (pas de redémarrage). Corpus copié juste avant le run 1 ; run 2 immédiatement après le run 1. Durées identiques entre les deux runs (73,5 s / 73,6 s). |

## 2. Corpus

| Élément | Valeur |
|---|---|
| Chemin racine | dossier de travail temporaire `…\scratchpad\corpus1` (copies à plat, préfixe séquentiel `NNNN_`) |
| Fichiers | **496** (497 constitués, 1 exclu après l'anomalie A1 — voir §6) |
| Volume total | 18 355 098 745 octets (~17,1 Gio) |
| Gel | 2026-07-05 ; provenance complète fichier par fichier dans `annexe-corpus1-manifest.csv` (nom corpus, chemin source, taille, origine) |

**Mode de constitution** (aucun filtrage de contenu, aucune préparation des fichiers ; sélection par extension et par source uniquement) :

| Origine | Sélection | Fichiers |
|---|---|---|
| `C:\ProgramData\Package Cache` | totalité des `*.exe`, `*.msi` | 190 |
| `C:\Users\<user>\Downloads` | totalité des `*.exe`, `*.msi`, `*.msix`, `*.appx`, `*.zip` | 107 |
| `C:\Windows\Installer` | les 200 premiers `*.msi`/`*.msp` par ordre lexical de nom (noms hexadécimaux → sélection arbitraire mais reproductible) | 200 |

Répartition par extension (corpus ajusté) : `.msi` 366, `.exe` 61, `.zip` 59, `.msp` 10.

**Écart de constitution** : le fichier `0206_Reborn 80000 Years c1-394 (epub).zip` (5,89 Go, unique fichier ≥ 2 Gio du corpus) a été déplacé en quarantaine après le crash A1 pour permettre le reste de la campagne. Le corpus mesuré est donc de 496 fichiers.

## 3. Protocole

Commandes exactes (PowerShell) :

```powershell
# Scan (base neuve), chronométrage externe Stopwatch, redirections
$sw = [System.Diagnostics.Stopwatch]::StartNew()
& InstallChecker.exe scan <corpus1> --db <corpus1.db> 1> corpus1-out.tsv 2> corpus1-err.txt
$sw.Stop()

# Run 2 (reproductibilité) avec échantillonnage mémoire (100 ms)
$p = Start-Process InstallChecker.exe -ArgumentList @('scan','<corpus1>','--db','<corpus1-run2.db>') -PassThru ...
while (-not $p.HasExited) { $p.Refresh(); $peak = [math]::Max($peak, $p.WorkingSet64); Start-Sleep -Milliseconds 100 }

# Chemin d'erreur locale (fichier ouvert en partage None, dossier séparé de 2 fichiers)
$fs = [IO.File]::Open('verrouille.txt','Open','Read','None')
& InstallChecker.exe scan <corpus1-lock> --db <corpus1-lock.db>
```

Requêtes : R1–R12 + C5, exécutées telles quelles via Python `sqlite3` (oracle indépendant du pipeline) — fichier versionné `campagne-corpus1.sql`, sorties complètes dans `annexe-R1-R12.txt`. Vérifications d'intégrité complémentaires (doublons, orphelins ×6, manquants ×6, `observation_id` NULL ×6) dans `annexe-integrite.txt`.

**Écarts au protocole prévu** : (a) C6 (double run) était prévu au corpus 2, exécuté ici par anticipation — coût marginal ; (b) le fichier verrouillé prévu dans la description du corpus 1 ne peut pas exister dans un corpus copié : le chemin d'erreur a été exercé par un micro-scan séparé (2 fichiers dont 1 verrouillé) ; (c) R9 décliné en R9bis (msi_properties) et R9ter (pe_info) en plus de version_info.

## 4. Résultats bruts

### Mesures générales

| Mesure | Valeur |
|---|---|
| Fichiers observés | 496 |
| Durée externe (run 1) | 73,5 s |
| Durée interne (R1, MIN→MAX `scanned_at`) | 72,9 s |
| Débit fichiers | 6,7 fichiers/s |
| Débit octets | ~250 Mo/s (18 355 098 745 octets / 73,5 s) |
| Erreurs locales | 0 (stderr : uniquement la ligne résumé `Scan terminé : 496 fichier(s), 0 erreur(s) locale(s).`) |
| Taille base (R12) | 430 080 octets (0,41 Mo) |
| Ratio base/corpus | 0,0023 % |
| Taille moyenne par observation (base/fichiers) | 867 octets |
| Mémoire crête (WorkingSet, run 2) | 3 020,7 Mo |
| Durée run 2 | 73,6 s |
| Tailles fichiers min / médiane / max | 192 octets / 700 Kio / 1,57 Gio |

### R4 — Répartition des conteneurs

| Conteneur | n | % |
|---|---|---|
| ole-cfb | 376 | 75,81 % |
| pe | 61 | 12,30 % |
| zip | 59 | 11,90 % |
| NULL | 0 | 0 % |

### VersionInfo (au moins une des 4 colonnes non NULL)

| | n | % |
|---|---|---|
| Présent | 57 | 11,5 % |
| Absent | 439 | 88,5 % |

R9 : les 4 colonnes (`product_name`, `company_name`, `product_version`, `file_version`) ont le même taux de NULL, 88,51 % — quand VersionInfo est présent, les 4 champs le sont.

### Authenticode

| | n | % |
|---|---|---|
| Certificat présent | 417 | 84,1 % |
| Absent | 79 | 15,9 % |

R8 (top signataires, fréquences brutes) : Microsoft Corporation 192, .NET 64, Python Software Foundation 59, Microsoft MOPR 28, Adobe 11, Cisco 9, Oracle 5… (25 premiers dans `annexe-R1-R12.txt`).

### PE (R7)

Lignes `pe_info` avec `machine` non NULL : **81** (16,3 %).

| machine | optional_header_magic | subsystem dominant | n |
|---|---|---|---|
| `014c` | `010b` | `0002` | 49 |
| `4b50` | NULL | NULL | 20 |
| `8664` | `020b` | `0002`/`0003` | 10 |
| `aa64` | `020b` | — | 2 |

Répartition subsystem (sur les 81) : `0002` 55, `0003` 6, NULL 20. Les 20 lignes `machine='4b50'` portent toutes sur des fichiers de conteneur `zip` — voir anomalie A2.

### MSI

| Mesure | n |
|---|---|
| MSI détectés (≥ 1 propriété non NULL) | 366 (73,8 % du corpus) |
| ProductName / ProductVersion / Manufacturer / ProductCode / UpgradeCode / ProductLanguage présents | 366 chacun (100,0 % des MSI détectés) |

R9bis : taux de NULL identique (26,21 %) sur les 6 colonnes — les propriétés MSI sont toutes-ou-rien sur ce corpus. Les 10 fichiers `ole-cfb` sans propriétés sont exactement les 10 `.msp` du corpus.

### APPX

| | n |
|---|---|
| Manifeste présent | 0 |
| Manifeste absent | 496 |

Aucun des 59 ZIP du corpus ne contient d'`AppxManifest.xml` : la capacité `appx_manifest` n'a pas été exercée par ce corpus (voir §8 Limites).

### R6 — Croisement conteneur × signaux

| Conteneur | n | product_name | certificat | props MSI | manifeste appx |
|---|---|---|---|---|---|
| ole-cfb | 376 | 0 | 360 | 366 | 0 |
| pe | 61 | 57 | 57 | 0 | 0 |
| zip | 59 | 0 | 0 | 0 | 0 |

### R10 — Multiplicité des sha256

496 observations, **380 contenus distincts** (23,4 % d'observations redondantes ; maximum 3 occurrences pour un même hash — top 10 dans l'annexe).

### R11 — Distribution des tailles

| Classe | n | Mo total |
|---|---|---|
| < 1 Mo | 288 | 130 |
| 1–10 Mo | 108 | 426 |
| 10–100 Mo | 60 | 1 695 |
| 100 Mo – 1 Go | 37 | 10 924 |
| > 1 Go | 3 | 4 330 |

## 5. Contrôles d'intégrité

| Contrôle | Résultat |
|---|---|
| R2 — égalité stricte des 7 comptes (496 partout) | **PASS** |
| R3 ×6 — orphelins (`observation_id` sans `scan_observations.id`) | **PASS** (0 partout) |
| Doublons ×6 (`observation_id` en double dans une table capacité) | **PASS** (0 partout) |
| Manquants ×6 (`scan_observations.id` sans ligne capacité) | **PASS** (0 partout) |
| `observation_id` NULL ×6 | **PASS** (0 partout) |
| C5 — `length(sha256) <> 64` | **PASS** (0) |
| Double run (corpus gelé, dumps triés hors `id`/`scanned_at`) | **PASS** (7 tables identiques) |

SQL utilisées : `campagne-corpus1.sql` (R1–R12, C5) et `annexe-integrite.txt` (sorties complètes, y compris les requêtes de doublons/orphelins/manquants/NULL par table).

## 6. Anomalies constatées

**A1 — Crash global sur fichier ≥ 2 Gio (critique).**
Fichier : `0206_Reborn 80000 Years c1-394 (epub).zip` (6 326 616 313 octets). Symptôme : exception non gérée, processus terminé (exit `-532462766` / 0xE0434352), scan interrompu au 206ᵉ fichier, **aucune observation persistée** (transaction unique jamais commitée — la base ne contenait que le schéma). Reproduit de façon déterministe sur le fichier isolé. Stderr complet dans `annexe-crash-stderr.txt` :

```
Unhandled exception. System.ArgumentException: Stream length minus starting position is too large to hold a PEImage. (Parameter 'peStream')
   at System.Reflection.Internal.StreamExtensions.GetAndValidateSize(Stream stream, Int32 size, String streamParameterName)
   at System.Reflection.PortableExecutable.PEReader..ctor(Stream peStream, PEStreamOptions options, Int32 size)
   at InstallChecker.PeInfoExtractor.Read(String path) in ...\PeInfoExtractor.cs:line 23
```

Fait observable : `PEReader` lève `ArgumentException` (et non `BadImageFormatException`, seule exception gérée par `PeInfoExtractor`) pour tout flux dont la taille excède `int.MaxValue`. Tout fichier ≥ 2 Gio, quel que soit son contenu, termine le processus. Violation de CLAUDE.md §9 (« aucun crash global autorisé », « isolation des erreurs par fichier »).

**A2 — Lignes `pe_info` renseignées sur des fichiers non-PE.**
20 lignes ont `machine='4b50'` (octets « PK »), `characteristics` et `timestamp` renseignés, `subsystem` et `optional_header_magic` NULL. Les 20 portent sur des fichiers de conteneur `zip`. Fait observable : `PEReader` accepte des flux sans en-tête MZ et les lit comme objets COFF ; les valeurs stockées sont les octets du début du ZIP relus comme en-tête COFF. Conforme à la philosophie « valeurs brutes telles que retournées par l'API », mais tout consommateur de `pe_info` doit savoir que `machine` non NULL n'implique pas « fichier PE ». 39 des 59 ZIP ont en revanche levé `BadImageFormatException` → lignes toutes-NULL : le comportement n'est pas uniforme au sein du même conteneur.

**A3 — Mémoire crête élevée.**
WorkingSet crête : 3 020,7 Mo pour une base finale de 0,41 Mo, sur un corpus dont le plus gros fichier fait 1,57 Gio. Constat brut, sans diagnostic (interdit à cette étape) ; le verdict C7 (« mémoire bornée ») exige la comparaison corpus 2/3.

## 7. Verdict par critère

| # | Critère | Constat | Verdict |
|---|---|---|---|
| C1 | Terminaison | Crash sur le corpus tel que constitué (A1) ; exit 0 seulement après exclusion du fichier ≥ 2 Gio | **FAIL** |
| C2 | Complétude | 496 observations = 496 lignes stdout ; 0 fichier manquant | PASS |
| C3 | Intégrité 1:1 | R2 : 7 × 496 | PASS |
| C4 | Intégrité référentielle | R3 ×6 : 0 orphelin | PASS |
| C5 | Validité des hash | 0 hash de longueur ≠ 64 | PASS |
| C6 | Reproductibilité | Double run corpus 1 gelé : dumps identiques (exécuté par anticipation, prévu corpus 2) | PASS |
| C7 | Mémoire bornée | Baseline enregistrée (3 020,7 Mo) ; comparaison inter-corpus impossible avec un seul corpus | NON CONCLUANT |
| C8 | Erreurs expliquées | 0 erreur sur le corpus ; chemin exercé par micro-scan : erreur attribuée au fichier précis avec message d'API, scan poursuivi, exit 0 | PASS |
| C9 | Baseline établie | Durées, débits, taille base, distributions R4–R11 archivées | PASS |

**Règle de lecture appliquée** : C1 FAIL → la campagne s'arrête ici, le pipeline n'est **pas** déclaré apte à servir de fondation. L'anomalie A1 est à traiter comme **bug réel** — le seul cas de modification autorisé du pipeline figé — puis la campagne corpus 1 est à rejouer intégralement avant le passage au corpus 2 et à la phase « moteur d'identité ».

## 8. Limites de la campagne

- **Biais de constitution** : sources dominées par Package Cache et Windows Installer → surreprésentation des MSI (75,8 %) et des signataires Microsoft ; les proportions de formats ne représentent que ce corpus, pas une bibliothèque cible.
- **Capacité `appx_manifest` non exercée** (0 manifeste) : aucun MSIX/APPX réel dans les sources ; sa mesure sur corpus réel reste à faire (corpus 2).
- **Aucune valeur statistique** (rôle assumé du corpus 1 : roder le protocole) ; distributions significatives attendues du corpus 2.
- **Cache disque non contrôlé** ; durées à lire comme ordre de grandeur. Timing par capacité impossible sans instrumentation (interdite).
- **Sélection lexicale** des 200 MSI de `C:\Windows\Installer` (sur 339) : arbitraire mais reproductible ; pas d'aléa contrôlé.
- **C7 non conclusif** par construction (un seul corpus).

## Annexes (versionnées dans ce dossier)

- `campagne-corpus1.sql` — requêtes R1–R12 + C5 telles qu'exécutées
- `annexe-R1-R12.txt` — sorties complètes des requêtes
- `annexe-integrite.txt` — mesures + vérifications d'intégrité complètes (doublons, orphelins, manquants, NULL)
- `annexe-crash-stderr.txt` — stderr complet du crash A1 (reproduction isolée)
- `annexe-corpus1-manifest.csv` — manifeste de provenance du corpus gelé (497 entrées, dont le fichier exclu)
