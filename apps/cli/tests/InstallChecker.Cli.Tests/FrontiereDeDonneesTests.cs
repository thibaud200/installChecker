using InstallChecker.Identity.Access.Observations;
using InstallChecker.Identity.Observations;
using InstallChecker.Scanner.Observations;
using Microsoft.Data.Sqlite;

namespace InstallChecker.Tests;

/// <summary>
/// La frontière de données entre le producteur d'Ω et le moteur (013 § 2 : « une frontière de
/// données, pas de code — leur seul point de contact est la base d'observations ») : le scan réel
/// écrit, C1 lit, et la fidélité de la projection se vérifie de bout en bout. Ces scénarios
/// exercent les deux systèmes à la fois : ils vivent dans la suite de la CLI, pas dans celle du
/// moteur (016 § 4.2, report 10 — jalon V3-1). Scénarios inchangés, déplacés depuis
/// LecteurDObservationsSqliteTests.
/// </summary>
public class FrontiereDeDonneesTests : IDisposable
{
    private readonly string _root = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "frontiere-donnees-tests-" + Guid.NewGuid())).FullName;

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(_root, recursive: true);
    }

    private string ScannerDossier(string dossier)
    {
        var db = Path.Combine(_root, Guid.NewGuid() + ".db");
        var exitCode = ScanCommand.Run(dossier, db, jsonOutput: false, TextWriter.Null, TextWriter.Null);
        Assert.Equal(0, exitCode);
        return db;
    }

    // --- Fidélité de projection ---

    [Fact]
    public void Projette_fidelement_les_valeurs_absentes_dun_fichier_sans_capacite_reconnue()
    {
        var fichier = Path.Combine(_root, "sans-capacite.txt");
        File.WriteAllText(fichier, "aucune capacité reconnue ici");

        var modele = SourceObservationsSqlite.Ouvrir(ScannerDossier(_root)).ProjeterModele();
        var acte = Assert.Single(modele.Actes);

        Assert.Equal(ValeurObservee.Absente.Instance, acte.Attributs[new Attribut("pe_info", "machine")]);
        Assert.Equal(ValeurObservee.Absente.Instance, acte.Attributs[new Attribut("authenticode", "subject")]);
        Assert.Equal(ValeurObservee.Absente.Instance, acte.Attributs[new Attribut("version_info", "product_name")]);
        Assert.Equal(ValeurObservee.Absente.Instance, acte.Attributs[new Attribut("file_headers", "container")]);
    }

    [Fact]
    public void Projette_fidelement_les_valeurs_presentes_dun_fichier_PE()
    {
        var copie = Path.Combine(_root, "kernel32.dll");
        File.Copy(Path.Combine(Environment.SystemDirectory, "kernel32.dll"), copie);

        var modele = SourceObservationsSqlite.Ouvrir(ScannerDossier(_root)).ProjeterModele();
        var acte = Assert.Single(modele.Actes);

        Assert.Equal(new FileInfo(copie).Length, acte.Taille);
        Assert.Equal(new ValeurObservee.Texte("pe"), acte.Attributs[new Attribut("file_headers", "container")]);
        var magicHex = Assert.IsType<ValeurObservee.Texte>(acte.Attributs[new Attribut("file_headers", "magic_hex")]);
        Assert.StartsWith("4d5a", magicHex.Valeur);
        Assert.IsType<ValeurObservee.Texte>(acte.Attributs[new Attribut("version_info", "company_name")]);
    }

    // --- Contexte : canal séparé (A1) ---

    [Fact]
    public void Le_contexte_porte_le_chemin_et_la_date_hors_du_modele()
    {
        var fichier = Path.Combine(_root, "a.txt");
        File.WriteAllText(fichier, "x");
        var db = ScannerDossier(_root);
        var lecteur = SourceObservationsSqlite.Ouvrir(db);

        var contexte = Assert.Single(lecteur.ProjeterContexte());
        Assert.Equal(fichier, contexte.Chemin);
        Assert.False(string.IsNullOrEmpty(contexte.DateDeScan));

        var acte = Assert.Single(lecteur.ProjeterModele().Actes);
        Assert.Equal(contexte.Identifiant, acte.Identifiant);
    }

    // --- Substituabilité du port (I42) sur scan réel ---

    private static int CompterActes(IObservationsSource source) => source.ProjeterModele().Actes.Count;

    [Fact]
    public void Ladaptateur_memoire_est_substituable_au_lecteur_SQLite()
    {
        var fichier = Path.Combine(_root, "a.txt");
        File.WriteAllText(fichier, "x");
        var sqlite = SourceObservationsSqlite.Ouvrir(ScannerDossier(_root));

        var modele = sqlite.ProjeterModele();
        var contexte = sqlite.ProjeterContexte();
        var memoire = new SourceObservationsEnMemoire(modele, contexte);

        Assert.Equal(CompterActes(sqlite), CompterActes(memoire));
        Assert.Same(modele, memoire.ProjeterModele());
        Assert.Same(contexte, memoire.ProjeterContexte());
    }
}
