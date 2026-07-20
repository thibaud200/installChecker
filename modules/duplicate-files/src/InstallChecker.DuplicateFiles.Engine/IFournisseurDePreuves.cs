namespace InstallChecker.DuplicateFiles;

public interface IFournisseurDePreuves
{
    ResultatFournisseur Extraire(FichierObserve fichier);
}
