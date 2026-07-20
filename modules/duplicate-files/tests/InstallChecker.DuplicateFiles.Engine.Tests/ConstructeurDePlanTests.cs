using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class ConstructeurDePlanTests
{
    private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private static readonly IReadOnlySet<string> AucunProtege = new HashSet<string>();

    private static (string, IReadOnlyList<string>) Groupe(string contenu, params string[] chemins) => (contenu, chemins);

    private static IReadOnlyList<string> CheminsProposes(PlanDeSuppression plan) =>
        plan.Propositions.Select(p => p.Chemin).ToList();

    [Fact]
    public void GroupeDeMoinsDeDeuxChemins_EstIgnore()
    {
        var plan = ConstructeurDePlan.Construire(new[] { Groupe(HashA, "C:\\a.exe") }, AucunProtege);

        Assert.Empty(plan.Propositions);
        Assert.Empty(plan.GarantiesParGroupe);
    }

    [Fact]
    public void SansProtege_ProposeToutSaufLePremierDeLOrdreRecu()
    {
        var plan = ConstructeurDePlan.Construire(new[] { Groupe(HashA, "C:\\a.exe", "C:\\b.exe", "C:\\c.exe") }, AucunProtege);

        Assert.Equal(new[] { "C:\\b.exe", "C:\\c.exe" }, CheminsProposes(plan)); // un chemin (le premier) demeure
    }

    [Fact]
    public void AuMoinsUnProtege_ProposeTousLesNonProteges()
    {
        var proteges = new HashSet<string> { "C:\\Windows\\a.exe" };
        var plan = ConstructeurDePlan.Construire(
            new[] { Groupe(HashA, "C:\\Windows\\a.exe", "C:\\b.exe", "C:\\c.exe") }, proteges);

        Assert.Equal(new[] { "C:\\b.exe", "C:\\c.exe" }, CheminsProposes(plan));
        Assert.DoesNotContain(plan.Propositions, p => p.Chemin == "C:\\Windows\\a.exe"); // protégé jamais proposé
    }

    [Fact]
    public void TousProteges_NeProposeRien()
    {
        var proteges = new HashSet<string> { "C:\\Windows\\a.exe", "C:\\Windows\\b.exe" };
        var plan = ConstructeurDePlan.Construire(
            new[] { Groupe(HashA, "C:\\Windows\\a.exe", "C:\\Windows\\b.exe") }, proteges);

        Assert.Empty(plan.Propositions);
        Assert.Empty(plan.GarantiesParGroupe);
    }

    [Fact]
    public void ChaqueProposition_PorteSonContenu()
    {
        var plan = ConstructeurDePlan.Construire(new[] { Groupe(HashA, "C:\\a.exe", "C:\\b.exe") }, AucunProtege);

        var proposition = Assert.Single(plan.Propositions);
        Assert.Equal(HashA, proposition.Contenu);
        Assert.Equal(IdentifiantsStables.PourGroupeExact(HashA), proposition.GroupeId);
        Assert.Equal(IdentifiantsStables.PourFichier(HashA, proposition.Chemin), proposition.FichierId);
    }

    [Fact]
    public void OrdreRecuPreserve_AucunTri()
    {
        // Entrée volontairement non triée : le premier reçu (z) demeure, pas le plus petit alphabétiquement.
        var plan = ConstructeurDePlan.Construire(new[] { Groupe(HashA, "C:\\z.exe", "C:\\a.exe", "C:\\m.exe") }, AucunProtege);

        Assert.Equal(new[] { "C:\\a.exe", "C:\\m.exe" }, CheminsProposes(plan));
    }

    [Fact]
    public void PlusieursGroupes_SontAplatisEnUneListe()
    {
        var plan = ConstructeurDePlan.Construire(
            new[] { Groupe(HashA, "C:\\a.exe", "C:\\b.exe"), Groupe(HashB, "D:\\x.exe", "D:\\y.exe") }, AucunProtege);

        Assert.Equal(2, plan.Propositions.Count);
        Assert.Contains(plan.Propositions, p => p.Contenu == HashA && p.Chemin == "C:\\b.exe");
        Assert.Contains(plan.Propositions, p => p.Contenu == HashB && p.Chemin == "D:\\y.exe");
    }

    [Theory]
    [InlineData(@"C:\Windows\System32\a.exe")]
    [InlineData(@"C:\Program Files\App\a.exe")]
    [InlineData(@"C:\Program Files (x86)\App\a.exe")]
    [InlineData(@"C:\$Recycle.Bin\a.exe")]
    public void Les_racines_systeme_sont_protegees_par_defaut(string chemin)
    {
        Assert.True(ProtectionDesChemins.EstProtegeParDefaut(chemin));
    }

    [Fact]
    public void Le_predicat_de_protection_exclut_les_chemins_proteges_par_prefixe()
    {
        var plan = ConstructeurDePlan.Construire(
            new[] { Groupe(HashA, @"C:\Windows\setup.exe", @"D:\Archives\setup.exe") },
            ProtectionDesChemins.EstProtegeParDefaut);

        Assert.Equal(new[] { @"D:\Archives\setup.exe" }, CheminsProposes(plan));
    }

    [Fact]
    public void Le_plan_expose_la_version_et_le_temoin_de_rang_un()
    {
        var plan = ConstructeurDePlan.Construire(
            new[] { Groupe(HashA, @"C:\garde.exe", @"C:\copie.exe") },
            AucunProtege);

        Assert.Equal("duplicate-files/safe-plan/v1", plan.VersionContrat);
        var garantie = Assert.Single(plan.GarantiesParGroupe);
        Assert.Equal(IdentifiantsStables.PourGroupeExact(HashA), garantie.GroupeId);
        Assert.Equal(HashA, garantie.ContenuSha256);
        Assert.Equal(@"C:\garde.exe", garantie.TemoinConservation.Chemin);
        Assert.Equal(
            IdentifiantsStables.PourFichier(HashA, @"C:\garde.exe"),
            garantie.TemoinConservation.FichierId);
    }

    [Fact]
    public void Un_protege_non_recommande_ne_permet_pas_de_proposer_le_rang_un()
    {
        var proteges = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            @"C:\Windows\copie-protegee.exe",
        };
        var plan = ConstructeurDePlan.Construire(
            new[]
            {
                Groupe(
                    HashA,
                    @"D:\garde-recommandee.exe",
                    @"C:\Windows\copie-protegee.exe",
                    @"D:\copie.exe"),
            },
            proteges);

        Assert.Equal([@"D:\copie.exe"], CheminsProposes(plan));
        Assert.Equal(
            @"D:\garde-recommandee.exe",
            Assert.Single(plan.GarantiesParGroupe).TemoinConservation.Chemin);
    }
}
