using Azure.Data.Tables;
using online_store_project.Models;
using System.Buffers.Text;
using System.Text.Json;

namespace online_store_project.Services.TableServices
{
    public class TableService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public TableService(IConfiguration configuration,HttpClient httpClient)
        {
            _httpClient = httpClient;
            
            var baseUrl = configuration["AzureFunctions:UserTableBaseUrl"]
                ?? throw new InvalidOperationException("AzureFunctions:UserTableBaseUrl configuration missing.");
            _httpClient.BaseAddress = new Uri(baseUrl);
            _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        }

        //CRUD Operations for CustomerEntity

        // Create or Update a CustomerEntity
        public async Task AddorUpdateCustomerEntity(CustomerEntity customerEntity)
        {
            try
            {
                if(customerEntity == null)
                    throw new ArgumentNullException(nameof(customerEntity),"Customer cannot be null.");

                //seriealize the customerEntity to JSON
                var json = JsonSerializer.Serialize(customerEntity, _jsonSerializerOptions);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/customers/create", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding or updating CustomerEntity: " + ex.Message);
            }

        }
        //Get a CustomerEntity by PartitionKey and RowKey
        public async Task<CustomerEntity> GetCustomerEntity(string partitionKey, string rowKey)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/customers/get?partitionKey={partitionKey}&rowKey={rowKey}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerEntity>(_jsonSerializerOptions)
                ?? throw new Exception("Failed to deserialize CustomerEntity.");
        }

        // Return all CustomerEntities
        public async Task<List<CustomerEntity>> GetAllCustomerEntities()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/customers/getall");
            response.EnsureSuccessStatusCode();

            var customerEntities = await response.Content.ReadFromJsonAsync<List<CustomerEntity>>(_jsonSerializerOptions);
            return customerEntities ?? new List<CustomerEntity>();
        }

        public async Task DeleteCustomerEntity(string partitionKey, string rowKey)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/customers/delete?partitionKey={partitionKey}&rowKey={rowKey}");
            response.EnsureSuccessStatusCode();
        }



    }
}
