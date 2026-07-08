# 017 — La couverture du registre par le moteur

**Statut** : premier acte documentaire opérationnel de la phase v2 de la série `docs/identity/`. S'appuie sur les documents 000→016, tous figés. Exécute le **report 1** du 016 § 4.1, par la voie 2 du 016 § 3 — le document précède le code : aucune ligne du moteur v2 n'existe à la date du présent acte.
**Nature** : un document de **contrat** — il prolonge la couche 010→015. Il définit le régime d'applicabilité du registre par le moteur : la notion de couverture, sa déclaration, la précondition qu'elle fonde, l'erreur qu'elle nomme, l'ordre de leur vérification. Aucun code, aucun algorithme, aucune transition de ℛ, aucune famille concrète rendue applicable.
**Raffinement assumé** : ce document raffine le 011 sur trois points précis — le contrat d'erreur (§ 5 : la liste close s'ouvre d'un cas, motivé au § 6 ci-dessous), les préconditions d'invocation (§ 4 : une quatrième s'ajoute, motivée au § 4 ci-dessous), et la clause de compatibilité des registres (§ 9, troisième puce : « le moteur applique l'état fourni, quel qu'il soit, pourvu qu'il soit cohérent » — bornée au § 9 ci-dessous). Il **s'adosse** au raffinement du 011 § 10 déjà acté par le 016 § 5.1, sans le redéfinir.
**Périmètre — exclusions constitutives** : rien de ce document n'entre dans le registre (015 § 1 l'interdit ; I53–I56 le verrouillent) ; rien ne décrit l'architecture du composant du report 2 (seul le contrat du siège de vérification est fixé, § 5) ; rien ne touche les reports 3 (forme canonique) et 6 (carte des refus de W₀) ; rien ne contractualise la liste des capacités d'un moteur particulier (I40 : le contrat définit le mécanisme, jamais l'inventaire).

---

## 1. Le défaut contractuel

Soit un registre ℛ **bien formé** — aucune cause de rejet du 014 § 5.1 : les quatorze champs présents, chaque famille parmi les huit connues — et **cohérent** — le prédicat que cite la ligne « registre incohérent » du 011 § 5 est satisfait — mais dont une convention en vigueur appartient à une famille que le moteur invoqué **ne sait pas appliquer**. Cette situation n'a, sous les documents 010→016, aucune issue contractuelle :

- elle n'est **aucune des six erreurs** du 011 § 5, dont la liste est close par son propre libellé (« les **seules** erreurs contractuelles ») ;
- elle ne peut pas produire de **refus** : la précision normative du 011 § 5 l'interdit par principe — « un défaut de gouvernance ne doit jamais se présenter aux consommateurs comme une connaissance du monde » ; le décalage entre un registre et un moteur n'est pas une connaissance du monde ;
- elle ne peut pas produire de **W** : un état dérivé en ignorant une convention que son index cite viole I34 (« aucun état du monde ne mobilise [autre chose] que le contenu de ℛ à la version indexée » — un W qui n'applique pas tout ce contenu prétend faussement en dépendre) et EXG-13 (le moteur « applique T(κ) à la lettre » — ne pas l'appliquer du tout n'est pas une application à la lettre) ; sa justification est incomplète par construction (I38).

Le comportement du moteur v1 dans cette situation — l'ignorance silencieuse, constatée à la clôture (016 § 4.1, report 1) — est donc une **non-conformité**, jamais une liberté. Et le défaut est double : non seulement le contrat d'erreur ne couvre pas le cas, mais une clause du 011 l'aggrave — la troisième puce du § 9 (« le moteur applique l'état fourni, quel qu'il soit, pourvu qu'il soit cohérent ») **présuppose** qu'un moteur conforme applique toute famille théorisée, présupposé que le 016 § 5.1 a déjà entamé en révélant le troisième cas du § 10 : la famille théorisée mais que le moteur ne sait pas encore appliquer. Le présent document achève ce que ce raffinement a commencé.

Enfin, le défaut a un précédent structurel exact dans le contrat lui-même : « **Ω incompatible** » (011 § 5 : « version du contrat d'observations **non supportée par le moteur** ») est la seule erreur relative au moteur — et elle n'a aucune symétrique côté ℛ. Ce document la lui donne.

---

## 2. La couverture, objet formel

**Définition 1 (couverture)** — La *couverture* d'un moteur est l'ensemble des **familles de conventions** (celles du 014 § 5.1) dont il sait appliquer toute convention conforme, au sens d'EXG-13 : à la lettre, dans la version indexée, sans latitude d'interprétation.

Trois propriétés définitoires :

- **la granularité est la famille** — jamais la convention individuelle. C'est la granularité que le 016 § 5.1 a fixée en raffinant le 011 § 10 : « une fois la famille applicable, la lettre du 011 § 10 redevient exacte — toute convention supplémentaire de cette famille est une donnée, sans changement de moteur ». Couvrir une famille est donc un **engagement fort** : le moteur s'oblige envers toute convention conforme de cette famille, présente et future — c'est le prix, assumé, de la promesse « le système apprend sans changer de moteur » ;
- **la couverture est une propriété de la version du moteur** — jamais du registre (015 § 1 : le registre « ne contient jamais … une référence à une implémentation » ; I56 : aucune structure nouvelle n'y est nécessaire), et jamais une option d'exécution : un réglage de couverture qui changerait W serait une troisième entrée, ce que le 011 § 2.2 exclut (« Il n'existe **aucune troisième entrée** (EXG-02) »). À version de moteur fixée, la couverture est fixée ;
- **le contrat définit le mécanisme, jamais l'inventaire** — conformément à I40 (« aucun choix d'implémentation ne peut restreindre, étendre ou réinterpréter le contrat »), le présent document ne dit nulle part quelles familles un moteur donné couvre : il dit ce qu'est une couverture, comment elle se déclare (§ 3) et ce qu'elle conditionne (§ 4). Le précédent est exact : le 011 § 9 contractualise « le moteur déclare les versions [du contrat de Ω] qu'il supporte » sans jamais nommer une version.

---

## 3. La déclaration de couverture

**Définition 2 (déclaration de couverture)** — L'énoncé, par version de moteur, de l'ensemble exact des familles couvertes. Elle obéit à trois règles :

- **explicite** — il n'existe pas de couverture tacite, déduite du comportement ou présumée d'après les familles « habituelles » : une famille non déclarée est non couverte, quel que soit ce que le code saurait faire (I62, § 12) ;
- **par version** — la conformité étant déclarée « par version de moteur » (011 § 8), et la couverture conditionnant les sorties (§ 4), la couverture déclarée est une **composante de la déclaration de conformité** : deux versions de moteur de couvertures différentes sont deux moteurs différents au sens du 011 § 8, chacune revalidée intégralement — c'est l'application directe du raffinement du 016 § 5.1 (« rendre une telle famille applicable est une évolution du moteur, intégralement revalidée (011 § 8) ») ;
- **stable sous l'index** — la déclaration ne dépend ni de Ω, ni de ℛ, ni d'aucun contexte d'invocation : elle précède toute invocation et vaut pour toutes (I63, § 12).

---

## 4. La précondition d'applicabilité

> **Raffinement assumé du 011 § 4.** Les préconditions d'invocation portant sur ℛ sont désormais au nombre de quatre : ℛ est présent, bien formé, cohérent, **et couvert** — toute convention en vigueur de ℛ appartient à une famille de la couverture déclarée du moteur invoqué.

La quatrième précondition a le même statut que les trois premières : elle se vérifie **avant toute dérivation**, et son échec est une erreur du contrat (§ 6) — jamais un refus, jamais un résultat dégradé. La postcondition « entier ou absent » du 011 § 4 est inchangée et s'applique : une invocation sous registre non couvert échoue **en entier** ; aucun W partiel — dérivé des seules conventions couvertes — n'existe, n'est émis, ni ne peut être obtenu. Le W partiel silencieux, précisément, est ce que ce document rend impossible (I61, § 12).

---

## 5. Le siège de la vérification

La vérification de couverture appartient à **C2**. La décision découle de trois faits du 014, sans alternative restante :

- **C2 est la seule frontière de ℛ** : « rien ne franchit C2 sans » l'établissement des garanties registre (014 § 1, C2, clause « garantit ») — la couverture est une garantie registre de plus, du même côté de la même frontière ;
- **le précédent est dans C1** : la seule erreur relative au moteur du contrat actuel — « Ω incompatible » — est portée par C1 (014 C1, clause « refuse » : « version de contrat non supportée → “Ω incompatible” »). La connaissance de ce que *ce* moteur supporte est donc déjà, par contrat, une connaissance de couche d'entrée : C2 connaissant la couverture de sa propre version de moteur n'est pas plus une entrée cachée que C1 connaissant ses versions de contrat supportées ;
- **les couches aval doivent rester sans erreur** : les clauses « refuse : rien » de C3, C4 et C5 (014 § 1) reposent sur I51 (« tout objet consommé est valide par construction ») ; loger la vérification en aval de C2 obligerait ces couches à refuser, détruisant leurs contrats.

**Complément assumé du contrat de C2 (014 § 1)** : la clause « refuse » de C2 gagne un quatrième cas — le registre non couvert (§ 6) — et sa clause « garantit » s'étend : rien ne franchit C2 sans que la couverture soit établie. **La portée d'I51 s'étend d'autant** : en aval de C2, tout référentiel est non seulement valide mais **couvert par construction** — c'est ce qui préserve, sans en changer un mot, la légitimité du silence de C3 face à une convention *absente* (014 C3 : « l'absence d'EQ-01 … produit simplement l'absence de toute instance ») : ce cas reste exact, car une convention *présente mais non couverte* n'atteint plus jamais C3.

Le composant du report 2 — le porteur de la fonction (Ω, ℛ) → W — n'est pas concerné au-delà de ce qu'il était déjà : il invoque C2, qui échoue nommément ; son architecture reste hors du présent périmètre.

---

## 6. La septième erreur : « registre non couvert »

> **Raffinement assumé du 011 § 5.** La table des erreurs contractuelles compte désormais **sept** entrées. La septième :
>
> | Erreur | Condition |
> |---|---|
> | **registre non couvert** | au moins une convention en vigueur de ℛ appartient à une famille hors de la couverture déclarée du moteur invoqué |
>
> Comme les six autres, elle est explicite, nommée et diagnosticable : elle identifie l'entrée fautive (ℛ — et, en son sein, la ou les conventions concernées avec leur famille) et la clause violée (la quatrième précondition du § 4). Le libellé de clôture du 011 § 5 (« les seules erreurs contractuelles ») se lit désormais sur la table à sept entrées.

**Pourquoi une erreur nouvelle — démonstration de non-extensibilité des six existantes.** Chaque candidate, examinée et écartée :

- **« registre absent »** — l'entrée est présente et identifiable : la condition n'est pas réalisée. ∎
- **« registre malformé »** — la forme est intrinsèque au registre : la check-list du 015 § 8 est intégralement vérifiable « par un humain sans outil » (015 § 8, point 24), donc sans moteur. Étendre « malformé » à la couverture rendrait la forme relative à une implémentation : le même registre, ayant passé les vingt-quatre points, serait « malformé » pour un moteur et pas pour un autre — contradiction avec le 015 § 8 et avec I53 (toute convention possède une représentation documentaire complète, propriété du document seul). ∎
- **« registre incohérent »** — la ligne du 011 § 5 **définit** cette erreur par l'échec du prédicat qu'elle cite, dont les membres (compatibilité, dépendances, acyclicité, minimalité, confluence) sont tous intrinsèques à ℛ. Y loger la couverture rendrait la cohérence relative au moteur — le même ℛ « cohérent » pour l'un, « incohérent » pour l'autre — contradiction avec la nature du prédicat ; et la sévérité attachée à l'incohérence (aucune élection nulle part) serait disproportionnée pour un simple défaut de maturité du moteur, qui n'est pas un défaut de gouvernance. ∎
- **« Ω absent »**, **« Ω invalide »** — l'entrée fautive serait mal identifiée : la discipline du 011 § 5 (« elle identifie l'entrée fautive ») l'interdit — l'entrée fautive est ℛ. ∎
- **« Ω incompatible »** — le bon **modèle** (relative au moteur, entrée par ailleurs valide), la mauvaise entrée : l'étendre à ℛ violerait la même discipline d'identification. Elle est le précédent de la septième erreur, pas son support. ∎

Aucune extension n'étant possible sans contradiction, l'erreur nouvelle est **nécessaire** — et elle est **disjointe** de l'incohérence par construction : le prédicat de cohérence n'est ni touché, ni étendu, ni réinterprété.

---

## 7. Les trois cas de la famille

Le champ `famille` d'une convention (014 § 5.1) connaît désormais trois cas, exhaustifs et mutuellement exclusifs :

| Cas | Issue | Statut |
|---|---|---|
| **famille inconnue** (hors des huit du 014 § 5.1) | « registre malformé » | inchangé (014 § 5.1, causes de rejet) |
| **famille connue, hors couverture** | « registre non couvert » (§ 6) | le cas nouveau |
| **famille connue, couverte** | la convention est appliquée par la couche que sa famille concerne (014 § 3 : C3 pour interprétation, équivalence, attente, catalogue ; C4 pour stratification, composition ; C5 pour élection, priorité) | inchangé |

La frontière entre les deux premiers cas est celle du 015 § 8 : ce qu'une relecture humaine sans outil peut rejeter est de la forme (famille inconnue) ; ce qui exige de connaître un moteur est de la couverture. La frontière entre les deux derniers est la déclaration du § 3, et elle seule — jamais une inspection du contenu de la convention.

---

## 8. L'ordre des vérifications du registre

> **Ordre de précédence** — Les vérifications de la frontière registre s'établissent dans cet ordre total, et le signalement est celui du **premier échec** : **absence** (« registre absent ») < **forme** (« registre malformé ») < **cohérence** (« registre incohérent ») < **couverture** (« registre non couvert »).

Deux justifications, l'une de bonne fondation, l'autre d'attribution :

- **bonne fondation** : chaque étage présuppose le précédent — la forme ne se vérifie que sur un registre présent ; la cohérence ne se vérifie que sur des conventions dont la forme a livré les champs (dépendances, familles) ; la couverture ne se vérifie que sur l'ensemble cohérent des conventions en vigueur ;
- **attribution** : les trois premiers défauts sont **intrinsèques au registre** — ils appartiennent à la gouvernance, qui les corrige dans ℛ ; le quatrième est **relatif au moteur** — il appartient à la maturité de l'implémentation, qui se corrige par une évolution revalidée (016 § 5.1). Signaler l'intrinsèque avant le relatif adresse chaque défaut à qui peut le corriger.

Cet ordre rend le signalement **déterministe** (I64, § 12) : un registre cumulant plusieurs défauts produit toujours la même erreur, sur toute machine, à tout instant — le déterminisme du 011 § 6, étendu aux échecs. L'ordre entre les vérifications de Ω et celles de ℛ — deux entrées distinctes — relève du contrat du porteur de la frontière et n'est pas fixé ici (périmètre, en-tête).

---

## 9. La compatibilité des versions et l'écart publié

> **Raffinement assumé du 011 § 9, troisième puce.** « Nouvelle version de ℛ : une transition ordinaire — aucune compatibilité à négocier : le moteur applique l'état fourni, quel qu'il soit, pourvu qu'il soit cohérent » se lit désormais : pourvu qu'il soit cohérent **et couvert**. La clause conservait un présupposé — tout moteur conforme applique toute famille théorisée — que le 016 § 5.1 a levé ; la présente borne en tire la conséquence contractuelle.

**L'écart publié.** Le comportement des moteurs change sur une classe d'index, et ce changement est gouverné par l'exception prévue d'I59 (« la correction publiée d'une non-conformité antérieure (011 § 9) ») :

- **la classe** : tout couple (Ω, ℛ) où ℛ est cohérent et contient au moins une convention en vigueur hors de l'ensemble {EQ-01 v1, CE-01 v1} — les seules que le moteur v1 applique réellement (016 § 4.1, report 1 : application « par identifiant codé ») ;
- **le comportement v1** : un W était émis, dérivé en ignorant les conventions excédentaires — chaque acte d'un tel W cite un index (Ω, ℛ) dont la dérivation n'a pas mobilisé tout le contenu : sa justification ment sur sa dépendance (I34, I38). La justification de l'écart est donc **uniforme et acte par acte à la fois** : tout acte de tout W de la classe porte le même vice, l'index cité n'est pas l'index appliqué ;
- **le comportement conforme au présent contrat** : l'erreur « registre non couvert », aucun W ;
- **la déclaration rétroactive** : conformément au mécanisme du 011 § 9, la v1 est déclarée **non conforme rétroactivement sur cette classe d'index** — et sur elle seule : sa conformité sur (Ω_corpus1, ℛ₀), et sur tout index dont le registre se limite aux conventions qu'elle applique, demeure entière.

---

## 10. La validation

- **W₀ est strictement inchangé.** ℛ₀ = {EQ-01 v1 (interprétation), CE-01 v1 (élection)} : tout moteur conforme doit produire W₀ depuis (Ω_corpus1, ℛ₀) (EXG-26, rappelé par 016 § 6 et I59) — donc tout moteur conforme couvre au moins les familles interprétation et élection, donc ℛ₀ est couvert par tout moteur conforme, donc l'erreur du § 6 est **indéclenchable sur l'index de l'oracle**. Le présent acte n'est pas une révision d'oracle : le report 6 du 016 § 4.2 reste le seul cas de ce type. ∎
- **La batterie s'étend mécaniquement.** Le 011 § 8 (point 4) exige que chaque erreur du § 5 soit provoquée sur des entrées construites : la table comptant sept entrées, la batterie compte un cas de plus — aucune autre clause du contrat de validation ne change.
- **Une espèce nouvelle de fixtures.** Les actifs de test gagnent des registres **valides, cohérents et non couverts** — la première espèce de fixture qui n'est pas « cassée » en soi : son défaut n'existe que relativement à la couverture du moteur testé. Leur construction n'exige aucune famille nouvelle réellement applicable : une convention conforme d'une famille non couverte suffit.
- **La non-régression est gouvernée.** Les W attendus passés restent valides sur leurs index (tous couverts) ; la classe d'index du § 9 sort du régime « sorties identiques » par la voie de l'écart publié — jamais silencieusement.

---

## 11. Compatibilité avec les documents figés — démonstration

| Clause figée | Tension apparente | Résolution |
|---|---|---|
| 011 § 5, « les seules erreurs contractuelles » | la liste close s'ouvre | raffinement assumé, déclaré en en-tête et motivé au § 6 — le mécanisme même par lequel le 011 § 5 a raffiné EXG-15. ∎ |
| 011 § 4, préconditions | une quatrième s'ajoute | raffinement assumé (§ 4) ; « entier ou absent » intact. ∎ |
| 011 § 9, troisième puce | contredite par l'erreur nouvelle | traitée nommément et bornée (§ 9) — le traitement que le 016 § 5.1 a appliqué au § 10 ; rien n'est passé sous silence. ∎ |
| 011 § 10 et 016 § 5.1 | double définition possible du régime des familles | le présent document s'adosse au raffinement du 016 § 5.1 par référence (§§ 2–3) et ne le redéfinit pas. ∎ |
| 011 § 8, conformité par version | la couverture pourrait sembler une donnée hors déclaration | elle devient composante de la déclaration de conformité (§ 3) — extension, aucun retrait. ∎ |
| I40 (011 § 12) | une erreur relative au moteur semble lier le contrat à l'implémentation | le contrat définit le mécanisme de déclaration, jamais l'inventaire (§ 2) — le précédent « Ω incompatible »/« le moteur déclare les versions qu'il supporte » établit la compatibilité. ∎ |
| 014 § 1, C2 (« refuse », trois cas) | un quatrième cas s'ajoute | complément assumé (§ 5), symétrique du cas « Ω incompatible » que C1 porte déjà. ∎ |
| 014 § 1, C3–C5 (« refuse : rien ») | la vérification pourrait les atteindre | elle est close en C2 ; I51 étendu (« couvert par construction ») les préserve mot pour mot (§ 5). ∎ |
| 014 § 5.1 (« famille inconnue » → malformé) | frontière avec le cas nouveau | articulation des trois cas (§ 7) : « malformé » garde exactement sa portée. ∎ |
| 015 § 1, § 8, I53–I56 | l'applicabilité pourrait chercher un support dans le registre | exclusion constitutive (en-tête, § 2) : aucun champ, aucune section, aucune grammaire touchés — le registre ignore la couverture à jamais. ∎ |
| 016 § 3 (le document précède le code) | — | le présent acte précède toute implémentation (Conclusion). ∎ |
| 016, I57–I60 | la v1 taguée ; les sorties passées | I57 : la v1 n'est ni réécrite ni réinterprétée — elle est déclarée non conforme rétroactivement sur une classe d'index, par le mécanisme prévu (011 § 9, § 9 ci-dessus) ; I59 : l'exception « correction publiée » est exactement celle exercée. ∎ |

---

## 12. Invariants — démontrés

> **I61 — Aucune dérivation sous un registre non couvert.**
> *Démonstration.* La couverture est une précondition (§ 4), vérifiée par C2 (§ 5), dont la clause « garantit » interdit que rien ne la franchisse sans établissement des garanties registre (014 § 1, C2, étendue au § 5). C2 étant la seule frontière de ℛ (014 § 1 : seule couche à connaître sa représentation) et les circulations inter-couches étant closes par les contrats du 014 § 3 (aucun canal hors table), aucun référentiel non couvert ne peut atteindre C3–C6 ; et « entier ou absent » (011 § 4) exclut tout W partiel émis avant l'échec. Une dérivation sous registre non couvert exigerait donc un canal qui n'existe dans aucun contrat — elle est impossible par construction. ∎

> **I62 — La couverture est déclarée, jamais implicite.**
> *Démonstration.* La condition de l'erreur du § 6 (« hors de la couverture **déclarée** ») n'est décidable que par référence à une déclaration : sans elle, l'erreur ne pourrait pas « identifier … la clause violée » comme le 011 § 5 l'exige de toute erreur — le contrat serait inapplicable. La déclaration est donc constitutive du régime, pas un accessoire ; et une couverture déduite du comportement varierait avec lui, rendant la précondition du § 4 circulaire (le comportement définirait la condition de sa propre licéité). L'explicitation est ainsi nécessaire — c'est I13 (« aucune convention n'est implicite »), appliqué au moteur lui-même. ∎

> **I63 — La couverture est une propriété de la version du moteur — jamais du registre, jamais d'une configuration.**
> *Démonstration.* Par élimination des deux seuls autres supports possibles. Dans ℛ : interdit par 015 § 1 (« [le registre] ne contient jamais … une référence à une implémentation ») et superflu par I56 (toute convention future est exprimable sans modifier la structure du registre — la couverture n'y a donc aucune place nécessaire). En configuration d'exécution : un réglage dont dépendrait l'issue de l'invocation serait une entrée influençant la sortie hors de (Ω, ℛ), ce que le 011 § 2.2 exclut absolument (« Il n'existe aucune troisième entrée (EXG-02) »). Reste le moteur lui-même ; et puisque la conformité est déclarée « par version de moteur » (011 § 8) et que la couverture conditionne les sorties (§ 4), sa granularité est la version. ∎

> **I64 — Le signalement des échecs de la frontière registre est déterministe.**
> *Démonstration.* Chacune des quatre vérifications du § 8 est une fonction déterministe de ses seules entrées : les trois premières de ℛ seul (absence, forme, cohérence — intrinsèques), la quatrième du couple (ℛ, couverture), la couverture étant fixée par version (I63). L'ordre du § 8 étant total, le « premier échec » est unique pour tout couple (ℛ, version de moteur) : même registre, même version ⟹ même erreur, sur toute machine, à tout instant. C'est le déterminisme du 011 § 6, obtenu sur les échecs par le même argument que sur les sorties : des fonctions pures composées dans un ordre fixé. ∎

---

## Conclusion

Le régime d'applicabilité est complet : une notion (la couverture, par famille, par version), une déclaration (explicite, composante de la conformité), une précondition (la quatrième du 011 § 4), une erreur (la septième du 011 § 5, symétrique de « Ω incompatible »), un ordre de vérification (total, déterministe), une articulation des cas de famille, un régime de transition (l'écart publié d'I59, avec déclaration rétroactive de non-conformité sur la classe d'index concernée), et quatre invariants démontrés. Le prédicat de cohérence, le registre, sa grammaire, l'oracle et W₀ sont intacts.

Conformément au 016 § 3, **le présent acte précède tout code** : aucune ligne du moteur v2 n'existe. Sa validation relève de l'autorité du projet, comme celle de chaque document de la série ; après elle, le report 1 du 016 § 4.1 disposera de son fondement documentaire complet, et l'implémentation pourra être entreprise sans aucune liberté normative résiduelle.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| défaut contractuel | registre bien formé, cohérent, non applicable : aucune issue sous 010→016 ; l'ignorance silencieuse de la v1 est une non-conformité (I34, EXG-13, I38) | 1 |
| couverture | ensemble des familles qu'une version de moteur sait appliquer (EXG-13) ; granularité : la famille (016 § 5.1) ; propriété de la version, jamais du registre ni d'une configuration | 2 |
| déclaration de couverture | explicite, par version, composante de la déclaration de conformité (011 § 8) ; jamais tacite | 3 |
| précondition d'applicabilité | quatrième précondition sur ℛ (raffinement du 011 § 4) : présent, bien formé, cohérent, **couvert** ; « entier ou absent » intact | 4 |
| siège | C2 — seule frontière de ℛ, sur le précédent de C1/« Ω incompatible » ; I51 étendu : couvert par construction ; C3–C5 préservés | 5 |
| « registre non couvert » | septième erreur du 011 § 5 (raffinement assumé) ; non-extensibilité des six démontrée ; disjointe de l'incohérence | 6 |
| les trois cas de la famille | inconnue → malformé (inchangé) ; connue non couverte → « registre non couvert » ; connue couverte → appliquée par sa couche (014 § 3) | 7 |
| ordre des vérifications | absence < forme < cohérence < couverture ; premier échec signalé ; intrinsèque avant relatif | 8 |
| compatibilité des versions | 011 § 9 borné (« cohérent **et couvert** ») ; écart publié (I59) : classe d'index caractérisée, justification uniforme, v1 déclarée non conforme rétroactivement sur cette classe seule | 9 |
| validation | W₀ inchangé (démontré) ; batterie étendue d'un cas ; fixtures d'espèce nouvelle (valides, cohérentes, non couvertes) | 10 |
| compatibilité documentaire | douze clauses figées, chacune résolue par raffinement déclaré — aucune par silence | 11 |
| invariants | I61 aucune dérivation sous registre non couvert, I62 couverture déclarée, I63 propriété de la version du moteur, I64 signalement déterministe | 12 |

**Ce que ce document ne fait volontairement pas** : rendre une famille applicable (chaque extension de couverture est une évolution de moteur revalidée, 016 § 5.1), effectuer une transition de ℛ, adopter ou définir une convention, fixer l'ordre des vérifications entre Ω et ℛ (contrat du porteur, report 2), décrire l'architecture du composant du report 2, réviser l'oracle ou la carte des refus (report 6), toucher à la grammaire du registre, écrire ou spécifier du code.
