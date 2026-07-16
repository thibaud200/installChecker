# Améliorations — module Duplicate Files (v1)

Décisions et pistes issues des tests de bout en bout de la v1 (2026-07-12).

---

## A1 — Réduire le corpus scanné avant observation

**Besoin (démontré par l'usage).** Pouvoir **réduire le corpus observé, en place, avant l'observation, en conservant le chemin réel** — pour se passer d'une copie préalable des fichiers (voir A2).

**Garde d'architecture.** La réduction ne s'appuie que sur des **métadonnées du système de fichiers** (nom, chemin, attributs, taille, dates), au niveau de l'orchestration du scan. Tout filtrage fondé sur le **contenu** (PE, MSI, AppX…) appartient aux **consommateurs** (rapport, UI, modules), **jamais au scan**.

**Axe chemin (piste, non implémenté).** Une **liste de répertoires système** est **exclue par défaut** (`C:\Windows`, `C:\Program Files`, `C:\Program Files (x86)`, `$Recycle.Bin`, …). Une option permet leur **inclusion explicite** au scan.

**État.** L'axe nom (`--ext`) est **livré**. L'axe chemin (exclusion de répertoires) reste une piste non implémentée.

---

## A2 — Ne jamais copier des fichiers pour filtrer

Le filtrage se fait **en place**, chemin d'origine conservé. Copier les fichiers voulus dans un dossier de staging fabrique de **faux doublons** (la copie a le même SHA-256 que l'original) et n'apporte rien : le rapport porte déjà le vrai chemin (`scan_observations.path`). On restreint *quels fichiers on observe* (A1), on ne les déplace jamais.

---

## A3 — Plan de suppression

**Implémenté (commande `plan`).** À partir des groupes de contenus identiques, produit une **liste plate de propositions** `{ contenu, chemin }`. Garanties : au moins une copie subsiste par contenu ; un chemin protégé n'est jamais proposé ; rien n'est exécuté. Constructeur pur, indépendant du classement et du rapport.

**Sécurité — protection.** Le mécanisme d'exclusion des chemins protégés est **en place** et testé, mais **aucune liste n'est encore fournie** (répertoires système, A1) : un ensemble vide est passé, donc des chemins système peuvent apparaître. À alimenter par A1.

**UI (non implémentée).** Une interface de revue et d'export du plan — consommateur du plan et du rapport, aucune logique métier, aucune suppression automatique — reste à faire.

---

## A4 — Multi-disque : base unique

**Décidé.** Une **base unique** pour tous les disques : un doublon peut exister *entre* volumes (même SHA-256) et une base unique le détecte ; des bases par volume exigeraient une déduplication inter-bases inutile.

**Livré (2026-07-16).** Le snapshot courant est implémenté selon
`docs/superpowers/specs/2026-07-16-multi-disque-design.md` : table `scans` (schéma v2),
identité de volume observée au scan (numéro de série local, racine UNC normalisée),
état courant = dernier scan par volume, appliqué par le lecteur Ω **avant toute dérivation** —
tous les consommateurs (`identity`, `duplicates`, `plan`) partagent le même état courant,
Ω restant append-only. Le lecteur reste bi-version (v1 : lecture intégrale, l'oracle de
conformité est intact ; v2 : état courant). Le rapport `duplicates` porte le volume de
chaque exemplaire.

---

> **Exploration non retenue (2026-07-12).** Une réflexion étendue a couvert WinGet/connecteurs, les modules futurs (Inventory / Version Manager / Library Maintenance), une UI détaillée, des invariants d'indépendance, et plusieurs modèles de sélection (glob, `--kind`, couches/axiomes). Aucun n'est cadré ni retenu en v1 — à re-brainstormer avec un contexte réel si le besoin devient concret.
