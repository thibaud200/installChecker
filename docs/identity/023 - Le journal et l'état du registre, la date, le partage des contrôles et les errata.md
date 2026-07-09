# 023 — Le journal et l'état du registre : la date, le partage des contrôles et les errata

**Statut** : septième acte documentaire de la phase v3 de la série `docs/identity/`. S'appuie sur les documents 000→022, figés. Exécute le **report 7** du 016 § 4.2 — « fixer la représentation normative de la date des entrées, le partage exact des contrôles entre C2 et la relecture humaine, et corriger l'exemple fautif » — par la voie 2 du 016 § 3, **le document précédant le code** : un volet logiciel strictement borné au lecteur de registre (C2) réalise ensuite ce que le présent acte fixe.
**Nature** : un acte de **régime du registre** — il ne touche ni la grammaire des fichiers (I56), ni le prédicat de cohérence, ni le contenu de ℛ₀ : il fixe la lecture de la date, partage les vingt-quatre points de la check-list entre C2 et la relecture humaine, et consigne deux errata de lettre du 015.
**Raffinement assumé** : ce document porte trois touchers déclarés — un **raffinement du 015 § 6.2** : « cinq sous-sections » se lit « cinq éléments » (la date au titre, quatre sous-sections), et la clause « le titre n'est jamais lu par C2 » est bornée : C2 lit **la date** du titre — le seul des cinq éléments sans sous-section — et rien d'autre du titre (§ 2) ; un **complément du 014 § 5.2** : la clause « Rejet » de l'historique gagne le cas « ordre chronologique décroissant » (§ 2) ; un **erratum du 015 § 7.4** (quatrième puce) : la cohérence entre `version du registre` et le compte d'actes relève de la relecture humaine, hors des causes de rejet de C2 (§ 3). S'y ajoute un **erratum de lettre du 015 § 7.3** (l'exemple fautif, § 4) — un constat, pas un toucher normatif. Hors ces points, aucun contenu normatif nouveau.
**Périmètre — exclusions constitutives** : rien ne touche l'identité d'un état d'Ω (report 5), la référence d'acte (report 8), la cause de τ (report 9), la forme canonique (report 3) ni la cohérence d'état de C6 (report 4) ; la grammaire du registre est inchangée (aucune section nouvelle — I56) ; ℛ₀ matérialisé reste valide au caractère près ; aucun document figé n'est modifié.

---

## 1. L'audit — les quatre constats, à la lettre

