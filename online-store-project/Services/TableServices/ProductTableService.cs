using Azure.Data.Tables;
using online_store_project.Models;
using System.Text;
using System.Text.Json;

namespace online_store_project.Services.TableServices
{
    public class ProductTableService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductTableService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            var baseUrl = configuration["AzureFunctions:ProductTableBaseUrl"]
                ?? throw new InvalidOperationException("AzureFunctions:ProductTableBaseUrl configuration missing.");
            _httpClient.BaseAddress = new Uri(baseUrl);

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            
        }

        //CRUD Operations for ProductEntity
        public async Task AddorUpdateProductEntityAsync(ProductEntity product)
        {
            try
            {
                if(product == null)
                {
                    throw new ArgumentNullException(nameof(product), "Product entity cannot be null.");
                }
                string json = JsonSerializer.Serialize(product);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/products/create", content);
                response.EnsureSuccessStatusCode();
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("An error occurred while adding or updating the product entity.", ex);
            }
        }

        //GET a single product entity by partition key and row key
        public async Task<ProductEntity> GetProductEntityAsync(string partitionKey, string rowKey)
        {
            HttpResponseMessage httpResponse = await _httpClient.GetAsync($"api/products/getproduct?partitionKey={partitionKey}&rowKey={rowKey}");
            httpResponse.EnsureSuccessStatusCode();

            string jsonContent = await httpResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductEntity>(jsonContent, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize the product entity.");
        }

        public async Task<List<ProductEntity>> GetAllProductEntitiesAsync()
        {
            HttpResponseMessage httpResponse = await _httpClient.GetAsync("api/products/getproducts");
            httpResponse.EnsureSuccessStatusCode();

            string jsonContent = await httpResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ProductEntity>>(jsonContent, _jsonOptions)
                ?? new List<ProductEntity>();

        }

        //delete entities
        public async Task DeleteProductEntityAsync(string partitionKey, string rowKey)
        {
            HttpResponseMessage httpResponse = await _httpClient.DeleteAsync($"api/products/delete?partitionKey={partitionKey}&rowKey={rowKey}");
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}
