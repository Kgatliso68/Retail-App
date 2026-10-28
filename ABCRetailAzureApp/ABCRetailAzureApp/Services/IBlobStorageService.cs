using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABCRetailAzureApp.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string containerName);
        Task<bool> DeleteImageAsync(string imageUrl, string containerName);

        // Generate a temporary SAS URL for a single blob
        Task<string> GetImageUrlAsync(string blobName, string containerName, TimeSpan sasValidity);

        // List all blobs and generate SAS URLs for each
        Task<List<string>> GetAllImageUrlsAsync(string containerName, TimeSpan sasValidity);
    }
}

