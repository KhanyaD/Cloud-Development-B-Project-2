using Azure.Storage.Files.Shares;

namespace Cloud_Development_B__project_1.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ShareClient _shareClient;
        private readonly ShareDirectoryClient _directoryClient;

        private const string ShareName = "logs";

        public FileStorageService(
            IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            _shareClient =
                new ShareClient(
                    connectionString,
                    ShareName);

            _shareClient.CreateIfNotExists();

            _directoryClient =
                _shareClient.GetRootDirectoryClient();
        }

        public async Task UploadFileAsync(
            string fileName,
            Stream fileStream)
        {
            fileName =
                Path.GetFileName(fileName);

            var fileClient =
                _directoryClient.GetFileClient(fileName);

            // Create the Azure File with the required size.
            await fileClient.CreateAsync(
                fileStream.Length);

            // Ensure the stream begins at the start.
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            await fileClient.UploadAsync(
                fileStream);
        }

        public async Task<Stream> DownloadFileAsync(
            string fileName)
        {
            fileName =
                Path.GetFileName(fileName);

            var fileClient =
                _directoryClient.GetFileClient(fileName);

            var response =
                await fileClient.DownloadAsync();

            return response.Value.Content;
        }

        public async Task<List<string>> GetAllFilesAsync()
        {
            var files =
                new List<string>();

            await foreach (
                var item in
                _directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return files
                .OrderBy(name => name)
                .ToList();
        }

        public async Task DeleteFileAsync(
            string fileName)
        {
            fileName =
                Path.GetFileName(fileName);

            var fileClient =
                _directoryClient.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync();
        }
    }
}