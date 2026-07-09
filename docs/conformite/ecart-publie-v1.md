# Écart publié — non-conformité rétroactive de la v1 sur une classe d'index (017 § 9)

**Nature** : la **publication** exigée par le 011 § 9 (« l'écart est publié, justifié acte par acte, et l'ancienne version est déclarée non conforme rétroactivement ») et exercée au titre de l'exception d'I59. Le contenu normatif est intégralement celui du **017 § 9**, que le présent document matérialise et rend opposable — sans le reformuler : chaque section cite sa lettre.

---

## 1. La classe d'index

« Tout couple (Ω, ℛ) où ℛ est cohérent et contient au moins une convention en vigueur hors de l'ensemble {EQ-01 v1, CE-01 v1} — les seules que le moteur v1 applique réellement (016 § 4.1, report 1 : application « par identifiant codé ») » (017 § 9). Cette caractérisation décrit le comportement constaté de la v1, un fait de clôture — jamais une déclaration de couverture rétroactive.

## 2. Le comportement v1 et sa justification

« Un W était émis, dérivé en ignorant les conventions excédentaires — chaque acte d'un tel W cite un index (Ω, ℛ) dont la dérivation n'a pas mobilisé tout le contenu : sa justification ment sur sa dépendance (I34, I38). » La justification de l'écart est « uniforme et acte par acte à la fois : tout acte de tout W de la classe porte le même vice, l'index cité n'est pas l'index appliqué » (017 § 9).

## 3. Le comportement conforme — les deux sous-classes, démontrées

Le comportement conforme dépend de la couverture déclarée du moteur invoqué (017 § 9) :

| Sous-classe | Comportement conforme | Démonstration (dépôt) |
|---|---|---|
| les conventions excédentaires appartiennent **toutes à des familles couvertes** | un **W complet** appliquant tout le contenu de ℛ — différent de celui que la v1 émettait | fixtures `RegistresValides/AvecEq02` et `AvecCe02`, dérivées de bout en bout par le porteur (`ConformiteV2Tests`, `PorteurTests`, suites C3/C5) |
| **au moins une** appartient à une famille hors couverture | l'erreur « **registre non couvert** », aucun W | fixture `RegistresCasses/RegistreNonCouvert`, par le porteur (`ConformiteV2Tests`, `PorteurTests`) |

Dans les deux cas, plus jamais de W silencieusement partiel — les deux issues sont la correction, sous ses deux formes, de la même non-conformité (017 § 9 ; I61).

## 4. La déclaration rétroactive

Conformément au mécanisme du 011 § 9, la v1 (tag `identity-v1.0`) est déclarée **non conforme rétroactivement sur cette classe d'index — et sur elle seule** : « sa conformité déclarée en son temps sur (Ω_corpus1, ℛ₀), et la validité de ses sorties passées sous leur index (I23, I57) sur tout index dont le registre se limite aux conventions qu'elle applique, demeurent entières » (017 § 9). La v1 n'est ni réécrite ni réinterprétée (I57) — elle est remplacée.
