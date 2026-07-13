using System.Text.Encodings.Web;
using System.Text.Json;
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Frontiere;

namespace InstallChecker;

/// <summary>
/// La commande <c>plan</c> (module Duplicate Files, A3) : dérive W comme <see cref="DuplicatesCommand"/>,
/// extrait les groupes de contenus identiques, les résout en chemins via Ω, et délègue la
/// construction du plan à <see cref="ConstructeurDePlan"/>. Les identifiants d'actes ne servent que
/// de clé de jointure (domaine → chemin) et n'entrent jamais dans le plan. L'ensemble protégé est
/// vide tant qu'A1 n'est pas implémenté. Toute erreur du moteur est restituée telle quelle.
/// </summary>
public static class PlanCommand
{
    private static readonly JsonSerializerOptions OptionsJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static int Deriver(string cheminBase, string cheminRegistre, TextWriter output, TextWriter errors)
    {
        try
        {
            var omega = new LecteurDObservationsSqlite(cheminBase);
            var w = Porteur.Deriver(omega, new LecteurDeRegistreMarkdown(cheminRegistre));

            var (groupes, _) = ExtracteurDeGroupes.Extraire(w);
            var actes = omega.ProjeterModele().Actes.ToDictionary(a => a.Identifiant);
            var contextes = omega.ProjeterContexte().ToDictionary(c => c.Identifiant);

            var groupesDeChemins = groupes.Select(g =>
                (actes[g.Domaine[0]].Empreinte,
                 (IReadOnlyList<string>)g.Domaine.Select(id => contextes[id].Chemin).ToList()));

            // A1 non implémenté : aucun chemin protégé fourni pour l'instant.
            var plan = ConstructeurDePlan.Construire(groupesDeChemins, new HashSet<string>());

            output.WriteLine(JsonSerializer.Serialize(plan, OptionsJson));
            return 0;
        }
        catch (Exception ex) when (EstUneErreurContractuelle(ex))
        {
            errors.WriteLine(ex.Message);
            return 1;
        }
    }

    private static bool EstUneErreurContractuelle(Exception ex) =>
        ex is ErreurOmega or ErreurDeRegistre or ActeInexistantDansWException;
}
