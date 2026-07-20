using InstallChecker.DuplicateFiles.Desktop.Adaptateurs;
using InstallChecker.DuplicateFiles.Desktop.Session;

namespace InstallChecker.DuplicateFiles.Desktop.Tests;

public sealed class AdaptateursCommandesTests
{
    [Fact]
    public async Task Analyseur_restitue_les_deux_JSON_sans_les_modifier()
    {
        var analyseur = new AnalyseurBibliotheque(
            (_, _, output, _) =>
            {
                output.Write("{\"Groupes\":[]}");
                return 0;
            },
            (_, output, _) =>
            {
                output.Write("{\"VersionContrat\":\"duplicate-files/version-redundancy/v1\",\"Groupes\":[]}");
                return 0;
            });

        var resultat = await analyseur.AnalyserAsync(Bibliotheque(), default);

        Assert.True(resultat.Reussi);
        Assert.Equal(0, resultat.RapportDoublons!.Value.GetProperty("Groupes").GetArrayLength());
        Assert.Equal(
            "duplicate-files/version-redundancy/v1",
            resultat.RapportVersions!.Value.GetProperty("VersionContrat").GetString());
    }

    [Fact]
    public async Task Scanner_signale_un_resultat_partiel_si_un_second_lecteur_echoue()
    {
        var appels = 0;
        var scanner = new ScannerBibliotheque((racine, _, _, output, errors, _, _) =>
        {
            appels++;
            if (appels == 1)
            {
                output.WriteLine($"{racine}\\fichier.exe\t42\tabcd");
                return 0;
            }

            errors.WriteLine("lecteur indisponible");
            return 1;
        });
        var bibliotheque = Bibliotheque() with { Racines = [@"C:\Corpus", @"D:\Archives"] };

        var resultat = await scanner.ExecuterAsync(bibliotheque, null, default);

        Assert.False(resultat.Reussi);
        Assert.True(resultat.Partiel);
        Assert.Equal(1, resultat.FichiersTraites);
        Assert.Contains(resultat.Diagnostics, d => d.Code == "ScanRacineEchoue");
    }

    [Fact]
    public async Task Analyseur_ne_reussit_pas_si_une_commande_metier_echoue()
    {
        var analyseur = new AnalyseurBibliotheque(
            (_, _, _, errors) =>
            {
                errors.WriteLine("échec doublons");
                return 1;
            },
            (_, output, _) =>
            {
                output.Write("{\"Groupes\":[]}");
                return 0;
            });

        var resultat = await analyseur.AnalyserAsync(Bibliotheque(), default);

        Assert.False(resultat.Reussi);
        Assert.Null(resultat.RapportDoublons);
        Assert.NotNull(resultat.RapportVersions);
    }

    private static BibliothequeUi Bibliotheque() => new(
        "Bibliothèque",
        "bibliotheque.db",
        "registre",
        [@"C:\Corpus"],
        "bibliotheque.session.json");
}
