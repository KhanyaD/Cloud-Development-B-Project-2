using Cloud_Development_B__project_1.Services;

namespace Cloud_Development_B__project_1.Functions
{
    public class ProductImageFunction
    {
        private readonly IBlobStorageService _blobStorageService;

        public ProductImageFunction(
            IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public async Task UploadProductImageAsync(
            string fileName,
            Stream stream)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "A file name is required.",
                    nameof(fileName));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(
                    nameof(stream));
            }

            await _blobStorageService.UploadAsync(
                fileName,
                stream);
        }

        public async Task<List<string>> GetProductImagesAsync()
        {
            return await _blobStorageService
                .GetAllBlobUrlsAsync();
        }

        public async Task DeleteProductImageAsync(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "A file name is required.",
                    nameof(fileName));
            }

            await _blobStorageService.DeleteAsync(
                fileName);
        }
    }
}