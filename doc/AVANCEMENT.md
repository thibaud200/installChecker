Au vu de la direction que tu as prise (collecter tous les faits observables avant de construire le moteur d'identité), je dirais qu'il reste une quinzaine d'étapes, réparties en deux grandes phases.

Phase 1 — Compléter le moteur d'observation (≈ 10–12 étapes)

Tu es actuellement ici :

✅ Scan
✅ SHA-256
✅ SQLite
✅ VersionInfo
✅ File Headers
✅ PE
✅ Authenticode
⏳ MSI Properties

Ensuite je verrais quelque chose comme :

✅ MSI Properties
NSIS (version, nom, etc.)
Inno Setup
ZIP / APPX / MSIX (lecture du manifeste)
Burn / WiX Bundle
InstallShield (si détectable)
Squirrel
Electron (package.json / app.asar si pertinent)
Archives génériques (ZIP, 7z...) sans interprétation
Métadonnées du système de fichiers (dates, attributs, taille allouée...)
Icône / ressources (facultatif mais souvent utile pour l'identité)

À ce stade, tu auras une base contenant uniquement des observations brutes.

Phase 2 — Construire le moteur d'identité (≈ 5–8 étapes)

Là seulement commence le vrai projet.

Par exemple :

Modèle d'identité (software_identity)
Règles de rapprochement (VersionInfo + MSI + certificat + ...)
Score de similarité
Détection des doublons
Détection des nouvelles versions
Gestion des conflits
Historique des observations
API / export / interface

C'est cette partie qui est la plus complexe intellectuellement.

Là où tu en es

Je dirais que tu as réalisé environ 30 à 35 % du projet.

En revanche, tu as terminé la partie la plus importante architecturalement : tu as posé des bases très propres. Le reste du développement sera beaucoup plus fluide, car toutes les futures capacités suivront le même modèle :

un extracteur indépendant ;
une table dédiée ;
des tests ;
aucune interprétation.

C'est exactement l'architecture qui permettra ensuite de construire un moteur d'identité robuste sans devoir refaire les fondations.