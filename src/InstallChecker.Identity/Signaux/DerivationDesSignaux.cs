using InstallChecker.Identity.Conventions;
using InstallChecker.Identity.Observations;

namespace InstallChecker.Identity.Signaux;

/// <summary>
/// C3 — dérivation des signaux (012 § 1.2, 014 C3). Fonction pure : (modèle d'observations,
/// référentiel de conventions) → instances de signaux. Ne consulte jamais Ω ni ℛ directement,
/// uniquement les objets déjà projetés par C1/C2 — et rien au-delà (012 § 3 : C3 ne lit jamais
/// les hypothèses, les actes, W ni τ, qui n'existent pas encore à cette couche).
///
/// Seule convention active à cette étape (013/014 plan É4) : EQ-01, qui fonde le signal
/// relationnel « contenu identique ». Une convention d'une autre famille (élection, p. ex. CE-01)
/// présente dans le référentiel n'a aucun effet ici — C3 n'agit que sur les familles qui la
/// concernent (012 § 1.2).
///
/// C3 n'a pas d'erreur propre (014 C3 : « refuse : rien ») : ses entrées sont valides par
/// construction (I51). L'absence d'EQ-01 dans le référentiel ne produit pas un refus — elle
/// produit simplement l'absence de toute instance (I13 : aucune instance sans convention fondatrice).
/// </summary>
public static class DerivationDesSignaux
{
    private const string IdentifiantEQ01 = "EQ-01";
    private const string TypeContenuIdentique = "contenu-identique";
    private const string AttributEmpreinte = "empreinte";

    public static IReadOnlyList<InstanceDeSignal> Deriver(ModeleObservations modele, Referentiel referentiel)
    {
        var eq01 = referentiel.ConventionsEnVigueur.SingleOrDefault(
            c => c.Identifiant == IdentifiantEQ01 && c.Famille == Famille.Interpretation);

        if (eq01 is null) return [];

        var type = new TypeDeSignal(TypeContenuIdentique, eq01.Ref);

        return modele.Actes
            .GroupBy(a => a.Empreinte, StringComparer.Ordinal)
            .Where(classe => classe.Count() >= 2)
            .SelectMany(classe => PairesDe(classe))
            .Select(paire => CreerInstance(type, paire))
            .OrderBy(instance => instance.Provenance[0].ActeId)
            .ThenBy(instance => instance.Provenance[1].ActeId)
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
