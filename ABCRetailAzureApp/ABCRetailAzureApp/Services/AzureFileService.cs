using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace ABCRetailAzureApp.Services
{
    public class AzureFileService : IAzureFileService
    {
        private readonly ShareClient _shareClient;
        private readonly string _shareName;

        public AzureFileService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage") ??
                throw new InvalidOperationException("Azure Storage connection string not found");

            _shareName = configuration["AzureFiles:ShareName"] ?? "abcretail-logs";
            _shareClient = new ShareClient(connectionString, _shareName);
        }

        
        private async Task EnsureShareAndDirectoryExistAsync(string directoryPath)
        {
            
            await _shareClient.CreateIfNotExistsAsync();

            if (string.IsNullOrWhiteSpace(directoryPath))
                return;

            
            var parts = directoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            ShareDirectoryClient currentDir = _shareClient.GetRootDirectoryClient();

            foreach (var part in parts)
            {
                currentDir = currentDir.GetSubdirectoryClient(part);
                await currentDir.CreateIfNotExistsAsync();
            }
        }

        private ShareFileClient GetFileClient(string fileName, string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return _shareClient.GetRootDirectoryClient().GetFileClient(fileName);
            }

            return _shareClient.GetDirectoryClient(directory).GetFileClient(fileName);
        }

        public async Task<string> UploadLogFileAsync(string fileName, string content, string directory = "logs")
        {
            await EnsureShareAndDirectoryExistAsync(directory);

            var fileClient = GetFileClient(fileName, directory);
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);

            using var stream = new MemoryStream(contentBytes);
            await fileClient.CreateAsync(contentBytes.Length);
            await fileClient.UploadAsync(stream);

            return fileClient.Uri.ToString();
        }

        public async Task<string> ReadLogFileAsync(string fileName, string directory = "logs")
        {
            var fileClient = GetFileClient(fileName, directory);

            if (await fileClient.ExistsAsync())
            {
                var response = await fileClient.DownloadAsync();
                using var reader = new StreamReader(response.Value.Content);
                return await reader.ReadToEndAsync();
            }

            return string.Empty;
        }

        public async Task<bool> DeleteLogFileAsync(string fileName, string directory = "logs")
        {
            var fileClient = GetFileClient(fileName, directory);

            if (await fileClient.ExistsAsync())
            {
                await fileClient.DeleteAsync();
                return true;
            }

            return false;
        }

        public async Task<List<string>> ListLogFilesAsync(string directory = "logs")
        {
            await EnsureShareAndDirectoryExistAsync(directory);

            var directoryClient = string.IsNullOrEmpty(directory)
                ? _shareClient.GetRootDirectoryClient()
                : _shareClient.GetDirectoryClient(directory);

            var fileNames = new List<string>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    fileNames.Add(item.Name);
                }
            }

            return fileNames;
        }

        public async Task AppendToLogFileAsync(string fileName, string content, string directory = "logs")
        {
            var fileClient = GetFileClient(fileName, directory);

            string existingContent = "";
            if (await fileClient.ExistsAsync())
            {
                existingContent = await ReadLogFileAsync(fileName, directory);
            }

            var newContent = existingContent + content;
            await UploadLogFileAsync(fileName, newContent, directory);
        }

        
        public async Task LogOrderProcessingAsync(string orderId, string action, string details, string level = "INFO")
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] ORDER_PROCESSING - OrderId: {orderId}, Action: {action}, Details: {details}{Environment.NewLine}";
            var fileName = $"order-processing-{DateTime.UtcNow:yyyy-MM-dd}.log";

            await AppendToLogFileAsync(fileName, logEntry, "logs/orders");
        }

        public async Task LogInventoryUpdateAsync(string productId, string action, int quantity, string level = "INFO")
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] INVENTORY_UPDATE - ProductId: {productId}, Action: {action}, Quantity: {quantity}{Environment.NewLine}";
            var fileName = $"inventory-updates-{DateTime.UtcNow:yyyy-MM-dd}.log";

            await AppendToLogFileAsync(fileName, logEntry, "logs/inventory");
        }

        public async Task LogImageProcessingAsync(string imageName, string action, string result, string level = "INFO")
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] IMAGE_PROCESSING - ImageName: {imageName}, Action: {action}, Result: {result}{Environment.NewLine}";
            var fileName = $"image-processing-{DateTime.UtcNow:yyyy-MM-dd}.log";

            await AppendToLogFileAsync(fileName, logEntry, "logs/images");
        }

        public async Task LogApplicationErrorAsync(string error, string stackTrace = "", string level = "ERROR")
        {
            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] [{level}] APPLICATION_ERROR - Error: {error}";
            if (!string.IsNullOrEmpty(stackTrace))
            {
                logEntry += $", StackTrace: {stackTrace}";
            }
            logEntry += Environment.NewLine;

            var fileName = $"application-errors-{DateTime.UtcNow:yyyy-MM-dd}.log";

            await AppendToLogFileAsync(fileName, logEntry, "logs/errors");
        }
    }
}

