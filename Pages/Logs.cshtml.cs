using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages
{
    public class LogsModel : PageModel
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<LogsModel> _logger;

        public LogsModel(
            IFileStorageService fileStorageService,
            ILogger<LogsModel> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        [BindProperty]
        public IFormFile? LogFile { get; set; }

        public List<string> FileNames { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadFilesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (LogFile == null || LogFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(LogFile),
                    "Please select a file to upload.");

                await LoadFilesAsync();
                return Page();
            }

            try
            {
                var fileName =
                    $"{Guid.NewGuid()}_{Path.GetFileName(LogFile.FileName)}";

                await using var stream = LogFile.OpenReadStream();

                await _fileStorageService.UploadFileAsync(
                    fileName,
                    stream);

                TempData["SuccessMessage"] =
                    "The file was uploaded successfully.";

                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to upload a file to Azure File Storage.");

                ErrorMessage = "Unable to upload the file to Azure File Storage.";
                await LoadFilesAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnGetDownloadAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }

            try
            {
                var stream =
                    await _fileStorageService.DownloadFileAsync(fileName);

                return File(
                    stream,
                    "application/octet-stream",
                    fileName);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to download file {FileName} from Azure File Storage.",
                    fileName);

                TempData["ErrorMessage"] =
                    "Unable to download the selected file from Azure File Storage.";

                return RedirectToPage();
            }
        }

        private async Task LoadFilesAsync()
        {
            try
            {
                FileNames = await _fileStorageService.GetAllFilesAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load files from Azure File Storage.");

                ErrorMessage = "Unable to access Azure File Storage.";
                FileNames = new List<string>();
            }
        }
    }
}