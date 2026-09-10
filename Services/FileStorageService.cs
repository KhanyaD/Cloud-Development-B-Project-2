using Azure.Storage.Files.Shares;

namespace Cloud_Development_B__project_1.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ShareClient _shareClient;
        private readonly ShareDirectoryClient _directoryClient;

        public FileStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");

            _shareClient = new ShareClient(connectionString, "contracts");
            _directoryClient = _shareClient.GetRootDirectoryClient();
        }

        public async Task UploadFileAsync(string fileName, Stream fileStream)
        {
            await _shareClient.CreateIfNotExistsAsync();

            var fileClient = _directoryClient.GetFileClient(fileName);

            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            await _shareClient.CreateIfNotExistsAsync();

            var fileClient = _directoryClient.GetFileClient(fileName);
            var response = await fileClient.DownloadAsync();

            return response.Value.Content;
        }

        public async Task<List<string>> GetAllFilesAsync()
        {
            await _shareClient.CreateIfNotExistsAsync();

            var files = new List<string>();

            await foreach (var item in _directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            await _shareClient.CreateIfNotExistsAsync();

            var fileClient = _directoryClient.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }
    }
}