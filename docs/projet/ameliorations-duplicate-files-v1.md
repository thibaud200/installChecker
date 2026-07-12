# Améliorations — module Duplicate Files (v1)

Décisions et pistes issues des tests de bout en bout de la v1 (2026-07-12).

---

## A1 — Réduire le corpus scanné avant observation

**Besoin (démontré par l'usage).** Pouvoir **réduire le corpus observé, en place, avant l'observation, en conservant le chemin réel** — pour se passer d'une copie préalable des fichiers (voir A2).

**Garde d'architecture.** La réduction ne s'appuie que sur des **métadonnées du système de fichiers** (nom, chemin, attributs, taille, dates), au niveau de l'orchestration du scan. Tout filtrage fondé sur le **contenu** (PE, MSI, AppX…) appartient aux **consommateurs** (rapport, UI, modules), **jamais au scan**.

**Axe chemin (piste, non implémenté).** Une **liste de répertoires système** est **exclue par défaut** (`C:\Windows`, `C:\Program Files`, `C:\Program Files (x86)`, `$Recycle.Bin`, …). Une option permet leur **inclusion explicite** au scan.

---

## A2 — Ne jamais copier des fichiers pour filtrer

Le filtrage se fait **en place**, chemin d'origine conservé. Copier les fichiers voulus dans un dossier de staging fabrique de **faux doublons** (la copie a le même SHA-256 que l'original) et n'apporte rien : le rapport porte déjà le vrai chemin (`scan_observations.path`). On restreint *quels fichiers on observe* (A1), on ne les déplace jamais.

---

## A3 — Interface graphique simple

Une UI pour revoir les groupes de doublons et décider quoi conserver.

**Décidé.** L'UI est un **consommateur du rapport** (aucune logique métier, aucun appel au moteur pour décider). En v1 elle **exporte un plan d'action** revu par l'humain et **n'exécute rien** — aucune suppression automatique. L'exécution viendra plus tard, une fois le snapshot d'A4 résolu.

**Sécurité du plan.** Les chemins appartenant aux **répertoires système** (A1) ne sont **jamais proposés à la suppression**, même lorsqu'ils apparaissent dans un groupe de doublons. Règle appliquée à la génération du plan ; les rangs et la politique de rétention restent inchangés.

---

## A4 — Multi-disque : base unique

**Décidé.** Une **base unique** pour tous les disques : un doublon peut exister *entre* volumes (même SHA-256) et une base unique le détecte ; des bases par volume exigeraient une déduplication inter-bases inutile.

**Chantier ouvert.** Avec des rescans, l'append-only (ADR-002) empile les observations d'un même fichier → faux doublons entre deux scans. Il faudra une notion de **snapshot courant** ; elle devra s'appliquer **à la source des observations (Ω) avant toute dérivation**, afin que tous les consommateurs partagent le même état courant, Ω restant append-only. Prérequis à toute suppression exécutée.

---

> **Exploration non retenue (2026-07-12).** Une réflexion étendue a couvert WinGet/connecteurs, les modules futurs (Inventory / Version Manager / Library Maintenance), une UI détaillée, des invariants d'indépendance, et plusieurs modèles de sélection (glob, `--kind`, couches/axiomes). Aucun n'est cadré ni retenu en v1 — à re-brainstormer avec un contexte réel si le besoin devient concret.