- **« Cinq sous-sections »** (015 § 6.2) : la clause annonce « un titre de niveau 2 suivi de **cinq sous-sections** de niveau 3 » — et sa propre grammaire en montre **quatre** (`type`, `convention`, `justification de l'acte`, `autorité`). Le § 6.3 compte juste, lui : « une entrée omet l'un des **cinq éléments** » — les cinq éléments du 014 § 5.2 (date, type, convention, justification, autorité), dont la date, qui vit au titre.
- **La date, invérifiable mécaniquement** : le 015 § 6.2 dispose que « le titre n'est jamais lu par C2 » — or la date ne vit **que** là ; le 014 § 5.2 (figé) exige pourtant le rejet d'une « entrée sans l'un de ces éléments », date comprise, et le 015 § 6.3 impose « l'ordre chronologique non décroissant » (check-list § 8, point 10). Tel quel : C2 ne peut ni constater l'absence de la date ni vérifier l'ordre — la lettre du 014 § 5.2 était partiellement inexigible.
- **Les points 17, 18, 19** : la check-list les présente comme constitutifs de la validité, et le 015 § 7.4 (quatrième puce) range même le point 17 (compte d'actes) parmi les « causes de rejet (014 § 5.3) » — alors que la lettre du 014 § 5.3 n'en dit rien (ses trois causes : version sans fichier, version jamais adoptée, version retirée encore citée — puis le prédicat).
- **L'exemple du 015 § 7.3** : sous la règle qu'il énonce lui-même (« triée par ordre alphabétique d'identifiant »), il donne « `EQ-01, version 1` **puis** `CE-01, version 1` » — et pour l'index documentaire, `EQ-01/v1.md` puis `CE-01/v1.md`. Or CE-01 précède EQ-01 alphabétiquement ; le § 7.5 du même document et le registre réel (vérifié au caractère près, 016 § 1.1) donnent l'ordre correct : CE-01 puis EQ-01, sur les deux listes.

## 2. La date : représentation normative et lecture par C2

**Analyse.** « Cinq sous-sections » est une **erreur de rédaction** — pas une information implicite : le § 6.3 et le 014 § 5.2 comptent cinq *éléments*, la grammaire quatre *sous-sections*, et la cinquième donnée (la date) a toujours eu sa place normative : la tête du titre, au format ISO (`## <date ISO> — <type> — <identifiant> v<version>`). Et la date **doit** être lisible par C2 : sans cela, deux clauses figées sont inexigibles (le rejet « entrée sans l'un de ces éléments » du 014 § 5.2, et l'ordre du 015 § 6.3 / point 10) — un contrat que son propre lecteur ne peut pas vérifier n'est pas total (014 § 1, forme commune).

> **Raffinement du 015 § 6.2 (déclaré en en-tête).** La représentation normative de la date d'une entrée est **la date ISO en tête de son titre de niveau 2**. C2 la lit — c'est la seule lecture du titre qui lui soit ouverte : pour `type` et `convention`, redits par le titre, **les sous-sections font foi** comme avant (la clause du 015 § 6.2 reste exacte pour eux, elle est simplement bornée à eux). Une entrée dont le titre ne porte pas de date ISO lisible est une entrée qui « omet l'un des cinq éléments » : **« registre malformé »** (014 § 5.2, déjà couvert en substance — la lecture le rend exigible).
>
> **Complément du 014 § 5.2 (déclaré en en-tête).** La clause « Rejet » de l'historique gagne un cas : **l'ordre chronologique décroissant** — la date d'une entrée antérieure à celle de l'entrée qui la précède — produit « registre malformé ». C'est la contrainte du 015 § 6.3, désormais exigible de C2 ; le point 10 de la check-list devient mécaniquement vérifiable. L'ordre des vérifications du 017 § 8 est inchangé : ces deux cas sont des cas de **forme**, au deuxième étage, comme tous les rejets du journal.

## 3. Le partage des contrôles — les vingt-quatre points partitionnés

**Analyse.** La check-list du 015 § 8 est la validation **documentaire** (É1 : « relisible et vérifiable par un humain sans outil ») ; les causes de rejet de C2 sont celles des 014 §§ 5.1–5.3 — un sous-ensemble. Les deux régimes n'avaient jamais été partagés point par point ; le 015 § 7.4 a même annexé à C2 une cause (le compte d'actes) que le 014 § 5.3 ne lui donne pas. Le partage, fixé :

| Régime | Points | Fondement |
|---|---|---|
| **C2 — mécanique** (→ erreurs nommées) | **1–9** (forme des fichiers de version : sections, unicité, non-vacuité, identifiant/version/chemin, famille, dépendances-fichiers, champs interdits, acyclicité), **10** (ordre chronologique — désormais, § 2), **11–12** (adoptions et retraits du journal), **13–16** (sections d'`etat.md`, fichiers, adoptions, retraits), **20** (dépendances en vigueur — le prédicat) | 014 §§ 5.1–5.3 ; § 2 ci-dessus |
| **Relecture humaine — documentaire** (→ validité d'É1, jamais un rejet de C2) | **17** (compte d'actes), **18** (date logique = acte le plus récent), **19** (index documentaire exact), **21** (incompatibilités — aucune section ne les porte à ce jour ; leur grammaire viendra avec la première convention qui en déclarera une), **22** (minimalité — la vérification du 009 § 8, sur états dérivés), **23** (justification empirique — de la prose, jamais lue par le moteur, I55), **24** (la relecture elle-même) | 014 § 5.3 (lettre close) ; 015 §§ 1 et 8 |

> **Erratum du 015 § 7.4 (quatrième puce, déclaré en en-tête).** « Une incohérence entre `version du registre` et le nombre d'actes de gouvernance » n'est pas une cause de rejet de C2 — le 014 § 5.3, qu'elle invoque, ne la contient pas : elle relève de la relecture humaine (point 17). La puce se lit désormais au régime documentaire ; les quatre autres puces du § 7.4 sont exactes et restent à C2.

Un registre peut donc être **accepté par C2 et documentairement invalide** (un point 17–24 en échec) : c'est voulu — la validité documentaire est celle de la gouvernance (l'humain, à l'adoption), le rejet de C2 celle de l'invocation (le moteur, à chaque lecture). Les deux verdicts ont chacun leur autorité, et le second n'a jamais prétendu épuiser le premier (015 § 8 : « chaque cause de rejet nommée par 014 §§ 5.1–5.3 **y figure** » — la check-list contient les causes de C2, jamais l'inverse).

## 4. L'exemple du 015 § 7.3 — l'erratum

**Analyse.** L'exemple viole réellement sa propre grammaire : « triée par ordre alphabétique d'identifiant » puis « `EQ-01, version 1` puis `CE-01, version 1` » — l'ordre alphabétique donne CE-01 d'abord (C < E, ordinal comme alphabétique). La faute est double (conventions en vigueur **et** index documentaire) et purement locale au § 7.3 : le § 7.5 — le contenu exact, celui qui fait foi et que le registre réel matérialise au caractère près — donne l'ordre correct sur les deux listes.

> **Erratum (constat).** Les deux énumérations en prose du 015 § 7.3 (« pour ℛ₀ : … ») sont fautives ; la lettre correcte est celle du § 7.5, seule instance normative complète. Le 015 étant figé, le présent constat tient lieu de correction — sur le modèle des résorptions du 019 : toute lecture du § 7.3 se résout en § 7.5. Aucune conséquence matérielle : le registre réel, C2 et les fixtures suivent le § 7.5 depuis l'origine.

## 5. Le volet logiciel — strictement borné à C2

Rendu nécessaire par le § 2 (deux exigences désormais mécaniques), et par lui seul :

- le lecteur de registre lit la date ISO en tête de chaque titre d'entrée — illisible → « registre malformé » ;
- il vérifie l'ordre chronologique non décroissant des entrées — violé → « registre malformé » ;
- **rien d'autre ne change** : ni les autres lectures du titre (les sous-sections font foi), ni les points 17–19 (relecture humaine), ni le prédicat, ni la couverture, ni l'ordre du 017 § 8 (les deux cas sont des cas de forme), ni ℛ₀ (dates `2026-07-05`, `2026-07-05` — non décroissant, valide tel quel).

---

## 6. Compatibilité avec les documents figés — démonstration

| Clause figée | Tension apparente | Résolution |
|---|---|---|
| 015 § 6.2 (« cinq sous-sections » ; « le titre n'est jamais lu par C2 ») | compte faux ; date inexigible | raffinement assumé (§ 2) : cinq éléments dont la date au titre ; la lecture du titre par C2 est ouverte pour la date seule — « les sous-sections font foi » demeure pour type et convention. ∎ |
| 014 § 5.2 (« entrée sans l'un de ces éléments » ; la liste « Rejet ») | la date était invérifiable ; l'ordre absent de la liste | la lecture de la date rend la clause exigible (aucun cas nouveau) ; l'ordre décroissant est un complément assumé, déclaré en en-tête. ∎ |
| 015 § 6.3 et § 8 point 10 (l'ordre chronologique) | invérifiable mécaniquement | désormais vérifié par C2 — la contrainte existait, seul son vérificateur manquait. ∎ |
| 014 § 5.3 (les trois causes d'état + le prédicat) | le 015 § 7.4 lui attribuait une 4ᵉ cause | erratum assumé du 015 § 7.4 (§ 3) : le compte d'actes relève de la relecture — la lettre du 014 § 5.3 est restaurée comme référence close de C2. ∎ |
| 015 § 8 (le verdict des vingt-quatre points) | semblait faire de chaque point un rejet de C2 | partagé (§ 3) : la check-list est la validité documentaire, qui contient les causes de C2 sans s'y réduire — sa propre lettre le disait (« chaque cause de rejet… y figure »). ∎ |
| 015 § 7.3 (l'exemple) | contredit sa propre règle | erratum constaté (§ 4) : le § 7.5 fait foi ; aucune conséquence matérielle. ∎ |
| I53–I56 (le registre) | la grammaire pourrait sembler bouger | aucune section, aucun champ, aucun type de document nouveaux — la date était déjà dans la grammaire du titre (015 § 6.2) ; I56 intact. ∎ |
| 017 § 8 (l'ordre des vérifications de ℛ) | deux cas de rejet nouveaux | tous deux au deuxième étage (forme), comme tout rejet du journal — l'ordre absence < forme < cohérence < couverture est inchangé, I64 intact. ∎ |
| 016 §§ 1.1 et 2 (ℛ₀ vérifié au caractère près ; gels) | le registre réel devait rester valide | il l'est tel quel : dates non décroissantes, ordre du § 7.5 — aucune adaptation. ∎ |
| 016 § 4.2 (report 7) | — | le présent acte est son exécution intégrale : le report 7 est clos. ∎ |
| 016 § 4.2 (reports 3, 4, 5, 8, 9) | — | non anticipés : ni forme canonique, ni C6, ni identité d'Ω, ni référence d'acte, ni τ. ∎ |

---

## Conclusion

La date des entrées a sa représentation normative — la tête du titre, lue par C2 ; l'ordre chronologique est exigible et vérifié ; les vingt-quatre points de la check-list sont partagés entre le rejet mécanique et la relecture humaine, chacun sous son autorité propre ; et les deux fautes de lettre du 015 (le compte des sous-sections, l'exemple du § 7.3) sont constatées et résolues sans toucher au document figé. Le registre réel reste valide au caractère près. Le report 7 du 016 § 4.2 est **clos**, sous réserve du volet logiciel qui suit le présent acte. Sa validation relève de l'autorité du projet.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| les quatre constats | « cinq sous-sections » vs quatre ; la date au titre jamais lu ; les points 17–19 sur-attribués à C2 (015 § 7.4) ; l'exemple du § 7.3 fautif | 1 |
| la date | représentation normative : la date ISO en tête du titre ; lue par C2 (seule lecture du titre ouverte) ; illisible → malformé ; ordre décroissant → malformé (complément du 014 § 5.2) | 2 |
| le partage | C2 : points 1–16, 10 et 20 (mécanique, erreurs nommées) ; relecture humaine : 17–19 et 21–24 (validité documentaire) ; erratum du 015 § 7.4 (compte d'actes → relecture) | 3 |
| l'erratum du § 7.3 | l'exemple en prose est fautif sur ses deux listes ; le § 7.5 fait foi ; aucune conséquence matérielle | 4 |
| le volet logiciel | borné à C2 : lecture de la date du titre, vérification de l'ordre — rien d'autre | 5 |
| compatibilité documentaire | onze clauses figées, chacune traitée nommément — aucune par silence | 6 |

**Ce que ce document ne fait volontairement pas** : modifier un document figé, toucher à la grammaire du registre (aucune section nouvelle — I56), réviser ℛ₀ ou exiger son adaptation, étendre C2 au-delà des deux lectures du § 2 (les points 17–19 restent humains), définir l'identité d'Ω (report 5), la référence d'acte (report 8), la cause de τ (report 9) ou la forme canonique (report 3), créer un invariant.
