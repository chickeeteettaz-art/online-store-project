using System;
using System.Linq;
using System.Threading.Tasks;
using BlobStorageFunction.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BlobStorageFunction
{
    public class Function1
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<Function1> _logger;

        public Function1(IBlobStorageService blobStorageService, ILogger<Function1> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        [Function("UploadBlob")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "blob/upload")] HttpRequest req)
        {
            _logger.LogInformation("Processing blob upload request.");

            try
            {
                if (!req.HasFormContentType || !req.Form.Files.Any())
                {
                    return new BadRequestObjectResult("Please provide a file in multipart/form-data format.");
                }

                var file = req.Form.Files[0];
                string? folder = req.Query["folder"];

                // Calls the original IBlobStorageService implementation
                string fileUrl = await _blobStorageService.UploadFileAsync(file, folder);

                return new OkObjectResult(new { Url = fileUrl });
            }
            catch (ArgumentException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file via IBlobStorageService.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}