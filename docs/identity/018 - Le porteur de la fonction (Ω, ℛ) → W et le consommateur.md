# 018 — Le porteur de la fonction (Ω, ℛ) → W et le consommateur

**Statut** : deuxième acte documentaire opérationnel de la phase v2 de la série `docs/identity/`. S'appuie sur les documents 000→016, figés, et sur le 017, validé (volet documentaire du report 1). Exécute le **volet documentaire du report 2** du 016 § 4.1 — « un composant unique portant les préconditions, les postconditions et le contrat d'erreur de la boîte noire (011) ; le consommateur prévu par 013 § 1.1 » —, par la voie 2 du 016 § 3 : le document précède le code ; à la date du présent acte, le porteur n'existe pas et le consommateur d'É9 n'a pas été livré tel que défini (016 § 4.1) ; le volet logiciel du report 2 (voie 3) reste à entreprendre après validation du présent document.
**Nature** : un document de **contrat** — il prolonge la couche 010→017. Il définit le porteur de la fonction (Ω, ℛ) → W : sa notion, son contrat, l'ordre des vérifications de l'invocation, son articulation avec les couches du 014, et le contrat du consommateur. Aucun code, aucun algorithme, aucune transition de ℛ, aucune architecture nouvelle.
**Raffinement assumé** : ce document ne raffine **aucune clause du 011** — il localise et compose, il ne modifie ni précondition, ni erreur, ni sortie, ni validation. Il porte un **complément assumé du 014 § 1 (C1)** : la clause « refuse » de C1 énumère trois cas sans les ordonner ; le § 4 fixe leur ordre de précédence — aucun cas n'est ajouté ni retiré. Il **exerce** la délégation du 017 § 8 (« l'ordre entre les vérifications de Ω et celles de ℛ … relève du contrat du porteur de la frontière ») sans la redéfinir. La définition du porteur (§ 2), son contrat (§ 3), les ordres de vérification (§ 4) et le contrat du consommateur (§ 6) sont le mécanisme que ces points instituent ; les §§ 5, 7 et 8 n'énoncent que des conséquences démontrées de clauses figées ; hors ces points, leur mécanisme et les invariants I65–I67 (§ 10), aucun contenu normatif nouveau.
**Périmètre — exclusions constitutives** : rien de ce document n'anticipe les reports ouverts du 016 § 4 — ni la sérialisation canonique matérielle de W (report 3 : elle est référencée, jamais spécifiée), ni la vérification de cohérence d'état de C6 (report 4), ni la définition de l'identité d'un état d'Ω (report 5 : l'index est convoyé, jamais défini), ni la vérification de la cause de τ (report 9 : τ est transporté sous le régime actuel du 014 § 7.5) ; rien ne décrit d'architecture au-delà des placements déjà actés par le 013 §§ 1–2 ; rien n'entre dans le registre ; rien ne définit l'interface concrète de la CLI (drapeaux, formats d'affichage, codes de sortie — implémentation) ; aucun code.

---

## 1. Le défaut contractuel

Le constat de clôture de la v1 (016 § 4.1, report 2) : « la fonction (Ω, ℛ) → W (EXG-01) n'a pas de porteur : la composition C1→C6 n'existe que dans les tests ; le consommateur CLI (jalon É9 du 013 § 8) n'a pas été livré tel que défini ». Ce défaut n'est pas un manque de confort — c'est une **non-conformité structurelle**, en trois conséquences :

- **le contrat public n'a pas de frontière réalisée** : les préconditions du 011 § 4 (quatre sur ℛ depuis le 017 § 4), la postcondition « entier ou absent » et le contrat d'erreur à sept entrées (017 § 6) sont des clauses de la boîte noire — or aucun composant ne *tient* cette frontière : chaque clause a son siège interne (C1, C2, C6), aucune n'a de garant public ;
- **le contrat est inexerçable hors validation** : le 011 § 11 interdit d'adresser une partie interne (« aucune partie n'est adressable de l'extérieur », I37) ; si la composition n'existe que dans les tests, un consommateur ne peut obtenir W par aucune voie licite — le moteur v1 est une boîte noire sans porte ;
- **le quatrième acteur n'existe pas** : les quatre rôles du 011 § 1 sont disjoints par contrat, mais le rôle « consommateur » n'a aucun titulaire livré — le jalon É9 (013 § 8) le définissait, il n'a pas été tenu.

