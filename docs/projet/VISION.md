# Vision du projet

---

# 1. Mission

Construire une plateforme permettant de comprendre, inventorier, comparer et maintenir une bibliothèque de logiciels Windows à partir de leurs fichiers d'installation.

Le projet repose sur un moteur d'identité générique capable de dériver un état du monde à partir d'observations. Ce moteur constitue une dépendance du projet et n'est pas spécifique au domaine des logiciels Windows.

Le présent document décrit exclusivement la vision fonctionnelle du produit construit au-dessus de ce moteur.

---

# 2. Objectifs

Le projet doit permettre de :

- inventorier une collection de fichiers d'installation ;
- identifier les logiciels représentés ;
- regrouper les différentes versions d'un même logiciel ;
- détecter les doublons ;
- détecter les versions obsolètes ;
- proposer une stratégie de conservation ;
- produire des rapports explicables ;
- assister la maintenance d'une bibliothèque logicielle.

Toutes les décisions produites par le système doivent rester explicables et reproductibles.

---

# 3. Public visé

Le projet s'adresse notamment :

- aux particuliers possédant une importante collection d'installateurs ;
- aux administrateurs système ;
- aux collectionneurs de logiciels ;
- aux archivistes numériques.

---

# 4. Philosophie

Le projet ne considère jamais un fichier comme une fin en soi.

Le fichier est un support permettant de représenter un logiciel.

Toutes les décisions portent donc sur les logiciels et leurs relations, pas uniquement sur leurs empreintes binaires.

---

# 5. Architecture fonctionnelle

Le produit est composé de plusieurs modules métier indépendants.

Exemple :

- inventaire ;
- détection des doublons ;
- gestion des versions ;
- comparaison avec des catalogues externes ;
- génération de rapports.

Chaque module consomme les informations produites par le moteur d'identité.

Aucun module ne modifie le moteur.

---

# 6. Premier module métier : Duplicate Files

Le premier module développé vise à aider l'utilisateur à rationaliser une collection de fichiers.

Il doit notamment permettre :

- de détecter les doublons exacts ;
- d'identifier les différentes copies d'un même fichier ;
- de comparer plusieurs versions ;
- de définir des politiques de conservation ;
- de produire un rapport avant toute suppression.

La suppression n'est jamais automatique.

---

# 7. Principes de conception

Le projet privilégie :

- l'explicabilité ;
- la reproductibilité ;
- la modularité ;
- les performances mesurées ;
- l'absence de dépendance métier dans le moteur d'identité.

---

# 8. Évolutions prévues

Le premier module métier est consacré à la gestion des doublons.

D'autres modules pourront être développés ultérieurement sans modification du moteur d'identité, par exemple :

- comparaison avec WinGet ;
- comparaison avec GitHub ;
- génération d'inventaires ;
- suivi des mises à jour ;
- analyse de collections logicielles.