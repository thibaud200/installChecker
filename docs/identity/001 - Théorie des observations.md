# 001 — Théorie des observations

**Statut** : deuxième document de la série `docs/identity/`. S'appuie sur le document 000 (« Fondements de l'identité »), validé et figé.
**Périmètre** : définition rigoureuse de l'observation, objet premier du moteur. Aucun algorithme, aucune pondération, aucun score, aucune règle métier, aucune décision de priorité.
**Position par rapport au 000** : ce document **raffine** le 000 sans le contredire. Le 000 avait défini l'observation globalement (Déf. 2) et esquissé le signal (Déf. 3, § 2.4) ; le présent document descend au grain élémentaire de l'observation, insère explicitement la couche *interprétation* dans la chaîne, et re-fonde au bon niveau les états que le 000 § 2.4 avait anticipés au niveau du signal. **Les signaux n'existent pas encore** : ils seront construits dans le document 002.

---

## 1. L'observation, objet premier

### 1.1 Définitions formelles

**Définition 1 (acte d'observation)** — L'exécution complète du pipeline sur un fichier, identifiée par un `observation_id`. Un acte produit une valeur (éventuellement ⊥) pour **chaque** attribut de **chaque** capacité (invariant 1:1 du pipeline). L'acte est daté et localisé (`scanned_at`, `path`) ; ces coordonnées appartiennent à l'acte, pas au contenu (axiome A1 du 000).

**Définition 2 (observation élémentaire)** — Le triplet

  o = ( ω, a, v )

où ω est un acte d'observation, a un **attribut** (SHA256, taille, `magic_hex`, `machine`, `product_name`, `subject`, `upgrade_code`, …) appartenant au répertoire d'une capacité, et v ∈ Val(a) ∪ {⊥} la valeur lue. C'est l'atome du système : tout ce que le moteur manipulera est composé d'observations élémentaires.

**Définition 3 (observation complète)** — La famille de toutes les observations élémentaires d'un même acte ω. C'est l'« observation » au sens du 000 (Déf. 2). On note toujours Ω l'ensemble persisté.

**Définition 4 (répertoire d'attributs)** — L'ensemble 𝒜 des attributs existants. 𝒜 est **ouvert** : toute capacité future l'étend. Chaque attribut possède un domaine de valeurs Val(a) défini par l'API d'extraction, pas par le moteur.

### 1.2 Une observation n'a aucune interprétation

Une observation élémentaire ne *signifie* rien : elle *rapporte*. `machine = '8664'` ne dit pas « application 64 bits » ; il dit : *l'extracteur PE, appliqué à ce contenu, a retourné la chaîne `8664` pour l'attribut machine*. Toute lecture au-delà de ce rapport appartient aux couches supérieures (§ 5).

### 1.3 La vérité d'une observation

Le contenu propositionnel d'une observation est : **« la méthode d'extraction M, appliquée à ce contenu, a retourné v »** — et non « ce contenu *est* v ».

Cette distinction (vérité *de la lecture* vs vérité *de la chose*) fonde tout le reste :

- une observation est **infailliblement vraie** en tant que rapport de lecture — le pipeline étant reproductible (mesuré au corpus 1 : double run, dumps identiques), le rapport est vérifiable ;
- une observation peut néanmoins **induire en erreur** si on la lit comme une vérité de la chose : les 20 lignes `machine='4b50'` du corpus 1 sont des rapports exacts (l'API a réellement retourné cela sur des ZIP) et des indices faux (« c'est un PE » serait erroné).

D'où la formule directrice du prompt de campagne, érigée ici en principe : *les observations sont fidèles à ce qui est lu, même lorsqu'elles peuvent conduire à des interprétations trompeuses*. L'erreur n'existe jamais au niveau de l'observation ; elle naît toujours au niveau d'une lecture.

---

## 2. Propriétés constitutives d'une observation

Chaque observation élémentaire possède, par construction du pipeline, les propriétés suivantes. Elles sont **constitutives** : un fait qui ne les aurait pas ne serait pas une observation au sens du système.

| Propriété | Définition | Ancrage |
|---|---|---|
| **Provenance** | Toute observation référence son acte (`observation_id`), donc sa capacité et son attribut. Il n'existe pas d'observation anonyme. | schéma : chaque table capacité porte `observation_id` |
| **Méthode d'extraction** | La valeur est celle retournée par une méthode identifiée et documentée (API, P/Invoke), appliquée par une capacité **autonome** qui ouvre elle-même le contenu. | ADR-003, ADR-005, ADR-006 |
| **Reproductibilité** | Le même contenu, relu par la même méthode, produit la même valeur. | corpus 1 : double run, 7 tables identiques |
| **Stabilité** | La valeur ne dépend ni de l'instant, ni du chemin, ni de l'ordre du scan. *Réserve théorique* : certaines méthodes déclaratives peuvent dépendre de l'environnement d'exécution (ressources multilingues lues selon la locale) — la stabilité est celle de la méthode, elle devra être qualifiée méthode par méthode (renvoi 002). | A1 ; réserve à instruire |
| **Domaine de valeurs** | Val(a) est fixé par la méthode, jamais par le moteur : chaînes libres (déclaratif), codes finis (structurel), condensats (physique). L'égalité sur Val(a) est l'égalité **byte-à-byte** — toute équivalence plus souple (casse, encodage, espaces) est une interprétation, pas une propriété du domaine. | philosophie « valeurs brutes » |
| **Possibilité d'absence** | ⊥ appartient à tout domaine. L'absence est une donnée de plein droit, produite par une lecture nominale qui n'a rien trouvé. Elle ne prouve rien (ni « non signé », ni « pas un PE »). | 88,5 % sans VersionInfo au corpus 1 |
| **Possibilité de contradiction** | Deux observations peuvent être incompatibles sous une lecture d'origine commune, **sans qu'aucune soit fausse** (§ 4.4). | définition § 4.4 |
| **Indépendance** | Aucune observation ne dépend d'une autre : chaque capacité ouvre le contenu elle-même et ignore les résultats des autres. Les corrélations constatées entre observations sont des faits du monde, jamais des artefacts de couplage du pipeline. | ADR-003 ; « aucune capacité ne lit une autre capacité » |

---

## 3. Typologie des observations

La typologie classe les **attributs** (non les capacités : une même capacité peut produire des attributs de types différents). Elle est fondée sur **ce qui garantit la valeur** — critère indépendant du code actuel, donc stable sous l'ajout de capacités futures.

**Type P — Observations physiques.** La valeur est une fonction mathématique des octets du contenu, calculable par quiconque, sans convention d'aucune sorte. *Garant : les mathématiques.* Exemples : `size`, `sha256`. Propriété clé : incontestables et incontrefaisables (modulo L7 du 000) ; ce sont les seules observations dont la lecture « de la chose » coïncide avec la lecture « du rapport ».

**Type S — Observations structurelles.** La valeur décrit la conformité des octets à une **convention de format** (en-têtes, magic numbers, organisation interne). *Garant : une spécification de format.* Exemples : `magic_hex`, `container`, `machine`, `subsystem`, `optional_header_magic`, l'existence d'une entrée nommée dans une archive. Propriété clé : exactes en tant que lectures, mais le format peut être imité ou accidentel — c'est le terreau des artefacts (§ 4.5) : les octets « PK » relus comme champ COFF `4b50` sont une lecture structurelle appliquée hors de son domaine nominal.

**Type D — Observations déclaratives.** La valeur a été **écrite intentionnellement par le producteur du contenu pour être lue** : chaînes de VersionInfo, propriétés MSI (`ProductName`, `UpgradeCode`, …), attributs du manifeste AppX. *Garant : la sincérité du producteur — c'est-à-dire rien.* Propriété clé : librement absentes, erronées, mensongères, périmées ; potentiellement très riches. Le corpus 1 en montre le régime réel : absentes à 88,5 % (VersionInfo), mais complètes à 100 % quand la table Property d'un MSI est présente.

**Type C — Observations cryptographiques.** La valeur résulte d'un **engagement vérifiable mathématiquement**, dont la production exige un secret (clé privée). *Garant : la cryptographie et l'infrastructure de confiance.* Exemples : `subject`, `issuer`, `thumbprint`, `serial_number`, bornes de validité. Propriété clé : coûteuses à contrefaire ; mais elles prouvent *l'acte de signer ce contenu*, jamais *ce qu'est* le contenu (000 § 3.2) — au corpus 1, 192 contenus distincts partagent le même sujet signataire.

**Type X — Observations contextuelles.** La valeur décrit **l'acte d'observation lui-même**, pas le contenu : `path`, `scanned_at`. Ce sont des observations à part entière (persistées, vraies, reproductibles en tant que rapports) — mais l'axiome A1 les exclut du domaine identitaire. La typologie doit les nommer précisément pour pouvoir les exclure proprement.

Remarques transversales :

- La typologie est une **partition des attributs** : chaque attribut a exactement un type.
- Elle ne préjuge d'aucune hiérarchie de confiance : dire « C est plus fiable que D » est une **qualification**, objet du document 002 — pas une propriété du type.
- Toute capacité future devra ranger ses attributs dans P/S/D/C/X, ou motiver l'introduction d'un type nouveau (révision explicite du présent document).

---

## 4. Les états d'une observation

Cinq états sont à définir : *présente, absente, illisible, contradictoire, artefactuelle*. Leur rigueur exige une distinction préalable :

> Les trois premiers sont des **états intrinsèques** — déterminés par l'acte de lecture seul. Les deux derniers sont des **statuts relationnels** — ils ne peuvent être établis qu'en confrontant l'observation à d'autres observations ou à une attente, c'est-à-dire par une couche supérieure. Une observation isolée ne peut être ni contradictoire ni artefactuelle *en soi*.

### 4.1 Présente

v ≠ ⊥ : la méthode a retourné une valeur. Rien de plus n'est affirmé — ni exactitude « de la chose », ni pertinence.

### 4.2 Absente

v = ⊥ à l'issue d'une **lecture nominale** : la méthode s'est appliquée normalement et n'a rien trouvé pour cet attribut (pas de ressource VersionInfo, pas de table Property, pas d'en-tête optionnel). L'absence est une information (« la lecture n'a rien produit »), jamais une preuve (« il n'y a rien ») : une signature par catalogue existe hors du fichier, un PE sans en-tête optionnel reste un objet COFF valide.

### 4.3 Illisible

La **lecture elle-même a échoué** : contenu inaccessible, flux corrompu interrompant la méthode, ressource verrouillée. Théoriquement distinct de l'absence : « je n'ai rien trouvé » ≠ « je n'ai pas pu regarder ».

**Point de rigueur assumé** : le modèle persisté actuel **projette l'illisible** — au niveau du fichier entier, sur l'inexistence d'acte (le fichier est signalé sur stderr, aucune ligne en base : décision actée et explicitement maintenue) ; au niveau d'une capacité, sur ⊥ (un ZIP corrompu produit la même ligne toute-⊥ qu'un non-ZIP). La théorie conserve la distinction ; le plan des données la perd. Conséquence pour les couches supérieures : **⊥ recouvre deux états théoriques distincts**, et aucune construction future ne pourra les séparer rétroactivement. C'est une perte d'information de conception, connue, bornée, à rappeler dans la qualification des signaux (renvoi 002).

### 4.4 Contradictoire

**Définition** — Deux observations (ou ensembles d'observations) sont *contradictoires* lorsqu'« aucune origine unique ne produirait nominalement les deux » : sous l'hypothèse d'un producteur unique et de méthodes appliquées dans leur domaine nominal, la conjonction des deux rapports est inattendue.

Propriétés :

- la contradiction est **relationnelle** (elle porte sur une paire ou un ensemble, jamais sur une observation seule) ;
- elle est **relative à une attente** (« ce que produirait nominalement une origine unique ») — c'est donc déjà un jugement de couche supérieure, que le présent document définit sans l'exercer ;
- **elle n'implique aucune fausseté** : les deux rapports de lecture restent exacts. Des métadonnées déclarant A et une signature prouvant B sont deux observations vraies ; la contradiction est un fait *sur le monde* (quelqu'un a menti, s'est trompé, ou l'attente était naïve), pas un défaut des données ;
- sa **résolution** (laquelle privilégier) exigera des conventions de priorité — décision explicitement reportée (000, L3), interdite ici.

### 4.5 Artefactuelle

**Définition** — Une observation est *artefactuelle* lorsque sa valeur résulte de l'application d'une méthode de lecture **hors de son domaine nominal** : la valeur décrit alors le comportement du mécanisme de lecture davantage que l'intention du contenu.

Cas canonique (corpus 1) : l'API PE accepte des flux sans en-tête MZ et relit les premiers octets d'un ZIP comme un en-tête COFF → `machine='4b50'` (les octets « PK »), `characteristics` et `timestamp` renseignés avec des fragments de la structure ZIP. Vingt observations exactes, fidèles, reproductibles — et dépourvues de la signification que leur attribut suggère.

Propriétés :

- le statut artefactuel est **une hypothèse**, jamais un constat : rien dans l'observation ne le signale ; il s'établit par confrontation (ici : `container='zip'` co-présent, absence d'en-tête optionnel) et reste soumis aux niveaux de certitude du 000 § 5 ;
- une observation soupçonnée artefactuelle **n'est jamais corrigée ni retirée** : elle demeure en base, vraie en tant que rapport ; c'est son *usage* qui sera écarté, avec trace du motif (P5) ;
- le répertoire des artefacts est **ouvert** : rien ne garantit que tous soient connus (000, L4). Chaque méthode d'extraction devra voir ses conditions d'artefact documentées (renvoi 002).

---

## 5. La chaîne fondamentale

Toute l'architecture à venir reposera sur cette stratification stricte :

  **Observation → Interprétation → Signal → Hypothèse → Identité**

| Couche | Nature | Question à laquelle elle répond | Faillibilité | Définie dans |
|---|---|---|---|---|
| **Observation** | fait brut persisté | « qu'a retourné la lecture ? » | infaillible (comme rapport) | **ce document** |
| **Interprétation** | lecture d'une observation à la lumière d'une convention (format, sémantique d'un code, équivalence de valeurs) | « que veut dire cette valeur ? » | faillible (conventions inadaptées, artefacts) | 002 |
| **Signal** | interprétation promue au rang d'indice identitaire, qualifiée (portée, conditions de validité) | « qu'est-ce que cela suggère sur l'origine ? » | faillible et qualifiée | 002 |
| **Hypothèse** | explication candidate d'un ensemble de signaux (origine commune, version, variante…) | « quelle origine expliquerait cela ? » | faillible, munie d'un niveau de certitude | 000 § 5, futurs docs |
| **Identité** | hypothèse retenue comme meilleure explication, révisable | « que retient-on ? » | faillible, défaisable | 000 § 3 |

Règles de la chaîne :

1. **Chaque couche ne consomme que la couche immédiatement inférieure.** Une identité ne « lit » jamais une observation directement : elle s'appuie sur des hypothèses, qui s'appuient sur des signaux, qui s'appuient sur des interprétations d'observations.
2. **L'erreur monte, la vérité reste en bas.** Toute erreur du système vit dans les couches 2 à 5 et se corrige en re-dérivant depuis la couche 1, intacte (P4 du 000).
3. **La couche 1 est la seule persistée comme source de vérité.** Les couches supérieures sont dérivées, recalculables, jetables.
4. Le passage observation → interprétation est **le premier acte faillible** du système. C'est précisément la frontière que le pipeline d'observation s'est interdit de franchir (« observation pure ») et où commence le moteur d'identité.

Exemple traversant la chaîne (donné ici pour illustrer la stratification, sans anticiper les couches 2+) :
`machine = '8664'` *(observation)* → « code COFF de l'architecture x86-64 » *(interprétation, convention de format)* → « contenu exécutable destiné à x64 » *(signal, à qualifier)* → « variante x64 d'un artefact édité » *(hypothèse)* → contribution éventuelle à une identité.

### 5.1 Reformulation des états du 000 § 2.4

Le 000 § 2.4 présentait quatre états « d'un signal » (présent/absent/contradictoire/artefactuel). Le présent document les re-fonde : *présente, absente, illisible* sont des états **d'observation** (§ 4.1–4.3) ; *contradictoire* et *artefactuelle* sont des statuts **assignés par les couches d'interprétation** (§ 4.4–4.5). Le tableau du 000 reste valide comme vue anticipée ; la présente stratification fait foi.

---

## 6. Propriétés mathématiques et invariants

### 6.1 Propriétés de manipulation

Pour tout le système à venir, les observations sont :

- **immuables** — jamais modifiées, jamais complétées ; un nouvel acte produit de nouvelles lignes, les anciennes demeurent (append-only) ;
- **jamais corrigées** — même reconnues artefactuelles ou contradictoires, elles restent telles quelles : c'est l'usage qui s'ajuste, avec trace ;
- **jamais fusionnées** — deux actes sur le même contenu restent deux familles d'observations distinctes ; toute synthèse (« ce contenu a été vu n fois ») est une construction dérivée qui référence ses sources, jamais un remplacement ;
- **jamais pondérées** — aucun poids, aucune importance, aucun ordre de préférence n'existe au niveau de l'observation ; toute pondération appartiendra aux couches signal et au-delà (et n'est définie dans aucun document à ce jour) ;
- **indépendantes** — aucune n'est dérivée d'une autre (§ 2, dernière ligne) ;
- **reproductibles** — même contenu, même méthode ⟹ même valeur (mesuré) ;
- **possiblement incomplètes** — ⊥ est un citoyen de plein droit de tout domaine ;
- **possiblement contradictoires sans être fausses** — § 4.4.

### 6.2 Invariants

> **I1 — Intangibilité.** Le moteur d'identité ne pourra jamais modifier, supprimer, corriger ou fusionner une observation. Toute violation de I1 est une faute de conception, quelle qu'en soit la justification.

> **I2 — Pérennité de la vérité.** Une observation reste vraie (comme rapport de lecture) quelle que soit l'évolution des identités, des signaux, des conventions ou des algorithmes futurs. Les révisions identitaires ne se propagent **jamais** vers le bas de la chaîne.

> **I3 — La pertinence appartient au consommateur.** Une observation peut devenir pertinente ou cesser de l'être selon les constructions futures ; **jamais l'inverse** : aucune construction ne peut exiger qu'une observation change pour lui convenir. Si une couche supérieure a besoin d'une donnée que les observations ne portent pas, la réponse est une capacité nouvelle (nouvel acte d'observation), pas un enrichissement rétroactif.

> **I4 — Complétude de la provenance.** Toute construction dérivée (interprétation, signal, hypothèse, identité) doit pouvoir citer l'ensemble exact des observations élémentaires dont elle procède (P5 du 000, reformulé au grain élémentaire).

---

## 7. Ce qui n'est PAS une observation

Les notions suivantes n'existent **dans aucune table** et ne doivent jamais être traitées comme des données :

| Notion | Pourquoi ce n'est pas une observation | Couche où elle vivra |
|---|---|---|
| « éditeur » | `subject` est une observation ; « l'éditeur » est l'hypothèse qu'un signataire corresponde à une organisation éditrice | signal / hypothèse |
| « version » (d'un logiciel) | `product_version` et `file_version` sont des chaînes déclaratives ; « la version » est une strate hypothétique (000, Déf. 7) | hypothèse |
| « logiciel », « produit » | aucune capacité ne les observe ; ce sont les identités elles-mêmes | identité |
| « famille » | relation entre identités (000, Déf. 10) | construction sur identités |
| « same software » | c'est ≡ₗ, l'objet final du moteur — le résultat, jamais l'entrée | identité |
| « installateur », « application » | qualifications fonctionnelles d'un contenu ; le pipeline n'observe que des structures | interprétation / signal |
| « signé » / « non signé » | l'observation est *un certificat a été extrait / rien n'a été extrait* ; « non signé » nie ce que l'absence ne prouve pas (catalogues) | interprétation |
| « application 64 bits » | l'observation est `machine='8664'` ; l'architecture cible est une interprétation de convention | interprétation |
| « doublon » | l'observation est l'égalité de deux `sha256` ; « doublon » ajoute un jugement d'intention (redondance) | signal / hypothèse |
| « est un PE », « est un MSI » | les observations sont `container`, en-têtes, propriétés ; « être un X » est une conclusion de format | interprétation |

Règle générale : **tout substantif qui désigne ce que le contenu *est* ou *représente* — plutôt que ce qu'une méthode *a lu* — n'est pas une observation.**

---

## 8. Exemples complets

Chaque exemple suit le même canon : *fait persisté → statut d'observation → ce que le futur signal pourra en faire (annoncé, non construit)*.

**E1 — `product_name = 'Visual Studio…'`** (déclaratif, présent)
Observation : la ressource VersionInfo portait cette chaîne. Rien de plus. Ce n'est pas encore un signal : la chaîne peut être exacte, générique, mensongère ou périmée. Le document 002 dira ce que vaut l'indice « déclaration de nom de produit ».

**E2 — deux actes, même `sha256`** (physique, présent ×2)
Observation : deux lectures ont produit le même condensat. Le futur signal sera « contenu identique » (≡ₘ, 000 Déf. 4) — le seul qui atteindra le niveau « certain » conventionnel. Au corpus 1 : 497 actes, 381 contenus distincts ; la répétition d'un sha256 est une paire d'observations, pas « un doublon » (§ 7).

**E3 — `subject = 'CN=Python Software Foundation…'`** (cryptographique, présent)
Observation : un certificat embarqué portait ce sujet. Le futur signal sera éventuellement « même signataire que… », jamais « même éditeur » sans qualification : 59 contenus du corpus 1 partagent ce sujet en couvrant de nombreuses versions et variantes — et 192 partagent un sujet Microsoft en couvrant des *produits* différents. Le certificat borne l'origine, il ne nomme pas le produit (000 § 3.2).

**E4 — `machine = '8664'`** (structurel, présent)
Observation : le champ machine de l'en-tête COFF valait `0x8664`. **Jamais « application 64 bits »** : cette conclusion appartient à l'interprétation (convention de format) puis au signal (qualification). La preuve que ce saut est dangereux est dans E5.

**E5 — `machine = '4b50'` sur `container = 'zip'`** (structurel, présent — statut artefactuel probable)
Observation : la méthode PE a retourné `4b50` (« PK »). Vingt cas mesurés. Le statut artefactuel est une hypothèse de couche supérieure (co-présence de `container='zip'`, absence d'en-tête optionnel) ; l'observation, elle, reste en base, exacte et intangible (I1, I2). Exemple canonique de la différence entre fidélité du rapport et véracité de l'indice.

**E6 — `product_version = ⊥`** (déclaratif, absent)
Observation : la lecture nominale n'a rien trouvé. État majoritaire au corpus 1 (88,5 %). L'absence ne dit pas « pas de version » : elle dit « rien de déclaré ici ». Tout futur signal devra distinguer « aucune déclaration » de « déclaration vide » si les domaines de valeurs le permettent.

**E7 — `.msp` : `container = 'ole-cfb'` présent, propriétés MSI toutes ⊥** (structurel présent + déclaratif absent)
Observation double : la structure conteneur est celle d'OLE-CFB, et la table Property est introuvable. Dix cas mesurés. La co-occurrence « conteneur présent / déclaratif absent » est une **configuration d'observations parfaitement cohérente** — deux formats partagent le même conteneur. Aucune contradiction : seule une attente naïve (« ole-cfb ⟹ propriétés MSI ») en fabriquerait une. L'attente sera l'affaire des interprétations.

**E8 — certificat présent, aucune information produit** (cryptographique présent + déclaratif absent)
Observation : `subject` renseigné, VersionInfo toute ⊥ (cas fréquent des MSI signés du corpus 1 : 360 certificats sur des ole-cfb dont 0 avec `product_name` VersionInfo). Configuration normale : les types P/S/D/C sont indépendants (§ 2) ; la richesse d'un type ne présage pas de la présence d'un autre.

---

## 9. Récapitulatif

| Objet | Définition | § |
|---|---|---|
| acte d'observation | exécution du pipeline sur un fichier, identifiée, datée | 1.1 |
| observation élémentaire | (acte, attribut, valeur ∈ Val(a) ∪ {⊥}) — l'atome du système | 1.1 |
| vérité d'une observation | vérité du **rapport de lecture**, jamais de la chose | 1.3 |
| propriétés constitutives | provenance, méthode, reproductibilité, stabilité, domaine, absence, contradiction possible, indépendance | 2 |
| typologie | P physiques, S structurelles, D déclaratives, C cryptographiques, X contextuelles — partition des attributs, ouverte | 3 |
| états intrinsèques | présente, absente, illisible (⊥ projette absent **et** illisible : perte actée) | 4.1–4.3 |
| statuts relationnels | contradictoire, artefactuelle — assignés par les couches supérieures, jamais intrinsèques | 4.4–4.5 |
| chaîne fondamentale | observation → interprétation → signal → hypothèse → identité ; chaque couche ne consomme que la précédente ; l'erreur monte, la vérité reste en bas | 5 |
| invariants | I1 intangibilité, I2 pérennité de la vérité, I3 pertinence côté consommateur, I4 provenance complète | 6.2 |
| non-observations | éditeur, version, logiciel, produit, famille, same software, signé, 64 bits, doublon, « est un X » | 7 |

**Ce que ce document ne fait volontairement pas** : construire les interprétations et les signaux (002), qualifier la valeur d'indice de quoi que ce soit, définir des équivalences de valeurs plus souples que l'égalité byte-à-byte, résoudre les contradictions, cataloguer les artefacts. Il fixe le sol sur lequel tout cela devra se tenir.
