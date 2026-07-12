using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class SyntheseDeBibliothequeTests
{
    // Un groupe classé : n exemplaires de rangs 1..n, tous de même taille (égaux en octets par
    // construction — invariant de la strate contenu). Le classeur conserve exactement un exemplaire
    // par fichier ; le cardinal du groupe est préservé.
    private static IReadOnlyList<ExemplaireClasse> Groupe(long taille, int n) =>
        Enumerable.Range(0, n)
            .Select(i => new ExemplaireClasse(
                new FichierEnrichi(i + 1, $@"C:\g\f{i}.exe", taille, false, false, false, "2026-01-01T00:00:00.0000000Z"),
                i + 1, "motif"))
            .ToList();

    [Fact]
    public void Les_cinq_agregats_sont_calcules_sur_un_groupe_de_deux()
    {
        var synthese = SyntheseDeBibliotheque.Calculer([Groupe(taille: 500, n: 2)]);

        Assert.Equal(1, synthese.NombreDeGroupes);
        Assert.Equal(1, synthese.NombreDeFichiersRedondants);
        Assert.Equal(500L, synthese.EspaceRecuperableOctets);
        Assert.Equal(1, synthese.NombreDeFichiersAConserver);
        Assert.Equal(1, synthese.NombreDeCandidatsASuppression);
    }

    [Fact]
    public void Les_cinq_agregats_sont_calcules_sur_un_triplet()
    {
        var synthese = SyntheseDeBibliotheque.Calculer([Groupe(taille: 500, n: 3)]);

        Assert.Equal(1, synthese.NombreDeGroupes);
        Assert.Equal(2, synthese.NombreDeFichiersRedondants);
        Assert.Equal(1000L, synthese.EspaceRecuperableOctets);
        Assert.Equal(1, synthese.NombreDeFichiersAConserver);
        Assert.Equal(2, synthese.NombreDeCandidatsASuppression);
    }

    [Fact]
    public void L_espace_recuperable_utilise_la_taille_commune_de_chaque_groupe()
    {
        var synthese = SyntheseDeBibliotheque.Calculer([Groupe(taille: 500, n: 2), Groupe(taille: 1000, n: 3)]);

        Assert.Equal(2, synthese.NombreDeGroupes);
        Assert.Equal(3, synthese.NombreDeFichiersRedondants);
        Assert.Equal(500L * 1 + 1000L * 2, synthese.EspaceRecuperableOctets);
    }

    [Fact]
    public void En_v1_les_fichiers_a_conserver_egalent_le_nombre_de_groupes_et_les_candidats_les_redondants()
    {
        var synthese = SyntheseDeBibliotheque.Calculer([Groupe(taille: 500, n: 2), Groupe(taille: 1000, n: 3)]);

        Assert.Equal(synthese.NombreDeGroupes, synthese.NombreDeFichiersAConserver);
        Assert.Equal(synthese.NombreDeFichiersRedondants, synthese.NombreDeCandidatsASuppression);
    }
}
