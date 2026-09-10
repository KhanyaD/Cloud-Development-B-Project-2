namespace Cloud_Development_B__project_1.Services
{
    public interface IBlobStorageService
    {
        Task UploadAsync(string fileName, Stream stream);

        Task<List<string>> GetAllBlobUrlsAsync();

        Task DeleteAsync(string fileName);
    }
}