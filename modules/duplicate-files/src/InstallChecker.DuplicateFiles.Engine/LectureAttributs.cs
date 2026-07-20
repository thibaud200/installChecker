using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles;

public static class LectureAttributs
{
    public static string? Texte(FichierObserve fichier, string capacite, string nom)
    {
        ArgumentNullException.ThrowIfNull(fichier);
        return fichier.Attributs.TryGetValue(new Attribut(capacite, nom), out var valeur)
            && valeur is ValeurObservee.Texte texte
            && !string.IsNullOrWhiteSpace(texte.Valeur)
                ? texte.Valeur
                : null;
    }
}
