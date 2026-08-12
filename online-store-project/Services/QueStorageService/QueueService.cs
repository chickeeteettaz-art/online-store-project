using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text;
using System.Text.Json;
using online_store_project.Models;

namespace online_store_project.Services.QueueServices
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(IConfiguration configuration)
        {
            var connectionString = configuration["AzureStorage:connectionString"];
            var queueName = configuration["AzureStorage:QueueName"] ?? "order-queue";

            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        // ========== SEND ==========
        public async Task SendMessageAsync(Models.QueueMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            string json = JsonSerializer.Serialize(message);
            string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            await _queueClient.SendMessageAsync(base64Message);
        }

        public async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be empty.");

            string base64Message = Convert.ToBase64String(Encoding.UTF8.GetBytes(message));
            await _queueClient.SendMessageAsync(base64Message);
        }

        
        public async Task<List<Models.QueueMessage>> ReceiveMessagesAsync(int maxMessages = 10, TimeSpan? visibilityTimeout = null)
        {
            var result = new List<Models.QueueMessage>();

            Azure.Storage.Queues.Models.QueueMessage[] messages = await _queueClient.ReceiveMessagesAsync(
                maxMessages: maxMessages,
                visibilityTimeout: visibilityTimeout ?? TimeSpan.FromSeconds(30));

            foreach (var msg in messages)
            {
                try
                {
                    string json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                    var queueMessage = JsonSerializer.Deserialize<Models.QueueMessage>(json);

                    if (queueMessage != null)
                    {
                        // Keep the Azure message metadata so we can delete it later
                        queueMessage.MessageId = msg.MessageId;
                        queueMessage.PopReceipt = msg.PopReceipt;   // needed for deletion
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

        // ========== PEEK (look at messages without making them invisible) ==========
        /// <summary>
        /// Peeks at messages without removing or hiding them.
        /// Useful for monitoring / admin views.
        /// </summary>
        public async Task<List<Models.QueueMessage>> PeekMessagesAsync(int maxMessages = 10)
        {
            var result = new List<Models.QueueMessage>();

            PeekedMessage[] messages = await _queueClient.PeekMessagesAsync(maxMessages: maxMessages);

            foreach (var msg in messages)
            {
                try
                {
                    string json = Encoding.UTF8.GetString(Convert.FromBase64String(msg.MessageText));
                    var queueMessage = JsonSerializer.Deserialize<Models.QueueMessage>(json);

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

        // ========== DELETE (after successful processing) ==========
        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt))
                throw new ArgumentException("MessageId and PopReceipt are required.");

            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }
    }
}