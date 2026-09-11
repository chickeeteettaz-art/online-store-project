using System.Text;
using System.Text.Json;

namespace online_store_project.Services.QueueServices
{
    public class QueueService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public QueueService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            string baseUrl = configuration["AzureFunctions:QueueServiceBaseUrl"]
                ?? throw new InvalidOperationException("AzureFunctions:QueueServiceBaseUrl configuration missing.");

            _httpClient.BaseAddress = new Uri(baseUrl);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task SendMessageAsync(Models.QueueMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            string json = JsonSerializer.Serialize(message);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("api/queue/send", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Models.QueueMessage>> ReceiveMessagesAsync(int maxMessages = 10)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/queue/receive?maxMessages={maxMessages}");
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Models.QueueMessage>>(jsonContent, _jsonOptions)
                   ?? new List<Models.QueueMessage>();
        }

        public async Task<List<Models.QueueMessage>> PeekMessagesAsync(int maxMessages = 10)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/queue/peek?maxMessages={maxMessages}");
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Models.QueueMessage>>(jsonContent, _jsonOptions)
                   ?? new List<Models.QueueMessage>();
        }

        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt))
                throw new ArgumentException("MessageId and PopReceipt are required.");

            string requestUri = $"api/queue/delete?messageId={Uri.EscapeDataString(messageId)}&popReceipt={Uri.EscapeDataString(popReceipt)}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(requestUri);

            response.EnsureSuccessStatusCode();
        }
    }
}