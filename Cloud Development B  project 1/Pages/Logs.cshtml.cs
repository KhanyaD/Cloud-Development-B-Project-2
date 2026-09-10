using Cloud_Development_B__project_1.Functions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages
{
    public class LogsModel : PageModel
    {
        private readonly FileShareFunction _fileShareFunction;
        private readonly ILogger<LogsModel> _logger;

        public LogsModel(
            FileShareFunction fileShareFunction,
            ILogger<LogsModel> logger)
        {
            _fileShareFunction =
                fileShareFunction;

            _logger =
                logger;
        }

        [BindProperty]
        public IFormFile? UploadedFile { get; set; }

        public List<string> Files { get; set; } =
            new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadFilesAsync();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (UploadedFile == null ||
                UploadedFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(UploadedFile),
                    "Please select a file.");

                await LoadFilesAsync();

                return Page();
            }

            const long maxFileSize =
                10 * 1024 * 1024;

            if (UploadedFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(UploadedFile),
                    "File size cannot exceed 10 MB.");

                await LoadFilesAsync();

                return Page();
            }

            try
            {
                var originalFileName =
                    Path.GetFileName(
                        UploadedFile.FileName);

                var fileName =
                    $"{Guid.NewGuid()}_{originalFileName}";

                await using var stream =
                    UploadedFile.OpenReadStream();

                await _fileShareFunction
                    .UploadLogFileAsync(
                        fileName,
                        stream);

                SuccessMessage =
                    "File uploaded successfully to Azure File Storage.";

                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to upload file to Azure File Storage.");

                ErrorMessage =
                    "Unable to upload the file to Azure File Storage.";

                await LoadFilesAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnGetDownloadAsync(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                ErrorMessage =
                    "The requested file could not be found.";

                return RedirectToPage();
            }

            try
            {
                var stream =
                    await _fileShareFunction
                        .DownloadLogFileAsync(fileName);

                return File(
                    stream,
                    "application/octet-stream",
                    fileName);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to download file {FileName}.",
                    fileName);

                ErrorMessage =
                    "Unable to download the selected file.";

                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                ErrorMessage =
                    "The selected file could not be found.";

                return RedirectToPage();
            }

            try
            {
                await _fileShareFunction
                    .DeleteLogFileAsync(fileName);

                SuccessMessage =
                    "File deleted successfully.";
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to delete file {FileName}.",
                    fileName);

                ErrorMessage =
                    "Unable to delete the selected file.";
            }

            return RedirectToPage();
        }

        private async Task LoadFilesAsync()
        {
            try
            {
                Files =
                    await _fileShareFunction
                        .GetLogFilesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load files from Azure File Storage.");

                ErrorMessage =
                    "Unable to load files from Azure File Storage.";

                Files =
                    new List<string>();
            }
        }
    }
}