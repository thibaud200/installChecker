using System.Globalization;
using System.Text.RegularExpressions;

namespace InstallChecker.DuplicateFiles;

public enum SchemaVersionComparable
{
    Numerique,
    Calendaire,
}

public readonly record struct VersionComparable(
    SchemaVersionComparable Schema,
    int Composant1,
    int Composant2,
    int Composant3,
    int Composant4,
    string Canonique) : IComparable<VersionComparable>
{
    private static readonly Regex FormeDate = new(
        @"^\d{4}(?<separateur>[.-])\d{2}\k<separateur>\d{2}$",
        RegexOptions.CultureInvariant);

    private static readonly Regex FormeNumerique = new(
        @"^\d+(?:\.\d+){0,3}$",
        RegexOptions.CultureInvariant);

    public static bool TryLire(
        string valeur,
        bool autoriserPrefixeV,
        out VersionComparable version)
    {
        version = default;
        if (string.IsNullOrWhiteSpace(valeur))
            return false;

        var candidate = valeur.Trim();
        if (FormeDate.IsMatch(candidate))
        {
            if (!DateOnly.TryParseExact(
                    candidate,
                    ["yyyy-MM-dd", "yyyy.MM.dd"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                return false;
            }

            version = new VersionComparable(
                SchemaVersionComparable.Calendaire,
                date.Year,
                date.Month,
                date.Day,
                0,
                date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            return true;
        }

        if (candidate.StartsWith('v') || candidate.StartsWith('V'))
        {
            if (!autoriserPrefixeV)
                return false;
            candidate = candidate[1..];
        }

        if (!FormeNumerique.IsMatch(candidate))
            return false;

        var morceaux = candidate.Split('.');
        var composants = new int[4];
        for (var i = 0; i < morceaux.Length; i++)
        {
            if (!int.TryParse(
                    morceaux[i],
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out composants[i]))
            {
                return false;
            }
        }

        var dernier = composants.Length - 1;
        while (dernier > 0 && composants[dernier] == 0)
            dernier--;

        var canonique = string.Join(
            ".",
            composants.Take(dernier + 1).Select(c => c.ToString(CultureInfo.InvariantCulture)));

        version = new VersionComparable(
            SchemaVersionComparable.Numerique,
            composants[0],
            composants[1],
            composants[2],
            composants[3],
            canonique);
        return true;
    }

    public int CompareTo(VersionComparable other)
    {
        if (Schema != other.Schema)
            throw new InvalidOperationException("deux schémas de version différents ne sont pas comparables");

        var comparaison = Composant1.CompareTo(other.Composant1);
        if (comparaison != 0) return comparaison;
        comparaison = Composant2.CompareTo(other.Composant2);
        if (comparaison != 0) return comparaison;
        comparaison = Composant3.CompareTo(other.Composant3);
        if (comparaison != 0) return comparaison;
        return Composant4.CompareTo(other.Composant4);
    }
}
