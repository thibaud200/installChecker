# Revue technique de l'implémentation — Module Duplicate Files v1

**Date** : 2026-07-11
**Objet** : revue statique exhaustive du **code effectivement écrit** (Tasks 1 → 6), comparé au plan `2026-07-11-module-duplicate-files-v1-implementation-rev3.md`, à la conception `2026-07-11-module-duplicate-files-design.md` (D1→D6), à l'architecture du dépôt et à `CLAUDE.md`.
**Nature** : rapport de conformité et de défauts. Revue **statique uniquement** — aucun build ni test n'a pu être exécuté (environnement Cloud sans SDK .NET, installation bloquée par la politique réseau). **Aucun résultat de compilation ou d'exécution n'est inventé** : toute affirmation est déduite du code lu.
**Portée lue** : les 12 fichiers source du module, les 5 fichiers de test du module, `DuplicatesCommand` + son test, le registre métier, plus deux vérifications dans le code réel du moteur (ré-entrance de `LecteurDObservationsSqlite`, dépendance du csproj).

---

## 0. Faits confirmés par le code lu

- **`LecteurDObservationsSqlite`** ouvre une **connexion neuve à chaque `Projeter*`** (`using var connection = Ouvrir()`) → la double projection d'Ω (`Porteur.Deriver` puis `GenerateurDeRapport`) est **sûre** (pas de bug de re-lecture) ; `Ouvrir()` lève `OmegaAbsentException` si le fichier manque → le chemin « base absente → code 1 » est correct.
- **csproj module** : `ProjectReference` vers **Identity uniquement**. Aucun Access/Core/SQLite.
- **Aucun** TODO/FIXME/HACK/NotImplemented dans le module ni la CLI.

---

## 1. Verdict global

**Conforme avec réserves** — aucune non-conformité, aucun écart bloquant ni fonctionnel important. Les réserves sont mineures ; la seule vraie barrière est que **rien n'a pu être compilé/exécuté** (pas de SDK), ce qui reste la seule preuve manquante.

### Conformité par composant

| Composant | Verdict | Justification (déduite du code) |
|---|---|---|
| `ExtracteurDeGroupes` | **Conforme** | Deux flux : groupes (`Election`+`Contenu`+`Certaine`+`CE-01`) ; `RefusStratesSuperieures` (`Refus`+`Strate != Contenu`). Flux « refus de contenu » supprimé (P2). |
| `EnrichisseurDeGroupe` | **Conforme** | Lit Ω seul (`Observations`), jamais W ; renommages P5 ; `Taille` ; ⊥/clé absente = faux ; invariant P7 documenté. |
| `PolitiqueRetentionV1` | **Conforme** | Ordre D4 exact ; richesse via champs P5 ; départage `Ordinal` + `ActeId` (déterministe). |
| `SyntheseDeBibliotheque` | **Conforme** | 5 agrégats ; **zéro** `using` moteur (pur) ; agrégats 4/5 par rang ; invariant cardinal documenté. |
| `GenerateurDeRapport` | **Conforme** | Synthèse (avant groupes), note dérivée, métriques par groupe (corr. B), étiquetage ; sortie 100 % DTO module. |
| DTO (`RapportDeDoublons`, `GroupeClasse`, `ExemplaireRapporte`, `NoteDeCapacite`, `Synthese`) | **Conforme** | Aucun type moteur ; `NonTranches` supprimé ; ordre synthèse→note→groupes. |
| `DuplicatesCommand` | **Conforme** | Adaptateur pur ; même prédicat d'erreur qu'`IdentityCommand` ; erreurs telles quelles ; sérialise le DTO. |
| Registre métier (`v1.md`, `historique.md`) | **Conforme** | Critères D4 dans l'ordre ; libellé P5 (« présence… », plus de « complétude »). |
| Tests | **Conforme** (avec gaps mineurs) | Voir § 4. |

---

## 2. Écarts bloquants

*Aucun.*

---

## 3. Écarts importants

*Aucun.* Architecture, dépendances, invariants, absence de fuite moteur, D1→D6 : tous respectés.

**Architecture** — `DuplicateFiles` → Identity seul ; aucun Access/Core/SQLite dans le module ; CLI = seul adaptateur (le module ne connaît pas SQLite) ; aucun DTO moteur exposé ; aucune logique métier dans la CLI.
**Invariants** — W/Ω même dérivation (même instance `omega` passée à `Porteur.Deriver` et au générateur) ; cardinal préservé (`Classer` attribue `index+1`, exactement un rang 1) ; agrégats corrects ; classement déterministe ; note dérivée sans exposer les refus ; `NonTranches` disparu ; métriques par groupe possédées par le générateur.
**D1→D6** — D1/D2 (état Ω désigné) ; D3 (suggestion, « candidat à la suppression », aucune suppression) ; D4 ; D5 (motif court, audit non dupliqué) ; D6 (registre versionné, implémentation littérale).

---

## 4. Écarts mineurs

