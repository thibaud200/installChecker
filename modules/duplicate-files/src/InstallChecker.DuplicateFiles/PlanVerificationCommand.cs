using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using InstallChecker.DuplicateFiles;

namespace InstallChecker;

public static class PlanVerificationCommand
{
    private static readonly JsonSerializerOptions OptionsLecture = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly JsonSerializerOptions OptionsEcriture = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    public static int Verifier(
        string cheminPlan,
        TextWriter output,
        TextWriter errors)
    {
        try
        {
            var json = File.ReadAllText(cheminPlan);
            var plan = JsonSerializer.Deserialize<PlanDeSuppression>(json, OptionsLecture)
                ?? throw new PlanInvalideException("plan JSON vide");
            var rapport = ValidateurDePlan.Verifier(
                plan,
                new ObservateurDeFichierLocal(),
                ProtectionDesChemins.EstProtegeParDefaut);

            output.WriteLine(JsonSerializer.Serialize(rapport, OptionsEcriture));
            return rapport.Executable ? 0 : 3;
        }
        catch (FileNotFoundException)
        {
            errors.WriteLine("plan introuvable");
            return 1;
        }
        catch (UnauthorizedAccessException)
        {
            errors.WriteLine("plan illisible");
            return 1;
        }
        catch (IOException)
        {
            errors.WriteLine("plan illisible");
            return 1;
        }
        catch (JsonException)
        {
            errors.WriteLine("plan JSON invalide");
            return 1;
        }
        catch (PlanInvalideException ex)
        {
            errors.WriteLine(ex.Message);
            return 1;
        }
    }
}
