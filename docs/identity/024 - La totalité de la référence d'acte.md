# 024 — La totalité de la référence d'acte

**Statut** : huitième acte documentaire de la phase v3 de la série `docs/identity/`. S'appuie sur les documents 000→023, figés. Exécute le **report 8** du 016 § 4.2 — « garantir l'unicité de la référence, ou la redéfinir » — par la voie 2 du 016 § 3, **le document précédant le code** : un volet logiciel strictement borné aux porteurs de la référence réalise ensuite ce que le présent acte fixe.
**Nature** : un document de **contrat** — il redéfinit la référence d'acte en la faisant rejoindre un objet que la théorie possédait déjà : l'identité de l'acte (014 § 2). La voie retenue est le **raffinement** (Cas B) : la totalité de la paire actuelle n'est pas dérivable de la série — elle n'était que supposée.
**Raffinement assumé** : ce document porte un unique toucher déclaré — un **raffinement du 014 § 7.5** : « chaque référence étant le couple (strate, plus petit identifiant du domaine) » se lit désormais : chaque référence est **l'identité de l'acte — le couple (strate, domaine explicitement énuméré et trié)** ; le plus petit identifiant du domaine demeure la **clé de tri** des références et devient la **désignation abrégée** de l'audit (§ 3). La désignation d'audit, jamais contractualisée auparavant, est le seul mécanisme nouveau. Hors ces points, aucun contenu normatif nouveau.
**Périmètre — exclusions constitutives** : rien ne touche l'identité d'un état d'Ω (report 5 — c'est l'identité des **actes de W** qui est en cause, jamais celle des états), la cause et les continuités de τ (report 9 — seule la clé de la correspondance change, jamais son régime), la forme canonique matérielle (report 3 — la représentation **logique** de τ seule est raffinée), la cohérence d'état de C6 (report 4) ; aucun document figé n'est modifié ; W₀ est inchangé.

---

## 1. L'audit — la référence, partout où elle vit

- **014 § 7.5** (la représentation de τ) : la correspondance est faite de « trois listes de références d'actes — conservés, abandonnés, nouveaux — **chaque référence étant le couple (strate, plus petit identifiant du domaine)** ; puis les continuités déclarées, comme couples de références » — le siège de la définition ;
- **012 § 5** (l'ordre canonique) : la représentation ordonne les actes « selon une clé **totale**, déterministe… Toute égalité de contenu se départage par les identifiants » — une exigence de totalité pour l'**ordre**, que le 014 § 7.3 réalise par (rang de strate, type, plus petit identifiant) ;
- **014 § 2** (l'identité de l'acte) : « Acte d'élection | **(domaine, strate) dans un W** » ; « Refus | **(domaine, strate) dans un W** » — l'identité totale existait déjà dans la série ;
- **014 § 7.3** (les domaines) : « la liste **explicitement énumérée et triée** des identifiants d'actes couverts (une seule représentation — jamais d'abréviation) » — et la règle d'agrégation des refus, dont la garde (« des domaines qu'aucun acte de l'état ne distingue ») **contemple expressément le recoupement** de domaines à une même strate ;
- **le constat du report 8** (016 § 4.2) : « deux actes d'une même strate peuvent partager leur plus petit identifiant dès que des refus partiels coexistent avec des élections » ;
- **les porteurs logiciels** : l'objet référence (τ, correspondance, continuités), la résolution d'acte de C7, la désignation d'audit du porteur (018) et de la commande `identity`.

## 2. L'analyse — les cinq questions

