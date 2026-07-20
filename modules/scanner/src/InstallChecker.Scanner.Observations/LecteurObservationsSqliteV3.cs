using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Observations;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Scanner.Observations;

/// <summary>Projette le stockage Scanner v3 vers le port générique d'observations Ω.</summary>
public sealed class LecteurObservationsSqliteV3(string cheminBase) : IObservationsSource
{
    public const long VersionSupportee = 3;

    private const string ScansCourants = "(SELECT MAX(id) FROM scans GROUP BY volume_id)";
    private const string ActesCourants =
        "(SELECT id FROM scan_observations WHERE scan_id IN " + ScansCourants + ")";

    private static readonly string[] TablesDeCapacite =
        ["version_info", "file_headers", "pe_info", "authenticode", "msi_properties", "appx_manifest"];

    public ModeleObservations ProjeterModele()
    {
        using var connexion = Ouvrir();
        var actesBruts = LireActesBruts(connexion);
        var attributs = actesBruts.Keys.ToDictionary(
            id => id,
            _ => new Dictionary<Attribut, ValeurObservee>());

        foreach (var table in TablesDeCapacite)
            LireCapacite(connexion, table, actesBruts.Keys, attributs);

        return new ModeleObservations(
            actesBruts
                .OrderBy(kv => kv.Key)
                .Select(kv => new ActeObservation(
                    kv.Key,
                    kv.Value.Taille,
                    kv.Value.Empreinte,
                    attributs[kv.Key]))
                .ToList());
    }

    public IReadOnlyList<ContexteObservation> ProjeterContexte()
    {
        using var connexion = Ouvrir();
        var contextes = new List<ContexteObservation>();
        try
        {
            using var commande = connexion.CreateCommand();
            commande.CommandText =
                "SELECT id, path, scanned_at FROM scan_observations " +
                "WHERE scan_id IN " + ScansCourants + ";";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                contextes.Add(new ContexteObservation(
                    LireEntierObligatoire(lecteur, 0, "scan_observations", "id"),
                    LireTexteObligatoire(lecteur, 1, "scan_observations", "path"),
                    LireTexteObligatoire(lecteur, 2, "scan_observations", "scanned_at")));
            }
        }
        catch (SqliteException ex)
        {
            throw new OmegaInvalideException($"scan_observations illisible : {ex.Message}");
        }

