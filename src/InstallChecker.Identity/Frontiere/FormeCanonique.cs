using System.Text;
using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Etat;

namespace InstallChecker.Identity.Frontiere;

/// <summary>
/// La forme canonique matérielle des sorties du moteur (report 3) : « le moteur définit une forme
/// canonique de sa sortie (ordre normalisé, encodage fixé) » (EXG-18) — celle que consigne
/// <c>docs/conformite/forme-canonique-materielle.md</c>, sur laquelle porte l'identité bit à bit.
/// JSON, UTF-8 sans BOM, non-ASCII en clair, LF exclusif avec LF final unique, indentation de
/// 2 espaces, échappement minimal (les seuls caractères que JSON impose), champs absents omis,
/// ordres du 014 § 7 (raffiné 024/025/026). L'écriture est manuelle : aucun sérialiseur générique
/// ne garantit ces octets sur toute plateforme (fins de ligne, échappement) — la spécification
/// est le code, octet par octet, et l'oracle indépendant la rejoue dans un autre langage.
/// </summary>
public static class FormeCanonique
{
    public static string Emettre(W w)
    {
        var e = new Emetteur();
        e.OuvreObjet();
        e.Cle("index");
        EmettreIndex(e, w.Index);
        e.Cle("actes");
        e.OuvreTableau(w.Actes.Count == 0);
        foreach (var acte in w.Actes)
        {
            e.Element();
            e.OuvreObjet();
            e.Champ("type", acte.Type == TypeActe.Election ? "élection" : "refus");
            e.Champ("strate", NomDeStrate(acte.Strate));
            e.Cle("domaine");
            e.TableauDEntiers(acte.Domaine);
            if (acte.Contenu is not null) e.Champ("contenu", acte.Contenu);
            if (acte.Niveau is not null) e.Champ("niveau", acte.Niveau.ToString()!.ToLowerInvariant());
            if (acte.Motif is not null) e.Champ("motif", acte.Motif);
            if (acte.Espece is not null) e.Champ("espece", acte.Espece.ToString()!.ToLowerInvariant());
            if (acte.Licences is not null) { e.Cle("licences"); TableauDeCouples(e, acte.Licences); }
            if (acte.Dependances is not null) { e.Cle("dependances"); TableauDeCouples(e, acte.Dependances); }
            if (acte.Dette is not null) { e.Cle("dette"); TableauDeCouples(e, acte.Dette); }
            e.FermeObjet();
        }
        e.FermeTableau();
        e.FermeObjet();
        return e.Document();
    }

    public static string Emettre(Transition tau)
    {
        var e = new Emetteur();
        e.OuvreObjet();
        e.Cle("index-avant");
        EmettreIndex(e, tau.IndexAvant);
        e.Cle("index-apres");
        EmettreIndex(e, tau.IndexApres);

        e.Cle("cause");
        e.OuvreObjet(tau.Cause.Omega is null && tau.Cause.Registre is null);
        if (tau.Cause.Omega is not null)
        {
            e.Cle("omega");
            e.OuvreObjet();
            e.Cle("ajoutes");
            e.TableauDEntiers(tau.Cause.Omega.Ajoutes);
            e.Cle("retires");
            e.TableauDEntiers(tau.Cause.Omega.Retires);
            e.FermeObjet();
        }
        if (tau.Cause.Registre is not null)
        {
            e.Cle("registre");
            e.OuvreObjet();
            e.Cle("adoptes");
            TableauDeCouples(e, tau.Cause.Registre.Adoptes);
            e.Cle("retires");
            TableauDeCouples(e, tau.Cause.Registre.Retires);
            e.FermeObjet();
        }
        e.FermeObjet();

        e.Cle("correspondance");
        e.OuvreObjet();
        e.Cle("conserves");
        TableauDeReferences(e, tau.Correspondance.Conserves);
        e.Cle("abandonnes");
        TableauDeReferences(e, tau.Correspondance.Abandonnes);
        e.Cle("nouveaux");
        TableauDeReferences(e, tau.Correspondance.Nouveaux);
        e.Cle("continuites");
        e.OuvreTableau(tau.Correspondance.Continuites.Count == 0);
        foreach (var (avant, apres) in tau.Correspondance.Continuites)
        {
            e.Element();
            e.OuvreObjet();
            e.Cle("avant");
            Reference(e, avant);
            e.Cle("apres");
            Reference(e, apres);
            e.FermeObjet();
        }
        e.FermeTableau();
        e.FermeObjet();
        e.FermeObjet();
        return e.Document();
    }

    private static void EmettreIndex(Emetteur e, IndexEtat index)
    {
        e.OuvreObjet();
        e.Cle("omega");
        e.OuvreObjet();
        e.Champ("version", index.Omega.Version);
        e.Champ("nombreActes", index.Omega.NombreActes);
        e.Champ("empreinteEtat", index.Omega.EmpreinteEtat);
        e.FermeObjet();
        e.Cle("registre");
        TableauDeCouples(e, index.Registre);
        e.FermeObjet();
    }

