using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles;

public static class ProjectionFichiersObserves
{
    public static IReadOnlyList<FichierObserve> Projeter(IObservationsSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var actes = source.ProjeterModele().Actes;
        var contextes = source.ProjeterContexte();

        var actesParId = IndexerActes(actes);
        var contextesParId = IndexerContextes(contextes);

        var contexteManquant = actesParId.Keys
            .Except(contextesParId.Keys)
            .Order()
            .Select(id => (long?)id)
            .FirstOrDefault();
        if (contexteManquant is long identifiantSansContexte)
            throw new InvalidOperationException($"contexte absent pour l'acte {identifiantSansContexte}");

        var contexteOrphelin = contextesParId.Keys
            .Except(actesParId.Keys)
            .Order()
            .Select(id => (long?)id)
            .FirstOrDefault();
        if (contexteOrphelin is long identifiantOrphelin)
            throw new InvalidOperationException($"contexte orphelin pour l'acte {identifiantOrphelin}");

        return actesParId.Values
            .OrderBy(a => a.Identifiant)
            .Select(acte =>
            {
                var chemin = contextesParId[acte.Identifiant].Chemin;
                var contenu = IdentifiantsStables.NormaliserSha256(acte.Empreinte);
                return new FichierObserve(
                    acte.Identifiant,
                    IdentifiantsStables.PourFichier(contenu, chemin),
                    chemin,
                    acte.Taille,
                    contenu,
                    acte.Attributs);
            })
            .ToList();
    }

    private static Dictionary<long, ActeObservation> IndexerActes(
        IReadOnlyList<ActeObservation> actes)
    {
        var resultat = new Dictionary<long, ActeObservation>();
        foreach (var acte in actes)
        {
            if (!resultat.TryAdd(acte.Identifiant, acte))
                throw new InvalidOperationException($"acte dupliqué : {acte.Identifiant}");
        }
        return resultat;
    }

    private static Dictionary<long, ContexteObservation> IndexerContextes(
        IReadOnlyList<ContexteObservation> contextes)
    {
        var resultat = new Dictionary<long, ContexteObservation>();
        foreach (var contexte in contextes)
        {
            if (!resultat.TryAdd(contexte.Identifiant, contexte))
                throw new InvalidOperationException($"contexte dupliqué : {contexte.Identifiant}");
        }
        return resultat;
    }
}
