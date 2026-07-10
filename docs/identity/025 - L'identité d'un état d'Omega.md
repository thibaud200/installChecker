# 025 — L'identité d'un état d'Ω

**Statut** : neuvième acte documentaire de la phase v3 de la série `docs/identity/`. S'appuie sur les documents 000→024, figés. Exécute le **report 5** du 016 § 4.2 — « réviser la définition de l'identité d'un état d'Ω avant toute réimplémentation » — par la voie 2 du 016 § 3, **le document précédant le code** : un volet logiciel réalise ensuite ce que le présent acte fixe.
**Nature** : un document de **contrat** — il révise la définition de l'identité d'un état d'Ω, en résout les quatre défauts inventoriés (016 § 4.2 ; 019 § 2, T2), et publie l'écart qui en résulte au mécanisme du 011 § 9 (I59, première exception). Les actes de W — domaines, contenus, niveaux, motifs, espèces, licences, dépendances, dettes — sont rigoureusement intacts : seule la **valeur** du champ d'identification de l'index évolue, parce que sa définition évolue.
**Raffinement assumé** : ce document porte quatre touchers déclarés — un **raffinement du 014 § 6** (le contrat de Ω) : le support **déclare sa fonction d'empreinte**, comme il déclare sa version de contrat (§ 3) ; un **raffinement du 014 § 7.2** : l'empreinte d'état se calcule sur l'encodage non ambigu de la suite triée des **couples (identifiant, empreinte de contenu)**, par la fonction déclarée du support (§ 3) ; un **complément du 014 §§ 1 et 3** : la clause « produit » de C1 gagne l'identité de l'état d'Ω, et la table des frontières gagne la ligne **C1 → C6** qui la porte — la symétrie exacte de C2 → C6 (§ 4) ; un **raffinement du 018 § 5** : l'exception qui y articulait le convoyage de l'identité d'Ω se **résorbe** — la fourniture devient la douzième traversée, ligne de la table (§ 4). S'y ajoute l'**exercice de l'écart publié** (011 § 9, I59 — un mécanisme figé exercé, jamais un contenu nouveau, § 5). Hors ces points, aucun contenu normatif nouveau.
**Périmètre — exclusions constitutives** : rien n'anticipe la forme canonique matérielle (report 3 — l'acte ne dit rien de la sérialisation de W ni de l'index), la cohérence d'état de C6 (report 4), ni la cause et les continuités de τ (report 9) ; les actes des reports 6, 8, 11 et 12 (021, 024, 020, 022) sont consommés tels quels ; aucun document figé n'est modifié ; l'artefact d'oracle et le registre sont intacts.

---

## 1. Le défaut — quatre problèmes, une seule racine

La définition du 014 § 7.2 — l'empreinte d'état comme « fonction d'empreinte du support » appliquée à « la concaténation, dans l'ordre canonique des identifiants d'actes, des **empreintes de contenu** » — manque son propre but, l'identification (011 § 2.1 : « un état **identifié** de Ω… son identification fait partie de l'index de toute sortie ») :

- **P1 — les identifiants sont absents de l'identité.** Soit Ω₁ = {acte 1 → contenu A, acte 2 → contenu B} et Ω₂ = {acte 5 → A, acte 9 → B} : deux supports conformes (014 § 6 n'exige des identifiants que l'unicité, la stabilité et l'ordre — jamais des valeurs), même version, même nombre, même concaténation H(A)‖H(B) — **même index**. Or leurs W diffèrent matériellement : les domaines énumèrent les identifiants. Deux états distincts, une identification — la classe entière des renumérotations s'effondre sur un même index, et avec elle la lettre d'EXG-18 lue à l'index, EXG-24 (un cache clé par l'index restituerait le W d'un autre Ω), I23/EXG-25 (« rejouer un index » ne désigne pas un état), et la correspondance de τ ;
- **P2 — la concaténation est ambiguë** dès que l'empreinte n'est pas de longueur fixe — ce que le contrat de Ω ne garantit nulle part : H(x)‖H(y) se redécoupe ;
- **P3 — la fonction n'est pas contractualisée** : le § 7.2 dit « la fonction du support », mais ni le contrat de Ω ni le port ne la déclarent — l'implémentation la **présume** (elle nomme SHA-256 dans le moteur), au rebours de la lettre même qu'elle prétend appliquer ;
- **P4 — l'identité n'a ni producteur ni frontière** (la tension T2, 019 § 2) : la clause « reçoit » de C6 la déclare, aucune ligne de la table du 014 § 3 ne la porte, aucune des sept responsabilités ne la produit — elle vit dans un utilitaire hors machine abstraite, invoqué par le porteur, au bord de la définition de celui-ci (018 § 2 : il ne calcule rien).

