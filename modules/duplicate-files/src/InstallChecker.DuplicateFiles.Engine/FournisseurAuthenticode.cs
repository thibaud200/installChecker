namespace InstallChecker.DuplicateFiles;

public sealed class FournisseurAuthenticode : IFournisseurDePreuves
{
    public const string Version = "authenticode/v1";

    public ResultatFournisseur Extraire(FichierObserve fichier)
    {
        ArgumentNullException.ThrowIfNull(fichier);
        var sujet = LectureAttributs.Texte(fichier, "authenticode", "subject");
        if (sujet is null)
            return ResultatFournisseur.Vide;

        return new ResultatFournisseur(
            [new PreuveVersionnee(
                fichier.FichierId,
                DimensionPreuveVersionnee.Editeur,
                sujet,
                NormalisationVersionnee.Texte(sujet),
                SourcePreuveVersionnee.Authenticode,
                ForcePreuveVersionnee.Forte,
                "AuthenticodeSubject",
                Version)],
            []);
    }
}
