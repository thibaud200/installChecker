using InstallChecker.DuplicateFiles;
using Microsoft.Data.Sqlite;

namespace InstallChecker;

/// <summary>
/// Lit, pour chaque observation de l'état courant d'une base v2 ou v3, le volume observé au scan
/// (spec multi-disque D5) — jointure scan_id → scans, aucune valeur recalculée. Sur une base v1
/// (aucune table scans) : dictionnaire vide, les champs volume du rapport restent absents.
/// Le filtre « dernier scan par volume » est le même que celui du lecteur Ω : les deux lectures
/// décrivent le même état courant.
/// </summary>
public static class LecteurDeVolumes
{
    public static IReadOnlyDictionary<long, VolumeDuFichier> Lire(string cheminBase)
    {
        using var connection = new SqliteConnection($"Data Source={cheminBase};Mode=ReadOnly");
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA user_version;";
        var version = (long)pragma.ExecuteScalar()!;
        if (version is not (2 or 3))
            return new Dictionary<long, VolumeDuFichier>();

        using var commande = connection.CreateCommand();
        commande.CommandText = version == 3
            ? """
                SELECT e.id, s.volume_id, s.volume_label
                FROM scan_entries e
                JOIN scans s ON s.id = e.scan_id
                WHERE e.scan_id IN (SELECT MAX(id) FROM scans GROUP BY volume_id);
                """
            : """
                SELECT o.id, s.volume_id, s.volume_label
                FROM scan_observations o
                JOIN scans s ON s.id = o.scan_id
                WHERE o.scan_id IN (SELECT MAX(id) FROM scans GROUP BY volume_id);
                """;

        var volumes = new Dictionary<long, VolumeDuFichier>();
        using var lecteur = commande.ExecuteReader();
        while (lecteur.Read())
        {
            volumes[lecteur.GetInt64(0)] = new VolumeDuFichier(
                lecteur.GetString(1),
                lecteur.IsDBNull(2) ? null : lecteur.GetString(2));
        }

        return volumes;
    }
}
