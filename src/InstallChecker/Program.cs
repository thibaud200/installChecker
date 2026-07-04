using InstallChecker;

if (args is ["scan", var root])
    return ScanCommand.Run(root, Console.Out, Console.Error);

Console.Error.WriteLine("Usage : installchecker scan <dossier>");
return 2;
