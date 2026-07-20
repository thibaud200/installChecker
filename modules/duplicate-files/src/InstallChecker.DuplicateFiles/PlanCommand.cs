using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Frontiere;
using InstallChecker.Scanner.Observations;

namespace InstallChecker;

/// <summary>
/// La commande <c>plan</c> (module Duplicate Files, A3) : dérive W comme <see cref="DuplicatesCommand"/>,
/// extrait les groupes de contenus identiques, applique le même classement de rétention que le
/// rapport, puis délègue la construction du plan à <see cref="ConstructeurDePlan"/>. Les identifiants
/// d'actes ne servent que de clé de jointure et n'entrent jamais dans le plan. Toute erreur du moteur
/// est restituée telle quelle.
/// </summary>
public static class PlanCommand
{
    private static readonly JsonSerializerOptions OptionsJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    public static int Deriver(string cheminBase, string cheminRegistre, TextWriter output, TextWriter errors)
    {
        try
        {
            var omega = SourceObservationsSqlite.Ouvrir(cheminBase);
            var w = Porteur.Deriver(omega, new LecteurDeRegistreMarkdown(cheminRegistre));

            var (groupes, _) = ExtracteurDeGroupes.Extraire(w);
            var actes = omega.ProjeterModele().Actes.ToDictionary(a => a.Identifiant);
            var contextes = omega.ProjeterContexte().ToDictionary(c => c.Identifiant);

            var groupesDeChemins = groupes.Select(g =>
            {
                var fichiers = EnrichisseurDeGroupe.Enrichir(g.Domaine, actes, contextes);
                var cheminsClasses = PolitiqueRetentionV1.Classer(fichiers)
                    .Select(e => e.Fichier.Chemin)
                    .ToList();
                return (actes[g.Domaine[0]].Empreinte, (IReadOnlyList<string>)cheminsClasses);
            });

            var plan = ConstructeurDePlan.Construire(groupesDeChemins, ProtectionDesChemins.EstProtegeParDefaut);

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
