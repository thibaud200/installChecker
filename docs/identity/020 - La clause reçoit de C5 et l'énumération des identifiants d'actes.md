# 020 — La clause « reçoit » de C5 et l'énumération des identifiants d'actes

**Statut** : quatrième acte documentaire de la phase v3 de la série `docs/identity/`. S'appuie sur les documents 000→019, figés. Exécute le **report 11** du 016 § 4.2 — « réviser la clause "reçoit" de C5 pour couvrir l'entrée réellement nécessaire » —, par la voie 2 du 016 § 3. Acte documentaire **seul** : l'implémentation existante est déjà conforme au contrat révisé — le contrat rattrape la réalité, aucun volet logiciel n'existe ni ne suivra.
**Nature** : un document de **contrat** — le plus petit acte normatif de la série : une clause complétée, une ligne de table ajoutée, une démonstration étendue. Aucun objet nouveau, aucun invariant nouveau, aucun code, aucun comportement.
**Raffinement assumé** : ce document porte trois touchers déclarés, et rien d'autre — un **raffinement du 014 § 1 (C5, clause « reçoit »)** : l'énumération des identifiants d'actes d'Ω s'y ajoute (§ 2) ; un **complément du 014 § 3** : la table des frontières gagne la ligne C1 → C5 qui porte cette énumération (§ 3) ; un **raffinement du 018 § 5** : l'énumération des traversées du porteur passe de dix à onze (§ 4). Hors ces trois points, aucun contenu normatif nouveau.
**Périmètre — exclusions constitutives** : rien d'autre du 014 n'est touché — en particulier les tensions T1 et T2 inventoriées au 019 § 2 restent intégralement aux actes des reports 9 et 5 ; rien n'anticipe les reports 3 à 9 et 12 ; aucun document figé n'est modifié ; aucun test, aucun code, aucune sortie.

---

## 1. Le défaut contractuel

La clause « reçoit » de C5 (014 § 1) déclare : « les productions de C4 + les conventions d'élection et de priorité (de C2) ». Or la clause « produit » du même contrat exige la complétude — « pour chaque domaine-strate à espace non trivial, exactement un acte » — et le 014 § 8 fixe la lettre des refus des strates supérieures : « chacun de domaine maximal : **les 497 actes, énumérés** ». Cette énumération n'est dérivable d'aucune des deux entrées déclarées :

- **les productions de C4 ne couvrent que les domaines des hypothèses** — jamais la totalité des actes d'Ω ; sur un Ω sans classe de contenu multi-actes, C4 ne produit rien du tout, et C5 doit pourtant refuser les strates supérieures sur le domaine maximal ;
- **les conventions n'énumèrent rien** — App(κ) est un domaine, jamais une énumération d'actes (015 § 1).

L'entrée manquante **existe pourtant depuis l'origine dans l'implémentation** : la décision des actes reçoit, en troisième position, l'énumération des identifiants d'actes d'Ω — convoyée par la composition de test en v1, puis par le porteur en v2 (une projection du modèle d'observations de C1). Le 016 § 4.2 (report 11) a inventorié ce décalage : « le contrat de C5 ne prévoit pas l'une de ses entrées réelles ». Le présent acte le résout dans le seul sens licite — **le contrat rattrape l'implémentation**, qui n'a jamais rien fait d'autre que ce que le 014 § 8 exigeait d'elle.

---

## 2. Le raffinement de la clause « reçoit » (014 § 1, C5)

> **Raffinement assumé du 014 § 1 (C5).** La clause « reçoit » de C5 se lit désormais : les productions de C4 + les conventions d'élection et de priorité (de C2) + **l'énumération des identifiants d'actes d'Ω** — la projection des seuls identifiants du modèle d'observations (de C1) : jamais les actes, jamais leurs attributs, jamais leurs valeurs.

Trois bornes, constitutives de l'entrée :

- **des identifiants seuls** — l'identifiant est la clé canonique du contrat de Ω (014 § 6 : « identifiant unique, stable et totalement ordonné » dont « l'ordre ne porte aucune sémantique ») : aucune valeur observée, aucun attribut, aucun contexte ne l'accompagne — A1, I8 et P6 restent saufs par construction ;
- **un seul usage** — C5 n'en consomme que le **domaine maximal** des refus des strates supérieures (014 § 8) ; aucune dérivation, aucune comparaison, aucun signal n'en provient (les signaux naissent en C3, les hypothèses en C4 — rien ne change) ;
- **une entrée re-dérivable** — l'énumération est une projection sans contenu propre du modèle d'observations, reconstructible à l'identique depuis Ω (I5, I10) : elle n'introduit aucune information que C1 n'ait déjà produite.

