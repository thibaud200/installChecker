namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Construit le plan de suppression (spec A3) à partir de groupes de chemins de même contenu et
/// d'une politique de protection. Composant pur, entièrement ignorant du moteur : il ne connaît
/// ni Ω, ni W, ni la politique de rétention ; il ne reçoit que des chemins ordonnés et un contenu opaque.
/// Il n'introduit aucun ordre : il préserve celui reçu, ne trie ni ne compare jamais de chemin.
/// </summary>
public static class ConstructeurDePlan
{
    public static PlanDeSuppression Construire(
        IEnumerable<(string Contenu, IReadOnlyList<string> Chemins)> groupes,
        IReadOnlySet<string> cheminsProteges) =>
        Construire(groupes, cheminsProteges.Contains);

    public static PlanDeSuppression Construire(
        IEnumerable<(string Contenu, IReadOnlyList<string> Chemins)> groupes,
        Func<string, bool> cheminProtege)
    {
        var propositions = new List<PropositionDeSuppression>();
        var garanties = new List<GarantieDeGroupe>();

        foreach (var (contenu, chemins) in groupes)
        {
            if (chemins.Count < 2)
                continue; // seuls les groupes d'au moins deux chemins sont concernés

            var contenuSha256 = IdentifiantsStables.NormaliserSha256(contenu);
            var groupeId = IdentifiantsStables.PourGroupeExact(contenuSha256);
            var temoin = chemins[0];
            var aProposer = chemins.Skip(1).Where(c => !cheminProtege(c)).ToList();

            if (aProposer.Count == 0)
                continue;

            garanties.Add(new GarantieDeGroupe(
                groupeId,
                contenuSha256,
                new TemoinDeConservation(
                    IdentifiantsStables.PourFichier(contenuSha256, temoin),
                    temoin)));

            foreach (var chemin in aProposer)
                propositions.Add(new PropositionDeSuppression(
                    contenuSha256,
                    chemin,
                    groupeId,
                    IdentifiantsStables.PourFichier(contenuSha256, chemin)));
        }

        return new PlanDeSuppression(
            propositions,
            VersionsContratDuplicateFiles.PlanSecuriseV1,
            garanties);
    }
}
