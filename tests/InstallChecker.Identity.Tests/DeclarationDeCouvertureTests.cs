using InstallChecker.Identity.Access.Registre;
using InstallChecker.Identity.Conventions;

namespace InstallChecker.Identity.Tests;

/// <summary>
/// La déclaration de couverture (017 §§ 2–3, jalon V2-1) : existence, exactitude,
/// stabilité, déterminisme, indépendance des entrées — I62 et I63 au niveau unitaire.
/// Aucun comportement du moteur n'est encore conditionné par elle (câblage : V2-2).
/// </summary>
public class DeclarationDeCouvertureTests
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

    [Fact]
    public void La_declaration_existe_et_contient_exactement_interpretation_et_election()
    {
        Assert.Equal(2, DeclarationDeCouverture.FamillesCouvertes.Count);
        Assert.Contains(Famille.Interpretation, (IReadOnlySet<Famille>)DeclarationDeCouverture.FamillesCouvertes);
        Assert.Contains(Famille.Election, (IReadOnlySet<Famille>)DeclarationDeCouverture.FamillesCouvertes);
    }

    [Fact]
    public void Toute_famille_non_declaree_est_non_couverte()
    {
        // I62 : il n'existe pas de couverture tacite — les six familles théorisées
        // mais non déclarées sont hors couverture, sans exception.
        foreach (var famille in Enum.GetValues<Famille>())
        {
            var attendu = famille is Famille.Interpretation or Famille.Election;
            Assert.Equal(attendu, DeclarationDeCouverture.Couvre(famille));
        }
    }

    [Fact]
    public void La_declaration_est_stable_et_deterministe_entre_invocations()
    {
        var premiere = DeclarationDeCouverture.FamillesCouvertes;
        var seconde = DeclarationDeCouverture.FamillesCouvertes;

        Assert.Same(premiere, seconde);
        Assert.True(DeclarationDeCouverture.Couvre(Famille.Interpretation));
        Assert.True(DeclarationDeCouverture.Couvre(Famille.Interpretation));
        Assert.False(DeclarationDeCouverture.Couvre(Famille.Attente));
        Assert.False(DeclarationDeCouverture.Couvre(Famille.Attente));
    }

    [Fact]
    public void La_declaration_est_independante_du_registre()
    {
        // I63 : propriété de la version du moteur, jamais du registre — la déclaration
        // lue avant et après la projection de ℛ₀ est identique, et rien de ce que le
        // registre contient ne peut l'atteindre (aucun point d'injection n'existe).
        var avant = DeclarationDeCouverture.FamillesCouvertes.ToHashSet();

        _ = new LecteurDeRegistreMarkdown(Path.Combine(RacineDuDepot(), "registre")).Projeter();

        var apres = DeclarationDeCouverture.FamillesCouvertes.ToHashSet();
        Assert.Equal(avant, apres);
    }
}
