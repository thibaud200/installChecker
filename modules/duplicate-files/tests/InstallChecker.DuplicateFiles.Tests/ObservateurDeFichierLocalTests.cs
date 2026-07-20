using System.Security.Cryptography;
using InstallChecker.DuplicateFiles;

namespace InstallChecker.Tests;

public class ObservateurDeFichierLocalTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "file-observer-tests-" + Guid.NewGuid())).FullName;

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Un_fichier_ordinaire_est_lu_sans_etre_modifie()
    {
        var chemin = Path.Combine(_root, "fichier.bin");
        File.WriteAllText(chemin, "contenu");
        var avant = File.ReadAllBytes(chemin);

        var observation = new ObservateurDeFichierLocal().Observer(chemin);

        Assert.Equal(EtatLectureFichier.Disponible, observation.Etat);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(avant)).ToLowerInvariant(), observation.HashObserve);
        Assert.Null(observation.Detail);
        Assert.Equal(avant, File.ReadAllBytes(chemin));
    }

    [Fact]
    public void Un_chemin_absent_est_un_resultat_local()
    {
        var observation = new ObservateurDeFichierLocal().Observer(Path.Combine(_root, "absent.bin"));

        Assert.Equal(EtatLectureFichier.Absent, observation.Etat);
        Assert.Null(observation.HashObserve);
        Assert.Equal("fichier absent", observation.Detail);
    }

    [Fact]
    public void Un_repertoire_nest_pas_un_fichier_pris_en_charge()
    {
        var observation = new ObservateurDeFichierLocal().Observer(_root);

        Assert.Equal(EtatLectureFichier.TypeNonPrisEnCharge, observation.Etat);
        Assert.Null(observation.HashObserve);
        Assert.Equal("type de chemin non pris en charge", observation.Detail);
    }
}