    private static void TableauDeCouples(Emetteur e, IReadOnlyList<ConventionRef> couples)
    {
        e.OuvreTableau(couples.Count == 0);
        foreach (var couple in couples)
        {
            e.Element();
            e.OuvreObjet();
            e.Champ("identifiant", couple.Identifiant);
            e.Champ("version", couple.Version);
            e.FermeObjet();
        }
        e.FermeTableau();
    }

    private static void TableauDeReferences(Emetteur e, IReadOnlyList<ReferenceActe> references)
    {
        e.OuvreTableau(references.Count == 0);
        foreach (var reference in references)
        {
            e.Element();
            Reference(e, reference);
        }
        e.FermeTableau();
    }

    private static void Reference(Emetteur e, ReferenceActe reference)
    {
        e.OuvreObjet();
        e.Champ("strate", NomDeStrate(reference.Strate));
        e.Cle("domaine");
        e.TableauDEntiers(reference.Domaine);
        e.FermeObjet();
    }

    private static string NomDeStrate(Hypotheses.Strate? strate) => strate!.ToString()!.ToLowerInvariant();

    /// <summary>
    /// L'écriture octet par octet de la disposition consignée : indentation de 2 espaces, LF
    /// exclusif, « : » suivi d'un espace, conteneurs vides sur place, LF final unique.
    /// </summary>
    private sealed class Emetteur
    {
        private readonly StringBuilder texte = new();
        private int profondeur;
        private bool premierMembre = true;
        private bool conteneurVideOuvert;

        public void OuvreObjet(bool vide = false) => Ouvre('{', '}', vide);

        public void FermeObjet() => Ferme('}');

        public void OuvreTableau(bool vide = false) => Ouvre('[', ']', vide);

        public void FermeTableau() => Ferme(']');

        public void Cle(string nom)
        {
            Separateur();
            Chaine(nom);
            texte.Append(": ");
            premierMembre = true; // la valeur qui suit n'est pas précédée d'une virgule
        }

        public void Element() => Separateur();

        public void Champ(string nom, string valeur)
        {
            Cle(nom);
            Chaine(valeur);
            premierMembre = false;
        }

        public void Champ(string nom, long valeur)
        {
            Cle(nom);
            texte.Append(valeur.ToString(System.Globalization.CultureInfo.InvariantCulture));
            premierMembre = false;
        }

        public void TableauDEntiers(IReadOnlyList<long> valeurs)
        {
            OuvreTableau(valeurs.Count == 0);
            foreach (var valeur in valeurs)
            {
                Separateur();
                texte.Append(valeur.ToString(System.Globalization.CultureInfo.InvariantCulture));
                premierMembre = false;
            }
            FermeTableau();
        }

        public string Document() => texte.Append('\n').ToString();

        private void Ouvre(char ouvrante, char fermante, bool vide)
        {
            Separateur();
            texte.Append(ouvrante);
            if (vide)
            {
                // « {} » et « [] » sur place (§ 1 de la consignation) — le Ferme apparié,
                // toujours immédiat, consommera le drapeau sans rien émettre.
                texte.Append(fermante);
                premierMembre = false;
                conteneurVideOuvert = true;
                return;
            }

            profondeur++;
            premierMembre = true;
        }

        private void Ferme(char fermante)
        {
            if (conteneurVideOuvert)
            {
                conteneurVideOuvert = false;
                return;
            }

            profondeur--;
            texte.Append('\n').Append(' ', profondeur * 2).Append(fermante);
            premierMembre = false;
        }

        private void Separateur()
        {
            if (!premierMembre)
            {
                texte.Append(',');
            }

            if (texte.Length > 0 && texte[^1] != ' ')
            {
                texte.Append('\n').Append(' ', profondeur * 2);
            }

            premierMembre = true;
        }

        /// <summary>Échappement minimal du § 1 de la consignation : «"», «\» et U+0000–U+001F seulement — le non-ASCII passe en clair.</summary>
        private void Chaine(string valeur)
        {
            texte.Append('"');
            foreach (var c in valeur)
            {
                switch (c)
                {
                    case '"': texte.Append("\\\""); break;
                    case '\\': texte.Append("\\\\"); break;
                    case '\b': texte.Append("\\b"); break;
                    case '\f': texte.Append("\\f"); break;
                    case '\n': texte.Append("\\n"); break;
                    case '\r': texte.Append("\\r"); break;
                    case '\t': texte.Append("\\t"); break;
                    default:
                        if (c < ' ')
                        {
                            texte.Append("\\u").Append(((int)c).ToString("x4", System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            texte.Append(c);
                        }

                        break;
                }
            }

            texte.Append('"');
        }
    }
}