Le défaut ne touche ni la théorie ni les contrats internes — les couches C1→C7 existent et sont conformes. Il manque la **réalisation de la frontière** : c'est elle que ce document contractualise.

---

## 2. Le porteur, objet contractuel

**Définition 1 (porteur)** — Le *porteur* est le composant du moteur qui réalise EXG-01 : il compose les couches C1→C6 derrière la frontière du 011, expose les trois invocations contractuelles (dérivation, transition, audit — cette dernière servie par C7, placée derrière la même frontière) et tient, à cette frontière, les préconditions, les postconditions et le contrat d'erreur de la boîte noire. Il appartient au moteur pur — il consomme les deux ports du 013 § 1.1 (« source d'observations », « source de registre »), jamais une représentation.

Trois propriétés définitoires :

- **il ne dérive rien** — aucun signal, aucune hypothèse, aucun acte, aucun objet : toute dérivation appartient aux couches C3→C6 (012 § 1), et le porteur n'est pas une couche — il est la composition. Un porteur qui produirait un objet propre serait une couche cachée, hors des contrats du 014 § 1 (I66, § 10) ;
- **il ne vérifie rien en propre** — les vérifications d'Ω appartiennent à C1, celles de ℛ — couverture comprise — à C2 (014 § 1 ; 017 § 5) ; en aval, tout objet consommé est valide par construction (I51) : toute re-vérification serait redondante par construction. Ce que le porteur possède en propre est exactement double : **l'ordre d'invocation** (§ 4), donc le signalement ; et **la garantie « entier ou absent » à la frontière publique** — réalisée par C6 (012 § 2 : « la postcondition "entier ou absent" (011 § 4) est la sienne ») et par l'absence de toute émission sur échec amont ;
- **il ne fuit rien** — le porteur est la frontière même que le 011 § 11 prévoit (« le moteur peut être composé de parties internes — le contrat s'applique à la frontière, et rien de la composition interne ne fuit (I37) ») : aucune partie n'est adressable à travers lui, aucun état intermédiaire n'est exposé autrement que par les chaînes d'audit.

---

## 3. Le contrat du porteur

Par référence aux contrats figés — aucune clause n'est dupliquée, chacune est localisée :

| Clause | Contenu |
|---|---|
| **reçoit** | pour la **dérivation** : les deux identifications — celle d'un support d'observations (le « reçoit » de C1, 014 § 1) et celle d'un répertoire de registre (le « reçoit » de C2, 014 § 1) — et **rien d'autre** : aucune option, aucun réglage (EXG-02 : « toute option de configuration modifiant le résultat » est interdite ; 011 § 2.2 : « Il n'existe aucune troisième entrée »). Pour la **transition** : deux index dont il reçoit les deux membres (011 § 3), accompagnés de la cause — dont le 014 § 7.5 définit la forme (type et détail), et que, dans le régime actuel, l'appelant fournit sans vérification (constat du 016 § 4.2, report 9) — transportée telle quelle, sans vérification ni définition : sa dérivation et sa vérification relèvent du report 9. Pour l'**audit** : une question du 011 § 7 sur un acte désigné d'un W désigné (le « reçoit » de C7, 014 § 1) |
| **produit** | W ; τ ; les réponses d'audit unité par unité — les trois sorties du 011 § 3, jamais une quatrième (I52) |
| **garantit** | les préconditions du 011 § 4, telles que raffinées par le 017 § 4, **portées par référence** : vérifiées là où les 014 et 017 les vérifient (C1 pour Ω, C2 pour ℛ), jamais re-vérifiées (I51) — pour la transition, elles valent pour chacun des deux index ; « **entier ou absent** » à la frontière publique ; le **déterminisme** du 011 § 6 — la composition de fonctions pures est une fonction pure (I43) |
| **refuse** | les **sept erreurs** du contrat (011 § 5, lu sur la table à sept entrées du 017 § 6), surfacées **telles quelles** depuis C1 et C2, dans l'ordre du § 4 — jamais renommées, jamais agrégées, jamais converties ; le cas propre de C7 (question sur un acte inexistant dans le W désigné — 014 § 1), surfacé au même régime ; toute défaillance interne signalée **comme telle** (011 § 4 : « elle doit se signaler comme telle et respecter la postcondition "entier ou absent" »), jamais déguisée en erreur contractuelle |
| **ignore** | le contenu de tout objet qu'il achemine — il compose, il ne lit pas (§ 5) ; la représentation des entrées — les ports du 013 § 1.1 la lui masquent (I46) |

---

## 4. L'ordre des vérifications de l'invocation

> **Ordre de précédence** — Les vérifications de l'invocation s'établissent dans cet ordre total, et le signalement est celui du **premier échec** : **Ω absent** < **Ω incompatible** < **Ω invalide** < **registre absent** < **registre malformé** < **registre incohérent** < **registre non couvert**.

Trois pièces, chacune à son régime :

- **l'ordre inter-entrées — Ω puis ℛ, en deux blocs** : aucune bonne fondation ne force ce choix (les vérifications des deux entrées sont indépendantes) — il est **normatif et assumé**, du même statut que le dernier étage du 017 § 8, et fixé une fois pour toutes au bénéfice du déterminisme. Trois ancres convergentes le fondent : la signature d'EXG-01 est **(Ω, ℛ)** ; le 011 § 4 énonce les préconditions d'Ω avant celles de ℛ ; la numérotation du 014 place C1 avant C2 ;
- **l'ordre intra-Ω — complément assumé du 014 § 1 (C1), démontré par bonne fondation** : l'absence se constate sans rien d'autre ; la compatibilité se lit sur les déclarations de version — « le support déclare la version de contrat qu'il honore ; C1 déclare celles qu'il supporte » (014 § 6) — et ne présuppose que la présence ; la validité, elle, est la conformité au contrat du § 6 du 014, **contrat versionné** : vérifier l'invariant 1:1 ou la structure des actes sous une version non supportée reviendrait à valider contre un contrat que le moteur ne connaît pas. La validité n'est donc décidable que sous une version supportée : **absent < incompatible < invalide**, ordre forcé. Là où le 017 § 8 pouvait choisir (la couverture était décidable dès la forme), la fondation force ici — les deux documents appliquent le même principe : la bonne fondation là où elle existe, le choix normatif assumé sinon. La clause « refuse » de C1 énumérait ces trois cas sans les ordonner : le présent ordre est un complément, jamais une correction ;
- **l'ordre intra-ℛ — celui du 017 § 8, repris tel quel** : absence < forme < cohérence < couverture, avec ses deux justifications (bonne fondation pour les trois premiers étages, attribution pour l'ordre entier). Rien n'y est ajouté ni retranché.

Pour la **transition**, les vérifications s'appliquent aux deux index membre par membre, dans l'ordre des sections du 014 § 7.5 : l'index avant, puis l'index après.

Cet ordre rend le signalement des échecs de l'invocation **déterministe et total** (I67, § 10) : une invocation cumulant des défauts sur les deux entrées produit toujours la même erreur, sur toute machine, à tout instant — le déterminisme du 011 § 6, porté à la frontière entière.

---

## 5. L'articulation avec les couches

**Le porteur réalise la table du 014 § 3 — il n'y ajoute aucun arc.** Démonstration : toute traversée **inter-couches** que le porteur opère — le modèle d'observations de C1 vers C3, les conventions de C2 vers C3, C4 et C5, les signaux de C3 vers C4, les hypothèses de C4 vers C5, les actes de C5 vers C6, l'identité de l'état de ℛ de C2 vers C6, W et τ de C6 vers C7, le canal de contexte de C1 vers C7 — **est une ligne de la table** du 014 § 3. Le porteur n'en consomme aucun contenu (I66) : il n'est pas une extrémité d'arc, il est le milieu qui transporte. Un arc nouveau exigerait qu'un objet d'une couche parvienne à une couche que la table ne désigne pas — « toute traversée hors tableau est un défaut de conception » (014 § 3) — ou que le porteur consomme l'objet lui-même, ce que la Définition 1 exclut. Une fourniture n'est pas de cette espèce : l'identité de l'état d'Ω, que la clause « reçoit » de C6 déclare comme entrée (014 § 1) sans qu'aucune ligne de la table ne la porte — honorer une clause « reçoit » déclarée n'est pas traverser une frontière hors table ; son régime est au troisième alinéa ci-dessous. ∎

**Le siège de la couverture est inchangé.** La vérification de couverture appartient à C2 (017 § 5) ; le comportement du porteur est exactement celui que le 017 § 5 décrivait par anticipation : « il invoque C2, qui échoue nommément ».

**L'index est convoyé, jamais défini.** La clause « reçoit » de C6 (014 § 1) déclare « l'ensemble des actes (de C5) + l'index (identité de l'état d'Ω, identité de l'état de ℛ) » : le porteur fournit cette clause. L'identité de l'état de ℛ circule par la ligne C2 → C6 de la table ; l'identité de l'état d'Ω est transmise selon le régime actuel du 014 § 7.2 — dont les défauts connus (016 § 4.2, report 5) se corrigent par l'acte du report 5, jamais ici : le présent document assigne le **convoyage**, jamais le **calcul**.

