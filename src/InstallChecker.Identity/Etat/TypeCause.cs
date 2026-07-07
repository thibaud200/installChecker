namespace InstallChecker.Identity.Etat;

/// <summary>Les deux seules causes de révision d'un état (006 § 6, Déf. 6) : Ω croît, ou ℛ change de version.</summary>
public enum TypeCause
{
    Omega,
    Registre,
}
