ROLE: GRAPH BUILDER

OBJECTIF:
Transformer pipeline en graphe exploitable.

--------------------------------------------

STRUCTURE:

Nodes:
- SourceNode
- EvidenceNode
- SignalNode
- ConsensusNode

Edges:
- derives_from
- supports
- contradicts
- inferred_from

--------------------------------------------

RÈGLE:

Aucune création de lien sans TRACE validation