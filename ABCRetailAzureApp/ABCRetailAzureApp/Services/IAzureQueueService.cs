using Azure.Storage.Queues.Models;

namespace ABCRetailAzureApp.Services
{
    public interface IAzureQueueService
    {
        Task SendMessageAsync(string queueName, string message);
        Task<QueueMessage?> ReceiveMessageAsync(string queueName);
        Task DeleteMessageAsync(string queueName, string messageId, string popReceipt);
        Task<int> GetQueueLengthAsync(string queueName);
        Task CreateQueueIfNotExistsAsync(string queueName);
        Task SendOrderProcessingMessageAsync(string orderId, string action, string details = "");
        Task SendInventoryUpdateMessageAsync(string productId, string action, int quantity = 0);
        Task SendImageProcessingMessageAsync(string imageName, string action, string containerName = "");
    }
}