---

## 6. Le consommateur

**Définition 2 (consommateur)** — Le *consommateur* du report 2 est la commande `identity` de la CLI existante, prévue par le 013 § 1.1 (« un consommateur (011 § 1) : invoque le moteur, restitue W et l'audit » — « jamais fusionné avec le moteur ») et définie comme jalon É9 (013 § 8). Son contrat :

| Clause | Contenu |
|---|---|
| **reçoit** | de l'opérateur — l'humain qui invoque la commande, et qui n'agit qu'à travers elle : la désignation d'un index (les deux identifications du § 3) et, le cas échéant, une demande de transition (deux index et la cause — forme du 014 § 7.5 ; fournie par l'appelant dans le régime actuel, constat du 016 § 4.2, report 9) ou une question d'audit |
| **fait** | invoque le porteur — et lui seul (I65) ; émet W **tel que produit**, sous la forme canonique du 013 § 4 (sérialisation matérielle : report 3) ; restitue les réponses d'audit unité par unité ; restitue toute erreur **nommée telle quelle** — le patron de la restitution des motifs (014 § 7.4, confirmé par les corrections de clôture, 016 § 1.1) : jamais traduite, jamais dégradée, jamais convertie en résultat |
| **ne fait jamais** | influencer W — « rien de ce qu'un consommateur fait n'influence W » (EXG-16) ; comparer directement deux W — « toute comparaison directe par un consommateur est un contresens dont le contrat ne répond pas » (011 § 9) : τ est la seule comparaison ; compléter, filtrer, réordonner ou annoter un W ; réessayer une invocation en modifiant une entrée ; écrire dans Ω ou dans ℛ — « les consommateurs n'écrivent rien » (011 § 11) |

