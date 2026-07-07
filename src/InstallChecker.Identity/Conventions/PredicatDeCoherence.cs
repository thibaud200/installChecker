using InstallChecker.Identity.Erreurs;

namespace InstallChecker.Identity.Conventions;

/// <summary>
/// Le prédicat de cohérence du registre (008 § 4), appliqué aux seules versions en vigueur (014 § 5.3) :
/// au plus une version par identifiant est simultanément en vigueur, toute dépendance déclarée doit
/// elle-même être en vigueur, et le graphe de dépendances est acyclique.
/// </summary>
public static class PredicatDeCoherence
{
    public static void Verifier(IReadOnlyList<Convention> conventionsEnVigueur)
    {
        VerifierUniciteDeLidentifiant(conventionsEnVigueur);

        var enVigueur = conventionsEnVigueur.ToDictionary(c => c.Ref);

        foreach (var convention in conventionsEnVigueur)
        {
            foreach (var dependance in convention.Dependances)
            {
                if (!enVigueur.ContainsKey(dependance))
                {
                    throw new RegistreIncoherentException(
                        $"dépendance insatisfaite : {convention.Identifiant} v{convention.Version} dépend de " +
                        $"{dependance.Identifiant} v{dependance.Version}, absente des conventions en vigueur");
                }
            }
        }

        VerifierAcyclicite(conventionsEnVigueur, enVigueur);
    }

    private static void VerifierUniciteDeLidentifiant(IReadOnlyList<Convention> conventionsEnVigueur)
    {
        var doublon = conventionsEnVigueur
            .GroupBy(c => c.Identifiant, StringComparer.Ordinal)
            .FirstOrDefault(groupe => groupe.Count() > 1);

        if (doublon is not null)
        {
            var versions = string.Join(", ", doublon.Select(c => $"v{c.Version}").OrderBy(v => v, StringComparer.Ordinal));
            throw new RegistreIncoherentException(
                $"plusieurs versions de {doublon.Key} sont simultanément en vigueur : {versions}");
        }
    }

    private static void VerifierAcyclicite(
        IReadOnlyList<Convention> conventions,
        IReadOnlyDictionary<ConventionRef, Convention> enVigueur)
    {
        var enCoursDeVisite = new HashSet<ConventionRef>();
        var visitees = new HashSet<ConventionRef>();

        foreach (var convention in conventions)
        {
            Visiter(convention);
        }

        void Visiter(Convention convention)
        {
            var reference = convention.Ref;
            if (visitees.Contains(reference)) return;

            if (!enCoursDeVisite.Add(reference))
            {
                throw new RegistreIncoherentException(
                    $"cycle de dépendances détecté impliquant {reference.Identifiant} v{reference.Version}");
            }

            foreach (var dependance in convention.Dependances)
            {
                Visiter(enVigueur[dependance]);
            }

            enCoursDeVisite.Remove(reference);
            visitees.Add(reference);
        }
    }
}
