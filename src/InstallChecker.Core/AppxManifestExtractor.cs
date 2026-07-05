using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;

namespace InstallChecker;

/// <summary>
/// Capacité autonome : observe les attributs bruts de l'élément &lt;Identity&gt; du fichier
/// AppxManifest.xml présent à la racine d'une archive ZIP lisible. Aucune validation de schéma,
/// aucune conclusion « c'est un MSIX » — un ZIP sans manifeste reste simplement une observation vide.
/// </summary>
public static class AppxManifestExtractor
{
    /// <summary>Toutes les propriétés null = pas d'archive ZIP lisible avec un AppxManifest.xml exploitable.</summary>
    public sealed record AppxManifest(string? Name, string? Publisher, string? Version, string? ProcessorArchitecture)
    {
        public static readonly AppxManifest None = new(null, null, null, null);
    }

    public static AppxManifest Read(string path)
    {
        try
        {
            using var zip = ZipFile.OpenRead(path);
            var entry = zip.GetEntry("AppxManifest.xml"); // nom exact, à la racine — c'est la convention du format
            if (entry is null)
                return AppxManifest.None;

            using var stream = entry.Open();
            var identity = XDocument.Load(stream).Root?.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Identity"); // insensible au namespace (il varie selon les versions du format)
            if (identity is null)
                return AppxManifest.None;

            return new AppxManifest(
                Name: identity.Attribute("Name")?.Value,
                Publisher: identity.Attribute("Publisher")?.Value,
                Version: identity.Attribute("Version")?.Value,
                ProcessorArchitecture: identity.Attribute("ProcessorArchitecture")?.Value);
        }
        catch (Exception ex) when (ex is InvalidDataException or XmlException)
        {
            return AppxManifest.None; // pas un ZIP, ou XML illisible : observation vide, pas une erreur
        }
    }
}
