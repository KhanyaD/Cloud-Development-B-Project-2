namespace Cloud_Development_B__project_1.Services
{
    public interface IFileStorageService
    {
        Task UploadFileAsync(string fileName, Stream fileStream);

        Task<Stream> DownloadFileAsync(string fileName);

        Task<List<string>> GetAllFilesAsync();

        Task DeleteFileAsync(string fileName);
    }
}