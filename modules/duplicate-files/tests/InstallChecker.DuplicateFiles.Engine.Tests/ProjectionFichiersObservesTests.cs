using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class ProjectionFichiersObservesTests
{
    private const string HashA = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string HashB = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    [Fact]
    public void La_projection_aligne_actes_contextes_et_identifiants_dans_un_ordre_stable()
    {
        var attributs = new Dictionary<Attribut, ValeurObservee>
        {
            [new Attribut("version_info", "product_name")] = new ValeurObservee.Texte("Outil"),
        };
        var source = new SourceEnMemoire(
            [
                new ActeObservation(2, 20, HashB.ToUpperInvariant(), new Dictionary<Attribut, ValeurObservee>()),
                new ActeObservation(1, 10, HashA.ToUpperInvariant(), attributs),
            ],
            [
                new ContexteObservation(2, @"C:\Corpus\outil-2.0.zip", "2026-07-19T00:00:00Z"),
                new ContexteObservation(1, @"C:\Corpus\outil-1.0.zip", "2026-07-19T00:00:00Z"),
            ]);

        var fichiers = ProjectionFichiersObserves.Projeter(source);

        Assert.Equal([1L, 2L], fichiers.Select(f => f.ActeId));
        Assert.Equal(@"C:\Corpus\outil-1.0.zip", fichiers[0].Chemin);
        Assert.Equal(HashA, fichiers[0].ContenuSha256);
        Assert.Equal(IdentifiantsStables.PourFichier(HashA, fichiers[0].Chemin), fichiers[0].FichierId);
        Assert.Same(attributs, fichiers[0].Attributs);
    }

    [Fact]
    public void Un_contexte_manquant_est_refuse()
    {
        var source = new SourceEnMemoire(
            [new ActeObservation(1, 10, HashA, new Dictionary<Attribut, ValeurObservee>())],
            []);

        var erreur = Assert.Throws<InvalidOperationException>(() => ProjectionFichiersObserves.Projeter(source));

        Assert.Contains("contexte absent", erreur.Message);
    }

    [Fact]
    public void Un_contexte_manquant_pour_lacte_zero_est_refuse_explicitement()
    {
        var source = new SourceEnMemoire(
            [new ActeObservation(0, 10, HashA, new Dictionary<Attribut, ValeurObservee>())],
            []);

        var erreur = Assert.Throws<InvalidOperationException>(() => ProjectionFichiersObserves.Projeter(source));

        Assert.Contains("contexte absent", erreur.Message);
    }

    [Fact]
    public void Un_contexte_orphelin_est_refuse()
    {
        var source = new SourceEnMemoire(
            [],
            [new ContexteObservation(1, @"C:\Corpus\orphelin.zip", "2026-07-19T00:00:00Z")]);

        var erreur = Assert.Throws<InvalidOperationException>(() => ProjectionFichiersObserves.Projeter(source));

        Assert.Contains("contexte orphelin", erreur.Message);
    }

    [Fact]
    public void La_lecture_dattribut_retourne_uniquement_un_texte_non_vide()
    {
        var fichier = new FichierObserve(
            1,
            IdentifiantsStables.PourFichier(HashA, @"C:\a.exe"),
            @"C:\a.exe",
            10,
            HashA,
            new Dictionary<Attribut, ValeurObservee>
            {
                [new Attribut("cap", "texte")] = new ValeurObservee.Texte(" valeur "),
                [new Attribut("cap", "vide")] = new ValeurObservee.Texte("  "),
                [new Attribut("cap", "entier")] = new ValeurObservee.Entier(12),
            });

        Assert.Equal(" valeur ", LectureAttributs.Texte(fichier, "cap", "texte"));
        Assert.Null(LectureAttributs.Texte(fichier, "cap", "vide"));
        Assert.Null(LectureAttributs.Texte(fichier, "cap", "entier"));
        Assert.Null(LectureAttributs.Texte(fichier, "cap", "absent"));
    }

    private sealed class SourceEnMemoire(
        IReadOnlyList<ActeObservation> actes,
        IReadOnlyList<ContexteObservation> contextes) : IObservationsSource
    {
        public ModeleObservations ProjeterModele() => new(actes);

        public IReadOnlyList<ContexteObservation> ProjeterContexte() => contextes;

        public IndexOmega ProjeterIdentite() => new(1, actes.Count, new string('0', 64));
    }
}
