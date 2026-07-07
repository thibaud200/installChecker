using InstallChecker.Identity.Actes;

namespace InstallChecker.Identity.Etat;

/// <summary>
/// C6 — assemblage de l'état du monde (012 § 1.2, 014 C6). Couche d'assemblage pure : elle ne
/// décide rien (les actes lui arrivent déjà décidés par C5), ne dérive rien, ne compare aucune
/// hypothèse, ne lit jamais Ω ni ℛ (elle reçoit leur index déjà constitué). Elle se borne à mettre
/// en forme canonique (014 § 7) l'ensemble des actes de C5, et à calculer la correspondance entre
/// deux telles formes (τ, 014 § 7.5).
///
/// La seule transformation qu'elle opère sur le contenu des actes est l'agrégation canonique des
/// refus de même (strate, espèce, motif) en un refus de domaine maximal (014 § 7.3) — une mise en
/// forme, jamais un jugement sur ce qui est refusé ou pourquoi.
/// </summary>
public static class AssemblageDeLetat
{
    public static W Assembler(EnsembleDesActes actes, IndexEtat index)
    {
        var refusAgreges = actes.Refus
            .GroupBy(r => (r.Strate, r.Espece, r.Motif))
            .Select(g => new Refus(
                g.Key.Strate,
                g.SelectMany(r => r.Domaine).Distinct().OrderBy(id => id).ToList(),
                g.Key.Espece,
                g.Key.Motif));

        var tousLesActes = actes.Elections.Select(ActeW.DepuisElection)
            .Concat(refusAgreges.Select(ActeW.DepuisRefus))
            .OrderBy(a => a.Strate)
            .ThenBy(a => a.Type)
            .ThenBy(a => a.Domaine[0])
            .ToList();

        return new W(index, tousLesActes);
    }

    public static Transition CalculerTransition(W avant, W apres, Cause cause)
    {
        var actesAvant = avant.Actes.ToDictionary(a => new ReferenceActe(a.Strate, a.Domaine[0]));
        var actesApres = apres.Actes.ToDictionary(a => new ReferenceActe(a.Strate, a.Domaine[0]));

        var conserves = new List<ReferenceActe>();
        var abandonnes = new List<ReferenceActe>();
        var nouveaux = new List<ReferenceActe>();

        foreach (var (reference, acte) in actesAvant)
        {
            if (actesApres.TryGetValue(reference, out var acteApres) && SontIdentiques(acte, acteApres))
            {
                conserves.Add(reference);
            }
            else
            {
                abandonnes.Add(reference);
            }
        }

        foreach (var (reference, acte) in actesApres)
        {
            if (!actesAvant.TryGetValue(reference, out var acteAvant) || !SontIdentiques(acteAvant, acte))
            {
                nouveaux.Add(reference);
            }
        }

        return new Transition(
            avant.Index,
            apres.Index,
            cause,
            new Correspondance(
                Trier(conserves),
                Trier(abandonnes),
                Trier(nouveaux),
                Continuites: []));
    }

    private static List<ReferenceActe> Trier(List<ReferenceActe> references) =>
        references.OrderBy(r => r.Strate).ThenBy(r => r.PlusPetitIdentifiantDuDomaine).ToList();

    /// <summary>
    /// Égalité de contenu entre deux actes de W (006 § 7 : « mêmes hypothèses, mêmes niveaux, mêmes
    /// motifs »). Une comparaison explicite est nécessaire : l'égalité de <c>record</c> ne compare
    /// les listes que par référence, jamais par contenu.
    /// </summary>
    private static bool SontIdentiques(ActeW a, ActeW b) =>
        a.Type == b.Type
        && a.Strate == b.Strate
        && a.Domaine.SequenceEqual(b.Domaine)
        && a.Contenu == b.Contenu
        && a.Niveau == b.Niveau
        && a.Motif == b.Motif
        && a.Espece == b.Espece
        && SequencesEgalesOuAbsentes(a.Licences, b.Licences)
        && SequencesEgalesOuAbsentes(a.Dependances, b.Dependances)
        && SequencesEgalesOuAbsentes(a.Dette, b.Dette);

    private static bool SequencesEgalesOuAbsentes<T>(IReadOnlyList<T>? a, IReadOnlyList<T>? b) =>
        (a is null && b is null) || (a is not null && b is not null && a.SequenceEqual(b));
}
