# Forme canonique matérielle de W et de τ (report 3)

**Nature** : la **consignation** de la forme canonique que le moteur définit pour ses sorties — « le moteur définit une forme canonique de sa sortie (ordre normalisé, encodage fixé) » (EXG-18) ; « chaque sortie possède une forme canonique **définie par l'implémentation** » (011 § 3). Ce document n'est pas un acte de la série normative : la théorie a délégué trois fois la syntaxe concrète (011 § 3 « aucune représentation n'est choisie ici » ; 012 § 5 « sans imposer de format » ; 016 § 4.1, report 3 en voie 3) — la présente consignation rend la définition de l'implémentation **opposable et reproductible hors moteur** (014 § 10, É7 : le fichier d'or est produit « par un oracle indépendant … depuis les mêmes définitions »).

Tout ce qui est *logique* est fixé ailleurs et repris par référence : sections, champs, ordres et tris par le **014 § 7** (tel que raffiné par les 024, 025 et 026) ; les contraintes d'encodage par le **013 § 4** (« UTF-8 sans BOM, fins de ligne uniformes, culture invariante, aucun espace optionnel variable, ordre des champs fixé par la spécification (014), nombres et chaînes sous forme normalisée »). Le présent document ne fixe **que la syntaxe concrète** — le seul degré de liberté que les lettres laissaient.

**Version** : 1 (campagne v3, jalon V3-10). Toute évolution de cette forme est une évolution de la conformité : elle régénère le fichier d'or et se consigne ici par une version nouvelle.

---

## 1. La syntaxe concrète

- **Format** : un document **JSON** unique (RFC 8259).
- **Encodage** : UTF-8 **sans BOM** ; les caractères non ASCII sont émis **en clair** (jamais échappés en `\uXXXX`).
- **Échappement** : uniquement ce que JSON impose — `"` → `\"`, `\` → `\\`, et les caractères de contrôle U+0000–U+001F (formes courtes `\b \f \n \r \t`, sinon `\u00xx` en hexadécimal **minuscule**). Rien d'autre n'est jamais échappé.
- **Fins de ligne** : **LF** (`\n`) exclusivement, sur toute plateforme ; le document se termine par **un LF final unique**.
- **Disposition** (fixe — le 013 § 4 interdit l'espace optionnel *variable*) : indentation de **2 espaces** ; un espace après `:`, aucun avant ; objets et tableaux non vides multi-lignes (un membre par ligne) ; objet vide `{}` et tableau vide `[]` sur place. C'est la disposition de `json.dumps(obj, ensure_ascii=False, indent=2)` de la bibliothèque standard Python — choisie précisément pour que l'oracle indépendant l'obtienne sans le moindre code de mise en forme.
- **Nombres** : entiers décimaux en culture invariante, sans signe positif ni zéro superflu (W et τ ne portent que des entiers).
- **Champ absent** : **omis** — le « — » du 014 § 7.3 est la notation documentaire de l'absence, jamais une valeur émise.

## 2. La forme de W

L'ordre et le contenu sont ceux du 014 § 7 — rappelés ici pour la lecture, jamais redéfinis :

```
{
  "index": {
    "omega": { "version", "nombreActes", "empreinteEtat" },      ← 014 § 7.2, raffiné 025
    "registre": [ { "identifiant", "version" }, … ]              ← liste triée des couples en vigueur
  },
  "actes": [                                                     ← tri : strate < type < plus petit identifiant (014 § 7.3)
    { "type", "strate", "domaine", "contenu", "niveau", "motif",
      "espece", "licences", "dependances", "dette" }             ← champs de l'autre type omis
  ]
}
```

Valeurs textuelles : `type` ∈ { `élection`, `refus` } ; `strate` ∈ { `contenu`, `variante`, `version`, `identite`, `famille` } ; `niveau`, `espece` et `motif` : le vocabulaire du 014 §§ 7.3–7.4, en minuscules.

## 3. La forme de τ

Les quatre sections du 014 § 7.5, tel que raffiné par le 024 (la référence d'acte = l'identité `{ "strate", "domaine" }`) et le 026 (la cause = les volets, les continuités dérivées) :

```
{
  "index-avant":  { … },                                         ← forme du § 2 (« index »)
  "index-apres":  { … },
  "cause": {                                                     ← volets omis si le membre n'a pas changé ; {} entre index égaux
    "omega":    { "ajoutes": [ … ], "retires": [ … ] },
    "registre": { "adoptes": [ { "identifiant", "version" }, … ], "retires": [ … ] }
  },
  "correspondance": {
    "conserves":   [ { "strate", "domaine" }, … ],
    "abandonnes":  [ … ],
    "nouveaux":    [ … ],
    "continuites": [ { "avant": { "strate", "domaine" }, "apres": { "strate", "domaine" } }, … ]
  }
}
```

## 4. Ce que cette consignation ne couvre pas

Les **réponses d'audit** (« chaque sortie possède une forme canonique », 011 § 3) n'ont pas de forme matérielle consignée : la lettre du report 3 (016 § 4.1) ne nomme que W, le 024 n'y a ajouté que τ — le manque est signalé à l'inventaire de la clôture v3, jamais résolu ici.
