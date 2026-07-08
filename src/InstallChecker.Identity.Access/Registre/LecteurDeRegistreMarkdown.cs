using System.Globalization;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Erreurs;

namespace InstallChecker.Identity.Access.Registre;

/// <summary>
/// L'adaptateur C2 (013 § 1.1, § 5) : projette le répertoire <c>registre/</c> — au format Markdown
/// à champs fixes défini par 015 § 3 — vers le référentiel de conventions du moteur pur. Seul
/// composant à connaître la grammaire Markdown du registre ; <c>InstallChecker.Identity</c> n'en
/// sait rien (013 § 1.2).
/// </summary>
public sealed class LecteurDeRegistreMarkdown(string cheminRegistre) : IRegistreSource
{
    private static readonly string[] TitresAttendus =
    [
        "identifiant", "version", "famille", "domaine d'application", "transformation",
        "dépendances", "régimes admis", "portée", "justification", "justification empirique",
        "limites", "conditions de révision", "date", "autorité",
    ];

    private static readonly IReadOnlyDictionary<string, Famille> Familles = new Dictionary<string, Famille>
    {
        ["interprétation"] = Famille.Interpretation,
        ["équivalence"] = Famille.Equivalence,
        ["priorité"] = Famille.Priorite,
        ["attente"] = Famille.Attente,
        ["catalogue"] = Famille.Catalogue,
        ["stratification"] = Famille.Stratification,
        ["composition"] = Famille.Composition,
        ["élection"] = Famille.Election,
    };

    public Referentiel Projeter()
    {
        if (!Directory.Exists(cheminRegistre))
        {
            throw new RegistreAbsentException($"répertoire de registre introuvable : {cheminRegistre}");
        }

        var conventions = LireToutesLesConventions(Path.Combine(cheminRegistre, "conventions"));
        VerifierDependancesPresentesDansLeRepertoire(conventions);

        var adoptees = LireHistorique(Path.Combine(cheminRegistre, "historique.md"), conventions);
        var enVigueurRefs = LireEtat(Path.Combine(cheminRegistre, "etat.md"), conventions, adoptees);

        var conventionsEnVigueur = enVigueurRefs.Select(reference => conventions[reference]).ToList();
        PredicatDeCoherence.Verifier(conventionsEnVigueur);

        return new Referentiel(conventionsEnVigueur);
    }

    // --- Fichiers de version (<ID>/v<n>.md) — 015 § 3, 014 § 5.1 ---

    private static Dictionary<ConventionRef, Convention> LireToutesLesConventions(string conventionsDir)
    {
        var conventions = new Dictionary<ConventionRef, Convention>();
        if (!Directory.Exists(conventionsDir)) return conventions;

        foreach (var repertoire in Directory.GetDirectories(conventionsDir).OrderBy(d => d, StringComparer.Ordinal))
        {
            var identifiantAttendu = Path.GetFileName(repertoire);
            foreach (var fichier in Directory.GetFiles(repertoire, "v*.md").OrderBy(f => f, StringComparer.Ordinal))
            {
                var nomFichier = Path.GetFileNameWithoutExtension(fichier);
                if (nomFichier.Length < 2 || nomFichier[0] != 'v' || !int.TryParse(nomFichier[1..], out var versionAttendue))
                {
                    throw new RegistreMalformeException($"nom de fichier de version invalide : {fichier}");
                }

                var convention = LireConvention(fichier, identifiantAttendu, versionAttendue);
                if (!conventions.TryAdd(convention.Ref, convention))
                {
                    throw new RegistreMalformeException($"version dupliquée : {convention.Identifiant} v{convention.Version}");
                }
            }
        }

        return conventions;
    }

