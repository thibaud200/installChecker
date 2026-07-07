namespace InstallChecker.Identity.Erreurs;

/// <summary>L'unique erreur nommée du contrat de C7 (011 § 7, 014 C7) : une question posée sur un acte absent du W désigné.</summary>
public sealed class ActeInexistantDansWException(string message) : Exception(message);
