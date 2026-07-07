using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Erreurs;

namespace InstallChecker.Identity.Tests;

public class LecteurDeRegistreMarkdownTests
{
    private static string RacineDuDepot()
    {
        var repertoire = new DirectoryInfo(AppContext.BaseDirectory);
        while (repertoire is not null && !File.Exists(Path.Combine(repertoire.FullName, "InstallChecker.slnx")))
        {
            repertoire = repertoire.Parent;
        }

        return repertoire?.FullName ?? throw new InvalidOperationException("racine du dépôt introuvable depuis " + AppContext.BaseDirectory);
    }

    private static string CheminFixture(string cas) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "RegistresCasses", cas, "registre");

    // --- ℛ₀ réel (E1) ---

    [Fact]
    public void Lit_R0_complet_deux_conventions_en_vigueur()
    {
        var referentiel = new LecteurDeRegistreMarkdown(Path.Combine(RacineDuDepot(), "registre")).Projeter();

        Assert.Equal(2, referentiel.ConventionsEnVigueur.Count);
        Assert.Equal(
            [new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)],
            referentiel.Index);
    }

    [Fact]
    public void Lit_EQ01_conformement_au_document_015()
    {
        var referentiel = new LecteurDeRegistreMarkdown(Path.Combine(RacineDuDepot(), "registre")).Projeter();
        var eq01 = referentiel.ConventionsEnVigueur.Single(c => c.Identifiant == "EQ-01");

        Assert.Equal(1, eq01.Version);
        Assert.Equal(Famille.Interpretation, eq01.Famille);
        Assert.Empty(eq01.Dependances);
        Assert.Equal(new DateOnly(2026, 7, 5), eq01.Date);
        Assert.Equal("Propriétaire du projet.", eq01.Autorite);
    }

    [Fact]
    public void Lit_CE01_conformement_au_document_015()
    {
        var referentiel = new LecteurDeRegistreMarkdown(Path.Combine(RacineDuDepot(), "registre")).Projeter();
        var ce01 = referentiel.ConventionsEnVigueur.Single(c => c.Identifiant == "CE-01");

        Assert.Equal(1, ce01.Version);
        Assert.Equal(Famille.Election, ce01.Famille);
        Assert.Equal([new ConventionRef("EQ-01", 1)], ce01.Dependances);
    }

    [Fact]
    public void La_dependance_CE01_vers_EQ01_est_satisfaite_par_le_predicat_de_coherence()
    {
        // Ne doit lever aucune exception : c'est la garantie même de C2 (014 § 1, C2).
        var referentiel = new LecteurDeRegistreMarkdown(Path.Combine(RacineDuDepot(), "registre")).Projeter();

        var ce01 = referentiel.ConventionsEnVigueur.Single(c => c.Identifiant == "CE-01");
        Assert.Contains(referentiel.Index, r => r == ce01.Dependances.Single());
    }

    // --- Registre absent ---

    [Fact]
    public void Registre_absent_est_refuse_nommement()
    {
        var chemin = Path.Combine(Path.GetTempPath(), "registre-inexistant-" + Guid.NewGuid());

        Assert.Throws<RegistreAbsentException>(() => new LecteurDeRegistreMarkdown(chemin).Projeter());
    }

    // --- Registres cassés (013 § 6, 014 § 5) ---

    [Fact]
    public void Champ_obligatoire_absent_est_refuse_comme_malforme()
    {
        Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("ChampAbsent")).Projeter());
    }

    [Fact]
    public void Section_inconnue_est_refusee_comme_malformee()
    {
        Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("SectionInconnue")).Projeter());
    }

    [Fact]
    public void Version_incoherente_avec_le_chemin_est_refusee_comme_malformee()
    {
        Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("VersionIncoherente")).Projeter());
    }

    [Fact]
    public void Dependance_manquante_dans_le_repertoire_est_refusee_comme_malformee()
    {
        Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("DependanceManquante")).Projeter());
    }

    [Fact]
    public void Registre_incoherent_dependance_non_en_vigueur_est_refuse_comme_incoherent()
    {
        Assert.Throws<RegistreIncoherentException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("RegistreIncoherent")).Projeter());
    }
}
