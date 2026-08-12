using Azure.Data.Tables;
using online_store_project.Models;

namespace online_store_project.Services.TableServices
{
    public class OrderTableService
    {
        private readonly TableClient _tableClient;

        public OrderTableService(IConfiguration configuration)
        {
            //getting the connection string and table name from appsettings.json
            var connectionString = configuration.GetSection("AzureStorage:ConnectionString").Value;
            var tableName = configuration.GetSection("AzureStorage:OrderTableName").Value;

            //initializing the TableServiceClient and TableClient
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        //CRUD Operations for OrderEntity
        public async Task AddorUpdateOrderEntityAsync(OrderEntity order)
        {
            await _tableClient.UpsertEntityAsync(order);
        }

        //GET a single order entity by partition key and row key
        public async Task<OrderEntity> GetOrderEntityAsync(string partitionKey, string rowKey)
        {
            var response = await _tableClient.GetEntityAsync<OrderEntity>(partitionKey, rowKey);
            return response.Value;
        }

        public async Task<List<OrderEntity>> GetAllOrderEntitiesAsync()
        {
            var entities = new List<OrderEntity>();
            await foreach (var entity in _tableClient.QueryAsync<OrderEntity>())
            {
                entities.Add(entity);
            }
            return entities;
        }

        //delete entities
        public async Task DeleteOrderEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
