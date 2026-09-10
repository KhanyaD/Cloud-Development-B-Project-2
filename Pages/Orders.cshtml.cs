using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Cloud_Development_B__project_1.Services;

namespace Cloud_Development_B__project_1.Pages
{
    public class OrdersModel : PageModel
    {
        private readonly IQueueStorageService _queueStorageService;

        public OrdersModel(IQueueStorageService queueStorageService)
        {
            _queueStorageService = queueStorageService;
        }

        [BindProperty]
        public string MessageContent { get; set; } = string.Empty;

        public List<string> QueueMessages { get; set; } = new List<string>();
        public string ProcessedMessage { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            QueueMessages = await _queueStorageService.GetAllMessagesAsync();
        }

        public async Task<IActionResult> OnPostSendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(MessageContent))
            {
                ModelState.AddModelError("MessageContent", "Please enter order details.");
                return Page();
            }

            await _queueStorageService.SendMessageAsync(MessageContent);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReceiveMessageAsync()
        {
            ProcessedMessage = await _queueStorageService.ReceiveMessageAsync();
            QueueMessages = await _queueStorageService.GetAllMessagesAsync();
            return Page();
        }
    }
}