1. **`FichierEnrichi` est documenté « DTO de travail interne au module » mais est exposé tel quel dans le rapport** (via `ExemplaireRapporte.Fichier`) : les trois booléens de présence et la date d'observation se retrouvent dans le JSON utilisateur. Ce n'est **pas** une fuite de type moteur (P4 respecté), et c'est identique à rev1 ; mais c'est une petite incohérence entre le commentaire (« interne ») et l'usage (exposé). *Observation, non prescription.*
2. **Test `La_synthese_precede_les_groupes_dans_le_rapport`** vérifie l'ordre des **paramètres du constructeur** par réflexion — un proxy structurel de « synthèse avant groupes dans la sortie », couplé à la signature du record plutôt qu'au JSON réellement émis. Correct mais légèrement fragile.
3. **Couverture triplet au niveau générateur/CLI-unitaire** : les tests unitaires du générateur n'exercent qu'une **paire**. Le comportement triplet (rang 3 = candidat, espace = taille × 2) n'est couvert qu'indirectement par le test d'intégration corpus (4 triplets réels). `SyntheseDeBibliotheque` teste bien le triplet isolément.
4. **`NomStrate`** possède des branches inatteignables en pratique (`Strate.Contenu` et le `_` par défaut) : les refus de contenu ne parviennent jamais à `DeriverNote`. Défensif, inoffensif.
5. **`DuplicatesCommand.Deriver`** : nom hérité d'`IdentityCommand.Deriver`, mais la méthode dérive W **et** construit le rapport — « Deriver » sous-décrit légèrement. Cohérence vs précision.
6. **`NoteDeCapacite` sérialisée en `"Note": null`** quand absente (options par défaut, sans `WhenWritingNull`) : bruit JSON cosmétique ; la disparition est correcte au niveau DTO.

---

## 5. Vérification des tests (sans supposer qu'un test est correct)

- **Ce qu'ils couvrent réellement** : extracteur (4 cas dont la collecte des refus supérieurs) ; enrichisseur (présence + ⊥/absence) ; politique (les 4 critères + départage stable + absence de signal) ; synthèse (paire, triplet, tailles par groupe, coïncidence v1) ; générateur (assemblage complet + note présente + note absente + non-fuite moteur par réflexion) ; CLI (corpus 112, 5 agrégats, espace>0, 4 strates, reproductibilité, base absente).
- **Test réflexif de non-fuite** (`Aucun_type_du_moteur_ne_fuit…`) : réel et solide — parcourt le graphe de propriétés des types `InstallChecker.*`, suit les arguments génériques, exclut les enums ; garantit P4 **au-delà** de la compilation. Point fort.
- **Gaps** (mineurs, cf. § 4 points 2–3) : proxy d'ordre par réflexion ; pas de triplet unitaire côté générateur.
- **Fragilité cross-platform** : `Path.GetFileNameWithoutExtension` sur des chemins Windows (`C:\…`) se comporte différemment sous Linux, mais la détection de copie (regex en fin de chaîne) reste correcte pour les cas testés, et les assertions du corpus (comptes, agrégats, note) sont indépendantes de la plateforme. Non bloquant (ADR-001 : Windows assumé).
- **`DuplicatesCommandTests` vs rev3 § 5** : attentes **exactement** conformes (112 groupes, synthèse + 5 agrégats, espace>0, note variante/version/identité/famille, reproductibilité, absence de `NonTranches`, erreur base absente code 1).

---

## 6. Performances (signal uniquement, aucun changement proposé)

1. **Ω reprojeté plusieurs fois par exécution** : `Porteur.Deriver` déclenche `ProjeterModele`/`ProjeterIdentite`(→`ProjeterModele`), puis `GenerateurDeRapport` rappelle `ProjeterModele` + `ProjeterContexte` — chacun ouvrant une **connexion SQLite neuve** lisant **toutes** les tables de capacité. C'est exactement la remarque P6 (rev3 § 7). Aucune optimisation — **conforme** à CLAUDE.md § 10.
2. **Matérialisation complète** : dictionnaires de **tous** les actes/contextes construits même si peu sont en groupe (P6). Signal.
3. `SyntheseDeBibliotheque.Calculer` effectue **5 parcours** indépendants de la collection de groupes — négligeable à 112 groupes ; aucun changement sans benchmark.

---

## 7. Points particulièrement réussis

1. **Garantie P4 à l'exécution** par un test réflexif du graphe de DTO — dépasse la simple vérification à la compilation.
2. **Extracteur à deux flux** parfaitement aligné sur P2 ; le flux « refus de contenu » est correctement **supprimé**, pas laissé en garde mort.
3. **`SyntheseDeBibliotheque` réellement pure** : aucun `using` moteur — vérifié.
4. **`DuplicatesCommand` reflète exactement** le régime d'erreur d'`IdentityCommand` (mêmes trois types) ; la ré-entrance confirmée du lecteur SQLite valide le chemin « base absente → `OmegaAbsentException` → code 1 ».
5. **Invariants documentés là où ils sont utilisés** (P7 dans l'enrichisseur/CLI, cardinal dans la synthèse) — traçabilité forte.
6. **Règle de dépendance tenue** : module → Identity seul ; Access/SQLite confinés à la CLI.
7. **Registre métier** aligné P5, `PolitiqueRetentionV1` traduction littérale, déterminisme garanti (`Ordinal` + `ActeId`).

---

## 8. Conclusion

L'implémentation est **prête à être fusionnée après exécution des tests** sur un environnement disposant du SDK .NET 10 (Windows pour la suite complète). Aucun écart bloquant ni important ; les 6 écarts mineurs sont des observations (dont plusieurs héritées de rev1 et conformes au plan), sans correction prescrite — le plan ne les interdit pas.

**Seule barrière réelle** : la validation exécutable, impossible dans cet environnement. À lancer avant fusion :

```
dotnet build
dotnet test
```

Tant que ces commandes n'ont pas tourné au vert, la conformité reste établie par **revue statique uniquement** — aucun build ni résultat de test n'est inventé. Si le build et les 22 tests passent (dont les 3 tests d'intégration corpus), l'implémentation peut être fusionnée en l'état.
