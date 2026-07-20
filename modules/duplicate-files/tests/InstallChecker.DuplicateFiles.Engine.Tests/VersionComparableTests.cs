using InstallChecker.DuplicateFiles;

namespace InstallChecker.DuplicateFiles.Tests;

public class VersionComparableTests
{
    [Theory]
    [InlineData("1", true, "1")]
    [InlineData("1.2.0", true, "1.2")]
    [InlineData("v1.10", true, "1.10")]
    [InlineData("2026-07-19", false, "2026-07-19")]
    [InlineData("2026.07.19", false, "2026-07-19")]
    public void Une_version_reconnue_possede_une_forme_canonique(
        string brute,
        bool autoriserPrefixeV,
        string canonique)
    {
        Assert.True(VersionComparable.TryLire(brute, autoriserPrefixeV, out var version));
        Assert.Equal(canonique, version.Canonique);
    }

    [Theory]
    [InlineData("1.2.3.4.5")]
    [InlineData("2.0-beta")]
    [InlineData("2026-13-40")]
    [InlineData("version finale")]
    [InlineData("v1.2")]
    public void Une_version_ambigue_est_refusee_sans_prefixe_v(string brute)
    {
        Assert.False(VersionComparable.TryLire(brute, autoriserPrefixeV: false, out _));
    }

    [Fact]
    public void La_comparaison_est_numerique_et_ignore_les_zeros_finaux()
    {
        Assert.True(VersionComparable.TryLire("1.10", false, out var dix));
        Assert.True(VersionComparable.TryLire("1.9", false, out var neuf));
        Assert.True(VersionComparable.TryLire("1.2", false, out var courte));
        Assert.True(VersionComparable.TryLire("1.2.0", false, out var longue));

        Assert.True(dix.CompareTo(neuf) > 0);
        Assert.Equal(0, courte.CompareTo(longue));
        Assert.Equal(courte, longue);
    }

    [Fact]
    public void Deux_dates_sont_comparees_chronologiquement()
    {
        Assert.True(VersionComparable.TryLire("2025-12-01", false, out var ancienne));
        Assert.True(VersionComparable.TryLire("2026-01-15", false, out var recente));

        Assert.True(recente.CompareTo(ancienne) > 0);
    }

    [Fact]
    public void Deux_schemas_differents_ne_sont_jamais_comparables()
    {
        Assert.True(VersionComparable.TryLire("1.2", false, out var numerique));
        Assert.True(VersionComparable.TryLire("2026-07-19", false, out var calendaire));

        Assert.Throws<InvalidOperationException>(() => numerique.CompareTo(calendaire));
    }

    [Theory]
    [InlineData("  Éditeur   Exemple  ", "ÉDITEUR EXEMPLE")]
    [InlineData("Produit", "PRODUIT")]
    public void Le_texte_est_normalise_sans_perdre_sa_ponctuation(string brute, string attendu)
    {
        Assert.Equal(attendu, NormalisationVersionnee.Texte(brute));
    }

    [Theory]
    [InlineData("AMD64", "x64")]
    [InlineData("win32", "x86")]
    [InlineData("aarch64", "arm64")]
    [InlineData("riscv64", "RISCV64")]
    public void Larchitecture_connue_est_canonisee(string brute, string attendu)
    {
        Assert.Equal(attendu, NormalisationVersionnee.Architecture(brute));
    }

    [Theory]
    [InlineData(@"C:\archives\outil-1.0.tar.gz", ".tar.gz")]
    [InlineData(@"C:\archives\outil-1.0.ZIP", ".zip")]
    [InlineData(@"C:\archives\README", "<sans-extension>")]
    public void Le_format_preserve_les_extensions_composees(string chemin, string attendu)
    {
        Assert.Equal(attendu, NormalisationVersionnee.Format(chemin));
    }
}
