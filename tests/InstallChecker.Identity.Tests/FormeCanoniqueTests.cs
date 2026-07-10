using System.Text;
using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Frontiere;

namespace InstallChecker.Identity.Tests;

/// <summary>
/// La forme canonique matérielle (report 3) : le test d'or — l'émission du moteur contre le
/// fichier W₀ attendu produit par l'oracle indépendant (014 § 10, É7 : « script hors moteur,
/// dans un autre langage ») — l'égalité est <b>bit à bit</b> (EXG-18, EXG-26) ; le déterminisme
/// d'EXG-27 porte enfin sur des octets émis.
/// </summary>
public class FormeCanoniqueTests
{
    private static string RacineDuDepot()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        return repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable");
    }

    private static LecteurDObservationsSqlite OmegaOracle() =>
        new(Path.Combine(RacineDuDepot(), "tests", "oracle", "corpus1-postA1.db"));

    private static LecteurDeRegistreMarkdown RegistreReel() =>
        new(Path.Combine(RacineDuDepot(), "registre"));

    [Fact]
    public void Le_test_dor_lemission_du_moteur_est_bit_a_bit_le_fichier_W0_attendu_de_loracle_independant()
    {
        var emission = FormeCanonique.Emettre(Porteur.Deriver(OmegaOracle(), RegistreReel()));

        var attendu = File.ReadAllBytes(Path.Combine(RacineDuDepot(), "tests", "oracle", "W0-attendu.json"));

        Assert.Equal(attendu, Encoding.UTF8.GetBytes(emission));
    }

    [Fact]
    public void Deux_emissions_de_W0_sont_identiques_bit_a_bit()
    {
        // EXG-27 : « double exécution → identité bit à bit (EXG-18) » — sur les octets émis.
        var premiere = FormeCanonique.Emettre(Porteur.Deriver(OmegaOracle(), RegistreReel()));
        var seconde = FormeCanonique.Emettre(Porteur.Deriver(OmegaOracle(), RegistreReel()));

        Assert.Equal(Encoding.UTF8.GetBytes(premiere), Encoding.UTF8.GetBytes(seconde));
    }

    [Fact]
    public void Lemission_respecte_la_consignation_LF_exclusif_non_ASCII_en_clair_LF_final_unique()
    {
        var emission = FormeCanonique.Emettre(Porteur.Deriver(OmegaOracle(), RegistreReel()));

        Assert.DoesNotContain('\r', emission);            // LF exclusif, sur toute plateforme
        Assert.Contains("\"élection\"", emission);        // non-ASCII en clair, jamais \uXXXX
        Assert.Contains("\"préalable-absent\"", emission);
        Assert.DoesNotContain("\\u", emission);
        Assert.EndsWith("}\n", emission);                 // LF final unique
        Assert.False(emission.EndsWith("}\n\n"));
    }

    [Fact]
    public void Lemission_de_tau_est_canonique_et_deterministe()
    {
        var premiere = FormeCanonique.Emettre(Porteur.Transitionner(OmegaOracle(), RegistreReel(), OmegaOracle(), RegistreReel()));
        var seconde = FormeCanonique.Emettre(Porteur.Transitionner(OmegaOracle(), RegistreReel(), OmegaOracle(), RegistreReel()));

        Assert.Equal(premiere, seconde);
        Assert.StartsWith("{\n  \"index-avant\": {\n", premiere);
        Assert.Contains("\"cause\": {},", premiere);      // index égaux : la cause vide (026 § 3)
        Assert.Contains("\"continuites\": [\n", premiere);
        Assert.DoesNotContain('\r', premiere);
        Assert.EndsWith("}\n", premiere);
    }
}
