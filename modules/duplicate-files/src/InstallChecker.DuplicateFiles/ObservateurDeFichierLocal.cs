using System.Security;
using System.Security.Cryptography;
using InstallChecker.DuplicateFiles;

namespace InstallChecker;

public sealed class ObservateurDeFichierLocal : IObservateurDeFichier
{
    public ObservationFichierCourant Observer(string chemin)
    {
        try
        {
            var attributs = File.GetAttributes(chemin);
            if ((attributs & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
            {
                return new ObservationFichierCourant(
                    EtatLectureFichier.TypeNonPrisEnCharge,
                    null,
                    "type de chemin non pris en charge");
            }

            using var flux = new FileStream(
                chemin,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 128 * 1024,
                FileOptions.SequentialScan);
            var hash = Convert.ToHexString(SHA256.HashData(flux)).ToLowerInvariant();
            return new ObservationFichierCourant(EtatLectureFichier.Disponible, hash, null);
        }
        catch (FileNotFoundException)
        {
            return Absent();
        }
        catch (DirectoryNotFoundException)
        {
            return Absent();
        }
        catch (UnauthorizedAccessException)
        {
            return Illisible();
        }
        catch (SecurityException)
        {
            return Illisible();
        }
        catch (IOException)
        {
            return Illisible();
        }
        catch (ArgumentException)
        {
            return TypeNonPrisEnCharge();
        }
        catch (NotSupportedException)
        {
            return TypeNonPrisEnCharge();
        }
    }

    private static ObservationFichierCourant Absent() =>
        new(EtatLectureFichier.Absent, null, "fichier absent");

    private static ObservationFichierCourant Illisible() =>
        new(EtatLectureFichier.Illisible, null, "fichier illisible");

    private static ObservationFichierCourant TypeNonPrisEnCharge() =>
        new(EtatLectureFichier.TypeNonPrisEnCharge, null, "type de chemin non pris en charge");
}
