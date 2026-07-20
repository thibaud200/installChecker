using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class ValidateurStructurePlanTests
{
    private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    [Fact]
    public void Un_plan_produit_par_le_constructeur_est_valide()
    {
        ValidateurStructurePlan.Valider(PlanValide());
    }

    [Fact]
    public void Une_version_inconnue_est_refusee()
    {
        var plan = PlanValide() with { VersionContrat = "duplicate-files/safe-plan/v2" };

        Assert.Throws<PlanInvalideException>(() => ValidateurStructurePlan.Valider(plan));
    }

    [Fact]
    public void Un_FichierId_falsifie_est_refuse()
    {
        var plan = PlanValide();
        var proposition = Assert.Single(plan.Propositions) with
        {
            FichierId = "file:sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
        };

        Assert.Throws<PlanInvalideException>(
            () => ValidateurStructurePlan.Valider(plan with { Propositions = [proposition] }));
    }

    [Fact]
    public void Le_temoin_ne_peut_pas_etre_une_proposition_sous_une_autre_casse()
    {
        var plan = PlanValide();
        var garantie = Assert.Single(plan.GarantiesParGroupe);
        var proposition = Assert.Single(plan.Propositions) with
        {
            Chemin = garantie.TemoinConservation.Chemin.ToUpperInvariant(),
            FichierId = garantie.TemoinConservation.FichierId,
        };

        Assert.Throws<PlanInvalideException>(
            () => ValidateurStructurePlan.Valider(plan with { Propositions = [proposition] }));
    }

    [Fact]
    public void Une_garantie_sans_proposition_est_refusee()
    {
        var plan = PlanValide();
        var garantieOrpheline = new GarantieDeGroupe(
            IdentifiantsStables.PourGroupeExact(HashB),
            HashB,
            new TemoinDeConservation(
                IdentifiantsStables.PourFichier(HashB, @"D:\garde.exe"),
                @"D:\garde.exe"));

        Assert.Throws<PlanInvalideException>(() => ValidateurStructurePlan.Valider(
            plan with { GarantiesParGroupe = [.. plan.GarantiesParGroupe, garantieOrpheline] }));
    }

    [Fact]
    public void Une_proposition_sans_garantie_est_refusee()
    {
        var plan = PlanValide() with { GarantiesParGroupe = [] };

        Assert.Throws<PlanInvalideException>(() => ValidateurStructurePlan.Valider(plan));
    }

    [Fact]
    public void Un_hash_non_normalise_est_refuse()
    {
        var plan = PlanValide();
        var garantie = Assert.Single(plan.GarantiesParGroupe) with
        {
            ContenuSha256 = HashA.ToUpperInvariant(),
        };

        Assert.Throws<PlanInvalideException>(() => ValidateurStructurePlan.Valider(
            plan with { GarantiesParGroupe = [garantie] }));
    }

    [Fact]
    public void Un_GroupeId_falsifie_est_refuse()
    {
        var plan = PlanValide();
        var garantie = Assert.Single(plan.GarantiesParGroupe) with
        {
            GroupeId = IdentifiantsStables.PourGroupeExact(HashB),
        };

        Assert.Throws<PlanInvalideException>(() => ValidateurStructurePlan.Valider(
            plan with { GarantiesParGroupe = [garantie] }));
    }

    [Fact]
    public void Deux_chemins_windows_equivalents_sont_refuses()
    {
        var plan = PlanValide();
        var garantie = Assert.Single(plan.GarantiesParGroupe);
        var cheminEquivalent = garantie.TemoinConservation.Chemin
            .ToUpperInvariant()
            .Replace('\\', '/');
        var proposition = Assert.Single(plan.Propositions) with
        {
            Chemin = cheminEquivalent,
            FichierId = IdentifiantsStables.PourFichier(HashA, cheminEquivalent),
        };

        Assert.Throws<PlanInvalideException>(
            () => ValidateurStructurePlan.Valider(plan with { Propositions = [proposition] }));
    }

    [Fact]
    public void Deux_FichierId_identiques_sont_refuses()
    {
        var plan = ConstructeurDePlan.Construire(
            new[]
            {
                (HashA, (IReadOnlyList<string>)[@"C:\garde.exe", @"C:\copie-1.exe", @"C:\copie-2.exe"]),
            },
            _ => false);
        var propositions = plan.Propositions.ToList();
        propositions[1] = propositions[1] with { FichierId = propositions[0].FichierId };

        Assert.Throws<PlanInvalideException>(
            () => ValidateurStructurePlan.Valider(plan with { Propositions = propositions }));
    }

    private static PlanDeSuppression PlanValide() => ConstructeurDePlan.Construire(
        new[]
        {
            (HashA, (IReadOnlyList<string>)[@"C:\garde.exe", @"C:\copie.exe"]),
        },
        _ => false);
}
