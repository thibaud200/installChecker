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
/// refus de même (strate, espèce, motif) *que rien d'autre dans l'état ne distingue* en un refus de
/// domaine maximal (014 § 7.3) — une mise en forme, jamais un jugement sur ce qui est refusé ou pourquoi.
/// </summary>
public static class AssemblageDeLetat
{
    public static W Assembler(EnsembleDesActes actes, IndexEtat index)
    {
        var refusAgreges = AgregerLesRefus(actes);

        var tousLesActes = actes.Elections.Select(ActeW.DepuisElection)
            .Concat(refusAgreges.Select(ActeW.DepuisRefus))
            .OrderBy(a => a.Strate)
            .ThenBy(a => a.Type)
            .ThenBy(a => a.Domaine[0])
            .ToList();

        return new W(index, tousLesActes);
    }

    /// <summary>
    /// Applique littéralement 014 § 7.3 : deux refus de même (strate, espèce, motif) ne fusionnent
    /// que si aucun autre acte de l'état (élection, ou refus d'une autre espèce/d'un autre motif) —
    /// à la même strate — ne porte sur un domaine qui recoupe le leur. Un recoupement signalerait
    /// qu'un acte distingue déjà une partie de ce que la fusion prétendrait indistinct ; le groupe
    /// reste alors non fusionné, chaque refus conservé tel que C5 l'a produit.
    /// </summary>
    private static IReadOnlyList<Refus> AgregerLesRefus(EnsembleDesActes actes)
    {
        var resultat = new List<Refus>();

        foreach (var groupe in actes.Refus.GroupBy(r => (r.Strate, r.Espece, r.Motif)))
        {
            var (strate, espece, motif) = groupe.Key;
            var domaineDuGroupe = groupe.SelectMany(r => r.Domaine).Distinct().ToHashSet();

            var autresActesDeLaStrate = actes.Elections
                .Where(e => e.Strate == strate)
                .Select(e => e.Domaine)
                .Concat(actes.Refus
                    .Where(r => r.Strate == strate && (r.Espece != espece || r.Motif != motif))
                    .Select(r => r.Domaine));

            var distingue = autresActesDeLaStrate.Any(domaine => domaine.Any(domaineDuGroupe.Contains));

            if (distingue)
            {
                resultat.AddRange(groupe);
            }
            else
            {
                resultat.Add(new Refus(strate, domaineDuGroupe.OrderBy(id => id).ToList(), espece, motif));
            }
        }

        return resultat;
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
