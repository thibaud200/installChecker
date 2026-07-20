namespace InstallChecker.DuplicateFiles;

/// <summary>
/// Un fichier d'un groupe de doublons, enrichi des attributs bruts d'Ω nécessaires au classement
/// (conception D4) et de la taille de l'acte (plan rev3 § 2.1, pour la synthèse). DTO de travail
/// interne au module. Les trois indicateurs disent exactement ce qui est observé — la <b>présence</b>
/// d'un attribut, jamais une « complétude » (renommage P5) :
/// <list type="bullet">
///   <item><see cref="SignatureAuthenticodePresente"/> — présence de <c>authenticode.subject</c> ;</item>
///   <item><see cref="EstUnPeLisible"/> — présence de <c>pe_info.machine</c> ;</item>
///   <item><see cref="PresenceMetadonneesMsi"/> — présence de <c>msi_properties.product_name</c>.</item>
/// </list>
/// <see cref="VolumeId"/>/<see cref="VolumeLabel"/> — le volume observé au scan (spec multi-disque
/// D5), <c>null</c> sur une base v1 : une absence légitime, jamais une erreur.
/// </summary>
public sealed record FichierEnrichi(
    long ActeId,
    string Chemin,
    long Taille,
    bool SignatureAuthenticodePresente,
    bool EstUnPeLisible,
    bool PresenceMetadonneesMsi,
    string DateDObservation,
    string? VolumeId = null,
    string? VolumeLabel = null);
