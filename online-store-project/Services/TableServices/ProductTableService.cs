using Azure.Data.Tables;
using online_store_project.Models;

namespace online_store_project.Services.TableServices
{
    public class ProductTableService
    {
        private readonly TableClient _tableClient;

        public ProductTableService(IConfiguration configuration)
        {
            //getting the connection string and table name from appsettings.json
            var connectionString = configuration.GetSection("AzureStorage:ConnectionString").Value;
            var tableName = configuration.GetSection("AzureStorage:ProductTableName").Value;

            //initializing the TableServiceClient and TableClient
            var serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        //CRUD Operations for ProductEntity
        public async Task AddorUpdateProductEntityAsync(ProductEntity product)
        {
            await _tableClient.UpsertEntityAsync(product);
        }

        //GET a single product entity by partition key and row key
        public async Task<ProductEntity> GetProductEntityAsync(string partitionKey, string rowKey)
        {
            var response = await _tableClient.GetEntityAsync<ProductEntity>(partitionKey, rowKey);
            return response.Value;
        }

        public async Task<List<ProductEntity>> GetAllProductEntitiesAsync()
        {
            var entities = new List<ProductEntity>();
            await foreach (var entity in _tableClient.QueryAsync<ProductEntity>())
            {
                entities.Add(entity);
            }
            return entities;
        }

        //delete entities
        public async Task DeleteProductEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