Aucune défaillance n'est **observable aujourd'hui** : le système ne contient qu'un Ω (l'artefact gelé), aucun cache n'existe, τ n'a jamais été exercé — le défaut est contractuel et latent, comme l'était celui du report 8.

## 2. L'audit préalable — les trois vérifications exigées

- **Aucune valeur d'empreinte d'état n'est figée** dans le dépôt : ni dans la série (000→024), ni dans les consignations (`docs/conformite/`), ni dans le registre, ni dans l'artefact, ni dans les tests — qui n'assertent que le **motif** (64 hexadécimaux) et, pour un seul d'entre eux, la **construction** du § 7.2 (recalculée dans le test — il migre avec la définition, il ne fige rien). Les valeurs 64-hex de `docs/mesures/` sont des empreintes de **contenu** — des données d'Ω, hors du sujet. Rien ne contraint donc à réviser l'oracle : la branche 2 d'I59 n'est **pas forcée par les artefacts** (§ 5 démontre qu'elle n'est pas requise du tout) ;
- **L'encodage non ambigu — comparaison et choix.** Trois stratégies : le *séparateur impossible* (exige un alphabet garanti des empreintes — le contrat n'en garantit aucun : rejeté) ; le *TLV binaire* (non ambigu, mais introduit un vocabulaire d'octets étranger aux données textuelles du contrat : rejeté comme plus lourd sans gain) ; le **préfixe de longueur décimale** (auto-délimitant pour tout alphabet, purement textuel, dérivable des seules données, vérifiable à l'œil) — **retenu**. Forme : pour chaque acte, en ordre canonique des identifiants, chaque champ s'encode « longueur décimale, deux-points, valeur, virgule » — l'identifiant (en décimal, culture invariante) puis l'empreinte de contenu : `n:v,` où n est la longueur en caractères de v ;
- **Le siège du calcul.** L'identité est produite **à la frontière de Ω** — par le support, à travers son adaptateur : c'est la seule lecture qui réalise la lettre du § 7.2 (« la fonction d'empreinte du support, celle-là même qui produit les empreintes de contenu » — une fonction ne peut être « celle du support » que si le support l'applique, pas si le moteur la présume) ; et c'est la meilleure séparation des responsabilités : l'utilitaire hors-machine disparaît, le porteur redevient pur convoyeur (I66 **renforcé** — il ne calcule plus rien), et l'identité de chaque entrée est produite du même côté de sa propre frontière — C1 pour Ω comme C2 pour ℛ, la symétrie que la table attendait.

## 3. La définition révisée