    private static Convention LireConvention(string chemin, string identifiantAttendu, int versionAttendue)
    {
        var sections = SectionParser.ExtraireSections(File.ReadAllText(chemin));
        var titres = sections.Select(s => s.Titre).ToList();

        if (!titres.SequenceEqual(TitresAttendus))
        {
            throw new RegistreMalformeException($"{chemin} : {DecrireEcartDeGrammaire(titres)}");
        }

        var champs = sections.ToDictionary(s => s.Titre, s => s.Contenu);
        foreach (var titre in TitresAttendus)
        {
            if (string.IsNullOrWhiteSpace(champs[titre]))
            {
                throw new RegistreMalformeException($"{chemin} : section « {titre} » vide");
            }
        }

        var identifiant = champs["identifiant"];
        if (identifiant != identifiantAttendu)
        {
            throw new RegistreMalformeException(
                $"{chemin} : identifiant « {identifiant} » incohérent avec le répertoire « {identifiantAttendu} »");
        }

        if (!int.TryParse(champs["version"], out var version) || version != versionAttendue)
        {
            throw new RegistreMalformeException(
                $"{chemin} : version « {champs["version"]} » incohérente avec le nom de fichier (attendu v{versionAttendue})");
        }

        if (!Familles.TryGetValue(champs["famille"], out var famille))
        {
            throw new RegistreMalformeException($"{chemin} : famille inconnue « {champs["famille"]} »");
        }

        if (!EssayerAnalyserDate(champs["date"], out var date))
        {
            throw new RegistreMalformeException($"{chemin} : date invalide « {champs["date"]} »");
        }

        var dependances = LireListeDeReferences(champs["dépendances"], chemin);

        return new Convention(
            identifiant,
            version,
            famille,
            champs["domaine d'application"],
            champs["transformation"],
            dependances,
            champs["régimes admis"],
            champs["portée"],
            champs["justification"],
            champs["justification empirique"],
            champs["limites"],
            champs["conditions de révision"],
            date,
            champs["autorité"]);
    }

    private static string DecrireEcartDeGrammaire(List<string> titres)
    {
        var manquants = TitresAttendus.Except(titres).ToList();
        var inconnus = titres.Except(TitresAttendus).ToList();
        var doublons = titres.GroupBy(t => t).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

        var raisons = new List<string>();
        if (manquants.Count > 0) raisons.Add($"section(s) absente(s) : {string.Join(", ", manquants)}");
        if (inconnus.Count > 0) raisons.Add($"section(s) inconnue(s) : {string.Join(", ", inconnus)}");
        if (doublons.Count > 0) raisons.Add($"section(s) dupliquée(s) : {string.Join(", ", doublons)}");
        if (raisons.Count == 0) raisons.Add("ordre des sections incorrect");

        return string.Join(" ; ", raisons);
    }

    private static void VerifierDependancesPresentesDansLeRepertoire(IReadOnlyDictionary<ConventionRef, Convention> conventions)
    {
        foreach (var convention in conventions.Values)
        {
            foreach (var dependance in convention.Dependances)
            {
                if (!conventions.ContainsKey(dependance))
                {
                    throw new RegistreMalformeException(
                        $"{convention.Identifiant} v{convention.Version} : dépendance " +
                        $"{dependance.Identifiant} v{dependance.Version} inexistante dans le répertoire des conventions");
                }
            }
        }
    }

    // --- Garde partagée : aucun titre de section dupliqué avant tout ToDictionary (même niveau de
    //     robustesse que LireConvention, qui l'obtient via SequenceEqual sur l'ensemble attendu) ---

    private static void VerifierAucunTitreDuplique(IReadOnlyList<(string Titre, string Contenu)> sections, string contexte)
    {
        var doublon = sections.GroupBy(s => s.Titre).FirstOrDefault(g => g.Count() > 1);
        if (doublon is not null)
        {
            throw new RegistreMalformeException($"{contexte} : section « {doublon.Key} » dupliquée");
        }
    }

    // --- Journal (historique.md) — 015 § 6, 014 § 5.2 ---

    private static readonly string[] ChampsEntreeHistorique = ["type", "convention", "justification de l'acte", "autorité"];

    private static HashSet<ConventionRef> LireHistorique(string chemin, IReadOnlyDictionary<ConventionRef, Convention> conventions)
    {
        if (!File.Exists(chemin))
        {
            throw new RegistreMalformeException($"historique.md introuvable : {chemin}");
        }

        var adoptees = new HashSet<ConventionRef>();

        foreach (var (titre, contenu) in SectionParser.ExtraireSections(File.ReadAllText(chemin)))
        {
            var sousSections = SectionParser.ExtraireSections(contenu, "### ");
            VerifierAucunTitreDuplique(sousSections, $"historique.md : entrée « {titre} »");
            var champs = sousSections.ToDictionary(s => s.Titre, s => s.Contenu);

            foreach (var attendu in ChampsEntreeHistorique)
            {
                if (!champs.TryGetValue(attendu, out var valeur) || string.IsNullOrWhiteSpace(valeur))
                {
                    throw new RegistreMalformeException($"historique.md : entrée « {titre} » sans champ « {attendu} »");
                }
            }

            var type = champs["type"].Trim();
            var reference = ParserReferenceUnique(champs["convention"], chemin);

            switch (type)
            {
                case "adoption":
                case "révision":
                    if (!conventions.ContainsKey(reference))
                    {
                        throw new RegistreMalformeException(
                            $"historique.md : {type} de {reference.Identifiant} v{reference.Version} sans fichier de version correspondant");
                    }

                    adoptees.Add(reference);
                    break;

                case "retrait":
                    if (!adoptees.Remove(reference))
                    {
                        throw new RegistreMalformeException(
                            $"historique.md : retrait de {reference.Identifiant} v{reference.Version} jamais adoptée");
                    }

                    break;

                case "remplacement":
                case "scission":
                case "fusion":
                    // types reconnus (007 § 10) ; aucune convention du registre actuel n'en use — rien à vérifier de plus ici.
                    break;

                default:
                    throw new RegistreMalformeException($"historique.md : type de transition inconnu « {type} »");
            }
        }

        return adoptees;
    }

