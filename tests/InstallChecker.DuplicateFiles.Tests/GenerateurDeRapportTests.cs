using InstallChecker.DuplicateFiles;
using InstallChecker.Identity.Actes;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Etat;
using InstallChecker.Identity.Hypotheses;
using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles.Tests;

public class GenerateurDeRapportTests
{
    private sealed class OmegaDeTest(ModeleObservations modele, IReadOnlyList<ContexteObservation> contexte)
        : IObservationsSource
    {
        public ModeleObservations ProjeterModele() => modele;
        public IReadOnlyList<ContexteObservation> ProjeterContexte() => contexte;
        public IndexOmega ProjeterIdentite() => throw new NotSupportedException("non consommé par GenerateurDeRapport");
    }

    private static readonly IndexEtat IndexDeTest = new(new IndexOmega(1, 0, "empreinte-de-test"), []);

    private static ActeW Election(params long[] domaine) => new(
        TypeActe.Election, Strate.Contenu, domaine, "même contenu", Niveau.Certaine, "motif d'élection",
        Espece: null, Licences: [new ConventionRef("CE-01", 1)], Dependances: null, Dette: null);

    private static ActeW RefusStrate(Strate strate) => new(
        TypeActe.Refus, strate, [1, 2], Contenu: null, Niveau: null, "aucune-convention-strate",
        Espece.Normatif, Licences: null, Dependances: null, Dette: null);

    // Deux actes de taille 200 : l'acte 2 porte une signature (plus riche → rang 1).
    private static OmegaDeTest OmegaDeuxFichiers() => new(
        new ModeleObservations(
        [
            new ActeObservation(1, 200, "A", new Dictionary<Attribut, ValeurObservee>()),
            new ActeObservation(2, 200, "A", new Dictionary<Attribut, ValeurObservee>
            {
                [new Attribut("authenticode", "subject")] = new ValeurObservee.Texte("Contoso"),
            }),
        ]),
        [
            new ContexteObservation(1, @"C:\a\setup.exe", "2026-01-01T00:00:00.0000000Z"),
            new ContexteObservation(2, @"C:\b\setup.exe", "2026-01-01T00:00:00.0000000Z"),
        ]);

    [Fact]
    public void Le_rapport_porte_la_synthese_la_note_de_capacite_et_les_groupes_etiquetes()
    {
        var w = new W(IndexDeTest, [Election(1, 2), RefusStrate(Strate.Variante), RefusStrate(Strate.Version)]);

        var rapport = GenerateurDeRapport.Generer(w, OmegaDeuxFichiers());

        // Synthèse (5 agrégats)
        Assert.Equal(1, rapport.Synthese.NombreDeGroupes);
        Assert.Equal(1, rapport.Synthese.NombreDeFichiersRedondants);
        Assert.Equal(200L, rapport.Synthese.EspaceRecuperableOctets);
        Assert.Equal(1, rapport.Synthese.NombreDeFichiersAConserver);
        Assert.Equal(1, rapport.Synthese.NombreDeCandidatsASuppression);

        // Note de capacité (dérivée des refus des strates supérieures, jamais les refus eux-mêmes)
        Assert.NotNull(rapport.Note);
        Assert.Contains("variante", rapport.Note!.StratesIndisponibles);
        Assert.Contains("version", rapport.Note.StratesIndisponibles);
        Assert.False(string.IsNullOrWhiteSpace(rapport.Note.Message));

        // Groupe : métriques par groupe + exemplaires étiquetés
        var groupe = Assert.Single(rapport.Groupes);
        Assert.Equal([1L, 2L], groupe.Domaine);
        Assert.Contains("CE-01", groupe.MotifCourt);
        Assert.Equal(200L, groupe.TailleUnitaire);
        Assert.Equal(200L, groupe.EspaceRecuperableOctets);
        Assert.Equal(2, groupe.Exemplaires.Count);

        Assert.Equal(2L, groupe.Exemplaires[0].Fichier.ActeId); // le plus riche (signature) classé premier
        Assert.Equal(1, groupe.Exemplaires[0].Rang);
        Assert.Equal("à conserver", groupe.Exemplaires[0].Etiquette);
        Assert.Equal(2, groupe.Exemplaires[1].Rang);
        Assert.Equal("candidat à la suppression", groupe.Exemplaires[1].Etiquette);
    }

    [Fact]
    public void La_synthese_precede_les_groupes_dans_le_rapport()
    {
        var parametres = typeof(RapportDeDoublons).GetConstructors()[0].GetParameters().Select(p => p.Name).ToList();

        Assert.True(parametres.IndexOf("Synthese") >= 0);
        Assert.True(parametres.IndexOf("Synthese") < parametres.IndexOf("Groupes"));
    }

    [Fact]
    public void La_note_de_capacite_disparait_quand_aucune_strate_superieure_ne_refuse()
    {
        var w = new W(IndexDeTest, [Election(1, 2)]);

        var rapport = GenerateurDeRapport.Generer(w, OmegaDeuxFichiers());

        Assert.Null(rapport.Note);
    }

    [Fact]
    public void Aucun_type_du_moteur_ne_fuit_dans_la_sortie_du_rapport()
    {
        var types = TypesAtteignables(typeof(RapportDeDoublons), new HashSet<Type>());

        Assert.DoesNotContain(types, t => t.Namespace?.StartsWith("InstallChecker.Identity") == true);
    }

    private static IReadOnlyList<Type> TypesAtteignables(Type racine, HashSet<Type> vus)
    {
        if (racine.IsGenericParameter || !vus.Add(racine))
            return [];

        var resultat = new List<Type> { racine };
        foreach (var arg in racine.GetGenericArguments())
            resultat.AddRange(TypesAtteignables(arg, vus));

        if (racine.Namespace?.StartsWith("InstallChecker") == true && !racine.IsEnum)
            foreach (var propriete in racine.GetProperties())
                resultat.AddRange(TypesAtteignables(propriete.PropertyType, vus));

        return resultat;
    }
}
