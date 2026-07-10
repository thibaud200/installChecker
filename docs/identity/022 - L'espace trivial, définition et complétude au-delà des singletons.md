# 022 — L'espace trivial : définition et complétude au-delà des singletons

**Statut** : sixième acte documentaire de la phase v3 de la série `docs/identity/`. S'appuie sur les documents 000→021, figés. Exécute le **report 12** du 016 § 4.2 — « trancher la notion d'espace trivial au-delà des singletons » — par la voie 2 du 016 § 3. Acte documentaire **seul** : aucune sortie du moteur ne change, aucun code, aucun test, aucun oracle.
**Nature** : un acte de **théorie** — le premier de la campagne v3 à toucher la couche 000→009. Il définit une notion que les documents figés présupposaient sans la définir — l'*espace trivial* — et démontre que cette définition est la seule compatible avec eux. La voie retenue est la **confirmation** : la théorie actuelle implique déjà la position normative ; le présent acte l'explicite, il n'introduit aucune règle.
**Raffinement assumé** : ce document porte un unique toucher déclaré — un **raffinement du 009 § 5** : sa « précision au 006 § 3 » (la complétude se lit sur les espaces non triviaux), énoncée pour les seules classes singletons, devient le cas particulier d'une définition générale (§ 4). Il **s'adosse**, sans en changer un mot, aux 005 §§ 3 et 11, 006 §§ 3, 4 et 10, 014 § 8 et 021. Hors ce point, sa définition (§ 4 — le contenu que le report exigeait) et aucun autre contenu normatif nouveau.
**Périmètre — exclusions constitutives** : rien ne touche l'identité d'un état d'Ω (report 5), le journal du registre (report 7), la référence d'acte (report 8), la cause et les continuités de τ (report 9), ni la forme canonique (report 3 — qui consommera la présente définition sans qu'elle l'anticipe) ; aucun document figé n'est modifié ; aucun code, aucun test, W₀ inchangé.

---

## 1. Le défaut — l'indécision inventoriée

Le 016 § 4.2 (report 12) constate : « la complétude des actes sur un domaine multi-actes sans signal constructible est **indécidée** : 005 § 11 pose que les hypothèses extrêmes existent "pour toute granularité et tout domaine" (l'espace n'est jamais vide), 006 § 3 interdit les zones muettes, et 009 § 5 restreint la complétude "aux domaines où l'espace d'hypothèses est non trivial" — la réconciliation n'est faite que pour les classes singletons. » Trois clauses figées, chacune vraie, dont la composition semble laisser un cas ouvert : que doit W à un domaine de plusieurs actes où rien ne se construit ?

## 2. L'audit — les trois pièces, à la lettre

- **005 § 11 (les extrêmes)** : la minimale (« toutes les origines distinctes ») et la maximale (« une seule origine ») « existent pour toute granularité et tout domaine — l'espace d'hypothèses d'une strate n'est donc **jamais vide** » ; et la pièce décisive : « dans les configurations de silence total (000 L2), **la minimale peut rester la seule non dominée : c'est la forme exacte que prend "aucune hypothèse formulable" dans le cadre stratifié** ». Le 005 § 3 qualifie par ailleurs la strate contenu — « la seule strate **observée** » — d'« espace d'hypothèses **dégénéré** (003 § 9) ».
- **006 §§ 3, 4 et 10 (la complétude et le refus)** : « tout domaine-strate non couvert par une élection l'est par un refus explicite — l'état ne laisse pas de zones muettes : il élit ou il refuse, mais il *répond* » (§ 3) ; et le § 4 définit, parmi les motifs structurels du refus, le **silence** : « aucune hypothèse non extrême formulable, 000 L2 — **la minimale seule non dominée, 005 § 11** » — la citation croisée est dans le texte figé lui-même ; l'état prudent « n'élit que là où l'élection est forcée… et refuse partout ailleurs » (§ 10).
- **009 § 5 (la vacuité des singletons)** : « les 269 classes singletons ne portent aucune hypothèse non triviale à la strate contenu : **rien à élire, rien à refuser** — leur couverture est le constat de vacuité de l'espace (précision au 006 § 3 : la complétude se lit sur les domaines où l'espace d'hypothèses est non trivial) » — la précision est posée, la notion d'espace trivial n'est définie nulle part.

