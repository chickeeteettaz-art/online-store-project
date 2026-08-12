using Azure.Data.Tables;
using online_store_project.Models;

namespace online_store_project.Services.TableServices
{
    public class TableService
    {
        private readonly TableClient _tableClient;

        public TableService(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("AzureStorage:ConnectionString").Value;
            var tableName = configuration.GetSection("AzureStorage:TableName").Value;
            
            var serviceClient = new TableServiceClient(connectionString);
            
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        //CRUD Operations for CustomerEntity

        // Create or Update a CustomerEntity
        public async Task AddorUpdateCustomerEntity(CustomerEntity customerEntity)
        {
            await _tableClient.UpsertEntityAsync(customerEntity);
        }

        
        //Get a CustomerEntity by PartitionKey and RowKey
        public async Task<CustomerEntity> GetCustomerEntity(string partitionKey, string rowKey)
        {
            var response = await _tableClient.GetEntityAsync<CustomerEntity>(partitionKey, rowKey);
            return response.Value;
        }

        // Return all CustomerEntities
        public async Task<List<CustomerEntity>> GetAllCustomerEntities()
        {
            var entities = new List<CustomerEntity>();
            await foreach (var entity in _tableClient.QueryAsync<CustomerEntity>())
            {
                entities.Add(entity);
            }
            return entities;
        }

        public async Task DeleteCustomerEntity(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }



    }
}