Avec ce contrat, les **quatre rôles du 011 § 1 deviennent tous opérants** : le producteur (le pipeline figé), la gouvernance (le registre), le moteur (le porteur et ses couches), le consommateur (la commande `identity`) — quatre titulaires, aucun cumul.

---

## 7. Production, validation, consommation

Trois régimes d'accès au moteur, strictement séparés :

- **la production** : pour tout consommateur, elle passe exclusivement par le porteur (I65, § 10) — c'est la conséquence directe du 011 § 11, pas une règle nouvelle ;
- **la validation** : elle relève d'un régime distinct et préexistant — la validation par morceaux du 012 § 8 et les cinq niveaux du 013 § 6. Les tests adressent les couches isolément ; c'est légitime, nécessaire, et **ce n'est pas une consommation** : les tests ne sont pas un acteur du 011 § 1 — ils sont l'instrument de la déclaration de conformité (011 § 8) ;
- **la consommation** : lecture des sorties contractuelles, sans influence (EXG-16), par le seul contrat public.

Le défaut de la v1 se requalifie alors exactement : le tort n'était pas que les tests composent les couches — c'est le régime du 012 § 8 — mais que la composition n'existât **que là** : la frontière publique n'était pas réalisée, et le contrat du 011 était structurellement inexerçable hors validation. Le porteur résorbe ce défaut sans toucher au régime de la validation.

---

