using Azure.Storage.Queues;
using MessageStoreFunction.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace MessageStoreFunction.Service
{
    public interface IQueueMessageFunction
    {
        Task SendMessageAsync(QueueMessage message);
        Task<List<QueueMessage>> ReceiveMessagesAsync(int maxMessages = 10, TimeSpan? visibilityTimeout = null);
        Task<List<QueueMessage>> PeekMessagesAsync(int maxMessages = 10);
        Task DeleteMessageAsync(string messageId, string popReceipt);
    }

    public class QueueMessageFunction : IQueueMessageFunction
    {
        private readonly QueueClient _queueClient;

        public QueueMessageFunction(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:ConnectionString"]
                ?? throw new InvalidOperationException("AzureStorage:ConnectionString configuration missing.");
            var queueName = configuration["AzureStorage:QueueName"] ?? "orderqueue";

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        public async Task SendMessageAsync(QueueMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            string json = JsonSerializer.Serialize(message);
            string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            await _queueClient.SendMessageAsync(base64Message);
        }

        public async Task<List<QueueMessage>> ReceiveMessagesAsync(int maxMessages = 10, TimeSpan? visibilityTimeout = null)
        {
            var result = new List<QueueMessage>();
            Azure.Storage.Queues.Models.QueueMessage[] messages = await _queueClient.ReceiveMessagesAsync(
                maxMessages: maxMessages,
                visibilityTimeout: visibilityTimeout ?? TimeSpan.FromSeconds(30));

            foreach (var msg in messages)
            {
                try
                {
                    string json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                    var queueMessage = JsonSerializer.Deserialize<QueueMessage>(json);

                    if (queueMessage != null)
                    {
                        queueMessage.MessageId = msg.MessageId;
                        queueMessage.PopReceipt = msg.PopReceipt;
                        result.Add(queueMessage);
                    }
                }
                catch
                {
                    // Skip malformed messages
                }
            }

            return result;
        }

        public async Task<List<QueueMessage>> PeekMessagesAsync(int maxMessages = 10)
        {
            var result = new List<QueueMessage>();
            Azure.Storage.Queues.Models.PeekedMessage[] messages = await _queueClient.PeekMessagesAsync(maxMessages: maxMessages);

            foreach (var msg in messages)
            {
                try
                {
                    string json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                    var queueMessage = JsonSerializer.Deserialize<QueueMessage>(json);

                    if (queueMessage != null)
                    {
                        queueMessage.MessageId = msg.MessageId;
                        result.Add(queueMessage);
                    }
                }
                catch
                {
                    // Skip malformed messages
                }
            }

            return result;
        }

        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt))
                throw new ArgumentException("MessageId and PopReceipt are required.");

            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }
    }
}