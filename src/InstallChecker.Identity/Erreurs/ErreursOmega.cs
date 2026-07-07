namespace InstallChecker.Identity.Erreurs;

/// <summary>Les trois erreurs nommées du contrat public de C1 (011 § 5, 014 C1).</summary>
public abstract class ErreurOmega(string message) : Exception(message);

public sealed class OmegaAbsentException(string message) : ErreurOmega(message);

public sealed class OmegaInvalideException(string message) : ErreurOmega(message);

public sealed class OmegaIncompatibleException(string message) : ErreurOmega(message);