## 8. La validation et la déclaration de conformité

- **Le porteur est le siège d'exercice de la conformité.** Les quatre points du 011 § 8 s'exercent désormais à la frontière publique : W₀ est produit **par le porteur** depuis (Ω_corpus1, ℛ₀) (point 1) ; la batterie minimale s'exécute à travers lui (point 2) ; le contrat d'audit s'exerce par ses invocations (point 3) ; chaque erreur est provoquée à la frontière publique (point 4, sous la borne du 017 § 10, inchangée). C'est une application du contrat de validation, jamais une extension : aucune clause du 011 § 8 ne change.
- **Le déterminisme du signalement est testable dans le régime existant.** I67 est vérifiable par les moyens du 011 § 8 point 4 : des entrées construites cumulant des défauts sur les deux entrées doivent produire l'erreur du premier échec de l'ordre du § 4 — des cas de la même espèce que ceux que la batterie construit déjà, aucune clause nouvelle.
- **La déclaration de conformité ne gagne aucune composante.** Le porteur appartient à la version de moteur que la déclaration couvre (011 § 8 : la conformité est déclarée par version de moteur, et le 017 § 3 y a inscrit la couverture) ; l'ordre du § 4 est fixé par le présent contrat, pas par version — il n'y a rien à déclarer, seulement à honorer.
- **Le consommateur a son jalon.** É9 (013 § 8) reste sa définition de livraison : « bout-en-bout sur l'artefact ; la CLI du pipeline reste intacte ». Sa livraison relève du volet logiciel du report 2.

---

## 9. Compatibilité avec les documents figés — démonstration

| Clause figée | Tension apparente | Résolution |
|---|---|---|
| EXG-01 | la fonction n'avait pas de porteur | le porteur la réalise sans la redéfinir : même signature, mêmes deux entrées, même sortie (§ 2). ∎ |
| EXG-02 et 011 § 2.2 | le porteur pourrait sembler introduire des paramètres | il reçoit exactement les deux identifications, plus les demandes de transition et d'audit du 011 §§ 3 et 7 — aucune option, aucun réglage (§ 3). ∎ |
| 011 § 4 | les préconditions pourraient sembler dupliquées ou déplacées | portées par référence, vérifiées là où les 014/017 les vérifient (C1, C2), jamais re-vérifiées (I51) ; « entier ou absent » réalisé par C6 (012 § 2) et garanti à la frontière — aucune duplication (§§ 2–3). ∎ |
| 011 § 5 et 017 § 6 | une hiérarchie d'erreurs nouvelle pourrait sembler naître | aucune erreur nouvelle, aucune retirée : les sept erreurs existantes, surfacées telles quelles ; seul l'ordre de signalement est fixé (§ 4). ∎ |
| 011 § 9 | les W passés et leur comparaison | la lettre du 011 § 9 devient une clause opposable du consommateur (§ 6) : τ est la seule comparaison. ∎ |
| 011 § 11 et I37 | le porteur pourrait sembler exposer la composition | il est la frontière même que le 011 § 11 prévoit — aucune partie adressable à travers lui, rien ne fuit ; I65 en découle (§ 10). ∎ |
| I40 (011 § 12) | le contrat du porteur pourrait sembler lier le 011 à une implémentation | le porteur est défini par son seul contrat (§ 3), sans technologie, sans structure interne ; son placement se **dérive** du 013 § 1.1 — `Identity` matérialise « la boîte noire du 011 », et le porteur, frontière réalisée de cette boîte, y appartient donc — jamais d'un choix technique. ∎ |
| 012 § 3 et I44 | un composant nouveau pourrait sembler un nœud nouveau du graphe | le porteur n'est pas une couche : il ne consomme ni ne produit aucun objet de dérivation (I66) — le graphe des « a le droit de connaître » est inchangé, son acyclicité aussi. ∎ |
| 014 § 1 (totalité ; C1) | l'ordre des refus de C1 n'était pas fixé | complément assumé, déclaré en en-tête : la clause « refuse » de C1 énumère, le § 4 ordonne — aucun cas ajouté ni retiré, la totalité du contrat est intacte. ∎ |
| 014 § 3 (« aucun canal hors table ») | le porteur pourrait sembler un canal nouveau | toute traversée inter-couches du porteur est une ligne de la table — aucun arc nouveau ; la fourniture de l'identité de l'état d'Ω à C6 est d'une autre espèce (entrée déclarée par la clause « reçoit » de C6, 014 § 1) — démontré au § 5. ∎ |
| I51 (014 § 12) | le porteur pourrait re-vérifier les objets | il ne vérifie rien en propre (§ 2) : I51 rend toute re-vérification superflue par construction. ∎ |
| 013 §§ 1 et 8 | le placement du porteur et le consommateur | le placement du porteur se dérive du 013 § 1.1 (il appartient à la boîte noire que `Identity` matérialise et consomme les ports qui y vivent) ; le consommateur est repris tel quel : la commande `identity` du jalon É9, « jamais fusionné avec le moteur ». ∎ |
| 016 § 4.1 (report 2) | — | le présent acte est son volet documentaire : le composant unique est contractualisé (§§ 2–5), le consommateur défini (§ 6). ∎ |
| 016 § 4.2 (reports 5 et 9) et reports 3 et 4 | le porteur touche leurs objets | frontières tenues : la sérialisation est référencée sans être spécifiée (report 3) ; la vérification de cohérence d'état reste celle de C6 (report 4) ; l'index est convoyé sans être défini (report 5, § 5) ; τ est transporté sous le régime actuel du 014 § 7.5 (report 9). ∎ |
| 017 §§ 5 et 8 | le siège de la couverture ; la délégation de l'ordre | le siège C2 est inchangé (« il invoque C2, qui échoue nommément ») ; la délégation du 017 § 8 est exercée, jamais redéfinie — l'ordre intra-ℛ est repris tel quel, l'ordre inter-entrées le complète sans le toucher (§ 4). ∎ |

