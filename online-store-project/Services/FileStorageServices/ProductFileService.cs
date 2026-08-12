using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using online_store_project.Models;

namespace online_store_project.Services.FileServices
{
    public class ProductFileService
    {
        private readonly ShareClient _shareClient;

        public ProductFileService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureStorage:connectionString"];
            _shareClient = new ShareClient(connectionString, "productfiles");
            _shareClient.CreateIfNotExists();
        }

        public async Task UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            // Use the original file name (FileName is more reliable than Name)
            string fileName = Path.GetFileName(file.FileName);

            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directory.GetFileClient(fileName);

            // If the file already exists, delete it first so we can overwrite
            if (await fileClient.ExistsAsync())
            {
                await fileClient.DeleteAsync();
            }

            // Create the file with the correct size
            await fileClient.CreateAsync(file.Length);

            // Upload the content
            using (Stream stream = file.OpenReadStream())
            {
                await fileClient.UploadRangeAsync(
                    new HttpRange(0, stream.Length),
                    stream);
            }
        }

        public async Task<List<ProductFile>> GetFilesAsync()
        {
            var files = new List<ProductFile>();

            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();

            await foreach (ShareFileItem item in directory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(new ProductFile
                    {
                        FileName = item.Name,
                        FileSize = item.FileSize ?? 0
                    });
                }
            }

            return files;
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.");

            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directory.GetFileClient(fileName);

            if (!await fileClient.ExistsAsync())
                throw new FileNotFoundException($"File '{fileName}' was not found.");

            ShareFileDownloadInfo download = await fileClient.DownloadAsync();
            return download.Content;
        }

        // Optional: Delete a file
        public async Task DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.");

            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directory.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync();
        }
    }
}