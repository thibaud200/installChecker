namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Composant pur qui calcule les cinq agrégats métier (plan rev3 § 2.1) à partir des seuls
/// <b>groupes classés</b>. Il ne lit ni Ω, ni W, ni le moteur : chaque exemplaire porte déjà sa
/// taille (<see cref="FichierEnrichi.Taille"/>) et son rang.
/// <para>
/// Invariant (plan rev3 § 2.1) : le classeur restitue exactement un exemplaire par fichier du
/// groupe ; il ne filtre ni ne supprime aucun membre. Le cardinal du groupe est donc préservé —
/// c'est ce qui garantit qu'« à conserver » (rang 1) et « candidats » (rang ≥ 2) se somment à
/// <c>n</c> par groupe. Les membres d'un groupe étant égaux en octets, leur taille est identique :
/// l'espace récupérable d'un groupe est <c>taille × (n − 1)</c>, la taille étant celle de n'importe
/// quel membre.
/// </para>
/// </summary>
public static class SyntheseDeBibliotheque
{
    public static Synthese Calculer(IReadOnlyList<IReadOnlyList<ExemplaireClasse>> groupesClasses) => new(
        NombreDeGroupes: groupesClasses.Count,
        NombreDeFichiersRedondants: groupesClasses.Sum(g => g.Count - 1),
        EspaceRecuperableOctets: groupesClasses.Sum(g => g[0].Fichier.Taille * (long)(g.Count - 1)),
        NombreDeFichiersAConserver: groupesClasses.Sum(g => g.Count(e => e.Rang == 1)),
        NombreDeCandidatsASuppression: groupesClasses.Sum(g => g.Count(e => e.Rang >= 2)));
}
