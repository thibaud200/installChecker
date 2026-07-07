namespace InstallChecker.Identity.Access.Registre;

/// <summary>
/// Parseur dédié, sans dépendance externe : extrait les sections d'un document du registre
/// (015 § 3) par leur marqueur de titre (« ## » ou « ### »). Ne connaît rien du contenu logique
/// des champs — c'est une opération purement syntaxique, réutilisée à tous les niveaux (fichier
/// de convention, entrée d'historique).
/// </summary>
internal static class SectionParser
{
    public static IReadOnlyList<(string Titre, string Contenu)> ExtraireSections(string markdown, string marqueur = "## ")
    {
        var sections = new List<(string Titre, string Contenu)>();
        string? titreCourant = null;
        var contenuCourant = new List<string>();

        foreach (var ligne in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            if (ligne.StartsWith(marqueur, StringComparison.Ordinal))
            {
                if (titreCourant is not null)
                {
                    sections.Add((titreCourant, string.Join('\n', contenuCourant).Trim()));
                }

                titreCourant = ligne[marqueur.Length..].Trim();
                contenuCourant.Clear();
            }
            else if (titreCourant is not null)
            {
                contenuCourant.Add(ligne);
            }
        }

        if (titreCourant is not null)
        {
            sections.Add((titreCourant, string.Join('\n', contenuCourant).Trim()));
        }

        return sections;
    }
}
