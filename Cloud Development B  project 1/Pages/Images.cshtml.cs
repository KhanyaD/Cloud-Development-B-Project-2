using Cloud_Development_B__project_1.Functions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace Cloud_Development_B__project_1.Pages
{
    public class ImagesModel : PageModel
    {
        private readonly ProductImageFunction _productImageFunction;
        private readonly ILogger<ImagesModel> _logger;

        public ImagesModel(
            ProductImageFunction productImageFunction,
            ILogger<ImagesModel> logger)
        {
            _productImageFunction = productImageFunction;
            _logger = logger;
        }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public List<ImageItem> Images { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
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
                    "Please select an image.");

                await LoadImagesAsync();
                return Page();
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".webp"
            };

            var extension =
                Path.GetExtension(ImageFile.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Please select a valid JPG, PNG, GIF or WEBP image.");

                await LoadImagesAsync();
                return Page();
            }

            const long maxFileSize = 5 * 1024 * 1024;

            if (ImageFile.Length > maxFileSize)
            {
                ModelState.AddModelError(
                    nameof(ImageFile),
                    "Image size cannot exceed 5 MB.");

                await LoadImagesAsync();
                return Page();
            }

            try
            {
                var originalFileName =
                    Path.GetFileName(ImageFile.FileName);

                var blobFileName =
                    $"{Guid.NewGuid()}_{originalFileName}";

                await using var stream =
                    ImageFile.OpenReadStream();

                await _productImageFunction
                    .UploadProductImageAsync(
                        blobFileName,
                        stream);

                SuccessMessage =
                    "Image uploaded successfully to Azure Blob Storage.";

                return RedirectToPage();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to upload image to Azure Blob Storage.");

                ErrorMessage =
                    "Unable to upload the image to Azure Blob Storage.";

                await LoadImagesAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(
            string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                ErrorMessage =
                    "The selected image could not be found.";

                return RedirectToPage();
            }

            try
            {
                await _productImageFunction
                    .DeleteProductImageAsync(fileName);

                SuccessMessage =
                    "Image deleted successfully.";
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to delete image {FileName}.",
                    fileName);

                ErrorMessage =
                    "Unable to delete the selected image.";
            }

            return RedirectToPage();
        }

        private async Task LoadImagesAsync()
        {
            try
            {
                var urls =
                    await _productImageFunction
                        .GetProductImagesAsync();

                Images = urls
                    .Select(CreateImageItem)
                    .OrderBy(image => image.DisplayName)
                    .ToList();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to load images from Azure Blob Storage.");

                ErrorMessage =
                    "Unable to load images from Azure Blob Storage.";

                Images = new List<ImageItem>();
            }
        }

        private static ImageItem CreateImageItem(
            string url)
        {
            var uri = new Uri(url);

            var fileName =
                Uri.UnescapeDataString(
                    Path.GetFileName(uri.AbsolutePath));

            return new ImageItem
            {
                Url = url,
                FileName = fileName,
                DisplayName =
                    CreateFriendlyDisplayName(fileName)
            };
        }

        private static string CreateFriendlyDisplayName(
            string fileName)
        {
            var name =
                Path.GetFileNameWithoutExtension(fileName);

            var underscorePosition =
                name.IndexOf('_');

            if (underscorePosition > 0)
            {
                var possibleGuid =
                    name[..underscorePosition];

                if (Guid.TryParse(possibleGuid, out _))
                {
                    name =
                        name[(underscorePosition + 1)..];
                }
            }

            name = name
                .Replace("_", " ")
                .Replace("-", " ");

            name =
                string.Join(
                    " ",
                    name.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries));

            if (string.IsNullOrWhiteSpace(name))
            {
                return "Product Image";
            }

            return CultureInfo
                .CurrentCulture
                .TextInfo
                .ToTitleCase(name.ToLower());
        }

        public class ImageItem
        {
            public string FileName { get; set; } =
                string.Empty;

            public string DisplayName { get; set; } =
                string.Empty;

            public string Url { get; set; } =
                string.Empty;
        }
    }
}