## 3. L'analyse — les cinq questions

- **Le 005 définit-il les espaces triviaux pour tout domaine ?** Non — le mot n'y reçoit aucune définition. Mais il fournit les deux clés : les extrêmes existent partout (aucun espace n'est vide), et le silence total a une *forme* dans le cadre stratifié — la minimale seule non dominée — qui est une configuration d'hypothèses, pas une absence d'espace.
- **Le 006 suppose-t-il la généralisation ?** Mieux : il la **contient**. Son § 4 nomme le motif exact du cas litigieux — « silence » — en citant la forme du 005 § 11 : pour le 006, un espace où seule la minimale surnage n'est pas une zone muette à exempter, c'est un domaine qui **refuse**, motivé.
- **Le 009 restreint-il la complétude aux singletons ?** Non — il en **exempte** les singletons (vacuité) et, pour les domaines multi-actes silencieux, sa carte (le § 6, requalifiée par le 021 en carte des cessions) prévoyait précisément des refus « silence » — les 12 % du corpus, le fichier géant (E3). Le 009 distingue donc de fait les deux cas ; il ne formalise pas le critère.
- **Le 009 est-il silencieux ?** Sur le critère, oui ; sur les cas, non — et ses cas tombent tous du bon côté de la définition du § 4.
- **Le 016 constate-t-il une contradiction réelle ?** Non : une **absence de formalisation**. Aucune des trois clauses n'en contredit une autre — il manquait le mot qui les compose.

## 4. La définition, et la réponse à la question centrale

> **Définition (espace trivial).** L'espace d'hypothèses d'un domaine-strate est *trivial* lorsqu'il ne pose aucune question départageable — c'est-à-dire lorsque ses hypothèses extrêmes (005 § 11) **coïncident ou dégénèrent en constat** :
>
> - **tout singleton, à toute strate** : sur un domaine d'un seul acte, la minimale et la maximale coïncident (« tout distinct » et « tout commun » y disent la même chose de l'unique acte) — une seule hypothèse dégénérée, aucune question ;
> - **à la strate contenu — la seule observée (005 § 3) — tout domaine hors des classes ≡ₘ multi-actes** : l'observation y répond avant toute hypothèse (003 § 9 : le consensus y est dégénéré) — des contenus distincts sont un fait constaté, pas une question ouverte ; il ne reste à cette strate que le cas dégénéré des classes multi-actes, que la convention-plafond porte au niveau « certaine » (004 E6, CE-01).
>
> Un espace trivial ne porte **ni élection ni refus** : c'est le constat de vacuité du 009 § 5, dont la lettre — énoncée pour les singletons — devient le cas particulier de la présente définition. La complétude (006 § 3) se lit sur les espaces **non triviaux**, et sur eux seulement.

