À partir du projet situé à : C:\git\installChecker

1. Charger le fichier CLAUDE.md à la racine du projet comme contexte principal.
2. Charger le contenu du fichier Master_prompt.md.
3. Utiliser CLAUDE.md comme source de règles, contraintes et vision du projet.
4. Exécuter Master_prompt.md en mode brainstorming avancé.

---

MODE DE TRAVAIL

- Raisonnement exploratoire (brainstorming)
- Analyse critique obligatoire du système existant
- Interdiction de considérer l’architecture actuelle comme optimale
- Autorisé à proposer plusieurs architectures concurrentes
- Comparaison objective obligatoire des solutions

---

ANALYSE ATTENDUE

Tu dois produire :

## 1. Analyse critique du système actuel
- architecture globale
- flux de données
- modules
- dépendances
- bottlenecks potentiels

## 2. Problèmes identifiés
- performance (CPU / I/O / mémoire)
- scalabilité
- couplage
- complexité
- dette technique

## 3. Architectures alternatives (minimum 2)
Pour chaque architecture :
- description
- principes
- structure logique
- avantages
- inconvénients
- impact performance
- impact maintenance

## 4. Comparaison
- tableau comparatif
- scoring (0–10) :
  - performance
  - scalabilité
  - maintenabilité
  - complexité
  - extensibilité

## 5. Recommandation finale
- architecture recommandée
- justification claire
- compromis acceptés
- raisons du rejet des autres options

## 6. Évolutions long terme
- améliorations possibles
- risques futurs
- extensions possibles

---

RÈGLES IMPORTANTES

- Chaque conclusion doit être justifiée
- Aucune architecture ne doit être supposée correcte a priori
- Toute optimisation doit être cohérente avec CLAUDE.md
- Les limites et risques doivent toujours être explicités