> **Raffinement du 014 § 6 (déclaré en en-tête).** Le contrat logique de Ω gagne une clause : le support **déclare sa fonction d'empreinte** — celle qui produit les empreintes de contenu de ses actes — comme il déclare sa version de contrat. Pour le support du pipeline figé (`user_version = 1`), la fonction déclarée est SHA-256 : un fait du support, désormais contractuel, jamais une présomption du moteur.
>
> **Raffinement du 014 § 7.2 (déclaré en en-tête).** L'identité d'un état d'Ω est le triplet (version de contrat, nombre d'actes, **empreinte d'état**), où l'empreinte d'état est la fonction d'empreinte **déclarée** du support, appliquée à l'**encodage à préfixe de longueur** (§ 2) de la suite, en ordre canonique des identifiants, des **couples (identifiant, empreinte de contenu)**. Rien d'autre n'entre dans l'identité — et rien de moins : les identifiants font partie de l'état (les domaines de W les énumèrent), ils font désormais partie de son identification. La lettre-principe du § 7.2 est conservée : « aucune fonction nouvelle n'est introduite — l'identité d'Ω est dérivée de ce que Ω contient déjà », et sa fonction est réellement celle du support.

Deux Ω de mêmes contenus et d'identifiants différents ont désormais des identités **distinctes** (les couples diffèrent) ; deux lectures du même support ont la même (déterminisme des données) ; et aucun redécoupage n'est possible (l'encodage est auto-délimitant). P1, P2, P3 sont résolus.

## 4. La production et le convoyage — T2 résorbée

> **Complément du 014 §§ 1 et 3 (déclaré en en-tête).** La clause « produit » de **C1** gagne : *l'identité de l'état d'Ω* (§ 3) — produite par le support à travers son adaptateur, sur le même régime que ses autres productions (déterminisme, fidélité). La table des frontières gagne la ligne :
>
> | Frontière | Objets qui traversent | Justification |
> |---|---|---|
> | **C1 → C6** | l'identité de l'état d'Ω | l'index de W (014 C6, clause « reçoit ») — la symétrie exacte de la ligne C2 → C6 (l'identité de l'état de ℛ) |
>
> **Raffinement du 018 § 5 (déclaré en en-tête).** L'exception qui y articulait ce convoyage (« l'identité de l'état d'Ω, que la clause "reçoit" de C6 déclare comme entrée sans qu'aucune ligne de la table ne la porte ») se **résorbe** : la ligne existe désormais, et la fourniture est la **douzième traversée** inter-couches du porteur — la démonstration du 018 § 5 se lit sur la table complétée, redevenue sans exception. La tension T2 du 019 § 2 est close ; le régime du 014 § 7.2 que le 018 § 5 transportait « sans le définir » est défini.

Le porteur ne calcule plus rien : il reçoit l'identité de C1 (le port l'expose) et la convoie à C6 — sa définition (018 § 2 : « il ne dérive rien ») est réalisée sans reste, et l'utilitaire hors machine abstraite est supprimé.

## 5. L'écart publié — I59, première branche ; pourquoi pas la seconde

**Une exception d'I59 est requise** : le champ « empreinte d'état » de l'index de toute émission change de valeur — les sorties futures ne sont pas bit-identiques aux sorties passées sur les mêmes supports.

