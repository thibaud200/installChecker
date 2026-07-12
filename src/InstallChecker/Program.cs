using InstallChecker;

// 013 § 4 : « UTF-8 sans BOM » — la console Windows encoderait sinon la sortie dans sa page de
// codes héritée (CP850), brisant l'identité bit à bit de l'émission canonique (EXG-18, report 3).
Console.OutputEncoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

if (args is ["scan", var root, .. var options])
{
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
}

if (args is ["identity", "derive", var cheminBase, var cheminRegistre])
    return IdentityCommand.Deriver(cheminBase, cheminRegistre, Console.Out, Console.Error);

if (args is ["identity", "audit", var baseAudit, var registreAudit, var question, var strate, var acte]
    && long.TryParse(acte, out var acteId))
    return IdentityCommand.Auditer(baseAudit, registreAudit, question, strate, acteId, Console.Out, Console.Error);

if (args is ["duplicates", var cheminBaseDup, var cheminRegistreDup])
    return DuplicatesCommand.Deriver(cheminBaseDup, cheminRegistreDup, Console.Out, Console.Error);

return Usage();

static int Usage()
{
    Console.Error.WriteLine("Usage : installchecker scan <dossier> [--db <fichier>] [--json]");
    Console.Error.WriteLine("        installchecker identity derive <base.db> <registre>");
    Console.Error.WriteLine("        installchecker identity audit <base.db> <registre> <question> <strate> <acte>");
    Console.Error.WriteLine("        installchecker duplicates <base.db> <registre>");
    return 2;
}
