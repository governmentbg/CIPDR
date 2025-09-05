namespace URegister.NomenclaturesCatalog.Contracts
{
    public interface IImportNrnmNsiService
    {
        Task ImportArea1(string nomenclatureType);
        Task ImportNrnmNsi();
        Task ImportEkStreet(string nomenclatureType);
    }
}
