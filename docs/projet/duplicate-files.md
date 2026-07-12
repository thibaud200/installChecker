# Vision du projet

---

# 1. Mission

Construire une plateforme modulaire permettant d'analyser, inventorier, comparer et maintenir une bibliothèque de logiciels Windows à partir de leurs fichiers d'installation.

Le projet a pour objectif d'aider l'utilisateur à comprendre le contenu de sa bibliothèque, à identifier les logiciels qu'elle contient, à détecter les redondances, à retrouver les versions les plus pertinentes et à faciliter sa maintenance dans le temps.

Le moteur d'identité utilisé par le projet est un composant générique indépendant. Le présent document décrit exclusivement le produit construit au-dessus de ce moteur.

---

# 2. Objectifs

Le projet doit permettre de :

- inventorier une collection de fichiers ;
- identifier les logiciels représentés ;
- regrouper les différentes versions d'un même logiciel ;
- détecter les doublons ;
- détecter les versions obsolètes ;
- proposer une stratégie de conservation ;
- produire des rapports explicables ;
- assister la maintenance d'une bibliothèque logicielle.

Toutes les décisions produites par le système doivent être explicables, reproductibles et auditables.

---

# 3. Public visé

Le projet s'adresse notamment :

- aux particuliers possédant une importante collection d'installateurs ;
- aux administrateurs système ;
- aux collectionneurs de logiciels ;
- aux archivistes numériques ;
- aux contributeurs de catalogues logiciels.

---

# 4. Philosophie

Le projet considère qu'un fichier n'est qu'une représentation physique d'un logiciel.

L'objectif n'est donc pas simplement de comparer des fichiers mais de comprendre les relations qui existent entre les logiciels qu'ils représentent.

Les décisions prises par le système portent sur ces relations et non uniquement sur les empreintes binaires.

---

# 5. Principes

Le projet privilégie :

- l'explicabilité des décisions ;
- la reproductibilité des résultats ;
- l'absence de comportement opaque ;
- la modularité ;
- les performances mesurées ;
- le fonctionnement hors ligne lorsque cela est possible.

Aucune décision importante ne doit être prise sans pouvoir être justifiée.

---

# 6. Architecture fonctionnelle

Le produit est composé de modules métier indépendants.

Chaque module répond à un besoin fonctionnel particulier.

Le moteur d'identité constitue une dépendance commune mais reste totalement indépendant des modules.

Un module métier ne modifie jamais le moteur.

---

# 7. Modules métier

Le dépôt peut accueillir plusieurs modules métier.

À titre d'exemple :

- Duplicate Files
- Software Inventory
- Version Manager
- WinGet Integration
- GitHub Integration
- Reporting
- Library Maintenance

Chaque module possède :

- ses objectifs ;
- ses règles métier ;
- ses rapports ;
- ses options ;
- ses interfaces.

---

# 8. Premier module : Duplicate Files

Le premier module développé est consacré à la gestion des doublons.

Il a pour objectifs de :

- détecter les fichiers identiques ;
- identifier les copies multiples ;
- regrouper les différentes versions d'un même logiciel ;
- comparer les fichiers selon plusieurs critères ;
- proposer des politiques de conservation ;
- produire un rapport avant toute suppression.

Aucune suppression n'est réalisée automatiquement.

Toute suppression reste une décision de l'utilisateur.

---

# 9. Architecture modulaire

Les besoins métier doivent être implémentés dans des modules indépendants.

Le moteur d'identité ne doit jamais évoluer pour répondre à un besoin propre à un module lorsqu'il peut être traité à ce niveau.

Les modules constituent le point d'extension naturel du projet.

---

# 10. Connecteurs

Les connecteurs permettent d'enrichir les informations produites par les modules métier.

Ils sont indépendants du moteur.

Exemples :

- WinGet
- GitHub
- sites éditeurs
- catalogues logiciels
- autres sources spécialisées

---

# 11. Évolutions

Le projet est conçu pour évoluer progressivement.

L'ajout d'un nouveau domaine métier doit se faire par l'ajout d'un nouveau module et non par une modification du moteur.

Le moteur reste commun à l'ensemble des modules.

---

# 12. Vision long terme

À terme, la plateforme doit être capable de répondre à des questions telles que :

- Quel logiciel représente ce fichier ?
- Cette version est-elle la plus récente ?
- Existe-t-il des copies inutiles ?
- Quelle copie faut-il conserver ?
- Ce logiciel est-il présent dans WinGet ?
- Existe-t-il une version plus récente ?
- Ce logiciel est-il correctement signé ?
- Quelle est la meilleure stratégie de maintenance de cette bibliothèque ?

Le projet vise à devenir une plateforme de gestion de bibliothèques logicielles construite autour d'un moteur d'identité générique.