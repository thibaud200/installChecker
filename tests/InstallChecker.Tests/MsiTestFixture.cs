using System.Runtime.InteropServices;

namespace InstallChecker.Tests;

/// <summary>
/// Fabrique un vrai fichier MSI via Windows Installer lui-même (MSIDBOPEN_CREATE) :
/// les propriétés attendues sont connues par construction, au caractère près.
/// </summary>
internal static class MsiTestFixture
{
    private static readonly IntPtr MsidbOpenCreate = new(3);

    public static void CreateMsi(string path, Dictionary<string, string> properties)
    {
        Check(MsiOpenDatabaseW(path, MsidbOpenCreate, out var database), "MsiOpenDatabase(CREATE)");
        try
        {
            Execute(database,
                "CREATE TABLE `Property` (`Property` CHAR(72) NOT NULL, `Value` CHAR(0) NOT NULL LOCALIZABLE PRIMARY KEY `Property`)");
            foreach (var (name, value) in properties)
                Execute(database, $"INSERT INTO `Property` (`Property`, `Value`) VALUES ('{name}', '{value}')");
            Check(MsiDatabaseCommit(database), "MsiDatabaseCommit");
        }
        finally
        {
            MsiCloseHandle(database);
        }
    }

    private static void Execute(IntPtr database, string query)
    {
        Check(MsiDatabaseOpenViewW(database, query, out var view), $"OpenView: {query}");
        try
        {
            Check(MsiViewExecute(view, IntPtr.Zero), $"Execute: {query}");
        }
        finally
        {
            MsiCloseHandle(view);
        }
    }

    private static void Check(uint result, string operation)
    {
        if (result != 0)
            throw new InvalidOperationException($"Échec fixture MSI ({operation}) : code {result}");
    }

    [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint MsiOpenDatabaseW(string databasePath, IntPtr persist, out IntPtr database);

    [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern uint MsiDatabaseOpenViewW(IntPtr database, string query, out IntPtr view);

    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiViewExecute(IntPtr view, IntPtr record);

    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiDatabaseCommit(IntPtr database);

    [DllImport("msi.dll", ExactSpelling = true)]
    private static extern uint MsiCloseHandle(IntPtr handle);
}