---

## 3. Le complément de la table des frontières (014 § 3)

> **Complément assumé du 014 § 3.** La table des frontières gagne une ligne :
>
> | Frontière | Objets qui traversent | Justification |
> |---|---|---|
> | **C1 → C5** | l'énumération des identifiants d'actes — jamais les actes, leurs attributs ni leurs valeurs | les refus de domaine maximal exigent la totalité des identifiants (014 § 8) ; aucune des entrées déclarées de C5 ne peut la fournir (§ 1) |

**La règle de traversée est intacte à la lettre.** Le 014 § 3 dispose : « un acte d'observation ne parvient jamais à C5 » — cela demeure exactement vrai : un identifiant n'est pas un acte d'observation (l'acte est « son identifiant **+ la famille complète de ses observations** + ses attributs de contenu », 014 § 2) ; ce qui traverse est la clé, jamais l'objet. De même, l'interdiction du 012 § 3 — « C5 ne relit jamais les observations, ni les signaux bruts » — demeure : une énumération d'identifiants ne contient aucune observation à relire. Le graphe des dépendances du 012 § 3 conserve toutes ses interdictions et son acyclicité (I44) : la ligne nouvelle va dans le sens du flux, de C1 vers l'aval, comme toutes les autres.

---

## 4. L'articulation avec le 018 § 5

> **Raffinement assumé du 018 § 5.** L'énumération des traversées inter-couches que le porteur opère passe de dix à **onze** : la onzième est l'énumération des identifiants d'actes, de C1 vers C5 — désormais une ligne de la table du 014 § 3 (§ 3 ci-dessus). La démonstration du 018 § 5 (« toute traversée inter-couches que le porteur opère est une ligne de la table ») reste vraie mot pour mot — elle se lit sur la table complétée ; son énumération était incomplète, jamais fausse : la traversée existait, la ligne manquait — c'était le défaut du contrat (§ 1), pas celui de la démonstration.

Le reste du 018 est intact, et le présent raffinement le **renforce** :

