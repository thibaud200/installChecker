using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace InstallChecker.DuplicateFiles;

public sealed class FournisseurAppx : IFournisseurDePreuves
{
    public const string Version = "appx/v1";

    private static readonly HashSet<string> ExtensionsNatives = new(
        [".appx", ".msix", ".appxbundle", ".msixbundle"],
        StringComparer.Ordinal);

    public ResultatFournisseur Extraire(FichierObserve fichier)
    {
        ArgumentNullException.ThrowIfNull(fichier);

        var name = LectureAttributs.Texte(fichier, "appx_manifest", "name");
        var publisher = LectureAttributs.Texte(fichier, "appx_manifest", "publisher");
        var versionBrute = LectureAttributs.Texte(fichier, "appx_manifest", "version");
        var architecture = LectureAttributs.Texte(fichier, "appx_manifest", "processor_architecture");
        if (name is null && publisher is null && versionBrute is null && architecture is null)
            return ResultatFournisseur.Vide;

        var preuves = new List<PreuveVersionnee>();
        var diagnostics = new List<DiagnosticVersionne>();
        var extension = NormalisationVersionnee.Format(fichier.Chemin);
        var format = ExtensionsNatives.Contains(extension) ? extension : "<appx-package>";
        Ajouter(preuves, fichier, DimensionPreuveVersionnee.Format, format, format, "AppxFormat");

        if (name is not null)
            AjouterTexte(preuves, fichier, DimensionPreuveVersionnee.LibelleFamille, name, "AppxName");
        if (publisher is not null)
            AjouterTexte(preuves, fichier, DimensionPreuveVersionnee.Editeur, publisher, "AppxPublisher");
        if (name is not null && publisher is not null)
        {
            Ajouter(
                preuves,
                fichier,
                DimensionPreuveVersionnee.CleFamille,
                $"{name} | {publisher}",
                CleFamille(name, publisher),
                "AppxNamePublisher");
        }

        if (architecture is not null)
            Ajouter(preuves, fichier, DimensionPreuveVersionnee.Architecture, architecture, NormalisationVersionnee.Architecture(architecture)!, "AppxProcessorArchitecture");
        if (versionBrute is not null)
            AjouterVersion(preuves, diagnostics, fichier, versionBrute);

        return new ResultatFournisseur(preuves, diagnostics);
    }

    private static string CleFamille(string name, string publisher)
    {
        using var flux = new MemoryStream();
        EcrireChamp(flux, NormalisationVersionnee.Texte(name));
        EcrireChamp(flux, NormalisationVersionnee.Texte(publisher));
        return $"appx-family:{Convert.ToHexString(SHA256.HashData(flux.ToArray())).ToLowerInvariant()}";
    }

    private static void EcrireChamp(Stream flux, string valeur)
    {
        var octets = Encoding.UTF8.GetBytes(valeur);
        Span<byte> longueur = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(longueur, checked((uint)octets.Length));
        flux.Write(longueur);
        flux.Write(octets);
    }

    private static void AjouterTexte(
        ICollection<PreuveVersionnee> preuves,
        FichierObserve fichier,
        DimensionPreuveVersionnee dimension,
        string brute,
        string regle) =>
        Ajouter(preuves, fichier, dimension, brute, NormalisationVersionnee.Texte(brute), regle);

    private static void AjouterVersion(
        ICollection<PreuveVersionnee> preuves,
        ICollection<DiagnosticVersionne> diagnostics,
        FichierObserve fichier,
        string brute)
    {
        var comparable = VersionComparable.TryLire(brute, autoriserPrefixeV: false, out var version);
        Ajouter(
            preuves,
            fichier,
            DimensionPreuveVersionnee.Version,
            brute,
            comparable ? version.Canonique : NormalisationVersionnee.Texte(brute),
            "AppxVersion");
        if (!comparable)
        {
            diagnostics.Add(new DiagnosticVersionne(
                fichier.FichierId,
                CodeDiagnosticVersionne.VersionNonComparable,
                SourcePreuveVersionnee.Appx,
                "AppxVersion non comparable en F1"));
        }
    }

    private static void Ajouter(
        ICollection<PreuveVersionnee> preuves,
        FichierObserve fichier,
        DimensionPreuveVersionnee dimension,
        string brute,
        string normalisee,
        string regle) =>
        preuves.Add(new PreuveVersionnee(
            fichier.FichierId,
            dimension,
            brute,
            normalisee,
            SourcePreuveVersionnee.Appx,
            ForcePreuveVersionnee.Forte,
            regle,
            Version));
}
