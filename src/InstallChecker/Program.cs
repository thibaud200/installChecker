using InstallChecker;

if (args is not ["scan", var root, .. var options])
    return Usage();

var db = "installchecker.db";
var json = false;
for (var i = 0; i < options.Length; i++)
{
    if (options[i] == "--json")
        json = true;
    else if (options[i] == "--db" && i + 1 < options.Length)
        db = options[++i];
    else
        return Usage();
}

return ScanCommand.Run(root, db, json, Console.Out, Console.Error);

static int Usage()
{
    Console.Error.WriteLine("Usage : installchecker scan <dossier> [--db <fichier>] [--json]");
    return 2;
}
