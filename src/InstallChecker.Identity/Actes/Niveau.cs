namespace InstallChecker.Identity.Actes;

/// <summary>Le niveau de certitude assigné par un acte d'élection (000 § 5.1). « Impossible » ne s'élit jamais (014 § 7.3).</summary>
public enum Niveau
{
    Possible,
    Probable,
    Certaine,
}
