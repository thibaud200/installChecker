using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class ValidateurDePlanTests
{
    private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    [Fact]
    public void Un_plan_valide_dont_tous_les_hashs_correspondent_est_executable()
    {
        var plan = PlanValide();
        var observateur = ObservateurDisponiblePourTous(plan, HashA);

        var rapport = ValidateurDePlan.Verifier(plan, observateur, _ => false);

        Assert.Equal("duplicate-files/safe-plan-verification/v1", rapport.VersionContrat);
        Assert.Equal(ModeVerificationPlan.Simulation, rapport.Mode);
        Assert.True(rapport.Executable);
        Assert.All(
            Assert.Single(rapport.Groupes).Fichiers,
            f => Assert.Equal(EtatVerificationFichier.Valide, f.Etat));
        Assert.Equal([1, 2], rapport.Journal.Select(e => e.Sequence));
        Assert.Equal(
            [EtapeJournalVerificationPlan.VerifierTemoin, EtapeJournalVerificationPlan.VerifierCandidat],
            rapport.Journal.Select(e => e.Etape));
        Assert.Equal(
            [RoleFichierPlan.TemoinConservation, RoleFichierPlan.Candidat],
            Assert.Single(rapport.Groupes).Fichiers.Select(f => f.Role));
    }

    [Fact]
    public void Un_hash_different_bloque_le_groupe()
    {
        var plan = PlanValide();
        var candidat = Assert.Single(plan.Propositions);
        var observateur = ObservateurDisponiblePourTous(plan, HashA);
        observateur.Observations[candidat.Chemin] =
            new ObservationFichierCourant(EtatLectureFichier.Disponible, HashB, null);

        var rapport = ValidateurDePlan.Verifier(plan, observateur, _ => false);

        var groupe = Assert.Single(rapport.Groupes);
        Assert.False(rapport.Executable);
        Assert.False(groupe.Executable);
        Assert.Equal([EtatVerificationFichier.HashDifferent], groupe.Blocages);
        Assert.Equal(
            EtatVerificationFichier.HashDifferent,
            groupe.Fichiers.Single(f => f.FichierId == candidat.FichierId).Etat);
    }

    [Fact]
    public void Un_candidat_devenu_protege_est_bloque_sans_etre_lu()
    {
        var plan = PlanValide();
        var candidat = Assert.Single(plan.Propositions);
        var observateur = ObservateurDisponiblePourTous(plan, HashA);

        var rapport = ValidateurDePlan.Verifier(
            plan,
            observateur,
            chemin => chemin == candidat.Chemin);

        Assert.False(rapport.Executable);
        Assert.Equal(
            EtatVerificationFichier.CheminProtege,
            Assert.Single(rapport.Groupes).Fichiers.Single(f => f.Role == RoleFichierPlan.Candidat).Etat);
        Assert.DoesNotContain(candidat.Chemin, observateur.CheminsLus);
    }

    [Theory]
    [InlineData(EtatLectureFichier.Absent, EtatVerificationFichier.Absent)]
    [InlineData(EtatLectureFichier.Illisible, EtatVerificationFichier.Illisible)]
    [InlineData(EtatLectureFichier.TypeNonPrisEnCharge, EtatVerificationFichier.TypeNonPrisEnCharge)]
    public void Les_echecs_de_lecture_sont_projetes_sans_perdre_le_detail(
        EtatLectureFichier etatLecture,
        EtatVerificationFichier etatAttendu)
    {
        var plan = PlanValide();
        var candidat = Assert.Single(plan.Propositions);
        var observateur = ObservateurDisponiblePourTous(plan, HashA);
        observateur.Observations[candidat.Chemin] =
            new ObservationFichierCourant(etatLecture, null, "detail normalise");

        var rapport = ValidateurDePlan.Verifier(plan, observateur, _ => false);
        var verification = Assert.Single(rapport.Groupes).Fichiers
            .Single(f => f.FichierId == candidat.FichierId);

        Assert.Equal(etatAttendu, verification.Etat);
        Assert.Null(verification.HashObserve);
        Assert.Equal("detail normalise", verification.Detail);
    }

    [Fact]
    public void Un_plan_invalide_est_refuse_avant_le_premier_appel_a_lobservateur()
    {
        var plan = PlanValide() with { VersionContrat = "contrat-invalide" };
        var observateur = new ObservateurDeTest();

        Assert.Throws<PlanInvalideException>(
            () => ValidateurDePlan.Verifier(plan, observateur, _ => false));
        Assert.Empty(observateur.CheminsLus);
    }

    [Fact]
    public void Un_plan_vide_est_un_no_op_executable()
    {
        var plan = new PlanDeSuppression([], VersionsContratDuplicateFiles.PlanSecuriseV1, []);

        var rapport = ValidateurDePlan.Verifier(plan, new ObservateurDeTest(), _ => false);

        Assert.True(rapport.Executable);
        Assert.Empty(rapport.Groupes);
        Assert.Empty(rapport.Journal);
    }

    private sealed class ObservateurDeTest : IObservateurDeFichier
    {
        public Dictionary<string, ObservationFichierCourant> Observations { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public List<string> CheminsLus { get; } = [];

        public ObservationFichierCourant Observer(string chemin)
        {
            CheminsLus.Add(chemin);
            return Observations[chemin];
        }
    }

    private static PlanDeSuppression PlanValide() => ConstructeurDePlan.Construire(
        new[]
        {
            (HashA, (IReadOnlyList<string>)[@"C:\garde.exe", @"C:\copie.exe"]),
        },
        _ => false);

    private static ObservateurDeTest ObservateurDisponiblePourTous(
        PlanDeSuppression plan,
        string hash)
    {
        var observateur = new ObservateurDeTest();
        foreach (var garantie in plan.GarantiesParGroupe)
        {
            observateur.Observations[garantie.TemoinConservation.Chemin] =
                new ObservationFichierCourant(EtatLectureFichier.Disponible, hash, null);
        }

        foreach (var proposition in plan.Propositions)
        {
            observateur.Observations[proposition.Chemin] =
                new ObservationFichierCourant(EtatLectureFichier.Disponible, hash, null);
        }

        return observateur;
    }
}
