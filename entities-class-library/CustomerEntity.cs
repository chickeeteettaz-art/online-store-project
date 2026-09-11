using Azure;
using Azure.Data.Tables;

namespace entities_class_library
{
    public class CustomerEntity:ITableEntity 
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
