SIGNAL UNIQUENESS ENFORCER



OBJECTIF:

Empêcher toute duplication explicite ou implicite des signaux à travers le pipeline.



\--------------------------------------------



RULE 1 — SINGLE SOURCE OF TRUTH



\- A signal\_id is created ONLY in TRACE\_AUDITOR

\- No other layer may create, split, merge, or rename a signal\_id



\--------------------------------------------



RULE 2 — IMMUTABILITY



Once a signal\_id is created in EVIDENCE\_MAP:



\- it is immutable

\- it cannot be reinterpreted as multiple signals

\- it cannot be aggregated into sub-signals



\--------------------------------------------



RULE 3 — NO SYNTHETIC DERIVATION



Forbidden in all downstream layers:



\- S1a / S1b splitting

\- “variant signals”

\- implicit duplication via rewording

\- hidden recompression into new IDs



\--------------------------------------------



RULE 4 — TRACE BINDING



Each signal\_id must remain bound to:



\- original source\_id

\- exact\_quote(s)

\- mapping\_type (exact | partial | inferred)



No new bindings allowed outside TRACE\_AUDITOR.



\--------------------------------------------



RULE 5 — SCORING CONSTRAINT



FINAL\_SCORING must:



\- treat each signal\_id as atomic unit

\- never expand or decompose signal\_id

\- never infer hidden sub-signals



\--------------------------------------------



RULE 6 — DECISION CONSTRAINT



DECISION\_GATE must:



\- only aggregate score outputs

\- never reference or reinterpret signal structure



\--------------------------------------------



VIOLATION CLASSIFICATION:



\- duplication → invalid pipeline state

\- recomposition → invalid scoring input

\- reinterpretation → invalid decision input



\--------------------------------------------



PIPELINE GUARANTEE:



Signal identity is preserved end-to-end without transformation.

