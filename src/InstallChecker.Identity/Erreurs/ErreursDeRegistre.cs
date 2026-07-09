namespace InstallChecker.Identity.Erreurs;

/// <summary>Les quatre erreurs nommées du contrat public de C2 (011 § 5 ; 017 §§ 5–6 : quatrième cas « refuse »).</summary>
public abstract class ErreurDeRegistre(string message) : Exception(message);

public sealed class RegistreAbsentException(string message) : ErreurDeRegistre(message);

public sealed class RegistreMalformeException(string message) : ErreurDeRegistre(message);

public sealed class RegistreIncoherentException(string message) : ErreurDeRegistre(message);

/// <summary>
/// La septième erreur du contrat (017 § 6) : au moins une convention en vigueur de ℛ appartient
/// à une famille hors de la couverture déclarée du moteur invoqué — condition lue après bonne
/// formation et cohérence (017 § 8). Produite exclusivement par C2 (017 § 5).
/// </summary>
public sealed class RegistreNonCouvertException(string message) : ErreurDeRegistre(message);