- **Comment la référence est-elle définie ?** Par le 014 § 7.5 : la paire (strate, plus petit identifiant du domaine).
- **Est-elle réellement totale ?** Non. La garde d'agrégation du 014 § 7.3 contemple des domaines qui se recoupent à une même strate ; dès lors, un état futur cohérent peut porter, à la même strate, une élection de domaine {1, 2} et un refus de domaine {1, 3} — deux actes, une seule paire (strate, 1). Et le type n'y suffirait pas : deux refus d'une même strate, de motifs distincts, peuvent partager leur plus petit identifiant ({1, 3} en sous-détermination, {1, 5} en silence). La paire n'est **pas** une clé.
- **Existe-t-il un cas actuel de partage ?** Non — et c'est démontrable (§ 3) : sous la couverture de la version courante, aucune strate ne porte à la fois élections et refus, les élections d'une strate ont des domaines disjoints (les classes d'une équivalence), et chaque strate refusée porte un refus agrégé unique. Le partage est un fait des **états futurs** ; la clé doit pourtant être totale par contrat (012 § 5), pas par chance.
- **Garantie ou supposée ?** Supposée seulement — c'est le constat exact du report 8.
- **Les conséquences ?** τ : la correspondance est un classement **exhaustif** (006 § 7, EXG-30) — une clé non totale la rendrait ambiguë ou la ferait échouer ; C7 et la commande `identity` : la résolution d'un acte désigné doit désigner **un** acte (I39 : tout acte est désignable et re-dérivable isolément) ; les invariants : aucun n'est violé aujourd'hui, mais I23 (τ restituable exactement) et EXG-30 reposaient sur une propriété non garantie.

**Cas A ou Cas B ?** La théorie ne dérive pas la totalité de la paire — elle dérive celle d'un **autre** objet : l'identité de l'acte (014 § 2), totale par la complétude (014 C5 : « pour chaque domaine-strate à espace non trivial, **exactement un** acte » — deux actes d'un même (domaine, strate) n'existent dans aucun W cohérent). **Cas B** : un raffinement est nécessaire — le plus petit possible : la référence rejoint l'identité.

## 3. La décision

> **Raffinement du 014 § 7.5 (déclaré en en-tête).** La référence d'acte est **l'identité de l'acte** (014 § 2) : le couple **(strate, domaine explicitement énuméré et trié)**. Elle est totale par la complétude (014 C5), déterministe et dérivée du seul contenu identitaire (012 § 5), et sa représentation suit la règle des domaines du 014 § 7.3 — jamais d'abréviation : la représentation de τ énumère désormais les domaines de ses références, comme W énumère les siens. Le **tri** des références est inchangé : (strate, plus petit identifiant du domaine) — la clé d'ordre du 014 § 7.5, qui n'a jamais eu besoin d'être injective pour ordonner (l'énumération départage, 012 § 5).
>
> **La désignation d'audit (mécanisme nouveau, déclaré).** La question d'audit (011 § 7, C7) désigne son acte par la **forme abrégée** : (strate, plus petit identifiant du domaine). Son univocité est démontrée sur tous les états dérivables sous la couverture de la version courante : à la strate contenu, les seuls actes sont les élections des classes ≡ₘ multi-actes — disjointes par construction (les classes d'une équivalence) — et aucun refus n'y existe (022 : le hors-classe y est trivial) ; à chaque strate supérieure, l'unique acte est le refus agrégé de domaine maximal (021 : le premier manque, identique partout, agrège tout — 014 § 7.3) ; nulle part deux actes d'une même strate ne partagent donc leur plus petit identifiant. Un état futur qui rendrait une désignation abrégée plurielle poserait la question avec la **référence complète** (le domaine) — la clause « refuse » de C7 (014 § 1 : « une question portant sur un acte inexistant… **seul cas** ») est inchangée : une désignation plurielle n'est pas un refus de C7, c'est une question incomplète du consommateur, qui se complète — et sa réalisation appartiendra à l'évolution de moteur qui rendra ces états atteignables, revalidée (016 § 5.1).

**Le volet logiciel imposé** : l'objet référence porte le domaine (l'égalité par valeur de séquence) ; la correspondance de τ se calcule sur la référence totale ; la résolution de C7 et les invocations d'audit du porteur et de la commande prennent la désignation abrégée (strate, plus petit identifiant) — leur forme actuelle, désormais nommée. Rien d'autre.

## 4. Vérifications

- **W₀ est inchangé** : la référence ne vit dans aucun acte de W (les actes portent leurs domaines, jamais leurs références) — seule la représentation logique de τ et la désignation d'audit sont touchées, et aucune τ réelle n'a jamais été émise (016 § 5.1) ;
- **aucun invariant n'est affaibli** : I39 est **renforcé** (tout acte est désignable par une référence désormais totale) ; I23 et EXG-30 reposent enfin sur une clé garantie ; I50 et I52 intacts (la référence reste un objet des contrats) ; I61–I67 hors de cause ;
- **les reports 3, 5 et 9 restent entièrement ouverts** : la forme **matérielle** de τ n'est pas spécifiée (report 3) ; l'identité des **états** d'Ω n'est pas touchée (report 5 — l'acte ne parle que de l'identité des actes de W) ; la cause et les continuités gardent leur régime (report 9 — la clé de la correspondance change, son calcul et son transport non).

---

## 5. Compatibilité avec les documents figés — démonstration

