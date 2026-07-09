using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Observations;

namespace InstallChecker.Identity.Signaux;

/// <summary>
/// C3 — dérivation des signaux (012 § 1.2, 014 C3). Fonction pure : (modèle d'observations,
/// référentiel de conventions) → instances de signaux. Ne consulte jamais Ω ni ℛ directement,
/// uniquement les objets déjà projetés par C1/C2 — et rien au-delà (012 § 3 : C3 ne lit jamais
/// les hypothèses, les actes, W ni τ, qui n'existent pas encore à cette couche).
///
/// C3 applique la famille <b>interprétation</b> — jamais une convention par identifiant (017 § 2,
/// report 1 : « appliquer chaque convention selon sa famille ») : toute convention en vigueur de
/// cette famille fonde ses propres instances, sans changement de moteur (016 § 5.1). L'espèce de
/// signal appliquée est celle que la théorie définit pour la famille à ce jour : le signal
/// relationnel d'identité de contenu sur l'empreinte (007 § 4, la relation ≡ₘ ; 014 § 6 : « deux
/// actes de même empreinte ont des contenus parfaitement égaux » — la seule relation que le
/// contrat de Ω garantit), en régime exact par nature (002 § 5). Une convention d'interprétation
/// dont la transformation sortirait de cette espèce serait une forme que la théorie ne définit
/// pas — révision documentaire d'abord (011 § 10), jamais une latitude du moteur (EXG-13).
/// Les conventions des autres familles n'ont aucun effet ici — C3 n'agit que sur les familles
/// qui la concernent (012 § 1.2, 014 § 3).
///
/// C3 n'a pas d'erreur propre (014 C3 : « refuse : rien ») : ses entrées sont valides par
/// construction — et couvertes par construction depuis le 017 § 5 (I51 étendu). L'absence de toute
/// convention d'interprétation ne produit pas un refus — elle produit simplement l'absence de
/// toute instance (I13 : aucune instance sans convention fondatrice).
/// </summary>
public static class DerivationDesSignaux
{
    private const string TypeContenuIdentique = "contenu-identique";
    private const string AttributEmpreinte = "empreinte";

    public static IReadOnlyList<InstanceDeSignal> Deriver(ModeleObservations modele, Referentiel referentiel)
    {
        var interpretations = referentiel.ConventionsEnVigueur
            .Where(c => c.Famille == Famille.Interpretation)
            .OrderBy(c => c.Identifiant, StringComparer.Ordinal)
            .ThenBy(c => c.Version)
            .ToList();

        if (interpretations.Count == 0) return [];

        var paires = modele.Actes
            .GroupBy(a => a.Empreinte, StringComparer.Ordinal)
            .Where(classe => classe.Count() >= 2)
            .SelectMany(PairesDe)
            .OrderBy(paire => paire.Premier)
            .ThenBy(paire => paire.Second)
            .ToList();

        return interpretations
            .SelectMany(convention =>
            {
                var type = new TypeDeSignal(TypeContenuIdentique, convention.Ref);
                return paires.Select(paire => CreerInstance(type, paire));
            })
            .ToList();
    }

    /// <summary>Toutes les paires non ordonnées d'une classe de contenu, identifiants croissants (007 § 4 : la relation ≡ₘ, 108 paires + 4 triplets mesurés au corpus 1).</summary>
    private static IEnumerable<(long Premier, long Second, string Empreinte)> PairesDe(IGrouping<string, ActeObservation> classe)
    {
        var identifiants = classe.Select(a => a.Identifiant).OrderBy(id => id).ToList();
        for (var i = 0; i < identifiants.Count; i++)
        {
            for (var j = i + 1; j < identifiants.Count; j++)
            {
                yield return (identifiants[i], identifiants[j], classe.Key);
            }
        }
    }

    private static InstanceDeSignal CreerInstance(TypeDeSignal type, (long Premier, long Second, string Empreinte) paire) =>
        new(
            type,
            paire.Empreinte,
            Regime.Exact,
            [
                new ObservationConsommee(paire.Premier, AttributEmpreinte),
                new ObservationConsommee(paire.Second, AttributEmpreinte),
            ]);
}
