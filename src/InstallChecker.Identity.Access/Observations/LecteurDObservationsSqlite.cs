using InstallChecker.Identity.Erreurs;
using InstallChecker.Identity.Observations;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Identity.Access.Observations;

/// <summary>
/// L'adaptateur C1 (013 § 1.1, § 5) : projette la base SQLite d'observations produite par le
/// pipeline figé (schéma <c>user_version = 1</c>) vers le modèle logique du moteur pur. Seul
/// composant à connaître les noms de tables et de colonnes ; <c>InstallChecker.Identity</c> n'en
/// sait rien (013 § 1.2, § 5) — il ne reçoit que des <see cref="ActeObservation"/>.
/// </summary>
public sealed class LecteurDObservationsSqlite(string cheminBase) : IObservationsSource
{
    private const long VersionDeContratSupportee = 1;

    // Une table = une capacité (001 § Provenance). Le répertoire est ouvert (001 Déf. 4) :
    // ajouter une capacité future n'exige qu'une entrée ici, jamais un nouveau type de modèle.
    private static readonly string[] TablesDeCapacite =
        ["version_info", "file_headers", "pe_info", "authenticode", "msi_properties", "appx_manifest"];

    public ModeleObservations ProjeterModele()
    {
        using var connection = Ouvrir();

        var actesBruts = LireActesBruts(connection);
        var attributsParActe = actesBruts.Keys.ToDictionary(id => id, _ => new Dictionary<Attribut, ValeurObservee>());

        foreach (var table in TablesDeCapacite)
        {
            LireCapacite(connection, table, actesBruts.Keys, attributsParActe);
        }

        var actes = actesBruts
            .OrderBy(kv => kv.Key)
            .Select(kv => new ActeObservation(kv.Key, kv.Value.Taille, kv.Value.Empreinte, attributsParActe[kv.Key]))
            .ToList();

        return new ModeleObservations(actes);
    }