    // --- État (etat.md) — 015 § 7, 014 § 5.3 ---

    private static readonly string[] TitresAttendusEtat =
        ["ℛ₀", "conventions en vigueur", "conventions retirées", "version du registre", "date logique", "index documentaire"];

    private static List<ConventionRef> LireEtat(
        string chemin,
        IReadOnlyDictionary<ConventionRef, Convention> conventions,
        IReadOnlySet<ConventionRef> adoptees)
    {
        if (!File.Exists(chemin))
        {
            throw new RegistreMalformeException($"etat.md introuvable : {chemin}");
        }

        var sectionsBrutes = SectionParser.ExtraireSections(File.ReadAllText(chemin));
        VerifierAucunTitreDuplique(sectionsBrutes, "etat.md");
        var sections = sectionsBrutes.ToDictionary(s => s.Titre, s => s.Contenu);

        foreach (var titre in TitresAttendusEtat)
        {
            if (!sections.TryGetValue(titre, out var contenu) || string.IsNullOrWhiteSpace(contenu))
            {
                throw new RegistreMalformeException($"etat.md : section « {titre} » absente ou vide");
            }
        }

        if (!int.TryParse(sections["version du registre"].Trim(), out _))
        {
            throw new RegistreMalformeException($"etat.md : « version du registre » invalide « {sections["version du registre"]} »");
        }

        if (!EssayerAnalyserDate(sections["date logique"], out _))
        {
            throw new RegistreMalformeException($"etat.md : « date logique » invalide « {sections["date logique"]} »");
        }

        var enVigueur = LireListeDeReferences(sections["conventions en vigueur"], chemin);

        foreach (var reference in enVigueur)
        {
            if (!conventions.ContainsKey(reference))
            {
                throw new RegistreMalformeException(
                    $"etat.md : {reference.Identifiant} v{reference.Version} citée en vigueur sans fichier correspondant");
            }

            if (!adoptees.Contains(reference))
            {
                throw new RegistreMalformeException(
                    $"etat.md : {reference.Identifiant} v{reference.Version} citée en vigueur sans adoption en vigueur dans historique.md");
            }
        }

        return enVigueur;
    }

    // --- Références de conventions : format partagé « <ID>, version <n> » (015 §§ 3.5, 6.2, 7.3) ---

    private static List<ConventionRef> LireListeDeReferences(string contenu, string contexte)
    {
        if (contenu.Trim() == "Aucune.") return [];

        var references = new List<ConventionRef>();
        foreach (var ligne in contenu.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var texte = ligne.Trim();
            if (!texte.StartsWith("- ", StringComparison.Ordinal))
            {
                throw new RegistreMalformeException($"{contexte} : ligne de référence invalide « {texte} »");
            }

            references.Add(ParserReferenceUnique(texte["- ".Length..], contexte));
        }

        return references;
    }

    private static ConventionRef ParserReferenceUnique(string texte, string contexte)
    {
        texte = texte.Trim();
        var separateur = texte.LastIndexOf(", version ", StringComparison.Ordinal);
        if (separateur < 0 || !int.TryParse(texte[(separateur + ", version ".Length)..].Trim(), out var version))
        {
            throw new RegistreMalformeException($"{contexte} : référence de convention invalide « {texte} »");
        }

        return new ConventionRef(texte[..separateur].Trim(), version);
    }

    private static bool EssayerAnalyserDate(string texte, out DateOnly date) =>
        DateOnly.TryParseExact(texte.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}
