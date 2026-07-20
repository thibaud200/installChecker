namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Note de capacité (plan rev3 § 2.2) : dérivée des refus globaux des strates supérieures, elle
/// énonce honnêtement une capacité non encore outillée — jamais les refus du moteur eux-mêmes.
/// Elle porte la liste des strates actuellement indisponibles et une phrase explicative. DTO du
/// module — aucun type moteur. S'auto-résorbe (absente du rapport) dès que ℛ adopte une convention
/// pour ces strates.
/// </summary>
public sealed record NoteDeCapacite(IReadOnlyList<string> StratesIndisponibles, string Message);
