namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Construit le plan de suppression (spec A3) à partir de groupes de chemins de même contenu et de
/// l'ensemble des chemins protégés. Composant pur, entièrement ignorant du moteur : il ne connaît
/// ni Ω, ni W, ni la politique de rétention ; il ne reçoit que des chemins et un contenu opaque.
/// Il n'introduit aucun ordre : il préserve celui reçu, ne trie ni ne compare jamais de chemin.
/// </summary>
public static class ConstructeurDePlan
{
    public static PlanDeSuppression Construire(
        IEnumerable<(string Contenu, IReadOnlyList<string> Chemins)> groupes,
        IReadOnlySet<string> cheminsProteges)
    {
        var propositions = new List<PropositionDeSuppression>();

        foreach (var (contenu, chemins) in groupes)
        {
            if (chemins.Count < 2)
                continue; // seuls les groupes d'au moins deux chemins sont concernés

            var nonProteges = chemins.Where(c => !cheminsProteges.Contains(c)).ToList();
            var aCheminProtege = nonProteges.Count != chemins.Count;

            // Un groupe avec au moins un chemin protégé subsiste par ce protégé : tous les
            // non-protégés peuvent être proposés. Sinon, on conserve mécaniquement le premier de
            // l'ordre reçu — pour qu'au moins une copie demeure, jamais un « survivant » choisi.
            var aProposer = aCheminProtege ? nonProteges : nonProteges.Skip(1);

            foreach (var chemin in aProposer)
                propositions.Add(new PropositionDeSuppression(contenu, chemin));
        }

        return new PlanDeSuppression(propositions);
    }
}
