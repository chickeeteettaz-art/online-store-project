using Microsoft.AspNetCore.Mvc;
using online_store_project.Services.FileServices;

namespace online_store_project.Controllers
{
    public class ProductFilesController : Controller
    {
        private readonly ProductFileService _fileService;

        public ProductFilesController(ProductFileService fileService)
        {
            _fileService = fileService;
        }

        // GET: /ProductFiles
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var files = await _fileService.GetFilesAsync();
            return View(files);
        }

        // GET: /ProductFiles/Upload
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // POST: /ProductFiles/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Please select a file to upload.");
                    return View();
                }

                await _fileService.UploadFileAsync(file);

                TempData["SuccessMessage"] = $"File '{file.FileName}' uploaded successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while uploading the file: {ex.Message}");
                return View();
            }
        }

        // GET: /ProductFiles/Download?fileName=example.pdf
        [HttpGet]
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                TempData["ErrorMessage"] = "File name is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var stream = await _fileService.DownloadFileAsync(fileName);

                var contentType = GetContentType(fileName);

                return File(stream, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                TempData["ErrorMessage"] = $"File '{fileName}' was not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not download file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Optional: Delete a file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                TempData["ErrorMessage"] = "File name is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _fileService.DeleteFileAsync(fileName);
                TempData["SuccessMessage"] = $"File '{fileName}' deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not delete file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method for content type
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                ".mp4" => "video/mp4",
                ".mp3" => "audio/mpeg",
                _ => "application/octet-stream"
            };
        }
    }
}