using Azure.Storage.Files.Shares.Models;

namespace ABCRetailAzureApp.Services
{
    public interface IAzureFileService
    {
        Task<string> UploadLogFileAsync(string fileName, string content, string directory = "logs");
        Task<string> ReadLogFileAsync(string fileName, string directory = "logs");
        Task<bool> DeleteLogFileAsync(string fileName, string directory = "logs");
        Task<List<string>> ListLogFilesAsync(string directory = "logs");
        Task AppendToLogFileAsync(string fileName, string content, string directory = "logs");
        Task LogOrderProcessingAsync(string orderId, string action, string details, string level = "INFO");
        Task LogInventoryUpdateAsync(string productId, string action, int quantity, string level = "INFO");
        Task LogImageProcessingAsync(string imageName, string action, string result, string level = "INFO");
        Task LogApplicationErrorAsync(string error, string stackTrace = "", string level = "ERROR");
    }
}
