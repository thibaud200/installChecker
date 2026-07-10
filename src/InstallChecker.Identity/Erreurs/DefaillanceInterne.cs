namespace InstallChecker.Identity.Erreurs;

/// <summary>
/// Une défaillance interne du moteur, signalée comme telle (011 § 4 : « elle doit se signaler
/// comme telle et respecter la postcondition "entier ou absent" ») — jamais déguisée en erreur
/// contractuelle (018 § 3). Le cas d'espèce est le refus de C6 : « un ensemble d'actes incohérent —
/// cas qui signale un défaut de C5, jamais une situation d'entrée » (014 C6, report 4). Ce type
/// n'appartient à aucune des deux hiérarchies d'erreurs nommées du contrat (ErreurOmega,
/// ErreurDeRegistre) : la liste des sept erreurs du 017 § 6 reste close, et la CLI ne le retient
/// pas (EstUneErreurContractuelle).
/// </summary>
public sealed class DefaillanceInterneException(string message) : Exception(message);
