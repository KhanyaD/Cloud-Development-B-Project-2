namespace Cloud_Development_B__project_1.Services
{
    public interface IQueueStorageService
    {
        Task SendMessageAsync(string message);

        Task<List<string>> PeekMessagesAsync(int maxMessages = 10);
    }
}