> **Réponse (la question centrale).** Un domaine de **plusieurs actes**, à une **strate hypothétique** (variante, version, identité, famille), dont l'espace ne permet aucune hypothèse constructive, est un **refus** — ni un espace vide, ni un espace trivial, ni une quatrième catégorie. Démonstration par élimination, depuis la série seule :
>
> - **pas vide** : 005 § 11 — les extrêmes existent pour tout domaine et toute granularité ;
> - **pas trivial** : sur plusieurs actes, la minimale et la maximale sont des hypothèses **distinctes**, aux contenus propositionnels incompatibles — la question « même origine ? » est réelle ; l'espace la pose, quelle que soit la pauvreté des signaux ;
> - **pas une quatrième catégorie** : la complétude du 006 § 3 est binaire — « il élit ou il refuse, mais il répond » ; et l'élection est exclue : l'unique non-dominée est la minimale, qu'aucune convention ne licencie (I27 ; P7 : l'absence de licence produit un refus, jamais une conjecture) ;
> - **donc un refus** — dont le 006 § 4 donne déjà le motif structurel : « silence » (la minimale seule non dominée, 005 § 11) — ou, selon la configuration, « sous-détermination » ou « incomparables ». Son espèce et son motif effectifs, dans un W donné, suivent la règle du premier maillon manquant (021) : sous un registre qui ne fonde pas la strate, le premier manque est **normatif** (aucune convention — le manque structurel vit plus loin dans la chaîne, réservé aux états futurs, 014 § 7.4) ; sous un registre qui la fonde, le manque structurel devient premier et le refus porte son motif de la carte des cessions (021 § 3).

## 5. Pourquoi W₀ et le moteur sont inchangés — démonstration

- **W₀ réalise déjà la définition, acte pour acte** : à la strate contenu, les 112 classes multi-actes sont élues (le cas dégénéré licencié), les 269 singletons et tout autre groupement relèvent de l'espace trivial — aucun acte (009 § 5, 014 § 8) ; aux strates hypothétiques, tout domaine multi-actes est non trivial — et sous ℛ₀ son premier manque est partout le même manque normatif : les refus s'agrègent au domaine maximal (014 § 7.3) — exactement les **quatre refus du 014 § 8**, dont les domaines (les 497 actes) couvrent déjà chacun de ces domaines. Rien à ajouter, rien à retirer : la définition décrit W₀, elle ne le modifie pas ;
- **le moteur réalise déjà la définition** : C4 ne construit d'hypothèses que sur les espaces non triviaux effectivement questionnés (les classes de contenu, sous les conventions en vigueur) ; C5 couvre les strates non fondées par les refus dérivés de ℛ, agrégés par C6 — le comportement validé en v1 et revalidé en v2 est la définition en acte ; aucune ligne ne change ;
- **aucun invariant n'est affaibli** : I19 (les hypothèses extrêmes des espaces non triviaux demeurent, indestructibles, dans l'attente des adoptions) ; I27 et P7 (aucune élection sans licence — c'est le pivot de la démonstration du § 4) ; I38 (tout refus reste intégralement motivé — et la définition explique pourquoi l'espace trivial n'est *pas* un refus : il n'y aurait aucun manque à nommer, donc aucune justification possible — un refus sur espace trivial violerait I38) ; I61–I67 hors de cause ;
- **le report 3 en héritera sans être anticipé** : le fichier W₀ d'or figera une complétude désormais définie — le présent acte ne dit rien de sa forme.

---

## 6. Compatibilité avec les documents figés — démonstration

