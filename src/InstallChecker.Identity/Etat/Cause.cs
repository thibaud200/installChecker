using InstallChecker.Identity.Conventions;

namespace InstallChecker.Identity.Etat;

/// <summary>
/// La cause d'une transition (026 § 3) : la suite des volets du changement d'index — zéro, un ou
/// deux, dans l'ordre Ω puis ℛ ; un volet par membre dont les deux index diffèrent, aucun pour un
/// membre inchangé. Entre deux index égaux la cause est vide : τ est une comparaison (EXG-30),
/// jamais une révision (006 Déf. 6). Dérivée par C6 des entrées de l'invocation — plus jamais
/// fournie par l'appelant (T1 close, 026 § 1).
/// </summary>
public sealed record Cause(VoletOmega? Omega, VoletRegistre? Registre);

/// <summary>Le volet Ω (026 Déf. 1) : le delta des deux énumérations d'identifiants — ajoutés et retirés (EXG-30 : « deux index quelconques », aucune inclusion imposée).</summary>
public sealed record VoletOmega(IReadOnlyList<long> Ajoutes, IReadOnlyList<long> Retires);

/// <summary>Le volet ℛ (026 Déf. 1) : le delta des couples (identifiant, version) entre les deux membres ℛ des index — la date et la justification restent re-dérivables de l'état ℛ désigné (régime I23, 026 § 2).</summary>
public sealed record VoletRegistre(IReadOnlyList<ConventionRef> Adoptes, IReadOnlyList<ConventionRef> Retires);