**La branche 2 (révision documentaire de l'oracle) ne s'applique pas — démonstration.** L'oracle officiel (EXG-39) est le quadruplet : documents 000→009, ℛ (ℛ₀), le corpus 1 archivé, W₀. Aucun de ses membres ne change : l'artefact est intact au bit près ; ℛ₀ aussi ; les documents 000→009 ne définissent pas l'empreinte d'état (elle naît au 014) ; et W₀ est caractérisé « à l'acte près » par le 014 § 8, dont la clause d'index fixe la **procédure par référence** — « empreinte d'état **calculée comme au § 7.2** » — jamais une valeur : le § 7.2 étant raffiné par le présent acte, la lettre du § 8 le suit sans être révisée. **Le report 6 demeure donc le seul cas de révision documentaire d'oracle de la série** — la ligne du 021 (« aucun autre report n'est une révision d'oracle ») est confirmée, pas contredite.

**La branche 1 (correction publiée d'une non-conformité antérieure) est la voie exacte.** L'ancienne identification portait une non-conformité latente à son propre rôle contractuel : le 011 § 2.1 exige « un état identifié », et une identification que deux états distincts partagent n'identifie pas ; l'implémentation présumait en outre la fonction du support au lieu de la recevoir — un écart à la lettre du § 7.2 qu'elle prétendait appliquer. L'écart est publié (matérialisé dans `docs/conformite/`, comme celui du 017 § 9) :

- **la classe** : tout couple (support d'observations, ℛ) — toute émission passée ;
- **le comportement ancien** : un index dont l'empreinte d'état couvrait les seuls contenus, par une fonction présumée, sur une concaténation ambiguë en droit ;
- **le comportement conforme au présent acte** : le même W, à l'acte près, sous un index dont l'empreinte suit la définition du § 3 ;
- **la justification, uniforme et acte par acte** : chaque acte de chaque W cite son index (011 § 3) ; le champ d'identification de cet index change de définition ; **aucun acte — domaine, contenu, niveau, motif, espèce, licence, dépendance, dette — ne change** ;
- **la déclaration rétroactive** : la v1 (tag `identity-v1.0`) et la v2 sont déclarées non conformes rétroactivement **sur l'identification d'état seulement**, et sur elle seule — leurs actes, leurs cartes de refus, leurs chaînes demeurent entiers.

## 6. Les sorties historiques — validité, rejouabilité, statut

- **Validité** : tout W émis reste valide **sous son index tel que défini à sa date** (I23 — l'index est daté par sa définition, comme toute convention l'est par sa version) ; rien n'est réécrit (I57 : la v1 reproduit ses sorties historiques, le tag est intact) ;
- **Rejouabilité** (EXG-25) : le moteur révisé, appliqué à tout support historique, reproduit **les mêmes actes** sous l'index révisé — la correspondance ancien ↔ nouveau index est mécanique (même support, deux définitions datées) ; la reconstruction du passé reste une invocation ordinaire ;
- **Statut documentaire** : les consignations de conformité v1 et v2 avaient expressément **provisionné** cet acte (« l'identité d'un état d'Ω suit le régime actuel du 014 § 7.2, dont la révision relève du report 5 ») — elles demeurent exactes comme consignations datées ; leur mise à jour éventuelle relève du bilan de campagne, jamais du présent acte.

---

## 7. Compatibilité avec les documents figés — démonstration

| Clause figée | Tension apparente | Résolution |
|---|---|---|
| 011 § 2.1 (« un état identifié de Ω ») | l'identification n'identifiait pas | le rôle est enfin rempli : les identifiants entrent dans l'identité (§ 3) — la clause est réalisée, pas modifiée. ∎ |
| 014 § 6 (le contrat de Ω) | la fonction d'empreinte absente du contrat | raffinement assumé, déclaré en en-tête : le support la déclare, comme sa version. ∎ |
| 014 § 7.2 (la définition) | les quatre défauts (§ 1) | raffinement assumé, déclaré en en-tête : couples (identifiant, empreinte), encodage à préfixe de longueur, fonction déclarée — la lettre-principe (« aucune fonction nouvelle ») conservée et enfin vraie. ∎ |
| 014 § 8 (W₀ « à l'acte près ») | l'empreinte de l'index change de valeur | la clause fixe la procédure **par référence** au § 7.2 : elle suit le raffinement sans être révisée — les 116 actes sont intacts. ∎ |
| 014 §§ 1 et 3 (C1 ; la table) | l'identité sans producteur ni ligne | complément assumé, déclaré en en-tête : C1 la produit, la ligne C1 → C6 la porte — la symétrie de C2 → C6 ; le patron est celui du 020. ∎ |
| 012 § 3 / I44 (le graphe) | un arc nouveau | dans le sens du flux, de C1 vers l'aval, comme tous — acyclicité intacte. ∎ |
| 018 §§ 2, 3 et 5 (le porteur) | il calculait l'identité via un utilitaire | il ne calcule plus rien — I66 renforcé ; l'exception du § 5 se résorbe en douzième traversée (raffinement déclaré) ; la clause « l'index est convoyé, jamais défini » a atteint son terme prévu : le report 5 le définit. ∎ |
| 019 § 2 (T2) | inventoriée, non résolue | résorbée par le présent acte — exactement l'attribution que le 019 lui donnait. ∎ |
| 020 (la ligne C1 → C5) | voisine de la ligne nouvelle | disjointe et de même patron : l'énumération vers C5 est une entrée de dérivation, l'identité vers C6 une identification — deux lignes, deux objets. ∎ |
| 021 (« le seul cas de révision d'oracle ») | le présent acte change l'index de W₀ | préservé, démontré au § 5 : la branche 2 n'est pas exercée — la caractérisation suit sa référence, le quadruplet EXG-39 est intact. ∎ |
| I23, I57, EXG-25 (le passé) | les index historiques changent de définition | ils restent valides sous leur définition d'époque, datée ; la v1 les reproduit ; la rejouabilité produit les mêmes actes sous l'index révisé (§ 6). ∎ |
| EXG-18, EXG-24 (l'identité bit à bit ; les caches) | reposaient sur une identification défaillante | reposent désormais sur une identification fidèle — renforcement, aucun changement de régime. ∎ |
| 016 § 4.2 (report 5) | — | le présent acte est son exécution intégrale : le report 5 est clos. ∎ |
| 016 § 4.2 (reports 3, 4, 9) | — | non anticipés : ni sérialisation, ni cohérence d'état de C6, ni cause ou continuités de τ. ∎ |

---

## Conclusion

L'identité d'un état d'Ω identifie enfin : les identifiants y entrent, l'encodage est sans ambiguïté, la fonction est celle que le support déclare, et l'identité est produite du bon côté de sa frontière — C1 la produit, la ligne C1 → C6 la porte, le porteur la convoie sans la calculer. Les quatre défauts du report 5 et la tension T2 sont résolus d'un seul geste ; l'écart est publié par la première branche d'I59, la seconde restant ce que le 021 avait établi — exercée une seule fois dans la série ; et pas un acte de W, pas un domaine, pas un motif n'a changé. Le report 5 du 016 § 4.2 est **clos**, sous réserve du volet logiciel qui suit le présent acte. Sa validation relève de l'autorité du projet.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| le défaut | quatre problèmes, une racine : l'identité couvrait les contenus sans les identifiants (collision constructible), sur une concaténation ambiguë, par une fonction présumée, sans producteur ni frontière (T2) | 1 |
| l'audit préalable | aucune valeur d'empreinte figée nulle part ; encodage : préfixe de longueur retenu (contre séparateur et TLV) ; siège : le support via son adaptateur — la lettre du § 7.2 enfin réalisée | 2 |
| la définition révisée | (version, nombre, empreinte) où empreinte = fonction **déclarée** du support sur l'encodage à préfixe de longueur des couples (identifiant, empreinte de contenu) triés | 3 |
| production et convoyage | C1 produit, la ligne C1 → C6 porte (symétrie C2 → C6) ; l'exception du 018 § 5 résorbée (douzième traversée) ; T2 close ; le porteur pur convoyeur | 4 |
| l'écart publié | I59 branche 1 : classe = toutes les émissions passées ; les actes intacts, le champ d'identification redéfini ; v1 et v2 non conformes rétroactivement sur l'identification d'état seule. Branche 2 non exercée : la caractérisation du 014 § 8 suit sa référence — le report 6 demeure le seul cas | 5 |
| les sorties historiques | valides sous leur index daté (I23), reproduites par la v1 (I57), rejouables sous l'index révisé (EXG-25) | 6 |
| compatibilité documentaire | quatorze clauses figées, chacune traitée nommément — aucune par silence | 7 |

**Ce que ce document ne fait volontairement pas** : modifier un document figé, changer un acte de W ou de W₀, réviser l'oracle (démontré au § 5), spécifier la sérialisation matérielle de W ou de l'index (report 3), toucher à la cohérence d'état de C6 (report 4) ou à la cause et aux continuités de τ (report 9), créer un invariant.
