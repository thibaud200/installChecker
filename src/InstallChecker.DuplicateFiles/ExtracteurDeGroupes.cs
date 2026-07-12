using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Sépare les actes de W en exactement deux flux (plan rev3 § 3, correction P2) :
/// <list type="bullet">
///   <item>les <b>groupes</b> — élections certaines de la strate contenu licenciées par CE-01
///   (D1/D2 : le v1 se limite à cette strate ; les strates supérieures n'ont aucune convention en
///   vigueur) ;</item>
///   <item>les <b>refus des strates supérieures</b> — les refus globaux portés par variante,
///   version, identité ou famille, collectés pour alimenter la note de capacité, jamais restitués
///   tels quels dans le rapport utilisateur.</item>
/// </list>
/// La strate contenu ne produit aucun refus par construction : ce flux ne l'expose donc pas.
/// </summary>
public static class ExtracteurDeGroupes
{
    public static (IReadOnlyList<ActeW> Groupes, IReadOnlyList<ActeW> RefusStratesSuperieures) Extraire(W w)
    {
        var groupes = w.Actes
            .Where(a => a.Type == TypeActe.Election
                     && a.Strate == Strate.Contenu
                     && a.Niveau == Niveau.Certaine
                     && a.Licences is not null
                     && a.Licences.Any(l => l.Identifiant == "CE-01"))
            .ToList();

        var refusStratesSuperieures = w.Actes
            .Where(a => a.Type == TypeActe.Refus && a.Strate != Strate.Contenu)
            .ToList();

        return (groupes, refusStratesSuperieures);
    }
}