- **I66 (« le porteur ne produit aucun objet propre »)** : la projection des identifiants est re-dérivable et sans contenu nouveau (§ 2, troisième borne) — le porteur n'y fabrique rien, il opère une traversée désormais nommée par la table ;
- **la clause « ignore » du 018 § 3** (« le contenu de tout objet qu'il achemine — il compose, il ne lit pas ») : projeter des identifiants n'est pas lire un contenu — les identifiants sont les clés de l'acheminement, jamais des valeurs observées ; la clause vise le contenu, que la projection ne touche pas ;
- **I67** : l'ordre des vérifications et le signalement sont hors de cause — l'énumération n'est disponible qu'après le bloc Ω validé, comme toutes les productions de C1.

---

## 5. Vérifications — pourquoi cet acte est un raffinement documentaire pur

- **Aucune logique nouvelle** : le paramètre existe depuis le jalon É6 de la v1 (la composition de test le passait) et le porteur de la v2 le convoie depuis le 018 — pas une ligne de code ne change, ni n'aurait de raison de changer ;
- **Aucun invariant modifié** : I49–I52 revérifiés — I50 (l'objet traversant est complet et défini : une énumération d'identifiants, § 2) ; I51 (validité par construction : l'énumération provient du modèle validé par C1) ; I52 (aucune sortie nouvelle) ; I44 (acyclicité, § 3) ; I61–I67 intacts (§ 4). Aucun invariant nouveau n'est créé ;
- **Aucun comportement, aucune sortie** : W est identique bit pour bit à contenu logique constant — le moteur faisait déjà, à la lettre, ce que le contrat dit désormais ; c'est la définition même du rattrapage ;
- **Aucun test à modifier** : les suites exercent déjà cette entrée (les refus de domaine maximal de W₀, les 497 actes énumérés, sont vérifiés depuis la v1) — le contrat révisé décrit ce qu'elles prouvent déjà.

---

## 6. Compatibilité avec les documents figés — démonstration

| Clause figée | Tension apparente | Résolution |
|---|---|---|
| 014 § 1 (C5, « reçoit ») | une entrée réelle non déclarée | raffinement assumé, déclaré en en-tête et motivé au § 1 — le contrat rattrape l'implémentation, aucun retrait. ∎ |
| 014 § 1 (C5, « produit » ; forme commune : contrats totaux) | la complétude exigeait une entrée indérivable | désormais dérivable de la clause révisée — la totalité du contrat devient effective, elle n'était qu'implicite. ∎ |
| 014 § 3 (« aucun canal hors table ») | la traversée existait sans ligne | complément assumé (§ 3) : la ligne C1 → C5 la nomme ; « un acte d'observation ne parvient jamais à C5 » demeure vrai à la lettre — un identifiant n'est pas un acte (014 § 2). ∎ |
| 014 § 8 | « les 497 actes, énumérés » sans source contractuelle | la source est déclarée (§ 2) — la lettre du § 8 devient dérivable du contrat de C5. ∎ |
| 012 § 3 (interdictions ; graphe) | C5 pourrait sembler « relire » Ω | rien n'est relu : aucune observation, aucun signal — des clés seules ; le graphe garde ses interdictions et son acyclicité (I44). ∎ |
| 018 § 3 (« ignore ») et I66 | le porteur projette les identifiants | la projection est sans contenu et re-dérivable — le porteur ne lit aucun contenu, ne fabrique aucun objet (§ 4). ∎ |
| 018 § 5 | l'énumération des traversées disait dix | raffinement assumé, déclaré en en-tête (§ 4) : onze, sur la table complétée — la démonstration reste vraie mot pour mot. ∎ |
| 019 § 2 (T1, T2) | tensions voisines du 014 | intactes — leurs actes sont ceux des reports 9 et 5 ; le présent acte ne touche ni la cause de τ ni l'identité d'Ω. ∎ |
| 016 § 4.2 (report 11) | — | le présent acte est son exécution intégrale : le report 11 est clos. ∎ |
| I40 (011 § 12) | le contrat pourrait sembler suivre un choix d'implémentation | c'est l'inverse qui est vrai : l'entrée est exigée par la lettre du 014 § 8, l'implémentation l'avait réalisée, le contrat la déclare — la norme précède, le code avait obéi. ∎ |

---

## Conclusion

Le contrat de C5 est complet : ses trois entrées sont déclarées, la traversée qui portait la troisième est nommée par la table des frontières, et la démonstration du porteur se lit désormais sur onze lignes au lieu de dix — sans qu'un seul comportement, une seule sortie ou une seule ligne de code n'ait changé. Le report 11 du 016 § 4.2 est **clos**. Sa validation relève de l'autorité du projet, comme celle de chaque document de la série.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| défaut contractuel | l'énumération des identifiants, exigée par 014 § 8, indérivable des entrées déclarées de C5 — convoyée depuis l'origine par l'implémentation (016 § 4.2, report 11) | 1 |
| clause « reçoit » révisée | + l'énumération des identifiants d'actes d'Ω (projection de C1) — identifiants seuls, un seul usage (domaine maximal), re-dérivable | 2 |
| table des frontières | ligne nouvelle C1 → C5 ; « un acte d'observation ne parvient jamais à C5 » intact à la lettre | 3 |
| articulation 018 § 5 | onze traversées ; démonstration vraie mot pour mot sur la table complétée ; I66 et « ignore » renforcés | 4 |
| raffinement pur | aucune logique, aucun invariant, aucun comportement, aucune sortie, aucun test — le contrat rattrape l'implémentation | 5 |
| compatibilité documentaire | dix clauses figées, chacune traitée nommément — aucune par silence | 6 |

**Ce que ce document ne fait volontairement pas** : toucher aux tensions T1 et T2 du 019 § 2 (reports 9 et 5), réconcilier la carte des refus (report 6), définir l'identité d'un état d'Ω (report 5), spécifier la cause de τ (report 9), trancher l'espace trivial (report 12), modifier un document figé, créer un invariant, écrire ou modifier du code ou des tests.
