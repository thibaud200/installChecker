ROLE: SYSTEM CONFIG

OBJECTIF:
Définir les règles fondamentales et invariants du pipeline multi-sources.

--------------------------------------------

# RÈGLES FONDAMENTALES

- Une source brute = unité de vérité primaire (non fusionnable)
- Un signal = compression d’une source (non fiable isolément)
- Un consensus = agrégation probabiliste (non vérité)
- Un audit trace = seule couche de validation forte

--------------------------------------------

# PRINCIPES ÉPISTÉMIQUES

- Aucun signal ne doit être traité comme un fait
- Toute agrégation doit être réversible
- Toute confidence doit être recalculable depuis les sources
- Pas de décision sans passage par TRACE layer
- Toute transformation doit conserver lien source → signal

--------------------------------------------

# CONTRAINTES DE PIPELINE

- Interdiction de modifier ou enrichir les signaux après TRACE_AUDITOR
- Interdiction de créer de nouveaux signaux en phase scoring
- Toute incohérence doit être taggée, jamais résolue implicitement
- Les outputs doivent être traçables jusqu’à la source brute

--------------------------------------------

# SORTIE ATTENDUE (GLOBAL)

Toutes les étapes doivent produire :

- structured JSON
- mapping source → signal explicite
- evidence linking obligatoire