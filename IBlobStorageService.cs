namespace Cloud_Development_B__project_1.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadBlobAsync(string blobName, Stream content, string contentType);
        Task<List<string>> GetAllBlobUrlsAsync();
        Task<Stream> DownloadBlobAsync(string blobName);
    }
}