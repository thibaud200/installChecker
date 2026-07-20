using System.Diagnostics.CodeAnalysis;

namespace InstallChecker.DuplicateFiles;

public static class ValidateurStructurePlan
{
    public static void Valider(PlanDeSuppression plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        Exiger(
            StringComparer.Ordinal.Equals(plan.VersionContrat, VersionsContratDuplicateFiles.PlanSecuriseV1),
            "version de contrat du plan non prise en charge");
        Exiger(plan.Propositions is not null, "propositions absentes");
        Exiger(plan.GarantiesParGroupe is not null, "garanties absentes");

        var garanties = new Dictionary<string, GarantieDeGroupe>(StringComparer.Ordinal);
        var fichierIds = new HashSet<string>(StringComparer.Ordinal);
        var chemins = new HashSet<string>(StringComparer.Ordinal);

        foreach (var garantie in plan.GarantiesParGroupe)
        {
            Exiger(garantie is not null, "garantie invalide");
            var hash = ExigerSha256Normalise(garantie.ContenuSha256);
            Exiger(
                StringComparer.Ordinal.Equals(garantie.GroupeId, IdentifiantsStables.PourGroupeExact(hash)),
                "GroupeId de garantie invalide");
            Exiger(garanties.TryAdd(garantie.GroupeId, garantie), "GroupeId de garantie duplique");

            var temoin = garantie.TemoinConservation;
            Exiger(temoin is not null, "temoin de conservation absent");
            var cheminTemoin = ExigerChemin(temoin.Chemin);
            Exiger(
                StringComparer.Ordinal.Equals(
                    temoin.FichierId,
                    IdentifiantsStables.PourFichier(hash, temoin.Chemin)),
                "FichierId du temoin invalide");
            Exiger(fichierIds.Add(temoin.FichierId), "FichierId duplique");
            Exiger(chemins.Add(cheminTemoin), "chemin de fichier duplique");
        }

        var groupesAvecProposition = new HashSet<string>(StringComparer.Ordinal);
        foreach (var proposition in plan.Propositions)
        {
            Exiger(proposition is not null, "proposition invalide");
            Exiger(
                garanties.TryGetValue(proposition.GroupeId, out var garantie),
                "proposition sans garantie");

            var hash = ExigerSha256Normalise(proposition.Contenu);
            Exiger(
                StringComparer.Ordinal.Equals(hash, garantie.ContenuSha256),
                "contenu de proposition incoherent");
            Exiger(
                StringComparer.Ordinal.Equals(proposition.GroupeId, IdentifiantsStables.PourGroupeExact(hash)),
                "GroupeId de proposition invalide");
            Exiger(
                StringComparer.Ordinal.Equals(
                    proposition.FichierId,
                    IdentifiantsStables.PourFichier(hash, proposition.Chemin)),
                "FichierId de proposition invalide");
            Exiger(fichierIds.Add(proposition.FichierId), "FichierId duplique");
            Exiger(chemins.Add(ExigerChemin(proposition.Chemin)), "chemin de fichier duplique");
            groupesAvecProposition.Add(proposition.GroupeId);
        }

        foreach (var groupeId in garanties.Keys)
            Exiger(groupesAvecProposition.Contains(groupeId), "garantie sans proposition");
    }

    private static string ExigerSha256Normalise(string valeur)
    {
        try
        {
            var normalise = IdentifiantsStables.NormaliserSha256(valeur);
            Exiger(StringComparer.Ordinal.Equals(valeur, normalise), "empreinte SHA-256 non normalisee");
            return normalise;
        }
        catch (ArgumentException ex)
        {
            throw new PlanInvalideException($"empreinte SHA-256 invalide: {ex.ParamName}");
        }
    }

    private static string ExigerChemin(string chemin)
    {
        try
        {
            return IdentifiantsStables.NormaliserCheminWindows(chemin);
        }
        catch (ArgumentException ex)
        {
            throw new PlanInvalideException($"chemin invalide: {ex.ParamName}");
        }
    }

    private static void Exiger([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
            throw new PlanInvalideException(message);
    }
}
