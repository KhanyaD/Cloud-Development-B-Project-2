using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace Cloud_Development_B__project_1.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        private const string ContainerName = "product-images";

        public BlobStorageService(IConfiguration configuration)
        {
            var connectionString =
                configuration["AzureStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is missing.");
            }

            var blobServiceClient =
                new BlobServiceClient(connectionString);

            _containerClient =
                blobServiceClient.GetBlobContainerClient(ContainerName);

            // Creates a PRIVATE container.
            // Do NOT enable anonymous/public access.
            _containerClient.CreateIfNotExists();
        }

        public async Task UploadAsync(
            string fileName,
            Stream stream)
        {
            fileName = Path.GetFileName(fileName);

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = GetContentType(fileName)
                }
            };

            await blobClient.UploadAsync(
                stream,
                options,
                cancellationToken: default);
        }

        public async Task<List<string>> GetAllBlobUrlsAsync()
        {
            var urls = new List<string>();

            await foreach (
                var blobItem in _containerClient.GetBlobsAsync())
            {
                var blobClient =
                    _containerClient.GetBlobClient(blobItem.Name);

                if (!blobClient.CanGenerateSasUri)
                {
                    throw new InvalidOperationException(
                        "The application cannot generate a secure Blob SAS URL. " +
                        "Check the Azure Storage connection configuration.");
                }

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = ContainerName,
                    BlobName = blobItem.Name,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                sasBuilder.SetPermissions(
                    BlobSasPermissions.Read);

                var sasUri =
                    blobClient.GenerateSasUri(sasBuilder);

                urls.Add(sasUri.ToString());
            }

            return urls;
        }

        public async Task DeleteAsync(string fileName)
        {
            fileName = Path.GetFileName(fileName);

            var blobClient =
                _containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }

        private static string GetContentType(string fileName)
        {
            var extension =
                Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}