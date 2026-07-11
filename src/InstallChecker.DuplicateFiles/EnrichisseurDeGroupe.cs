using InstallChecker.Identity.Observations;

namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Relit dans Ω (jamais dans W) les attributs bruts nécessaires au classement d'un groupe
/// (conception D4) et la taille de l'acte (plan rev3 § 2.1). Un attribut absent — explicitement ⊥
/// ou simplement hors du dictionnaire — n'est jamais une erreur : c'est une observation légitime,
/// traitée comme « faux » pour ce critère (conception § 6).
/// <para>
/// Invariant (P7) : <b>W et Ω proviennent de la même dérivation</b>. Tout identifiant d'un domaine
/// élu de W existe donc dans Ω ; l'indexation directe <c>actes[id]</c> / <c>contextes[id]</c> est
/// sûre par construction. Cet invariant ne tiendrait plus si un appelant fournissait un W et un Ω
/// d'index différents — ce que le module ne fait jamais.
/// </para>
/// </summary>
public static class EnrichisseurDeGroupe
{
    public static IReadOnlyList<FichierEnrichi> Enrichir(
        IReadOnlyList<long> domaine,
        IReadOnlyDictionary<long, ActeObservation> actes,
        IReadOnlyDictionary<long, ContexteObservation> contextes) =>
        domaine.Select(id =>
        {
            var acte = actes[id];
            var contexte = contextes[id];
            return new FichierEnrichi(
                id,
                contexte.Chemin,
                acte.Taille,
                ValeurPresente(acte, new Attribut("authenticode", "subject")),
                ValeurPresente(acte, new Attribut("pe_info", "machine")),
                ValeurPresente(acte, new Attribut("msi_properties", "product_name")),
                contexte.DateDeScan);
        }).ToList();

    private static bool ValeurPresente(ActeObservation acte, Attribut attribut) =>
        acte.Attributs.TryGetValue(attribut, out var valeur) && valeur is not ValeurObservee.Absente;
}