| Clause figée | Tension apparente | Résolution |
|---|---|---|
| 005 § 11 (« l'espace n'est jamais vide ») | semblait exiger des actes partout | confirmé mot pour mot : jamais vide ≠ jamais trivial — les extrêmes existent, et coïncident sur les singletons ; la non-vacuité fonde précisément le refus des multi-actes (§ 4). ∎ |
| 006 § 3 (« pas de zones muettes ») | semblait contredire la vacuité des singletons | confirmé : la complétude se lit sur les espaces non triviaux (la précision du 009 § 5, généralisée) — un espace trivial n'est pas une zone muette : aucune question n'y attend de réponse. ∎ |
| 006 § 4 (le motif « silence ») | — | confirmé et mobilisé : le texte figé nommait déjà le refus du cas litigieux, en citant la forme du 005 § 11 — la réponse du § 4 est la sienne. ∎ |
| 006 § 10 (l'état prudent « refuse partout ailleurs ») | « partout » semblait inclure les espaces triviaux | relu sur les espaces non triviaux : là où aucune question n'existe, il n'y a rien à refuser — la lettre du 009 § 5 (« rien à élire, rien à refuser »). ∎ |
| 009 § 5 (la précision, restreinte aux singletons) | la restriction semblait arbitraire | raffinement assumé, déclaré en en-tête : la précision devient le cas particulier de la définition — sa lettre est conservée, sa portée est fondée. ∎ |
| 009 § 6 / 021 (la carte des cessions : « silence » sur les 12 %, E3) | des refus multi-actes semblaient contredire la vacuité | confirmés : ces domaines sont non triviaux (plusieurs actes, question réelle) — leurs refus sont exactement ce que le § 4 démontre, aux motifs que le 014 § 7.4 réserve et que le 021 date. ∎ |
| 014 §§ 7.3 et 8 (l'agrégation ; les quatre refus) | — | confirmés : les domaines multi-actes non triviaux des strates non fondées sont couverts par les refus agrégés de domaine maximal — la complétude du § 4 est réalisée par l'oracle tel quel. ∎ |
| 014 C5 / 011 § 4 (« tout domaine-strate à espace non trivial ») | la quantification restait indéfinie | définie (§ 4) : le contrat se lit désormais sur une notion fondée — aucune clause n'en change. ∎ |
| 021 (la règle du premier maillon ; la carte des cessions) | — | consommé tel quel : l'espèce et le motif des refus des espaces non triviaux suivent sa règle ; aucune interaction nouvelle. ∎ |
| 016 § 4.2 (report 12) | — | le présent acte est son exécution intégrale : le report 12 est clos. ∎ |
| 016 § 4.2 (reports 5, 7, 8, 9) et report 3 | — | non anticipés : ni identité d'Ω, ni journal, ni référence d'acte, ni τ, ni forme canonique — le report 3 consommera la définition sans qu'elle en dise rien. ∎ |

---

## Conclusion

La notion que trois documents figés présupposaient est définie, et la définition démontre qu'ils n'ont jamais divergé : l'espace trivial est celui dont les extrêmes coïncident ou dégénèrent en constat — les singletons, et le hors-classe de la seule strate observée ; partout ailleurs, l'espace pose une question réelle et l'état **répond** — il élit où la configuration est forcée et licenciée, il refuse ailleurs, au motif que la règle du premier maillon désigne. Un domaine multi-actes sans hypothèse constructive est un refus : le 006 § 4 le disait déjà, en citant le 005 § 11 — il ne manquait que le mot. W₀, le moteur, les tests et les invariants sont inchangés. Le report 12 du 016 § 4.2 est **clos**. Sa validation relève de l'autorité du projet.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| le défaut | la composition 005 § 11 + 006 § 3 + 009 § 5 laissait le cas multi-actes indécidé — absence de formalisation, jamais contradiction | 1–3 |
| espace trivial | les extrêmes coïncident (singletons) ou dégénèrent en constat (hors-classe de la strate contenu, la seule observée) — ni élection ni refus : le constat de vacuité du 009 § 5, généralisé | 4 |
| la question centrale | un domaine multi-actes sans hypothèse constructive, à une strate hypothétique = **un refus** (ni vide, ni trivial, ni quatrième catégorie) — motif structurel du 006 § 4, espèce et motif effectifs par la règle du premier maillon (021) | 4 |
| l'invariance | W₀ acte pour acte, le moteur, les tests, I1–I67 : inchangés — la définition décrit l'existant, elle ne le modifie pas | 5 |
| compatibilité documentaire | douze clauses figées, chacune traitée nommément — aucune par silence | 6 |

**Ce que ce document ne fait volontairement pas** : modifier un document figé, changer un acte de W₀, introduire une règle (la voie B est écartée : la théorie suffisait), définir la forme canonique (report 3), l'identité d'Ω (report 5), le journal (report 7), la référence d'acte (report 8) ou la cause de τ (report 9), créer un invariant, écrire ou modifier du code ou des tests.
