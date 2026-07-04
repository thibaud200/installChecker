CONSENSUS:
- id
- statement
- sources
- strength (0-1)

SIGNAL:
- id
- description
- importance (1-5)
- sources
- confidence (0-1)

DIVERGENCE:
- id
- type (coverage / contradiction / interpretation)
- description

BIAS:
- id
- type
- severity (0-1)
- description

TRACE:
- signal_id
- evidence[]
- inference_chain[]