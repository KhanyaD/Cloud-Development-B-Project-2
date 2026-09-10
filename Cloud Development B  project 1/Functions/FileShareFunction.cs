using Cloud_Development_B__project_1.Services;

namespace Cloud_Development_B__project_1.Functions
{
    public class FileShareFunction
    {
        private readonly IFileStorageService _fileStorageService;

        public FileShareFunction(
            IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task UploadLogFileAsync(
            string fileName,
            Stream fileStream)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "A file name is required.",
                    nameof(fileName));
            }

            if (fileStream == null)
            {
                throw new ArgumentNullException(
                    nameof(fileStream));
            }

            await _fileStorageService.UploadFileAsync(
                fileName,
                fileStream);
        }

        public async Task<List<string>> GetLogFilesAsync()
        {
            return await _fileStorageService
                .GetAllFilesAsync();
        }

        public async Task<Stream> DownloadLogFileAsync(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "A file name is required.",
                    nameof(fileName));
            }

            return await _fileStorageService
                .DownloadFileAsync(fileName);
        }

        public async Task DeleteLogFileAsync(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "A file name is required.",
                    nameof(fileName));
            }

            await _fileStorageService
                .DeleteFileAsync(fileName);
        }
    }
}