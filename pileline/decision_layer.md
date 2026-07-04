ROLE: DECISION LAYER

INPUT:
- structured_output
- trace_audit

PROCESS:

1. FILTER SIGNALS
   - garder uniquement SIGNALS avec confidence > 0.6

2. WEIGHTING
   - multiplier par:
     - source agreement
     - trace reliability

3. CONTRADICTION RESOLUTION
   - si divergence:
     - downgrade confidence

4. OUTPUT FINAL
   - ranked_signals
   - risk_map
   - recommended_experiments (si applicable)