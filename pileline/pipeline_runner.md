\# PIPELINE RUNNER (STRICT MODE)



ORDER OF EXECUTION:



1\. TRACE\_AUDITOR

&#x20;  → must output EVIDENCE\_MAP only (no scoring)



2\. DECISION\_SCORER

&#x20;  → must use ONLY:

&#x20;     - EVIDENCE\_MAP

&#x20;     - CONFIDENCE\_AUDIT

&#x20;     - SOURCE\_WEIGHTS



&#x20;  → forbidden:

&#x20;     - reinterpret signals

&#x20;     - regenerate mappings



3\. DECISION\_GATE

&#x20;  → deterministic classification only

&#x20;  → no new reasoning allowed

