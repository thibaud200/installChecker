ROLE: PIPELINE ORCHESTRATOR

OBJECTIF:
Exécuter la pipeline complète sans réinterprétation ni ajout de connaissance.

--------------------------------------------

CONTRAINTES GLOBALES:

- Aucune création de nouveaux signaux
- Aucune interprétation hors source
- Aucun recalcul entre étapes
- Chaque étape utilise uniquement la sortie de l’étape précédente

--------------------------------------------

ÉTAPE 1 — TRACE_AUDITOR

INPUT:
- sources brutes uniquement

OUTPUT ATTENDU:
- EVIDENCE_MAP
- CONFIDENCE_AUDIT
- ERROR_DETECTION
- UNRESOLVED

STOP: ne rien faire d’autre à ce stade

--------------------------------------------

ÉTAPE 2 — FINAL_SCORING

INPUT:
- EVIDENCE_MAP
- CONFIDENCE_AUDIT
- SOURCE_WEIGHTS

RÈGLES:
- utiliser uniquement les signal_ids existants
- aucun consensus implicite
- aucun regroupement non présent dans TRACE

OUTPUT:
- architecture_option
- score (0–100)
- confidence (0–1)
- justification (signal_ids uniquement)

STOP

--------------------------------------------

ÉTAPE 3 — DECISION_GATE

INPUT:
- output FINAL_SCORING uniquement

RÈGLES:
- application de règles déterministes uniquement
- aucune réinterprétation

OUTPUT:
- DECISION TYPE
- selected option
- rationale (référence scoring uniquement)
- risk flags hérités

--------------------------------------------

RÈGLE FINALE:

TRACE → SCORING → DECISION = CHAÎNE LINÉAIRE STRICTE

Aucune boucle, aucune rétroaction, aucun enrichissement.