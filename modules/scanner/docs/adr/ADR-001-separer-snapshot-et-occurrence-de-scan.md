# ADR-001 - Séparer snapshot d'observation et occurrence de scan

**Statut** : accepté et implémenté le 2026-07-19.

## Contexte

Le stockage v2 recopie le cœur et toutes les capacités d'un fichier à chaque scan. Le lecteur évite
les faux doublons en sélectionnant le dernier scan par volume, mais la base grossit inutilement pour
des fichiers inchangés.

Identity et Identity.Access sont gelés. L'historique des passages et la reproductibilité doivent être
conservés.

## Décision

Le schéma Scanner v3 sépare :

- un snapshot immuable et dédupliqué contenant les observations brutes ;
- une entrée légère reliant un chemin et un scan à ce snapshot.

La clé du snapshot couvre toutes les valeurs brutes et une version du contrat d'extraction. Le
Scanner fournit un adaptateur `IObservationsSource` pour v3 et délègue les bases v1/v2 au lecteur
historique gelé.

## Alternatives

- Continuer l'append intégral : rejeté pour son coût de stockage.
- Upsert par chemin : rejeté, car il détruit l'historique et confond chemin et observation.
- Dédupliquer seulement par hash : rejeté, car les capacités et leur version de contrat font partie
  de l'observation reproductible.
- Modifier Identity.Access : rejeté à cause du gel architectural.

## Conséquences

- Les données lourdes d'un fichier inchangé ne sont stockées qu'une fois.
- Chaque scan conserve une occurrence légère et explicable.
- Les bases v1/v2 restent lisibles, mais Scanner exige une nouvelle base pour écrire en v3.
- Un nouveau lecteur vit physiquement dans le module Scanner.
- Le JSON est émis après commit ; un export fichier remplace son contenu au lieu de l'étendre.
- La déduplication n'évite pas la relecture du fichier et n'est jamais utilisée comme identité métier.
