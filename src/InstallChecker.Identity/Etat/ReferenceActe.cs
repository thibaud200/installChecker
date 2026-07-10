using InstallChecker.Identity.Hypotheses;

namespace InstallChecker.Identity.Etat;

/// <summary>
/// La référence d'un acte de W (024 § 3) : l'identité de l'acte — le couple (strate, domaine
/// explicitement énuméré et trié), totale par la complétude (014 C5 : « exactement un acte » par
/// domaine-strate). Le plus petit identifiant du domaine demeure la clé de tri (014 § 7.5) et la
/// désignation abrégée de l'audit (024 § 3). Égalité par valeur, séquence du domaine comprise.
/// </summary>
public sealed record ReferenceActe(Strate Strate, IReadOnlyList<long> Domaine)
{
    public long PlusPetitIdentifiantDuDomaine => Domaine[0];

    public bool Equals(ReferenceActe? autre) =>
        autre is not null && Strate == autre.Strate && Domaine.SequenceEqual(autre.Domaine);

    public override int GetHashCode() =>
        HashCode.Combine(Strate, Domaine.Count, Domaine[0], Domaine[^1]);

    public override string ToString() =>
        $"ReferenceActe {{ Strate = {Strate}, Domaine = [{string.Join(", ", Domaine)}] }}";
}
