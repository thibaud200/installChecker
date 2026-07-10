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

    // --- Divergence 1 (revue de clôture) : deux versions d'un même identifiant en vigueur simultanément ---

    [Fact]
    public void Deux_versions_dun_meme_identifiant_en_vigueur_sont_refusees_comme_incoherentes()
    {
        Assert.Throws<RegistreIncoherentException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("DeuxVersionsMemeIdentifiant")).Projeter());
    }

    // --- D1 (audit final) : aucune exception .NET ne doit fuiter sur une section dupliquée ---

    [Fact]
    public void Section_dupliquee_dans_etat_md_est_refusee_comme_malformee()
    {
        Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("SectionEtatDupliquee")).Projeter());
    }

    [Fact]
    public void Sous_section_dupliquee_dans_une_entree_dhistorique_est_refusee_comme_malformee()
    {
        Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("SousSectionHistoriqueDupliquee")).Projeter());
    }

    // --- V2-2 : la quatrième vérification et la septième erreur (017 §§ 4–8) ---

    [Fact]
    public void Registre_valide_coherent_non_couvert_est_refuse_comme_non_couvert()
    {
        // La fixture d'espèce nouvelle du 017 § 10 : rien n'y est « cassé » en soi — son défaut
        // n'existe que relativement à la couverture déclarée du moteur.
        Assert.Throws<RegistreNonCouvertException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("RegistreNonCouvert")).Projeter());
    }

    [Fact]
    public void La_septieme_erreur_identifie_les_conventions_fautives_leur_famille_et_la_clause_violee()
    {
        var erreur = Assert.Throws<RegistreNonCouvertException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("RegistreNonCouvert")).Projeter());

        // 017 § 6 : l'entrée fautive (la ou les conventions concernées avec leur famille)…
        Assert.Contains("AT-01 v1", erreur.Message);
        Assert.Contains("Attente", erreur.Message);
        // … et elle seule : EQ-01, couverte, n'est pas fautive.
        Assert.DoesNotContain("EQ-01", erreur.Message);
        // … et la clause violée (la quatrième précondition, 017 § 4).
        Assert.Contains("quatrième précondition", erreur.Message);
    }

    [Fact]
    public void Le_signalement_de_la_septieme_erreur_est_deterministe()
    {
        // I64 : même registre, même version de moteur ⟹ même erreur — jusqu'au message.
        var premiere = Assert.Throws<RegistreNonCouvertException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("RegistreNonCouvert")).Projeter());
        var seconde = Assert.Throws<RegistreNonCouvertException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("RegistreNonCouvert")).Projeter());

        Assert.Equal(premiere.Message, seconde.Message);
    }

    [Fact]
    public void Malforme_et_non_couvert_produit_malforme()
    {
        // 017 § 8 : absence < forme < cohérence < couverture — le premier échec est signalé ;
        // la couverture ne masque jamais une erreur qui la précède.
        Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("MalformeEtNonCouvert")).Projeter());
    }

    [Fact]
    public void Incoherent_et_non_couvert_produit_incoherent()
    {
        Assert.Throws<RegistreIncoherentException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("IncoherentEtNonCouvert")).Projeter());
    }

    // --- V3-5 : la date des entrées et l'ordre chronologique (023 § 2, report 7) ---

    [Fact]
    public void Un_journal_desordonne_est_refuse_comme_malforme()
    {
        // 015 § 6.3 (« ordre chronologique non décroissant »), rendu exigible de C2 par le 023 § 2.
        var erreur = Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("JournalDesordonne")).Projeter());

        Assert.Contains("ordre chronologique", erreur.Message);
    }

    [Fact]
    public void Une_entree_sans_date_de_titre_est_refusee_comme_malformee()
    {
        // 014 § 5.2 : « entrée sans l'un de ces éléments » — la date, désormais lisible (023 § 2).
        var erreur = Assert.Throws<RegistreMalformeException>(
            () => new LecteurDeRegistreMarkdown(CheminFixture("EntreeSansDateDeTitre")).Projeter());

        Assert.Contains("date ISO", erreur.Message);
    }
}
