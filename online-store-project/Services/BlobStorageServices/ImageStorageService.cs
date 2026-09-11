
using System.Net.Http.Headers;

namespace online_store_project.Services.BlobStorageServices
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string? folder = null);
        Task DeleteFileAsync(string blobName);
    }

    public class ImageStorageService : IBlobStorageService
    {
        private readonly HttpClient _httpClient;

        // HttpClient is automatically provided via AddHttpClient in Program.cs
        public ImageStorageService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> UploadFileAsync(IFormFile file, string? folder = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was provided for upload.", nameof(file));
            }

            // Prepare multipart form content
            using var content = new MultipartFormDataContent();

            await using var stream = file.OpenReadStream();
            using var streamContent = new StreamContent(stream);

            if (!string.IsNullOrEmpty(file.ContentType))
            {
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            }

            // Attach the file stream to the HTTP request payload
            content.Add(streamContent, "file", file.FileName);

            // Construct relative URL path
            var relativeUrl = string.IsNullOrEmpty(folder)
                ? "api/blob/upload"
                : $"api/blob/upload?folder={Uri.EscapeDataString(folder)}";

            // Fallback check to avoid URI errors if BaseAddress was not configured in DI
            var requestUri = _httpClient.BaseAddress != null
                ? new Uri(relativeUrl, UriKind.Relative)
                : new Uri($"http://localhost:7063/{relativeUrl.TrimStart('/')}");

            var response = await _httpClient.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Azure Function upload failed with status {(int)response.StatusCode} ({response.StatusCode}): {errorDetails}");
            }

            // Deserialize response payload: { "url": "https://..." }
            var result = await response.Content.ReadFromJsonAsync<UploadResponse>();

            return result?.Url ?? throw new InvalidOperationException("The file was uploaded, but the Azure Function returned a null URL response.");
        }

        public async Task DeleteFileAsync(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
            }

            var relativeUrl = $"api/blob/delete?blobName={Uri.EscapeDataString(blobName)}";

            var requestUri = _httpClient.BaseAddress != null
                ? new Uri(relativeUrl, UriKind.Relative)
                : new Uri($"http://localhost:7063/{relativeUrl.TrimStart('/')}");

            var response = await _httpClient.DeleteAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Azure Function deletion failed with status {(int)response.StatusCode} ({response.StatusCode}): {errorDetails}");
            }
        }

        // Internal DTO to bind JSON response from Azure Function
        private record UploadResponse(string Url);
    }
}