using System.Text.Json;
using System.Text.Json.Serialization;
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Observations;
using InstallChecker.Identity.Etat;

namespace InstallChecker.DuplicateFiles.Tests;

public class GenerateurRedondanceVersionneeTests
{
    [Fact]
    public void Deux_versions_nommees_forment_un_groupe_consultatif()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Fichier(1, 'a', @"C:\corpus\outil-1.0.zip"),
            Fichier(2, 'b', @"C:\corpus\outil-2.0.zip")));

        var groupe = Assert.Single(rapport.Groupes);
        Assert.Equal("2", groupe.VersionReference);
        Assert.Equal(NiveauConfianceVersionnee.Faible, groupe.Confiance);
        Assert.Equal(RoleComparaisonVersionnee.VersionAnterieure, groupe.Artefacts[0].Role);
        Assert.Equal(RoleComparaisonVersionnee.ReferenceRecente, groupe.Artefacts[1].Role);
        Assert.All(groupe.Artefacts, a => Assert.Equal(
            [ActionVersionnee.Examiner, ActionVersionnee.Ignorer], a.Actions));
        Assert.All(groupe.Artefacts, a => Assert.Contains(
            RaisonBlocageVersionnee.SuppressionAutomatiqueInterdite, a.Blocages));
    }

    [Fact]
    public void Les_versions_calendaires_sont_comparees_dans_leur_propre_schema()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Fichier(1, 'a', @"C:\docs\manuel-2025-12-31.pdf"),
            Fichier(2, 'b', @"C:\docs\manuel-2026-01-01.pdf")));

        Assert.Equal("2026-01-01", Assert.Single(rapport.Groupes).VersionReference);
    }

    [Fact]
    public void Une_famille_avec_une_seule_version_nest_pas_un_candidat()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Fichier(1, 'a', @"C:\a\outil-1.zip"),
            Fichier(2, 'b', @"C:\b\outil-1.zip")));

        Assert.Empty(rapport.Groupes);
        Assert.Equal(2, rapport.ExclusionsParMotif[MotifExclusionVersionnee.MemeVersionSeulement]);
    }

    [Fact]
    public void Plusieurs_chemins_du_meme_contenu_restent_un_seul_artefact()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Fichier(1, 'a', @"C:\a\outil-1.zip"),
            Fichier(2, 'a', @"D:\copie\outil-1.zip"),
            Fichier(3, 'b', @"C:\a\outil-2.zip")));

        var groupe = Assert.Single(rapport.Groupes);
        Assert.Equal(2, groupe.Artefacts.Count);
        Assert.Equal(2, groupe.Artefacts.Single(a => a.Version == "1").Fichiers.Count);
        Assert.Equal(2, rapport.Synthese.NombreContenus);
    }

    [Fact]
    public void Des_architectures_connues_differentes_ne_sont_jamais_comparees()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Fichier(1, 'a', @"C:\a\outil-1-x64.zip"),
            Fichier(2, 'b', @"C:\a\outil-2-arm64.zip")));

        Assert.Empty(rapport.Groupes);
        Assert.Equal(2, rapport.ExclusionsParMotif[MotifExclusionVersionnee.VarianteIncompatible]);
    }

    [Fact]
    public void Deux_MSI_sans_architecture_restent_comparables_avec_revue_obligatoire()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Msi(1, 'a', "1.0", "1036"),
            Msi(2, 'b', "2.0", "1036")));

        var groupe = Assert.Single(rapport.Groupes);
        Assert.True(groupe.Variante.Partielle);
        Assert.Equal(NiveauConfianceVersionnee.Moyenne, groupe.Confiance);
        Assert.Contains(RaisonBlocageVersionnee.VarianteNonObservee, groupe.Blocages);
        Assert.Contains(RaisonBlocageVersionnee.RevueHumaineObligatoire, groupe.Blocages);
    }

    [Fact]
    public void Une_architecture_connue_et_une_architecture_absente_ne_creent_pas_de_relation()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Fichier(1, 'a', @"C:\a\outil-1-x64.exe"),
            Fichier(2, 'b', @"C:\a\outil-2.exe")));

        Assert.Empty(rapport.Groupes);
        Assert.Equal(2, rapport.ExclusionsParMotif[MotifExclusionVersionnee.VarianteIncompatible]);
    }

    [Fact]
    public void Les_langues_MSI_produisent_des_groupes_distincts()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Msi(1, 'a', "1", "1036"),
            Msi(2, 'b', "2", "1036"),
            Msi(3, 'c', "1", "1033"),
            Msi(4, 'd', "2", "1033")));

        Assert.Equal(2, rapport.Groupes.Count);
        Assert.Equal(["1033", "1036"], rapport.Groupes.Select(g => g.Variante.Langue));
        Assert.Equal(2, rapport.Groupes.Select(g => g.GroupeId).Distinct().Count());
    }

    [Fact]
    public void La_cle_native_MSI_survit_a_un_changement_de_nom_du_produit()
    {
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Msi(1, 'a', "1", "1036", "Outil historique"),
            Msi(2, 'b', "2", "1036", "Outil actuel")));

        var groupe = Assert.Single(rapport.Groupes);
        Assert.Equal("2", groupe.VersionReference);
    }

    [Fact]
    public void Une_version_structuree_en_conflit_avec_le_nom_est_exclue()
    {
        var attributs = VersionInfo("Outil", "Contoso", "9.0");
        var rapport = GenerateurRedondanceVersionnee.Generer(Omega(
            Fichier(1, 'a', @"C:\a\outil-1.exe", attributs),
            Fichier(2, 'b', @"C:\a\outil-2.exe", VersionInfo("Outil", "Contoso", "2.0"))));

        Assert.Empty(rapport.Groupes);
        Assert.Equal(1, rapport.ExclusionsParMotif[MotifExclusionVersionnee.ConflitDeVersion]);
    }

    [Fact]
    public void Le_rapport_est_deterministe_quelle_que_soit_lordre_des_observations()
    {
        var a = Fichier(1, 'a', @"C:\a\outil-1.zip");
        var b = Fichier(2, 'b', @"C:\a\outil-2.zip");

        var premier = GenerateurRedondanceVersionnee.Generer(Omega(a, b));
        var second = GenerateurRedondanceVersionnee.Generer(Omega(b, a));

        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
        };
        Assert.Equal(
            JsonSerializer.Serialize(premier, options),
            JsonSerializer.Serialize(second, options));
    }

    private static IReadOnlyDictionary<Attribut, ValeurObservee> VersionInfo(
        string produit,
        string societe,
        string version) => new Dictionary<Attribut, ValeurObservee>
    {
        [new("version_info", "product_name")] = new ValeurObservee.Texte(produit),
        [new("version_info", "company_name")] = new ValeurObservee.Texte(societe),
        [new("version_info", "product_version")] = new ValeurObservee.Texte(version),
    };

    private static Entree Msi(
        long id,
        char hash,
        string version,
        string langue,
        string produit = "Outil") => Fichier(
        id,
        hash,
        $@"C:\msi\outil-{version}.msi",
        new Dictionary<Attribut, ValeurObservee>
        {
            [new("msi_properties", "upgrade_code")] =
                new ValeurObservee.Texte("{12345678-1234-1234-1234-1234567890AB}"),
            [new("msi_properties", "product_name")] = new ValeurObservee.Texte(produit),
            [new("msi_properties", "product_version")] = new ValeurObservee.Texte(version),
            [new("msi_properties", "product_language")] = new ValeurObservee.Texte(langue),
        });

    private static Entree Fichier(
        long id,
        char hash,
        string chemin,
        IReadOnlyDictionary<Attribut, ValeurObservee>? attributs = null) =>
        new(id, new string(hash, 64), chemin, attributs ?? new Dictionary<Attribut, ValeurObservee>());

    private static IObservationsSource Omega(params Entree[] entrees) => new OmegaDeTest(entrees);

    private sealed record Entree(
        long Id,
        string Hash,
        string Chemin,
        IReadOnlyDictionary<Attribut, ValeurObservee> Attributs);

    private sealed class OmegaDeTest(IReadOnlyList<Entree> entrees) : IObservationsSource
    {
        public ModeleObservations ProjeterModele() => new(
            entrees.Select(e => new ActeObservation(e.Id, 100, e.Hash, e.Attributs)).ToList());

        public IReadOnlyList<ContexteObservation> ProjeterContexte() => entrees
            .Select(e => new ContexteObservation(e.Id, e.Chemin, "2026-01-01T00:00:00.0000000Z"))
            .ToList();

        public IndexOmega ProjeterIdentite() => throw new NotSupportedException();
    }
}
