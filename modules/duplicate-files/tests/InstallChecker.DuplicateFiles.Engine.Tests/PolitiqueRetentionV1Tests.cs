using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class PolitiqueRetentionV1Tests
{
    private static FichierEnrichi Fichier(
        long acteId, string chemin, bool authenticode = false, bool pe = false, bool msi = false,
        string date = "2026-01-01T00:00:00.0000000Z", long taille = 0) =>
        new(acteId, chemin, taille, authenticode, pe, msi, date);

    [Fact]
    public void Le_plus_riche_en_observations_est_classe_premier()
    {
        var pauvre = Fichier(1, @"C:\a\setup.exe");
        var riche = Fichier(2, @"C:\b\setup.exe", authenticode: true, pe: true, msi: true);

        var classement = PolitiqueRetentionV1.Classer([pauvre, riche]);

        Assert.Equal(riche.ActeId, classement[0].Fichier.ActeId);
        Assert.Equal(1, classement[0].Rang);
        Assert.Equal(pauvre.ActeId, classement[1].Fichier.ActeId);
        Assert.Equal(2, classement[1].Rang);
    }

    [Fact]
    public void A_richesse_egale_le_nom_qui_ne_ressemble_pas_a_une_copie_est_prefere()
    {
        var original = Fichier(1, @"C:\a\setup.exe");
        var copie = Fichier(2, @"C:\b\setup (1).exe");

        var classement = PolitiqueRetentionV1.Classer([copie, original]);

        Assert.Equal(original.ActeId, classement[0].Fichier.ActeId);
    }

    [Fact]
    public void A_richesse_et_nom_egaux_le_plus_recemment_observe_est_prefere()
    {
        var ancien = Fichier(1, @"C:\a\setup.exe", date: "2020-01-01T00:00:00.0000000Z");
        var recent = Fichier(2, @"C:\b\setup.exe", date: "2026-01-01T00:00:00.0000000Z");

        var classement = PolitiqueRetentionV1.Classer([ancien, recent]);

        Assert.Equal(recent.ActeId, classement[0].Fichier.ActeId);
    }

    [Fact]
    public void A_tout_egal_lordre_alphabetique_du_chemin_departage_de_facon_stable()
    {
        var b = Fichier(1, @"C:\b\setup.exe");
        var a = Fichier(2, @"C:\a\setup.exe");

        var premier = PolitiqueRetentionV1.Classer([b, a]);
        var second = PolitiqueRetentionV1.Classer([b, a]);

        Assert.Equal(a.ActeId, premier[0].Fichier.ActeId);
        Assert.Equal(premier.Select(e => e.Fichier.ActeId), second.Select(e => e.Fichier.ActeId));
    }

    [Fact]
    public void Labsence_de_tout_signal_ne_provoque_aucune_erreur()
    {
        var f1 = Fichier(1, @"C:\a\setup.exe");
        var f2 = Fichier(2, @"C:\b\setup.exe");

        var classement = PolitiqueRetentionV1.Classer([f1, f2]);

        Assert.Equal(2, classement.Count);
        Assert.Equal([1, 2], classement.Select(e => e.Rang));
    }

    [Fact]
    public void Les_criteres_de_classement_sont_structures_dans_lordre_de_la_politique()
    {
        var fichier = Fichier(7, @"C:\a\setup (1).exe", authenticode: true);

        var criteres = PolitiqueRetentionV1.Expliquer(fichier);

        Assert.Equal(
            [
                CritereRetention.RichesseObservations,
                CritereRetention.NomDeCopie,
                CritereRetention.DateObservation,
                CritereRetention.Chemin,
                CritereRetention.ActeIdDepartage,
            ],
            criteres.Select(c => c.Critere));
        Assert.Equal([1, 2, 3, 4, 5], criteres.Select(c => c.Priorite));
        Assert.Equal("1/3", criteres[0].Valeur);
        Assert.Equal("True", criteres[1].Valeur);
        Assert.Equal("7", criteres[4].Valeur);
    }
}
