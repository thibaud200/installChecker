using System.Runtime.InteropServices;
using System.Text;

namespace InstallChecker;

/// <summary>
/// Capacité autonome : observe la table Property d'une base MSI via l'API native msi.dll,
/// en lecture seule stricte (MSIDBOPEN_READONLY). Jamais de session d'installation, de réparation
/// ni de validation. L'extracteur décide seul si le fichier est une base MSI lisible.
/// </summary>
public static class MsiPropertiesExtractor
{
    /// <summary>Toutes les propriétés null = pas une base MSI lisible, ou propriétés absentes (pas une erreur).</summary>
    public sealed record MsiProperties(
        string? ProductName, string? ProductVersion, string? Manufacturer,
        string? ProductCode, string? UpgradeCode, string? ProductLanguage)
    {
        public static readonly MsiProperties None = new(null, null, null, null, null, null);
    }

    private const uint NoError = 0;

    public static MsiProperties Read(string path)
    {
        // MSIDBOPEN_READONLY = (LPCTSTR)0 : ouverture en lecture seule, aucune écriture possible.
        if (MsiOpenDatabaseW(path, IntPtr.Zero, out var database) != NoError)
            return MsiProperties.None;

        try
        {
            var values = ReadPropertyTable(database);
            return new MsiProperties(
                ProductName: values.GetValueOrDefault("ProductName"),
                ProductVersion: values.GetValueOrDefault("ProductVersion"),
                Manufacturer: values.GetValueOrDefault("Manufacturer"),
                ProductCode: values.GetValueOrDefault("ProductCode"),
                UpgradeCode: values.GetValueOrDefault("UpgradeCode"),
                ProductLanguage: values.GetValueOrDefault("ProductLanguage"));
        }
        finally
        {
            MsiCloseHandle(database);
        }
    }

    /// <summary>Lit la table Property entière, telle quelle. Table absente → dictionnaire vide.</summary>
    private static Dictionary<string, string> ReadPropertyTable(IntPtr database)
    {
        var values = new Dictionary<string, string>();
        if (MsiDatabaseOpenViewW(database, "SELECT `Property`, `Value` FROM `Property`", out var view) != NoError)
            return values;

        try
        {
            if (MsiViewExecute(view, IntPtr.Zero) != NoError)
                return values;
            while (MsiViewFetch(view, out var record) == NoError)
            {
                try
                {
                    values[GetString(record, 1)] = GetString(record, 2);
                }
                finally
                {
                    MsiCloseHandle(record);
                }
            }
            return values;
        }
        finally
        {
            MsiCloseHandle(view);
        }
    }

    private static string GetString(IntPtr record, uint field)
    {
        var length = 0u; // premier appel : buffer vide, ERROR_MORE_DATA attendu, length reçoit la taille requise
        MsiRecordGetStringW(record, field, new StringBuilder(1), ref length);
        length++;
        var buffer = new StringBuilder((int)length);
        MsiRecordGetStringW(record, field, buffer, ref length);
        return buffer.ToString();
    }

    [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint MsiOpenDatabaseW(string databasePath, IntPtr persist, out IntPtr database);

    [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint MsiDatabaseOpenViewW(IntPtr database, string query, out IntPtr view);

    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiViewExecute(IntPtr view, IntPtr record);

    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiViewFetch(IntPtr view, out IntPtr record);

    [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint MsiRecordGetStringW(IntPtr record, uint field, StringBuilder valueBuffer, ref uint bufferSize);

    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiCloseHandle(IntPtr handle);
}
