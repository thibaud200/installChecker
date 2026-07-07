using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.Identity.Etat;

/// <summary>
/// Une ligne de la section <c>actes</c> de W (014 § 7.3) : le champ obligatoire pour l'un des deux
/// types, absent (« — ») pour l'autre — exactement la table du document, sans champ supplémentaire
/// ni omis. Construite uniquement depuis un <see cref="ActeElection"/> ou un <see cref="Refus"/>
/// déjà décidés par C5 ; C6 ne fait qu'en changer la forme.
/// </summary>
public sealed record ActeW(
    TypeActe Type,
    Strate Strate,
    IReadOnlyList<long> Domaine,
    string? Contenu,
    Niveau? Niveau,
    string Motif,
    Espece? Espece,
    IReadOnlyList<ConventionRef>? Licences,
    IReadOnlyList<ConventionRef>? Dependances,
    IReadOnlyList<ConventionRef>? Dette)
{
    public static ActeW DepuisElection(ActeElection e) => new(
        TypeActe.Election, e.Strate, e.Domaine, e.ContenuPropositionnel, e.Niveau, e.Motif,
        Espece: null, e.Licences, e.Dependances, e.Dette);

    public static ActeW DepuisRefus(Refus r) => new(
        TypeActe.Refus, r.Strate, r.Domaine, Contenu: null, Niveau: null, r.Motif, r.Espece,
        Licences: null, Dependances: null, Dette: null);
}
