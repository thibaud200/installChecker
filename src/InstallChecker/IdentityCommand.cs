using System.Text.Encodings.Web;
using System.Text.Json;
using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Frontiere;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker;

/// <summary>
/// La commande <c>identity</c> (013 § 1.1, jalon É9 ; 018 § 6) : le consommateur du moteur —
/// « jamais fusionné avec le moteur ». Elle câble les adaptateurs d'<c>Identity.Access</c> sur les
/// ports du porteur, invoque le porteur — et lui seul (I65) —, émet W tel que produit et restitue
/// l'audit unité par unité. Toute erreur du moteur est restituée <b>telle quelle</b> — jamais
/// traduite, renommée, dégradée ni agrégée (018 § 6, le patron du 014 § 7.4). Aucune logique
/// métier : le consommateur n'influence jamais W (EXG-16), ne compare jamais deux W (011 § 9 :
/// τ est la seule comparaison), ne complète, ne filtre ni ne réordonne rien, n'écrit ni dans Ω ni
/// dans ℛ (011 § 11). L'émission suit les décisions du 013 § 4 (JSON, ordre canonique des actes
/// déjà porté par W, champs dans l'ordre du 014 § 7.3) ; la forme canonique <b>matérielle</b> bit
/// à bit relève du report 3 (016 § 4.1) et n'est pas spécifiée ici.
/// </summary>
public static class IdentityCommand
{
    private static readonly JsonSerializerOptions OptionsJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public static int Deriver(string cheminBase, string cheminRegistre, TextWriter output, TextWriter errors)
    {
        try
        {
            var w = Porteur.Deriver(
                new LecteurDObservationsSqlite(cheminBase),
                new LecteurDeRegistreMarkdown(cheminRegistre));

            output.WriteLine(JsonSerializer.Serialize(FormeEmise(w), OptionsJson));
            return 0;
        }
        catch (Exception ex) when (EstUneErreurContractuelle(ex))
        {
            errors.WriteLine(ex.Message);
            return 1;
        }
    }

    public static int Auditer(
        string cheminBase, string cheminRegistre, string question, string strate, long acteId,
        TextWriter output, TextWriter errors)
    {
        if (!Enum.TryParse<Strate>(strate, ignoreCase: true, out var strateAnalysee))
        {
            errors.WriteLine($"strate inconnue : {strate} (attendues : contenu, variante, version, identite, famille)");
            return 2;
        }

        var omega = new LecteurDObservationsSqlite(cheminBase);
        var registre = new LecteurDeRegistreMarkdown(cheminRegistre);
        try
        {
            object? reponse = question switch
            {
                "pourquoi-election" => Porteur.PourquoiCetteElection(omega, registre, strateAnalysee, acteId),
                "pourquoi-refus" => Porteur.PourquoiCeRefus(omega, registre, strateAnalysee, acteId),
                "conventions" => Porteur.DeQuellesConventionsDependCetActe(omega, registre, strateAnalysee, acteId),
                "observations" => Porteur.DeQuellesObservationsDependIl(omega, registre, strateAnalysee, acteId),
                "ecartees" => Porteur.QuALonEcarte(omega, registre, strateAnalysee, acteId),
                "renier" => Porteur.QueFaudraitIlRenierPourQueCeciTombe(omega, registre, strateAnalysee, acteId),
                _ => null,
            };

            if (reponse is null)
            {
                errors.WriteLine(
                    $"question inconnue : {question} (attendues : pourquoi-election, pourquoi-refus, conventions, observations, ecartees, renier)");
                return 2;
            }

            output.WriteLine(JsonSerializer.Serialize(reponse, OptionsJson));
            return 0;
        }
        catch (Exception ex) when (EstUneErreurContractuelle(ex))
        {
            errors.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>
    /// Les erreurs nommées du contrat (011 § 5 lu sur sept entrées — 017 § 6 — et le cas propre de
    /// C7, 014 § 1) : restituées telles quelles. Toute autre exception est une défaillance de
    /// l'implémentation — « elle doit se signaler comme telle » (011 § 4) : elle n'est pas retenue ici.
    /// </summary>
    private static bool EstUneErreurContractuelle(Exception ex) =>
        ex is ErreurOmega or ErreurDeRegistre or ActeInexistantDansWException;

    /// <summary>La forme émise de W : les sections et l'ordre des champs du 014 § 7 — présentation du consommateur, jamais la forme canonique matérielle (report 3).</summary>
    private static object FormeEmise(W w) => new
    {
        index = new
        {
            omega = new
            {
                version = w.Index.Omega.Version,
                nombreActes = w.Index.Omega.NombreActes,
                empreinteEtat = w.Index.Omega.EmpreinteEtat,
            },
            registre = w.Index.Registre.Select(r => new { identifiant = r.Identifiant, version = r.Version }),
        },
        actes = w.Actes.Select(a => new
        {
            type = a.Type == TypeActe.Election ? "élection" : "refus",
            strate = a.Strate.ToString().ToLowerInvariant(),
            domaine = a.Domaine,
            contenu = a.Contenu,
            niveau = a.Niveau?.ToString().ToLowerInvariant(),
            motif = a.Motif,
            espece = a.Espece?.ToString().ToLowerInvariant(),
            licences = a.Licences?.Select(r => new { identifiant = r.Identifiant, version = r.Version }),
            dependances = a.Dependances?.Select(r => new { identifiant = r.Identifiant, version = r.Version }),
            dette = a.Dette?.Select(r => new { identifiant = r.Identifiant, version = r.Version }),
        }),
    };
}
