namespace InstallChecker.Identity.Erreurs;

/// <summary>Les trois erreurs nommées du contrat public de C2 (011 § 5, 014 C2).</summary>
public abstract class ErreurDeRegistre(string message) : Exception(message);

public sealed class RegistreAbsentException(string message) : ErreurDeRegistre(message);

public sealed class RegistreMalformeException(string message) : ErreurDeRegistre(message);

public sealed class RegistreIncoherentException(string message) : ErreurDeRegistre(message);
