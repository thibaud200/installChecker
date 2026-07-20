namespace InstallChecker.DuplicateFiles;

public sealed class FournisseurPe : IFournisseurDePreuves
{
    public const string Version = "pe/v1";

    public ResultatFournisseur Extraire(FichierObserve fichier)
    {
        ArgumentNullException.ThrowIfNull(fichier);
        var machine = LectureAttributs.Texte(fichier, "pe_info", "machine");
        if (machine is null)
            return ResultatFournisseur.Vide;

        return new ResultatFournisseur(
            [new PreuveVersionnee(
                fichier.FichierId,
                DimensionPreuveVersionnee.Architecture,
                machine,
                NormalisationVersionnee.Architecture(machine)!,
                SourcePreuveVersionnee.Pe,
                ForcePreuveVersionnee.Forte,
                "PeMachine",
                Version)],
            []);
    }
}
