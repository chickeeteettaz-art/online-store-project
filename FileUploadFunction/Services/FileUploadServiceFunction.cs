using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using FileUploadFunction.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FileUploadFunction.Services
{
    public interface IFileUploadServiceFunction
    {
        Task UploadFileAsync(IFormFile file);
        Task<List<ProductFile>> GetFilesAsync();
        Task<Stream> DownloadFileAsync(string fileName);
        Task DeleteFileAsync(string fileName);
    }

    public class FileUploadServiceFunction : IFileUploadServiceFunction
    {
        private readonly ShareClient _shareClient;

        public FileUploadServiceFunction(IConfiguration configuration)
        {
            string connectionString = configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException("AzureStorage:ConnectionString is missing.");

            _shareClient = new ShareClient(connectionString, "productfiles");
            _shareClient.CreateIfNotExists();
        }

        public async Task UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.", nameof(file));

            string fileName = Path.GetFileName(file.FileName);

            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directory.GetFileClient(fileName);

            if (await fileClient.ExistsAsync())
            {
                await fileClient.DeleteAsync();
            }

            await fileClient.CreateAsync(file.Length);

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
                throw new ArgumentException("File name is required.", nameof(fileName));

            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directory.GetFileClient(fileName);

            if (!await fileClient.ExistsAsync())
                throw new FileNotFoundException($"File '{fileName}' was not found.");

            ShareFileDownloadInfo download = await fileClient.DownloadAsync();
            return download.Content;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.", nameof(fileName));

            ShareDirectoryClient directory = _shareClient.GetRootDirectoryClient();
            ShareFileClient fileClient = directory.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync();
        }
    }
}