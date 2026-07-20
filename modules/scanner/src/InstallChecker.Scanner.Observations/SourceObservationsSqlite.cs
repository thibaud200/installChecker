using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Observations;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Scanner.Observations;

/// <summary>Sélectionne l'adaptateur appartenant à la version physique du support.</summary>
public static class SourceObservationsSqlite
{
    public static IObservationsSource Ouvrir(string cheminBase) =>
        LireVersion(cheminBase) == LecteurObservationsSqliteV3.VersionSupportee
            ? new LecteurObservationsSqliteV3(cheminBase)
            : new LecteurDObservationsSqlite(cheminBase);

    private static long? LireVersion(string cheminBase)
    {
        if (!File.Exists(cheminBase))
            return null;

        try
        {
            using var connexion = new SqliteConnection($"Data Source={cheminBase};Mode=ReadOnly");
            connexion.Open();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "PRAGMA user_version;";
            return (long)commande.ExecuteScalar()!;
        }
        catch (SqliteException)
        {
            return null;
        }
    }
}
