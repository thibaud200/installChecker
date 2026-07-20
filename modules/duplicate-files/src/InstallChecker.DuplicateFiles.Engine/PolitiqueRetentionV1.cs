using System.Text.RegularExpressions;
using System.Globalization;

namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Politique de rétention v1 (conception D4/D6) — implémentation littérale de
/// modules/duplicate-files/registre-metier/politique-retention/v1.md. Ordre des critères, du plus
/// au moins déterminant : richesse des observations, qualité du nom de fichier, ancienneté
/// (le plus récent préféré), emplacement (ordre alphabétique, aucune notion de dossier canonique
/// en v1). L'identifiant d'acte est un cinquième départage strictement mécanique, garantissant un
/// ordre total — il ne reflète aucun jugement métier. Une révision de cet ordre exige d'abord une
/// nouvelle version documentée (vN.md), jamais un patch silencieux de ce fichier (D6).
/// </summary>
public static class PolitiqueRetentionV1
{
    private static readonly Regex NomDeCopie =
        new(@"(\(\d+\)|-\s*copie|-\s*copy)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<ExemplaireClasse> Classer(IReadOnlyList<FichierEnrichi> fichiers) =>
        fichiers
            .OrderByDescending(RichesseDesObservations)
            .ThenBy(EstNomDeCopie)
            .ThenByDescending(f => f.DateDObservation, StringComparer.Ordinal)
            .ThenBy(f => f.Chemin, StringComparer.Ordinal)
            .ThenBy(f => f.ActeId)
            .Select((f, index) => new ExemplaireClasse(f, index + 1, Motif(f)))
            .ToList();

    private static int RichesseDesObservations(FichierEnrichi f) =>
        (f.SignatureAuthenticodePresente ? 1 : 0)
        + (f.EstUnPeLisible ? 1 : 0)
        + (f.PresenceMetadonneesMsi ? 1 : 0);

    private static bool EstNomDeCopie(FichierEnrichi f) => NomDeCopie.IsMatch(Path.GetFileNameWithoutExtension(f.Chemin));

    public static IReadOnlyList<CritereClassement> Expliquer(FichierEnrichi f) =>
    [
        new(CritereRetention.RichesseObservations, 1, $"{RichesseDesObservations(f)}/3"),
        new(CritereRetention.NomDeCopie, 2, EstNomDeCopie(f).ToString()),
        new(CritereRetention.DateObservation, 3, f.DateDObservation),
        new(CritereRetention.Chemin, 4, f.Chemin),
        new(CritereRetention.ActeIdDepartage, 5, f.ActeId.ToString(CultureInfo.InvariantCulture)),
    ];

    private static string Motif(FichierEnrichi f) =>
        $"richesse={RichesseDesObservations(f)}/3, nomDeCopie={EstNomDeCopie(f)}, " +
        $"dateObservation={f.DateDObservation}, chemin={f.Chemin}";
}
