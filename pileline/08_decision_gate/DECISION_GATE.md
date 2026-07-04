ROLE: DECISION_GATE

INPUT CONTRACT STRICT

Ce module ne peut utiliser QUE :

- FINAL_SCORING output
- EVIDENCE_MAP (read-only, pour justification uniquement)

INTERDICTION ABSOLUE :
- recalcul des scores
- création de nouveaux signaux
- réinterprétation des sources
- modification des poids
- exécution de logique d’audit

--------------------------------------------

OBJECTIF:

Transformer un score final en décision stable et traçable.

--------------------------------------------

INPUT:

- architecture_option
- score (0–100)
- confidence (0–1)
- justification (from FINAL_SCORING)

--------------------------------------------

DECISION RULES:

1. Score ≥ 80 AND confidence ≥ 0.7
   → ACCEPT

2. Score 60–79 OR confidence 0.5–0.7
   → ACCEPT_REDUCED_SCOPE

3. Score 40–59
   → DEFER (requires more data)

4. Score < 40
   → REJECT

5. confidence < 0.5 (regardless of score)
   → ADVISORY_ONLY

--------------------------------------------

CONSTRAINT RULES:

- If FINAL_SCORING flags contradictions → downgrade decision by one level
- If UNRESOLVED critical items exist → cannot output ACCEPT (max ACCEPT_REDUCED_SCOPE)

--------------------------------------------

OUTPUT:

DECISION:
- type: ACCEPT | ACCEPT_REDUCED_SCOPE | DEFER | REJECT | ADVISORY_ONLY
- selected_option
- rationale: must reference FINAL_SCORING fields only
- risk_flags: inherited from TRACE_AUDITOR + SCORING

--------------------------------------------

RULE:

DECISION IS A TRANSFORMATION, NOT AN ANALYSIS

NO NEW INFORMATION IS ALLOWED