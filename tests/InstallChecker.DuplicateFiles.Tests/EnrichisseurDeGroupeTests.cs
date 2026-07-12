using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class EnrichisseurDeGroupeTests
{
    private static ActeObservation Acte(long id, IReadOnlyDictionary<Attribut, ValeurObservee> attributs) =>
        new(id, Taille: 100, Empreinte: "A", attributs);

    [Fact]
    public void Les_attributs_bruts_presents_sont_lus_comme_vrais_et_la_taille_est_reprise()
    {
        var actes = new Dictionary<long, ActeObservation>
        {
            [1] = Acte(1, new Dictionary<Attribut, ValeurObservee>
            {
                [new Attribut("authenticode", "subject")] = new ValeurObservee.Texte("Contoso"),
                [new Attribut("pe_info", "machine")] = new ValeurObservee.Texte("x64"),
                [new Attribut("msi_properties", "product_name")] = new ValeurObservee.Texte("Contoso Setup"),
            }),
        };
        var contextes = new Dictionary<long, ContexteObservation>
        {
            [1] = new(1, @"C:\installers\setup.exe", "2026-07-01T00:00:00.0000000Z"),
        };

        var fichiers = EnrichisseurDeGroupe.Enrichir([1], actes, contextes);

        Assert.Equal(@"C:\installers\setup.exe", fichiers[0].Chemin);
        Assert.Equal(100L, fichiers[0].Taille);
        Assert.True(fichiers[0].SignatureAuthenticodePresente);
        Assert.True(fichiers[0].EstUnPeLisible);
        Assert.True(fichiers[0].PresenceMetadonneesMsi);
        Assert.Equal("2026-07-01T00:00:00.0000000Z", fichiers[0].DateDObservation);
    }

    [Fact]
    public void Un_attribut_absent_ou_manquant_du_dictionnaire_nest_pas_une_erreur()
    {
        // conception § 6 : ⊥ (absence) est une observation légitime, jamais une erreur — le
        // dictionnaire peut même ne pas contenir la clé du tout.
        var actes = new Dictionary<long, ActeObservation>
        {
            [1] = Acte(1, new Dictionary<Attribut, ValeurObservee>
            {
                [new Attribut("authenticode", "subject")] = ValeurObservee.Absente.Instance,
            }),
        };
        var contextes = new Dictionary<long, ContexteObservation> { [1] = new(1, @"C:\installers\a.exe", "2026-07-01T00:00:00Z") };

        var fichiers = EnrichisseurDeGroupe.Enrichir([1], actes, contextes);

        Assert.False(fichiers[0].SignatureAuthenticodePresente);
        Assert.False(fichiers[0].EstUnPeLisible);
        Assert.False(fichiers[0].PresenceMetadonneesMsi);
    }
}
