using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace ABCRetailAzureApp.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;

        public BlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.", nameof(file));

            var connectionString = _configuration.GetConnectionString("AzureStorage");
            var containerClient = new BlobContainerClient(connectionString, containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            var fullUrl = blobClient.Uri.ToString();

            
            Console.WriteLine($"Generated blob URL: {fullUrl}");
            System.Diagnostics.Debug.WriteLine($"Generated blob URL: {fullUrl}");

            return fullUrl;
        }


        public async Task<bool> DeleteImageAsync(string imageUrl, string containerName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return false;

                var connectionString = _configuration.GetConnectionString("AzureStorage");
                var containerClient = new BlobContainerClient(connectionString, containerName);

                string blobName;

               
                if (imageUrl.StartsWith("https://"))
                {
                   
                    var uri = new Uri(imageUrl);
                    blobName = Path.GetFileName(uri.AbsolutePath);
                }
                else
                {
                    
                    blobName = imageUrl;
                }

                Console.WriteLine($"Attempting to delete blob: {blobName} from container: {containerName}");

                var blobClient = containerClient.GetBlobClient(blobName);
                var result = await blobClient.DeleteIfExistsAsync();

                Console.WriteLine($"Delete result: {result.Value} for blob: {blobName}");

                return result.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex.Message}");
                
                return false;
            }
        }

        public async Task<string> GetImageUrlAsync(string blobName, string containerName, TimeSpan sasValidity)
        {
            var connectionString = _configuration.GetConnectionString("AzureStorage");
            var containerClient = new BlobContainerClient(connectionString, containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                
                var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(sasValidity));
                return sasUri.ToString();
            }

            return null;
        }

        public async Task<List<string>> GetAllImageUrlsAsync(string containerName, TimeSpan sasValidity)
        {
            var connectionString = _configuration.GetConnectionString("AzureStorage");
            var containerClient = new BlobContainerClient(connectionString, containerName);

            var imageUrls = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(sasValidity));
                imageUrls.Add(sasUri.ToString());
            }

            return imageUrls;
        }
    }
}

