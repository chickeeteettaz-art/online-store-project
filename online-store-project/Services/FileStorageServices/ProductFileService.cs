using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using online_store_project.Models;

namespace online_store_project.Services.FileServices
{
    public class ProductFileService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductFileService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            string baseUrl = configuration["AzureFunctions:FileUploadBaseUrl"]
                ?? throw new InvalidOperationException("AzureFunctions:BaseUrl configuration missing.");

            _httpClient.BaseAddress = new Uri(baseUrl);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.", nameof(file));

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            using var streamContent = new StreamContent(stream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", file.FileName);

            HttpResponseMessage response = await _httpClient.PostAsync("api/files/upload", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ProductFile>> GetFilesAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/files");
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductFile>>(jsonContent, _jsonOptions) ?? new List<ProductFile>();
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.", nameof(fileName));

            HttpResponseMessage response = await _httpClient.GetAsync($"api/files/download?fileName={Uri.EscapeDataString(fileName)}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new FileNotFoundException($"File '{fileName}' was not found.");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name is required.", nameof(fileName));

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/files?fileName={Uri.EscapeDataString(fileName)}");
            response.EnsureSuccessStatusCode();
        }
    }
}