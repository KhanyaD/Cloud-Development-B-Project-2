using Cloud_Development_B__project_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cloud_Development_B__project_1.Pages
{
    public class ImagesModel : PageModel
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<ImagesModel> _logger;

        public ImagesModel(
            IBlobStorageService blobStorageService,
            ILogger<ImagesModel> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<string> ImageUrls { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadImagesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Please select an image to upload.");

                await LoadImagesAsync();
                return Page();
            }

            try
            {
                var fileName =
                    $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";

                await using var stream = ImageFile.OpenReadStream();

                await _blobStorageService.UploadAsync(fileName, stream);

                SuccessMessage = "The image was uploaded successfully.";

                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to upload image to Azure Blob Storage.");

                ErrorMessage = "Unable to upload the image.";
                await LoadImagesAsync();

                return Page();
            }
        }

        private async Task LoadImagesAsync()
        {
            try
            {
                ImageUrls = await _blobStorageService.GetAllBlobUrlsAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load images from Azure Blob Storage.");

                ErrorMessage = "Unable to load images from Azure Blob Storage.";
                ImageUrls = new List<string>();
            }
        }
    }
}