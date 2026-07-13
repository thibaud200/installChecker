using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class ConstructeurDePlanTests
{
    private static readonly IReadOnlySet<string> AucunProtege = new HashSet<string>();

    private static (string, IReadOnlyList<string>) Groupe(string contenu, params string[] chemins) => (contenu, chemins);

    private static IReadOnlyList<string> CheminsProposes(PlanDeSuppression plan) =>
        plan.Propositions.Select(p => p.Chemin).ToList();

    [Fact]
    public void GroupeDeMoinsDeDeuxChemins_EstIgnore()
    {
        var plan = ConstructeurDePlan.Construire(new[] { Groupe("h", "C:\\a.exe") }, AucunProtege);

        Assert.Empty(plan.Propositions);
    }

    [Fact]
    public void SansProtege_ProposeToutSaufLePremierDeLOrdreRecu()
    {
        var plan = ConstructeurDePlan.Construire(new[] { Groupe("h", "C:\\a.exe", "C:\\b.exe", "C:\\c.exe") }, AucunProtege);

        Assert.Equal(new[] { "C:\\b.exe", "C:\\c.exe" }, CheminsProposes(plan)); // un chemin (le premier) demeure
    }

    [Fact]
    public void AuMoinsUnProtege_ProposeTousLesNonProteges()
    {
        var proteges = new HashSet<string> { "C:\\Windows\\a.exe" };
        var plan = ConstructeurDePlan.Construire(
            new[] { Groupe("h", "C:\\Windows\\a.exe", "C:\\b.exe", "C:\\c.exe") }, proteges);

        Assert.Equal(new[] { "C:\\b.exe", "C:\\c.exe" }, CheminsProposes(plan));
        Assert.DoesNotContain(plan.Propositions, p => p.Chemin == "C:\\Windows\\a.exe"); // protégé jamais proposé
    }

    [Fact]
    public void TousProteges_NeProposeRien()
    {
        var proteges = new HashSet<string> { "C:\\Windows\\a.exe", "C:\\Windows\\b.exe" };
        var plan = ConstructeurDePlan.Construire(
            new[] { Groupe("h", "C:\\Windows\\a.exe", "C:\\Windows\\b.exe") }, proteges);

        Assert.Empty(plan.Propositions);
    }

    [Fact]
    public void ChaqueProposition_PorteSonContenu()
    {
        var plan = ConstructeurDePlan.Construire(new[] { Groupe("empreinte-xyz", "C:\\a.exe", "C:\\b.exe") }, AucunProtege);

        Assert.All(plan.Propositions, p => Assert.Equal("empreinte-xyz", p.Contenu));
    }

    [Fact]
    public void OrdreRecuPreserve_AucunTri()
    {
        // Entrée volontairement non triée : le premier reçu (z) demeure, pas le plus petit alphabétiquement.
        var plan = ConstructeurDePlan.Construire(new[] { Groupe("h", "C:\\z.exe", "C:\\a.exe", "C:\\m.exe") }, AucunProtege);

        Assert.Equal(new[] { "C:\\a.exe", "C:\\m.exe" }, CheminsProposes(plan));
    }

    [Fact]
    public void PlusieursGroupes_SontAplatisEnUneListe()
    {
        var plan = ConstructeurDePlan.Construire(
            new[] { Groupe("h1", "C:\\a.exe", "C:\\b.exe"), Groupe("h2", "D:\\x.exe", "D:\\y.exe") }, AucunProtege);

        Assert.Equal(2, plan.Propositions.Count);
        Assert.Contains(plan.Propositions, p => p.Contenu == "h1" && p.Chemin == "C:\\b.exe");
        Assert.Contains(plan.Propositions, p => p.Contenu == "h2" && p.Chemin == "D:\\y.exe");
    }
}
