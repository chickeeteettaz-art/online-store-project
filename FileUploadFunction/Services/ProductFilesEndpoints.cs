using FileUploadFunction.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FileUploadFunction.Functions
{
    public class ProductFilesEndpoints
    {
        private readonly IFileUploadServiceFunction _fileService;

        public ProductFilesEndpoints(IFileUploadServiceFunction fileService)
        {
            _fileService = fileService;
        }

        [Function("UploadFile")]
        public async Task<IActionResult> Upload([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "files/upload")] HttpRequest req)
        {
            if (!req.HasFormContentType || req.Form.Files.Count == 0)
                return new BadRequestObjectResult("No file stream provided.");

            var file = req.Form.Files[0];
            await _fileService.UploadFileAsync(file);

            return new OkObjectResult(new { Message = "File uploaded successfully." });
        }

        [Function("GetFiles")]
        public async Task<IActionResult> GetFiles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "files")] HttpRequest req)
        {
            var files = await _fileService.GetFilesAsync();
            return new OkObjectResult(files);
        }

        [Function("DownloadFile")]
        public async Task<IActionResult> Download([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "files/download")] HttpRequest req)
        {
            string? fileName = req.Query["fileName"];
            if (string.IsNullOrWhiteSpace(fileName))
                return new BadRequestObjectResult("Parameter 'fileName' is required.");

            try
            {
                var fileStream = await _fileService.DownloadFileAsync(fileName);
                return new FileStreamResult(fileStream, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };
            }
            catch (FileNotFoundException)
            {
                return new NotFoundObjectResult($"File '{fileName}' not found.");
            }
        }

        [Function("DeleteFile")]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "files")] HttpRequest req)
        {
            string? fileName = req.Query["fileName"];
            if (string.IsNullOrWhiteSpace(fileName))
                return new BadRequestObjectResult("Parameter 'fileName' is required.");

            await _fileService.DeleteFileAsync(fileName);
            return new OkResult();
        }
    }
}