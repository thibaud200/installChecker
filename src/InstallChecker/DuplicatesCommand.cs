using System.Text.Encodings.Web;
using System.Text.Json;
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Frontiere;

namespace InstallChecker;

/// <summary>
/// La commande <c>duplicates</c> (module Duplicate Files v1, plan rev3) : câble les mêmes
/// adaptateurs qu'<see cref="IdentityCommand.Deriver"/> sur <see cref="Porteur.Deriver"/>, puis
/// passe W et la même source Ω à <see cref="GenerateurDeRapport.Generer"/> — W et Ω proviennent de
/// la même dérivation (invariant P7). Aucune logique métier ici : l'extraction, le classement et
/// l'assemblage vivent dans <c>InstallChecker.DuplicateFiles</c> ; la commande ne décide rien.
/// Toute erreur du moteur est restituée <b>telle quelle</b>, jamais traduite (même régime
/// qu'<c>identity</c>, 018 § 6). La sortie ne contient que des DTO du module — aucun type moteur.
/// </summary>
public static class DuplicatesCommand
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
            var rapport = GenerateurDeRapport.Generer(w, omega);

            output.WriteLine(JsonSerializer.Serialize(rapport, OptionsJson));
            return 0;
        }
        catch (Exception ex) when (EstUneErreurContractuelle(ex))
        {
            errors.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>
    /// Les erreurs nommées du contrat du moteur, restituées telles quelles — mêmes types que
    /// <see cref="IdentityCommand"/>. Toute autre exception est une défaillance d'implémentation
    /// (011 § 4) : non retenue ici.
    /// </summary>
    private static bool EstUneErreurContractuelle(Exception ex) =>
        ex is ErreurOmega or ErreurDeRegistre or ActeInexistantDansWException;
}
