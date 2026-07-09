using System.Collections.Frozen;

namespace InstallChecker.Identity.Conventions;

/// <summary>
/// La déclaration de couverture de la présente version du moteur (017 §§ 2–3) : l'énoncé,
/// par version de moteur, de l'ensemble des familles couvertes. La déclaration est
/// constitutive — déclarer, c'est couvrir, et couvrir oblige (EXG-13) : une famille non
/// déclarée est non couverte par définition, quel que soit ce que le code saurait faire (I62).
/// Elle est une propriété de la version du moteur — jamais du registre (015 § 1, I56),
/// jamais une option d'exécution (011 § 2.2, EXG-02) — et précède toute invocation,
/// stable sous l'index (017 § 3, I63) : d'où sa forme statique et figée, sans aucun
/// point d'injection.
/// </summary>
public static class DeclarationDeCouverture
{
    /// <summary>
    /// Les familles couvertes par la présente version : interprétation et élection —
    /// la couverture minimale de tout moteur conforme (017 § 10), et rien de plus :
    /// les six autres familles, théorisées mais non déclarées, produiront « registre
    /// non couvert » lorsque la quatrième vérification sera câblée (jalon V2-2).
    /// </summary>
    public static FrozenSet<Famille> FamillesCouvertes { get; } =
        new[] { Famille.Interpretation, Famille.Election }.ToFrozenSet();

    /// <summary>Vrai si la famille appartient à la couverture déclarée (017 § 2, Définition 1).</summary>
    public static bool Couvre(Famille famille) => FamillesCouvertes.Contains(famille);
}
