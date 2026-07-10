# L'oracle indépendant du test d'or (014 § 10, É7 ; 013 § 12) : produit le fichier W₀ attendu
# « hors moteur, dans un autre langage, depuis les mêmes définitions » — la base archivée
# (corpus1-postA1.db), la caractérisation normative de W₀ (014 § 8, carte des refus réconciliée
# par le 021) et la forme canonique matérielle consignée (docs/conformite/forme-canonique-materielle.md).
# Bibliothèque standard exclusivement (sqlite3, hashlib, json) — aucune ligne du moteur.
#
# Usage :  python oracle-w0.py   (depuis tests/oracle/ ; écrit W0-attendu.json à côté)

import hashlib
import json
import pathlib
import sqlite3

ICI = pathlib.Path(__file__).parent

# --- Ω : la base archivée (contrat user_version = 1) ---
connexion = sqlite3.connect(ICI / "corpus1-postA1.db")
actes = sorted(connexion.execute("SELECT id, sha256 FROM scan_observations").fetchall())
connexion.close()

# --- L'identité de l'état d'Ω (025 § 3) : SHA-256 — la fonction déclarée du support pour
#     user_version = 1 — sur l'encodage à préfixe de longueur « n:v, » des couples
#     (identifiant, empreinte de contenu), en ordre canonique des identifiants. ---
encodage = "".join(f"{len(champ)}:{champ}," for i, sha in actes for champ in (str(i), sha))
empreinte_etat = hashlib.sha256(encodage.encode("utf-8")).hexdigest()

# --- ℛ₀ : les couples en vigueur (EXG-39, membre 2 ; 014 § 8), triés par identifiant ---
registre = [{"identifiant": "CE-01", "version": 1}, {"identifiant": "EQ-01", "version": 1}]

# --- Les 112 élections (014 § 8, EXG-26) : une par classe de contenu partagé (≥ 2 actes),
#     strate contenu, niveau « certaine », motif « unique-maximale », licence CE-01 v1,
#     dépendances CE-01 v1 + EQ-01 v1, dette vide — triées par plus petit identifiant. ---
classes = {}
for i, sha in actes:
    classes.setdefault(sha, []).append(i)

elections = [
    {
        "type": "élection",
        "strate": "contenu",
        "domaine": domaine,
        "contenu": sha,
        "niveau": "certaine",
        "motif": "unique-maximale",
        "licences": [{"identifiant": "CE-01", "version": 1}],
        "dependances": registre,
        "dette": [],
    }
    for sha, domaine in sorted(classes.items(), key=lambda e: e[1][0])
    if len(domaine) >= 2
]

# --- Les 4 refus normatifs de domaine maximal (014 § 8, carte du 021) : un par strate
#     supérieure, espèce « normatif », motif « aucune-convention-strate » (variante, version,
#     identite) puis « préalable-absent » (famille). ---
tous = [i for i, _ in actes]
refus = [
    {"type": "refus", "strate": strate, "domaine": tous, "motif": motif, "espece": "normatif"}
    for strate, motif in [
        ("variante", "aucune-convention-strate"),
        ("version", "aucune-convention-strate"),
        ("identite", "aucune-convention-strate"),
        ("famille", "préalable-absent"),
    ]
]

w0 = {
    "index": {
        "omega": {"version": 1, "nombreActes": len(actes), "empreinteEtat": empreinte_etat},
        "registre": registre,
    },
    "actes": elections + refus,
}

# --- L'émission canonique consignée : json.dumps(ensure_ascii=False, indent=2) est, par choix
#     de la consignation (§ 1), exactement la disposition attendue ; LF exclusif, LF final unique,
#     UTF-8 sans BOM. ---
with open(ICI / "W0-attendu.json", "w", encoding="utf-8", newline="\n") as fichier:
    fichier.write(json.dumps(w0, ensure_ascii=False, indent=2) + "\n")

print(f"W0-attendu.json : {len(elections)} élections + {len(refus)} refus, empreinte {empreinte_etat}")
