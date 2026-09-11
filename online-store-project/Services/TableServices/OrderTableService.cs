using Azure.Data.Tables;
using online_store_project.Models;
using System.Text.Json;

namespace online_store_project.Services.TableServices
{
    public class OrderTableService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public OrderTableService(IConfiguration configuration,HttpClient httpClient)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["AzureFunctions:OrderTableBaseUrl"]
                ?? throw new InvalidOperationException("AzureFunctions:OrderTableBaseUrl configuration missing.");
            _httpClient.BaseAddress = new Uri(baseUrl);
            _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
        }

        //CRUD Operations for OrderEntity
        public async Task AddorUpdateOrderEntityAsync(OrderEntity order)
        {
            if(order==null)
                throw new ArgumentNullException(nameof(order), "Order entity cannot be null.");
            
            var json = JsonSerializer.Serialize(order, _jsonSerializerOptions);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("api/orders/create", content);
            response.EnsureSuccessStatusCode();
        }

        //GET a single order entity by partition key and row key
        public async Task<OrderEntity> GetOrderEntityAsync(string partitionKey, string rowKey)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/orders/getorder?partitionKey={partitionKey}&rowKey={rowKey}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<OrderEntity>(json, _jsonSerializerOptions) 
                ?? throw new InvalidOperationException("Failed to deserialize order entity.");
        }

        public async Task<List<OrderEntity>> GetAllOrderEntitiesAsync()
        {
            HttpResponseMessage responseMessage = await _httpClient.GetAsync("api/orders/getall");
            responseMessage.EnsureSuccessStatusCode();

            var json = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<OrderEntity>>(json, _jsonSerializerOptions) 
                ?? throw new InvalidOperationException("Failed to deserialize order entities.");
        }

        //delete entities
        public async Task DeleteOrderEntityAsync(string partitionKey, string rowKey)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/orders/delete?partitionKey={partitionKey}&rowKey={rowKey}");
            response.EnsureSuccessStatusCode();

        }
    }
}
