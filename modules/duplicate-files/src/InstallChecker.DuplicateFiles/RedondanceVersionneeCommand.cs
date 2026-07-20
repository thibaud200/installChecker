using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Erreurs;
using InstallChecker.Scanner.Observations;

namespace InstallChecker;

public static class RedondanceVersionneeCommand
{
    private static readonly JsonSerializerOptions OptionsJson = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    public static int Deriver(string cheminBase, TextWriter output, TextWriter errors)
    {
        try
        {
            var omega = SourceObservationsSqlite.Ouvrir(cheminBase);
            var rapport = GenerateurRedondanceVersionnee.Generer(omega);
            var json = JsonSerializer.Serialize(rapport, OptionsJson);
            output.WriteLine(json);
            return 0;
        }
        catch (ErreurOmega ex)
        {
            errors.WriteLine(ex.Message);
            return 1;
        }
    }
}