    public IReadOnlyList<ContexteObservation> ProjeterContexte()
    {
        using var connection = Ouvrir();

        var contextes = new List<ContexteObservation>();
        try
        {
            using var commande = connection.CreateCommand();
            commande.CommandText = "SELECT id, path, scanned_at FROM scan_observations;";
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

    private SqliteConnection Ouvrir()
    {
        if (!File.Exists(cheminBase))
        {
            throw new OmegaAbsentException($"support d'observations introuvable : {cheminBase}");
        }

        var connection = new SqliteConnection($"Data Source={cheminBase};Mode=ReadOnly");
        try
        {
            connection.Open();

            using var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA user_version;";
            var version = (long)pragma.ExecuteScalar()!;

            if (version != VersionDeContratSupportee)
            {
                throw new OmegaIncompatibleException(
                    $"version de contrat non supportée : {cheminBase} : user_version={version}, attendu {VersionDeContratSupportee}");
            }
        }
        catch (SqliteException ex)
        {
            connection.Dispose();
            throw new OmegaAbsentException($"support d'observations illisible : {cheminBase} : {ex.Message}");
        }
        catch
        {
            connection.Dispose();
            throw;
        }

        return connection;
    }

    private static Dictionary<long, (long Taille, string Empreinte)> LireActesBruts(SqliteConnection connection)
    {
        var actes = new Dictionary<long, (long, string)>();
        try
        {
            using var commande = connection.CreateCommand();
            commande.CommandText = "SELECT id, size, sha256 FROM scan_observations;";
            using var lecteur = commande.ExecuteReader();
            while (lecteur.Read())
            {
                // « acte sans identifiant » et identifiant dupliqué sont des violations nommées du
                // contrat de Ω (014 C1, clause « refuse » ; 014 § 6 : « identifiant unique, stable ») :
                // « Ω invalide », jamais une exception .NET (011 § 5).
                var id = LireEntierObligatoire(lecteur, 0, "scan_observations", "id");
                if (!actes.TryAdd(id, (
                    LireEntierObligatoire(lecteur, 1, "scan_observations", "size"),
                    LireTexteObligatoire(lecteur, 2, "scan_observations", "sha256"))))
                {
                    throw new OmegaInvalideException(
                        $"scan_observations : identifiant d'acte dupliqué : {id} (014 § 6 : identifiant unique)");
                }
            }
        }
        catch (SqliteException ex)
        {
            throw new OmegaInvalideException($"scan_observations illisible : {ex.Message}");
        }

        return actes;
    }

    /// <summary>
    /// Lit intégralement une table de capacité et fusionne ses colonnes (hors <c>observation_id</c>)
    /// dans les attributs de chaque acte concerné. Vérifie l'invariant 1:1 (014 § 6) : exactement
    /// une ligne par acte connu, aucune ligne orpheline.
    /// </summary>
    private static void LireCapacite(
        SqliteConnection connection,
        string table,
        IReadOnlyCollection<long> identifiantsConnus,
        IReadOnlyDictionary<long, Dictionary<Attribut, ValeurObservee>> attributsParActe)
    {
        var vus = new HashSet<long>();
        try
        {
            using var commande = connection.CreateCommand();
            commande.CommandText = $"SELECT * FROM {table};";
            using var lecteur = commande.ExecuteReader();

            var colonnes = Enumerable.Range(0, lecteur.FieldCount).Select(lecteur.GetName).ToList();
            var indexObservationId = colonnes.IndexOf("observation_id");

            if (indexObservationId < 0)
            {
                throw new OmegaInvalideException(
                    $"{table} : colonne observation_id absente (structure inattendue — 011 § 5)");
            }

            while (lecteur.Read())
            {
                var observationId = LireEntierObligatoire(lecteur, indexObservationId, table, "observation_id");

                if (!attributsParActe.TryGetValue(observationId, out var attributs))
                {
                    throw new OmegaInvalideException(
                        $"{table} : observation_id {observationId} sans acte correspondant dans scan_observations");
                }

                if (!vus.Add(observationId))
                {
                    throw new OmegaInvalideException(
                        $"{table} : plusieurs lignes pour l'observation {observationId} (invariant 1:1 rompu)");
                }

                for (var i = 0; i < colonnes.Count; i++)
                {
                    if (i == indexObservationId) continue;
                    attributs[new Attribut(table, colonnes[i])] = LireValeur(lecteur, i, table, colonnes[i]);
                }
            }
        }
        catch (SqliteException ex)
        {
            throw new OmegaInvalideException($"table de capacité illisible ou absente : {table} : {ex.Message}");
        }

        var manquant = identifiantsConnus.Except(vus).ToList();
        if (manquant.Count > 0)
        {
            throw new OmegaInvalideException(
                $"{table} : aucune ligne pour l'observation {manquant[0]} (invariant 1:1 rompu)");
        }
    }

    // --- Lecture des colonnes obligatoires de scan_observations (014 § 6 : taille et empreinte de
    //     contenu toujours portées) — réutilise le garde IsDBNull/type de LireValeur plutôt que de
    //     le dupliquer ; une valeur absente ou de type inattendu y est « structure inattendue » (Ω invalide).

    private static long LireEntierObligatoire(SqliteDataReader lecteur, int index, string table, string colonne) =>
        LireValeur(lecteur, index, table, colonne) is ValeurObservee.Entier entier
            ? entier.Valeur
            : throw new OmegaInvalideException($"{table}.{colonne} : valeur obligatoire absente (014 § 6)");

    private static string LireTexteObligatoire(SqliteDataReader lecteur, int index, string table, string colonne) =>
        LireValeur(lecteur, index, table, colonne) is ValeurObservee.Texte texte
            ? texte.Valeur
            : throw new OmegaInvalideException($"{table}.{colonne} : valeur obligatoire absente (014 § 6)");

    private static ValeurObservee LireValeur(SqliteDataReader lecteur, int index, string table, string colonne)
    {
        if (lecteur.IsDBNull(index)) return ValeurObservee.Absente.Instance;

        var type = lecteur.GetFieldType(index);
        if (type == typeof(long)) return new ValeurObservee.Entier(lecteur.GetInt64(index));
        if (type == typeof(string)) return new ValeurObservee.Texte(lecteur.GetString(index));

        throw new OmegaInvalideException(
            $"{table}.{colonne} : type de valeur hors contrat (texte, entier ou absence attendus — 014 § 6)");
    }
}