        return contextes.OrderBy(c => c.Identifiant).ToList();
    }

    public IndexOmega ProjeterIdentite()
    {
        var modele = ProjeterModele();
        var actes = modele.Actes.OrderBy(a => a.Identifiant).ToList();
        var encodage = new StringBuilder();
        foreach (var acte in actes)
        {
            AjouterChamp(encodage, acte.Identifiant.ToString(CultureInfo.InvariantCulture));
            AjouterChamp(encodage, acte.Empreinte);
        }

        var empreinte = Convert.ToHexStringLower(
            SHA256.HashData(Encoding.UTF8.GetBytes(encodage.ToString())));
        return new IndexOmega(VersionSupportee, actes.Count, empreinte);
    }

    private SqliteConnection Ouvrir()
    {
        if (!File.Exists(cheminBase))
            throw new OmegaAbsentException($"support d'observations introuvable : {cheminBase}");

        var connexion = new SqliteConnection($"Data Source={cheminBase};Mode=ReadOnly");
        try
        {
            connexion.Open();
            using var commande = connexion.CreateCommand();
            commande.CommandText = "PRAGMA user_version;";
            var version = (long)commande.ExecuteScalar()!;
            if (version != VersionSupportee)
            {
                throw new OmegaIncompatibleException(
                    $"version de contrat non supportée : {cheminBase} : user_version={version}, attendu {VersionSupportee}");
            }
        }
        catch (SqliteException ex)
        {
            connexion.Dispose();
            throw new OmegaAbsentException($"support d'observations illisible : {cheminBase} : {ex.Message}");
        }
        catch
        {
            connexion.Dispose();
            throw;
        }

        return connexion;
    }

    private static Dictionary<long, (long Taille, string Empreinte)> LireActesBruts(SqliteConnection connexion)
    {
        var actes = new Dictionary<long, (long, string)>();
        try
        {
            using var commande = connexion.CreateCommand();
            commande.CommandText =
                "SELECT id, size, sha256 FROM scan_observations " +
                "WHERE scan_id IN " + ScansCourants + ";";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                var id = LireEntierObligatoire(lecteur, 0, "scan_observations", "id");
                if (!actes.TryAdd(id, (
                    LireEntierObligatoire(lecteur, 1, "scan_observations", "size"),
                    LireTexteObligatoire(lecteur, 2, "scan_observations", "sha256"))))
                {
                    throw new OmegaInvalideException(
                        $"scan_observations : identifiant d'acte dupliqué : {id}");
                }
            }
        }
        catch (SqliteException ex)
        {
            throw new OmegaInvalideException($"scan_observations illisible : {ex.Message}");
        }

        return actes;
    }

    private static void LireCapacite(
        SqliteConnection connexion,
        string table,
        IReadOnlyCollection<long> identifiants,
        IReadOnlyDictionary<long, Dictionary<Attribut, ValeurObservee>> attributsParActe)
    {
        var vus = new HashSet<long>();
        try
        {
            using var commande = connexion.CreateCommand();
            commande.CommandText = $"SELECT * FROM {table} WHERE observation_id IN {ActesCourants};";
            using var lecteur = commande.ExecuteReader();
            var colonnes = Enumerable.Range(0, lecteur.FieldCount).Select(lecteur.GetName).ToList();
            var indexObservationId = colonnes.IndexOf("observation_id");
            if (indexObservationId < 0)
                throw new OmegaInvalideException($"{table} : colonne observation_id absente");

            while (lecteur.Read())
            {
                var id = LireEntierObligatoire(lecteur, indexObservationId, table, "observation_id");
                if (!attributsParActe.TryGetValue(id, out var attributs))
                    throw new OmegaInvalideException($"{table} : observation_id {id} sans acte correspondant");
                if (!vus.Add(id))
                    throw new OmegaInvalideException($"{table} : plusieurs lignes pour l'observation {id}");

                for (var i = 0; i < colonnes.Count; i++)
                {
                    if (i != indexObservationId)
                        attributs[new Attribut(table, colonnes[i])] = LireValeur(lecteur, i, table, colonnes[i]);
                }
            }
        }
        catch (SqliteException ex)
        {
            throw new OmegaInvalideException($"table de capacité illisible ou absente : {table} : {ex.Message}");
        }

        var manquant = identifiants.Except(vus).FirstOrDefault();
        if (identifiants.Count != vus.Count)
            throw new OmegaInvalideException($"{table} : aucune ligne pour l'observation {manquant}");
    }

    private static long LireEntierObligatoire(SqliteDataReader lecteur, int index, string table, string colonne) =>
        LireValeur(lecteur, index, table, colonne) is ValeurObservee.Entier entier
            ? entier.Valeur
            : throw new OmegaInvalideException($"{table}.{colonne} : valeur obligatoire absente");

    private static string LireTexteObligatoire(SqliteDataReader lecteur, int index, string table, string colonne) =>
        LireValeur(lecteur, index, table, colonne) is ValeurObservee.Texte texte
            ? texte.Valeur
            : throw new OmegaInvalideException($"{table}.{colonne} : valeur obligatoire absente");

    private static ValeurObservee LireValeur(SqliteDataReader lecteur, int index, string table, string colonne)
    {
        if (lecteur.IsDBNull(index))
            return ValeurObservee.Absente.Instance;
        if (lecteur.GetFieldType(index) == typeof(long))
            return new ValeurObservee.Entier(lecteur.GetInt64(index));
        if (lecteur.GetFieldType(index) == typeof(string))
            return new ValeurObservee.Texte(lecteur.GetString(index));
        throw new OmegaInvalideException($"{table}.{colonne} : type de valeur hors contrat");
    }

    private static void AjouterChamp(StringBuilder encodage, string valeur) =>
        encodage.Append(valeur.Length).Append(':').Append(valeur).Append(',');
}
