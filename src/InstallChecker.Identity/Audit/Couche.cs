namespace InstallChecker.Identity.Audit;

/// <summary>
/// La couche productrice d'un maillon de chaîne d'audit (012 § 6). C2 n'y figure jamais : les
/// conventions qu'il projette sont citées comme données par les maillons qui les appliquent, jamais
/// comme une étape de la chaîne (012 § 1.1 : les conventions ne sont pas une couche).
/// </summary>
public enum Couche
{
    C1,
    C3,
    C4,
    C5,
    C6,
}
