using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Scanner.Observations;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Scanner.Observations.Tests;

public sealed class LecteurObservationsSqliteV3Tests : IDisposable
{
    private readonly string _racine = Directory.CreateTempSubdirectory("omega-v3-source-").FullName;
    private readonly string _baseDir = Directory.CreateTempSubdirectory("omega-v3-db-").FullName;
    private string BasePath => Path.Combine(_baseDir, "observations.db");

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_racine, recursive: true);
        Directory.Delete(_baseDir, recursive: true);
    }

    [Fact]
    public void V3_projette_une_occurrence_par_chemin_meme_si_le_snapshot_est_partage()
    {
        File.WriteAllText(Path.Combine(_racine, "a.bin"), "identique");
        File.WriteAllText(Path.Combine(_racine, "b.bin"), "identique");
        Scanner();

        var source = SourceObservationsSqlite.Ouvrir(BasePath);
        var modele = source.ProjeterModele();
        var contexte = source.ProjeterContexte();

        Assert.Equal(2, modele.Actes.Count);
        Assert.Single(modele.Actes.Select(a => a.Empreinte).Distinct());
        Assert.Equal(2, contexte.Select(c => c.Chemin).Distinct().Count());
        Assert.All(modele.Actes, acte => Assert.Equal(6, acte.Attributs.Keys.Select(a => a.Capacite).Distinct().Count()));
    }

    [Fact]
    public void V3_ne_projette_que_le_dernier_scan_du_volume()
    {
        File.WriteAllText(Path.Combine(_racine, "a.bin"), "identique");
        Scanner();
        Scanner();

        Assert.Single(SourceObservationsSqlite.Ouvrir(BasePath).ProjeterModele().Actes);
    }

    [Fact]
    public void Identite_v3_est_stable_et_declare_deux_actes()
    {
        File.WriteAllText(Path.Combine(_racine, "a.bin"), "identique");
        File.WriteAllText(Path.Combine(_racine, "b.bin"), "identique");
        Scanner();
        var source = SourceObservationsSqlite.Ouvrir(BasePath);

        var premiere = source.ProjeterIdentite();
        var seconde = source.ProjeterIdentite();

        Assert.Equal(premiere, seconde);
        Assert.Equal(3, premiere.Version);
        Assert.Equal(2, premiere.NombreActes);
        Assert.Matches("^[0-9a-f]{64}$", premiere.EmpreinteEtat);
    }

    [Fact]
    public void Base_v2_est_deleguee_au_lecteur_historique()
    {
        using (var connexion = new SqliteConnection($"Data Source={BasePath}"))
        {
            connexion.Open();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "PRAGMA user_version = 2;";
            commande.ExecuteNonQuery();
        }

        Assert.IsType<LecteurDObservationsSqlite>(SourceObservationsSqlite.Ouvrir(BasePath));
    }

    [Fact]
    public void Version_inconnue_est_refusee_par_le_contrat_historique()
    {
        using (var connexion = new SqliteConnection($"Data Source={BasePath}"))
        {
            connexion.Open();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "PRAGMA user_version = 4;";
            commande.ExecuteNonQuery();
        }

        var source = SourceObservationsSqlite.Ouvrir(BasePath);
        Assert.Throws<OmegaIncompatibleException>(() => source.ProjeterModele());
    }

    private void Scanner() =>
        Assert.Equal(0, ScanCommand.Run(_racine, BasePath, false, TextWriter.Null, TextWriter.Null));
}
