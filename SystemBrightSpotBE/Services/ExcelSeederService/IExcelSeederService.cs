namespace SystemBrightSpotBE.Services.ExcelSeederService
{
    public interface IExcelSeederService
    {
        Dictionary<string, List<(string Text, string Color)>> ReadExcelCategoryFile(string pathCategory);
        Task HandleImportExcelUserFileAsync(string path);
        Task SeedDataFromExcelAsync(string pathCategory, string pathUser);
    }
}