---

## 10. Invariants — démontrés

> **I65 — Toute production de W pour un consommateur passe par le porteur.**
> *Démonstration.* Les dépendances autorisées du 011 § 11 ne donnent aux consommateurs qu'un seul accès : « consommateurs → contrat public du moteur » ; le porteur est la réalisation de ce contrat (§§ 2–3). Toute autre voie exigerait soit d'adresser une partie interne — ce que le 011 § 11 exclut (« aucune partie n'est adressable de l'extérieur ») —, soit un canal que les contrats du 014 § 3 ne contiennent pas (« toute traversée hors tableau est un défaut de conception »). Et par I37, un W obtenu hors du contrat ne serait de toute façon pas une sortie du moteur : « ce qui ne passe pas par le contrat n'existe pas ». La validation n'y fait pas exception : elle relève d'un régime distinct (012 § 8) et n'est pas un consommateur (§ 7). ∎

> **I66 — Le porteur ne produit aucun objet propre.**
> *Démonstration.* Les sorties du moteur sont exhaustivement W, τ, les réponses d'audit et les erreurs nommées (011 §§ 3 et 5 ; I52), et chacune possède sa couche productrice au 014 § 1 : C6 pour W et τ, C7 pour les réponses et pour son unique cas de refus nommé (014 § 1), C1 et C2 pour les erreurs de frontière. Un objet produit par le porteur lui-même serait donc soit une sortie sans couche productrice — inexprimable dans les objets des contrats, ce qu'I52 rend non conforme —, soit un objet interne franchissant une frontière hors table (I50, 014 § 3). L'interdiction d'inventer du 012 § 2 (« tout élément de sortie sans antécédent d'entrée est une fabrication — interdite à tout étage ») s'applique au porteur comme à toute partie du moteur : il achemine et il ordonne ; il ne fabrique rien. ∎

> **I67 — Le signalement des échecs de l'invocation est déterministe et total.**
> *Démonstration.* Chacune des sept vérifications du § 4 est une fonction déterministe de ses seules entrées : les trois de la frontière Ω du couple (Ω, versions de contrat supportées — déclarées par version de moteur, 011 § 9) ; les quatre de la frontière ℛ du couple (ℛ, couverture déclarée), fixé par version (I63), avec le déterminisme déjà établi par I64. L'ordre du § 4 étant total sur les sept, le « premier échec » est unique pour tout triplet (Ω, ℛ, version de moteur) : mêmes entrées, même version ⟹ même erreur, sur toute machine, à tout instant. C'est l'argument d'I64 — « des fonctions pures composées dans un ordre fixé » — porté des quatre vérifications de la frontière registre aux sept vérifications de la frontière entière ; le résultat d'I64 en est la restriction à ℛ, intacte. ∎

---

## Conclusion

La frontière est réalisée : un porteur (qui compose C1→C6, ne dérive rien, ne vérifie rien en propre, ne fuit rien), un contrat par référence (les deux identifications et rien d'autre ; les trois sorties et rien d'autre ; les sept erreurs telles quelles), un ordre total des vérifications de l'invocation (Ω puis ℛ ; absent < incompatible < invalide, démontré ; l'ordre du 017 § 8 repris tel quel), une articulation démontrée avec les frontières du 014 (aucun arc nouveau), un consommateur contractualisé (la commande `identity` d'É9, sans influence et sans comparaison directe), la séparation opérante des trois régimes (production, validation, consommation), et trois invariants démontrés. Le contrat public du 011 devient exerçable par son quatrième acteur — sans qu'aucune de ses clauses n'ait changé.

Conformément au 016 § 3, **le présent acte précède tout code** : le porteur n'existe pas, et le consommateur d'É9 reste à livrer tel que défini. Sa validation relève de l'autorité du projet ; après elle, le report 2 disposera de son fondement documentaire complet, et le volet logiciel — le porteur, puis É9 — pourra être entrepris sans liberté normative résiduelle, conjointement au volet moteur du report 1 dont il est le véhicule naturel.

---

## Récapitulatif

| Objet | Définition | § |
|---|---|---|
| défaut contractuel | EXG-01 sans porteur : frontière non réalisée, contrat inexerçable hors validation (011 § 11), quatrième acteur sans titulaire (É9 non livré) | 1 |
| porteur | le composant qui réalise EXG-01 : compose C1→C6 derrière la frontière du 011, consomme les ports du 013 § 1.1 ; ne dérive rien, ne vérifie rien en propre, ne fuit rien | 2 |
| contrat du porteur | reçoit, pour la dérivation, les deux identifications et rien d'autre (EXG-02) — pour la transition et l'audit, les demandes du 011 §§ 3 et 7 ; produit les trois sorties du 011 § 3 ; garantit les préconditions par référence (I51) et « entier ou absent » ; refuse les sept erreurs telles quelles | 3 |
| ordre des vérifications | Ω puis ℛ (normatif assumé, trois ancres) ; intra-Ω : absent < incompatible < invalide (bonne fondation, complément assumé du 014 § 1 C1) ; intra-ℛ : 017 § 8 tel quel ; premier échec signalé | 4 |
| articulation | le porteur réalise la table du 014 § 3, aucun arc nouveau (démontré) ; siège C2 inchangé (017 § 5) ; l'index convoyé, jamais défini (report 5) | 5 |
| consommateur | la commande `identity` (013 § 1.1, É9) : invoque le porteur seul, émet et restitue tel quel, n'influence jamais (EXG-16), ne compare jamais deux W (011 § 9) | 6 |
| séparation des régimes | production par le porteur (I65) ; validation par morceaux (012 § 8), qui n'est pas une consommation ; consommation sans influence | 7 |
| validation | le porteur, siège d'exercice des quatre points du 011 § 8 ; I67 testable dans le régime existant ; aucune composante nouvelle de la déclaration de conformité | 8 |
| compatibilité documentaire | quinze clauses figées, chacune traitée nommément — aucune par silence | 9 |
| invariants | I65 production par le porteur seul, I66 aucun objet propre, I67 signalement déterministe et total | 10 |

**Ce que ce document ne fait volontairement pas** : spécifier la sérialisation canonique matérielle de W (report 3), toucher à la vérification de cohérence d'état de C6 (report 4), définir l'identité d'un état d'Ω (report 5), vérifier ou définir la cause de τ (report 9), rendre une famille applicable ou effectuer une transition de ℛ, décrire une architecture au-delà des placements du 013, définir l'interface concrète de la CLI (drapeaux, formats, codes de sortie), créer un report, écrire ou spécifier du code.
