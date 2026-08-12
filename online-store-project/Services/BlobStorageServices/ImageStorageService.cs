using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace online_store_project.Services.BlobStorageServices
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string? folder = null);
        Task DeleteFileAsync(string blobName);
    }
    public class ImageStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public ImageStorageService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"];

            var containerName = configuration["AzureStorage:ContainerName"];


            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string? folder = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            // Optional: validate
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type");

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var blobName = string.IsNullOrEmpty(folder)
                ? fileName
                : $"{folder.TrimEnd('/')}/{fileName}";

            var blobClient = _containerClient.GetBlobClient(blobName);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, httpHeaders);

            return blobClient.Uri.ToString(); // full public URL
        }

        public async Task DeleteFileAsync(string blobName)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
