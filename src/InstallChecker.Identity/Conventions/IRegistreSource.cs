namespace InstallChecker.Identity.Conventions;

/// <summary>
/// Le port « source de registre » consommé par C2 (013 § 1.1). Le moteur définit ce contrat ;
/// un adaptateur de InstallChecker.Identity.Access l'implémente pour un format physique donné.
/// </summary>
public interface IRegistreSource
{
    Referentiel Projeter();
}
