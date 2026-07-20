# Rapport de validation — correctif A1 + re-campagne corpus 1

Exécuté le 2026-07-05, dans la continuité de `rapport-2026-07-05-corpus1.md` (verdict C1 FAIL). Périmètre strict : correction du bug A1 dans `PeInfoExtractor`, re-campagne intégrale du corpus 1 avec le fichier de quarantaine réintégré. Aucune autre modification.

**Synthèse** : **C1 PASS**. Les 497 fichiers (y compris le fichier de 5,89 Gio qui terminait le processus) sont observés, exit 0, intégrité parfaite, reproductibilité stricte. Les seules différences avec le rapport précédent sont celles attendues du correctif. **Le pipeline d'observation est déclaré apte à servir de fondation** (C7 différé par construction aux corpus 2/3) et repasse à l'état figé.

---

## 1. Modifications réalisées

**Un seul fichier de production modifié : `src/InstallChecker.Core/PeInfoExtractor.cs`** (commit `e14b575`).

Avant :

```csharp
catch (BadImageFormatException)
{
    return PeInfo.None;
}
```

Après :

```csharp
catch (Exception ex) when (ex is BadImageFormatException or ArgumentException)
{
    return PeInfo.None;
}
```

`ArgumentException` (levée par le constructeur `PEReader` pour tout flux dont la taille excède `int.MaxValue`) produit désormais exactement le même résultat qu'un non-PE : record `PeInfo.None` (tout NULL), aucune propagation. Tout autre type d'exception continue de remonter normalement. Un commentaire référence le bug A1.

**Un test ajouté** (`tests/InstallChecker.Tests/ScanCommandTests.cs`) : `PeInfoExtractor_FileOver2GiB_ReturnsAllNullWithoutThrowing` — fabrique un fichier de 2 Gio (`int.MaxValue + 1`) par `FileStream.SetLength`, instantané et sans contenu écrit, indépendant du corpus réel ; vérifie l'absence d'exception et l'égalité avec `PeInfo.None`.

**Rien d'autre** : `ObservationStore`, `ScanCommand`, DDL, JSON, ADR, autres extracteurs — intacts (vérifié par `git diff` : 2 fichiers changés, 19 lignes ajoutées, 1 supprimée).

Erratum corrigé au passage dans le rapport précédent : la taille du fichier A1 est 6 326 602 004 octets (et non 6 326 616 313).

## 2. Résultat des tests

| Vérification | Résultat |
|---|---|
| Build (Debug et Release) | 0 erreur, 0 warning |
| Tests | **39/39 verts** (38 existants inchangés + 1 test A1) |

## 3. Re-campagne corpus 1 (497 fichiers, 24 681 700 749 octets)

Protocole identique au rapport précédent (mêmes commandes, mêmes requêtes R1–R12 + C5 de `campagne-corpus1.sql`, même corpus gelé + fichier réintégré), binaire Release au commit `e14b575`.

| Mesure | Valeur | Rappel avant correctif (496 fichiers) |
|---|---|---|
| Fichiers observés | **497** | 496 (+ crash au 206ᵉ sur le corpus complet) |
| Exit code | **0** | −532462766 (0xE0434352) sur le corpus complet |
| Durée externe run 1 | 94,0 s | 73,5 s |
| Durée interne (R1) | 93,3 s | 72,9 s |
| Débit | 5,3 fichiers/s ; ~263 Mo/s | 6,7 fichiers/s ; ~250 Mo/s |
| Erreurs locales | 0 | 0 |
| Taille base | 0,41 Mo (R12 identique) | 0,41 Mo |
| Mémoire crête (run 2) | 3 276,4 Mo | 3 020,7 Mo |
| Durée run 2 | 94,1 s | 73,6 s |

**Fichier géant en base** (vérification ciblée, observation 206) : `size` 6 326 602 004, `sha256` calculé, conteneur `zip`, ligne `pe_info` **toute NULL** — le comportement exact demandé.

**Intégrité** : R2 = 7 × 497 ; 0 orphelin, 0 doublon, 0 manquant, 0 `observation_id` NULL sur les 6 tables ; 0 hash de longueur ≠ 64 — **aucune violation** (sorties complètes : `annexe-integrite-post-A1.txt`).

