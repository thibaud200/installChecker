using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Auxiliaire;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Frontiere;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;
using InstallChecker.Identity.Signaux;

namespace InstallChecker.Identity.Tests;

/// <summary>
/// Le porteur (018, jalon V2-5) : W₀, τ et l'audit par la frontière publique ; l'ordre total du
/// signalement (I67) ; « entier ou absent » ; aucune production propre (I66). Les couches restent
/// exercées isolément par leurs suites (012 § 8, via InternalsVisibleTo) — ces tests-ci exercent
/// la composition, pas les couches.
/// </summary>
public class PorteurTests
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

    private static LecteurDeRegistreMarkdown RegistreCasse(string cas) =>
        new(Path.Combine(AppContext.BaseDirectory, "Fixtures", "RegistresCasses", cas, "registre"));

    private static SourceObservationsEnMemoire OmegaValideEnMemoire() =>
        new(new ModeleObservations([
            new ActeObservation(1, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
            new ActeObservation(2, 1, "A", new Dictionary<Attribut, ValeurObservee>()),
        ]), []);

    // --- Doubles d'entrée en échec : simulent les refus nommés de C1/C2 pour la matrice I67
    //     (les vérifications elles-mêmes sont exercées par les suites des adaptateurs) ---

    private sealed class OmegaEnEchec(ErreurOmega erreur) : IObservationsSource
    {
        public ModeleObservations ProjeterModele() => throw erreur;

        public IReadOnlyList<ContexteObservation> ProjeterContexte() => throw erreur;
    }

    private sealed class RegistreEnEchec(ErreurDeRegistre erreur) : IRegistreSource
    {
        public Referentiel Projeter() => throw erreur;
    }

    private sealed class RegistreEnMemoire(Referentiel referentiel) : IRegistreSource
    {
        public Referentiel Projeter() => referentiel;
    }

    private static string Liste(IReadOnlyList<ConventionRef>? refs) =>
        refs is null ? "-" : string.Join(";", refs);

    private static string Cle(W w) =>
        $"omega={w.Index.Omega.Version}:{w.Index.Omega.NombreActes}:{w.Index.Omega.EmpreinteEtat}|" +
        $"registre=[{string.Join(";", w.Index.Registre)}]|" +
        string.Join("|", w.Actes.Select(a =>
            $"{a.Type}:{a.Strate}:[{string.Join(",", a.Domaine)}]:{a.Contenu}:{a.Niveau}:{a.Motif}:{a.Espece}:" +
            $"lic=[{Liste(a.Licences)}]:dep=[{Liste(a.Dependances)}]:dette=[{Liste(a.Dette)}]"));

    // --- W₀ par le porteur — et rien d'autre que les productions des couches (I66) ---

    [Fact]
    public void W0_est_produit_par_le_porteur_identique_a_la_composition_des_couches()
    {
        var w = Porteur.Deriver(OmegaOracle(), RegistreReel());

        Assert.Equal(116, w.Actes.Count);
        Assert.Equal(112, w.Actes.Count(a => a.Type == TypeActe.Election));

        // I66 : le porteur ne produit aucun objet propre — sa sortie coïncide, champ par champ,
        // avec la composition directe des couches sur les mêmes entrées.
        var modele = OmegaOracle().ProjeterModele();
        var referentiel = RegistreReel().Projeter();
        var hypotheses = ConstructionDesHypotheses.Construire(DerivationDesSignaux.Deriver(modele, referentiel));
        var actes = DecisionDesActes.Decider(hypotheses, referentiel, modele.Actes.Select(a => a.Identifiant).ToList());
        var manuel = AssemblageDeLetat.Assembler(
            actes, new IndexEtat(IndexOmegaCalculateur.Calculer(modele), referentiel.Index));

        Assert.Equal(Cle(manuel), Cle(w));
    }

    // --- τ par le porteur : les deux membres dans l'ordre du 014 § 7.5, la cause telle quelle ---

    [Fact]
    public void La_transition_entre_deux_index_identiques_conserve_tout_et_transporte_la_cause_telle_quelle()
    {
        var cause = new Cause(TypeCause.Omega, "cause de test — transportée telle quelle (018 § 3, report 9)");

        var tau = Porteur.Transitionner(OmegaOracle(), RegistreReel(), OmegaOracle(), RegistreReel(), cause);

        Assert.Equal(116, tau.Correspondance.Conserves.Count);
        Assert.Empty(tau.Correspondance.Abandonnes);
        Assert.Empty(tau.Correspondance.Nouveaux);
        Assert.Same(cause, tau.Cause); // ni vérifiée, ni redéfinie, ni copiée : telle quelle
    }

    // --- audit par le porteur : re-dérivé de l'index, identique à C7, déterministe ---

    [Fact]
    public void Laudit_par_le_porteur_repond_comme_C7_et_se_rederive_a_lidentique()
    {
        var w = Porteur.Deriver(OmegaOracle(), RegistreReel());
        var election = w.Actes.First(a => a.Type == TypeActe.Election);
        var premiere = Porteur.PourquoiCetteElection(OmegaOracle(), RegistreReel(), election.Strate, election.Domaine[0]);
        var seconde = Porteur.PourquoiCetteElection(OmegaOracle(), RegistreReel(), election.Strate, election.Domaine[0]);

        Assert.NotEmpty(premiere.Maillons);
        Assert.Null(premiere.ManqueNomme);
        Assert.Equal(premiere.Maillons.Count, seconde.Maillons.Count);
        for (var i = 0; i < premiere.Maillons.Count; i++)
        {
            Assert.Equal(premiere.Maillons[i].Couche, seconde.Maillons[i].Couche);
            Assert.Equal(premiere.Maillons[i].ObjetProduit, seconde.Maillons[i].ObjetProduit);
            Assert.Equal(premiere.Maillons[i].Conventions, seconde.Maillons[i].Conventions);
        }

        var dependances = Porteur.DeQuellesConventionsDependCetActe(OmegaOracle(), RegistreReel(), election.Strate, election.Domaine[0]);
        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("EQ-01", 1)], dependances.Dependances);
    }

    [Fact]
    public void Une_question_sur_un_acte_inexistant_est_refusee_nommement_par_C7_a_travers_le_porteur()
    {
        Assert.Throws<ActeInexistantDansWException>(
            () => Porteur.PourquoiCetteElection(OmegaOracle(), RegistreReel(), Strate.Contenu, -1));
    }

    // --- I67 : la matrice multi-défauts — le premier échec de l'ordre total du 018 § 4, toujours ---

    [Fact]
    public void La_matrice_multi_defauts_signale_toujours_le_premier_echec_de_lordre_total()
    {
        var casMultiDefauts = new (IObservationsSource Omega, IRegistreSource Registre, Type Attendu)[]
        {
            // Ω absent < tout ℛ : le bloc Ω précède le bloc ℛ (018 § 4).
            (new OmegaEnEchec(new OmegaAbsentException("t")), RegistreCasse("RegistreNonCouvert"), typeof(OmegaAbsentException)),
            // Ω incompatible < Ω invalide et < registre absent.
            (new OmegaEnEchec(new OmegaIncompatibleException("t")), new RegistreEnEchec(new RegistreAbsentException("t")), typeof(OmegaIncompatibleException)),
            // Ω invalide < registre malformé.
            (new OmegaEnEchec(new OmegaInvalideException("t")), RegistreCasse("MalformeEtNonCouvert"), typeof(OmegaInvalideException)),
            // Ω valide : le bloc ℛ signale selon 017 § 8 — absence < forme < cohérence < couverture.
            (OmegaValideEnMemoire(), new LecteurDeRegistreMarkdown(Path.Combine(Path.GetTempPath(), "registre-inexistant-" + Guid.NewGuid())), typeof(RegistreAbsentException)),
            (OmegaValideEnMemoire(), RegistreCasse("MalformeEtNonCouvert"), typeof(RegistreMalformeException)),
            (OmegaValideEnMemoire(), RegistreCasse("IncoherentEtNonCouvert"), typeof(RegistreIncoherentException)),
            (OmegaValideEnMemoire(), RegistreCasse("RegistreNonCouvert"), typeof(RegistreNonCouvertException)),
        };

        foreach (var (omega, registre, attendu) in casMultiDefauts)
        {
            var erreur = Record.Exception(() => Porteur.Deriver(omega, registre));
            Assert.NotNull(erreur);
            Assert.IsType(attendu, erreur);
        }
    }

    [Fact]
    public void Le_signalement_du_porteur_est_deterministe_jusquau_message()
    {
        var premiere = Record.Exception(() => Porteur.Deriver(OmegaValideEnMemoire(), RegistreCasse("RegistreNonCouvert")));
        var seconde = Record.Exception(() => Porteur.Deriver(OmegaValideEnMemoire(), RegistreCasse("RegistreNonCouvert")));

        Assert.NotNull(premiere);
        Assert.NotNull(seconde);
        Assert.Equal(premiere.GetType(), seconde.GetType());
        Assert.Equal(premiere.Message, seconde.Message);
    }

    // --- les erreurs sont surfacées telles quelles : jamais renommées, jamais converties (018 § 3) ---

    [Fact]
    public void Lerreur_surfacee_par_le_porteur_est_exactement_celle_de_la_couche()
    {
        var directe = Record.Exception(() => RegistreCasse("RegistreNonCouvert").Projeter());
        var parLePorteur = Record.Exception(() => Porteur.Deriver(OmegaValideEnMemoire(), RegistreCasse("RegistreNonCouvert")));

        Assert.NotNull(directe);
        Assert.NotNull(parLePorteur);
        Assert.Equal(directe.GetType(), parLePorteur.GetType());
        Assert.Equal(directe.Message, parLePorteur.Message);
    }

    // --- « entier ou absent » à la frontière (011 § 4) : chaque invocation échoue en entier ---

    [Fact]
    public void Toute_invocation_echoue_en_entier_jamais_partiellement()
    {
        // Dérivation : l'échec du bloc ℛ ne laisse aucun W — l'exception est l'unique issue.
        Assert.Throws<RegistreNonCouvertException>(
            () => Porteur.Deriver(OmegaValideEnMemoire(), RegistreCasse("RegistreNonCouvert")));

        // Transition : l'échec du second membre ne laisse aucune τ partielle.
        Assert.Throws<RegistreNonCouvertException>(
            () => Porteur.Transitionner(
                OmegaOracle(), RegistreReel(),
                OmegaValideEnMemoire(), RegistreCasse("RegistreNonCouvert"),
                new Cause(TypeCause.Registre, "t")));

        // Audit : l'échec de l'entrée ne laisse aucune réponse partielle.
        Assert.Throws<RegistreNonCouvertException>(
            () => Porteur.PourquoiCetteElection(
                OmegaValideEnMemoire(), RegistreCasse("RegistreNonCouvert"), Strate.Contenu, 1));
    }

    // --- l'apprentissage traverse la frontière : le registre enrichi, le porteur inchangé ---

    [Fact]
    public void Un_registre_enrichi_traverse_la_frontiere_publique_sans_changement_de_moteur()
    {
        var registre = new LecteurDeRegistreMarkdown(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", "RegistresValides", "AvecCe02", "registre"));

        var w = Porteur.Deriver(OmegaValideEnMemoire(), registre);

        var election = Assert.Single(w.Actes, a => a.Type == TypeActe.Election);
        Assert.Equal([new ConventionRef("CE-01", 1), new ConventionRef("CE-02", 1)], election.Licences);
    }
}
