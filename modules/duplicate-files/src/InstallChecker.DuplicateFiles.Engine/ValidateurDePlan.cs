namespace InstallChecker.DuplicateFiles;

public static class ValidateurDePlan
{
    public static RapportDeVerificationPlan Verifier(
        PlanDeSuppression plan,
        IObservateurDeFichier observateur,
        Func<string, bool> cheminProtege)
    {
        ValidateurStructurePlan.Valider(plan);
        ArgumentNullException.ThrowIfNull(observateur);
        ArgumentNullException.ThrowIfNull(cheminProtege);

        var propositionsParGroupe = plan.Propositions
            .GroupBy(p => p.GroupeId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
        var groupes = new List<VerificationGroupePlan>();
        var journal = new List<EntreeJournalVerificationPlan>();
        var sequence = 1;

        foreach (var garantie in plan.GarantiesParGroupe)
        {
            var fichiers = new List<VerificationFichierPlan>();
            var temoin = VerifierFichier(
                garantie.GroupeId,
                garantie.ContenuSha256,
                garantie.TemoinConservation.FichierId,
                garantie.TemoinConservation.Chemin,
                RoleFichierPlan.TemoinConservation,
                observateur,
                cheminProtege);
            fichiers.Add(temoin);
            journal.Add(new EntreeJournalVerificationPlan(
                sequence++,
                garantie.GroupeId,
                temoin.FichierId,
                EtapeJournalVerificationPlan.VerifierTemoin,
                temoin.Etat));

            foreach (var proposition in propositionsParGroupe[garantie.GroupeId])
            {
                var candidat = VerifierFichier(
                    garantie.GroupeId,
                    garantie.ContenuSha256,
                    proposition.FichierId,
                    proposition.Chemin,
                    RoleFichierPlan.Candidat,
                    observateur,
                    cheminProtege);
                fichiers.Add(candidat);
                journal.Add(new EntreeJournalVerificationPlan(
                    sequence++,
                    garantie.GroupeId,
                    candidat.FichierId,
                    EtapeJournalVerificationPlan.VerifierCandidat,
                    candidat.Etat));
            }

            var blocages = fichiers
                .Where(f => f.Etat != EtatVerificationFichier.Valide)
                .Select(f => f.Etat)
                .Distinct()
                .ToList();
            groupes.Add(new VerificationGroupePlan(
                garantie.GroupeId,
                blocages.Count == 0,
                blocages,
                fichiers));
        }

        return new RapportDeVerificationPlan(
            VersionsContratDuplicateFiles.VerificationPlanV1,
            ModeVerificationPlan.Simulation,
            groupes.All(g => g.Executable),
            groupes,
            journal);
    }

    private static VerificationFichierPlan VerifierFichier(
        string groupeId,
        string hashAttendu,
        string fichierId,
        string chemin,
        RoleFichierPlan role,
        IObservateurDeFichier observateur,
        Func<string, bool> cheminProtege)
    {
        if (role == RoleFichierPlan.Candidat && cheminProtege(chemin))
        {
            return new VerificationFichierPlan(
                groupeId,
                fichierId,
                chemin,
                hashAttendu,
                null,
                role,
                EtatVerificationFichier.CheminProtege,
                "chemin protege");
        }

        var observation = observateur.Observer(chemin);
        return new VerificationFichierPlan(
            groupeId,
            fichierId,
            chemin,
            hashAttendu,
            observation.HashObserve,
            role,
            DeriverEtat(observation, hashAttendu),
            observation.Detail);
    }

    private static EtatVerificationFichier DeriverEtat(
        ObservationFichierCourant observation,
        string hashAttendu) => observation.Etat switch
    {
        EtatLectureFichier.Absent => EtatVerificationFichier.Absent,
        EtatLectureFichier.Illisible => EtatVerificationFichier.Illisible,
        EtatLectureFichier.TypeNonPrisEnCharge => EtatVerificationFichier.TypeNonPrisEnCharge,
        EtatLectureFichier.Disponible when
            StringComparer.Ordinal.Equals(observation.HashObserve, hashAttendu) => EtatVerificationFichier.Valide,
        EtatLectureFichier.Disponible => EtatVerificationFichier.HashDifferent,
        _ => throw new InvalidOperationException("etat de lecture inconnu"),
    };
}