| Clause figée | Tension apparente | Résolution |
|---|---|---|
| 014 § 7.5 (« le couple (strate, plus petit identifiant du domaine) ») | la paire n'est pas totale | raffinement assumé, déclaré en en-tête : la référence rejoint l'identité de l'acte ; la paire demeure clé de tri et désignation abrégée. ∎ |
| 014 § 2 (l'identité de l'acte : (domaine, strate)) | — | confirmée et mobilisée : la référence redéfinie **est** cet objet — aucun objet nouveau. ∎ |
| 014 C5 / 011 § 4 (« exactement un acte » par domaine-strate) | — | c'est la démonstration de totalité : deux actes de même (strate, domaine) n'existent dans aucun W cohérent. ∎ |
| 014 § 7.3 (« jamais d'abréviation » ; la garde d'agrégation) | la paire abrégeait ; la garde contemple les recoupements | la référence complète s'aligne sur la règle des domaines ; la garde est la preuve que la collision était constructible — le constat du report 8, fondé. ∎ |
| 012 § 5 (la clé totale de l'ordre) | — | intacte : l'ordre canonique de W et le tri des références sont inchangés — la totalité exigée pour l'ordre est réalisée par l'énumération qui départage, celle exigée pour la référence l'est désormais par l'identité. ∎ |
| 014 § 1, C7 (« seul cas ») | une désignation plurielle semblait exiger un refus nouveau | aucun cas nouveau : la désignation abrégée est univoque sous la couverture courante (démontré, § 3) ; une pluralité future se résout par la référence complète, jamais par une erreur. ∎ |
| 018 § 3 (le contrat du porteur, invocation d'audit) | la forme de désignation change dans la surface | elle n'y était pas contractualisée (« une question du 011 § 7 sur un acte désigné d'un W désigné ») : la désignation abrégée la nomme sans raffiner le 018. ∎ |
| 006 § 7 / EXG-30 (la correspondance exhaustive) | reposaient sur une clé supposée | reposent désormais sur une clé garantie — renforcement, aucun changement de régime. ∎ |
| 021 et 022 (la carte des cessions ; l'espace trivial) | — | consommés tels quels : ce sont eux qui rendent l'univocité actuelle démontrable (un refus agrégé par strate ; aucun refus au contenu). ∎ |
| 016 § 4.2 (report 8) | — | le présent acte est son exécution intégrale : le report 8 est clos. ∎ |
| 016 § 4.2 (reports 3, 5, 9) et report 4 | — | non anticipés : forme matérielle, identité des états d'Ω, cause et continuités, cohérence de C6 — intacts (§ 4). ∎ |

---

## Conclusion

La référence d'acte cesse d'être une abréviation supposée unique : elle est l'identité de l'acte — (strate, domaine) — totale par la complétude que la série garantissait déjà ; la paire (strate, plus petit identifiant) garde ses deux rôles réels, le tri et la désignation abrégée de l'audit, cette dernière démontrée univoque sur tous les états de la couverture courante. τ, C7 et la commande `identity` reposent désormais sur une clé garantie, sans qu'un acte de W, un motif ou un contrat de couche n'ait changé. Le report 8 du 016 § 4.2 est **clos**, sous réserve du volet logiciel qui suit le présent acte. Sa validation relève de l'autorité du projet.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| le défaut | la paire (strate, plus petit identifiant) n'est totale que par la forme des états actuels — la garde d'agrégation du 014 § 7.3 rend la collision constructible (élection {1,2} / refus {1,3}) | 1–2 |
| la décision (Cas B) | la référence d'acte = l'identité de l'acte (014 § 2) : (strate, domaine énuméré et trié) — totale par « exactement un acte » (014 C5) ; tri inchangé | 3 |
| la désignation d'audit | la forme abrégée (strate, plus petit identifiant) — univoque sous la couverture courante (démontré via 021/022) ; pluralité future → référence complète, jamais une erreur nouvelle | 3 |
| le volet logiciel | l'objet référence porte le domaine ; la correspondance de τ sur la clé totale ; C7, porteur et commande sur la désignation abrégée — rien d'autre | 3 |
| l'invariance | W₀ intact, I39/I23/EXG-30 renforcés, reports 3/5/9 entièrement ouverts | 4 |
| compatibilité documentaire | onze clauses figées, chacune traitée nommément — aucune par silence | 5 |

**Ce que ce document ne fait volontairement pas** : modifier un document figé, changer un acte de W₀, définir l'identité d'un état d'Ω (report 5), la cause ou les continuités de τ (report 9), la forme canonique matérielle (report 3), la cohérence d'état de C6 (report 4), ajouter un cas de refus à C7, créer un invariant.