**Reproductibilité** : double run sur corpus gelé, dumps triés (hors `id`/`scanned_at`) identiques sur les 7 tables — **PASS**.

**Chemin d'erreur locale** : micro-scan fichier verrouillé rejoué sur le binaire corrigé — erreur attribuée, scan poursuivi, exit 0.

## 4. Comparaison avec le rapport précédent

Diff complet des sorties R1–R12 (`annexe-R1-R12-post-A1.txt` vs `annexe-R1-R12.txt`) — la liste exhaustive des différences est :

| Différence constatée | Attendue ? |
|---|---|
| Disparition du crash ; exit 0 | ✔ oui |
| R1/R2 : 497 observations au lieu de 496 ; +6 326 602 004 octets de corpus | ✔ oui |
| R4/R6 : conteneur `zip` 59 → 60 (75,65 / 12,27 / 12,07 %) | ✔ oui (le fichier géant est un ZIP) |
| R10 : 380 → 381 contenus distincts | ✔ oui (contenu unique) |
| R11 : classe « > 1 Go » 3 → 4 fichiers (4 330 → 10 363 Mo) | ✔ oui |
| R9/R9bis/R9ter : dénominateur 497, pourcentages décalés par arrondi (ex. 88,51 → 88,53 %) | ✔ oui (mécanique) |
| Durées/débits : +20,5 s (~6,33 Go hachés en plus) | ✔ oui |
| Mémoire crête : 3 020,7 → 3 276,4 Mo | constat brut (A3 toujours ouverte, verdict C7 aux corpus 2/3) |
| **R5 (signaux), R7 (structure PE), R8 (signataires) : strictement identiques** | ✔ oui — aucun signal modifié par le correctif |

Aucune autre différence. Les anomalies A2 (`pe_info` renseigné sur des ZIP lus comme COFF — les 20 lignes `machine='4b50'` sont inchangées) et A3 (mémoire crête) restent ouvertes, **hors périmètre de ce correctif**, en attente d'arbitrage.

## 5. Verdict par critère

| # | Critère | Verdict | Rappel précédent |
|---|---|---|---|
| C1 | Terminaison | **PASS** (exit 0 sur le corpus complet, fichier géant inclus) | FAIL |
| C2 | Complétude | PASS (497 = 497, 0 manquant) | PASS |
| C3 | Intégrité 1:1 | PASS (7 × 497) | PASS |
| C4 | Intégrité référentielle | PASS (0 orphelin ×6) | PASS |
| C5 | Validité des hash | PASS (0) | PASS |
| C6 | Reproductibilité | PASS (dumps identiques ×7) | PASS |
| C7 | Mémoire bornée | Différé par construction (comparaison corpus 2/3) ; baseline 3 276,4 Mo archivée | NON CONCLUANT |
| C8 | Erreurs expliquées | PASS | PASS |
| C9 | Baseline établie | PASS (archivée, remplace la baseline 496) | PASS |

## 6. Confirmation de non-régression du pipeline

- `git diff ed9c2cd..e14b575 -- src tests` : **2 fichiers** (`PeInfoExtractor.cs`, `ScanCommandTests.cs`), rien d'autre.
- Architecture, `ObservationStore`, `ScanCommand`, DDL, projection JSON, ADR, autres capacités : **inchangés**.
- Aucune heuristique ajoutée, aucune consultation de `file_headers`, aucun test d'extension, aucun filtrage avant `PEReader` : `PeInfoExtractor` reste totalement autonome (le correctif est un élargissement du `catch` existant).
- R5/R7/R8 identiques octet pour octet : le correctif n'a modifié aucune valeur observée sur les 496 fichiers déjà mesurés.

**Conclusion** : A1 corrigée et validée empiriquement. Conformément à la règle de lecture de la méthodologie (C1–C8 PASS, C7 différé par construction), le pipeline d'observation est **déclaré apte à servir de fondation** et repasse à l'état **figé**, prêt pour la phase « Identity Resolution Engine ».

## Annexes (versionnées dans ce dossier)

- `annexe-R1-R12-post-A1.txt` — sorties complètes R1–R12 + C5 de la re-campagne
- `annexe-integrite-post-A1.txt` — mesures et vérifications d'intégrité complètes
- (inchangés : `campagne-corpus1.sql`, `annexe-corpus1-manifest.csv` — le corpus gelé est identique, fichier 0206 réintégré)
