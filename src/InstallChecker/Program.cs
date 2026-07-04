using InstallChecker;

return args switch
{
    ["scan", var root] => ScanCommand.Run(root, "installchecker.db", Console.Out, Console.Error),
    ["scan", var root, "--db", var db] => ScanCommand.Run(root, db, Console.Out, Console.Error),
    _ => Usage(),
};

static int Usage()
{
    Console.Error.WriteLine("Usage : installchecker scan <dossier> [--db <fichier>]");
    return 2;
}
