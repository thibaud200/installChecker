using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.DuplicateFiles.Tests;

public class ExtracteurDeGroupesTests
{
    private static readonly IndexEtat IndexDeTest = new(new IndexOmega(1, 0, "empreinte-de-test"), []);

    private static ActeW Election(Strate strate, Niveau niveau, string? licence, params long[] domaine) => new(
        TypeActe.Election, strate, domaine, "même contenu", niveau, "motif de test", Espece: null,
        Licences: licence is null ? null : [new ConventionRef(licence, 1)], Dependances: null, Dette: null);

    private static ActeW Refus(Strate strate, params long[] domaine) => new(
        TypeActe.Refus, strate, domaine, Contenu: null, Niveau: null, "motif de refus", Espece.Normatif,
        Licences: null, Dependances: null, Dette: null);

    [Fact]
    public void Une_election_certaine_licenciee_par_CE01_en_strate_contenu_devient_un_groupe()
    {
        var w = new W(IndexDeTest, [Election(Strate.Contenu, Niveau.Certaine, "CE-01", 1, 2)]);

        var (groupes, refusStratesSuperieures) = ExtracteurDeGroupes.Extraire(w);

        Assert.Single(groupes);
        Assert.Equal([1L, 2L], groupes[0].Domaine);
        Assert.Empty(refusStratesSuperieures);
    }

    [Fact]
    public void Une_election_de_niveau_non_certaine_nest_pas_un_groupe()
    {
        var w = new W(IndexDeTest, [Election(Strate.Contenu, Niveau.Probable, "CE-01", 1, 2)]);

        var (groupes, _) = ExtracteurDeGroupes.Extraire(w);

        Assert.Empty(groupes);
    }

    [Fact]
    public void Une_election_hors_strate_contenu_nest_pas_un_groupe()
    {
        var w = new W(IndexDeTest, [Election(Strate.Variante, Niveau.Certaine, "CE-01", 1, 2)]);

        var (groupes, _) = ExtracteurDeGroupes.Extraire(w);

        Assert.Empty(groupes);
    }

    [Fact]
    public void Les_refus_globaux_des_strates_superieures_sont_collectes_pour_la_note_de_capacite()
    {
        // Plan rev3 § 3 (P2) : les refus globaux des strates supérieures (variante/version/identité/
        // famille) sont collectés à part — jamais restitués tels quels — pour alimenter la note de
        // capacité. Le second flux du composant contient exactement ces refus.
        var w = new W(IndexDeTest,
        [
            Election(Strate.Contenu, Niveau.Certaine, "CE-01", 1, 2),
            Refus(Strate.Variante, 1, 2),
            Refus(Strate.Version, 1, 2),
        ]);

        var (groupes, refusStratesSuperieures) = ExtracteurDeGroupes.Extraire(w);

        Assert.Single(groupes);
        Assert.Equal(2, refusStratesSuperieures.Count);
        Assert.Contains(refusStratesSuperieures, r => r.Strate == Strate.Variante);
        Assert.Contains(refusStratesSuperieures, r => r.Strate == Strate.Version);
    }
}
