using MessageStoreFunction.Models;
using MessageStoreFunction.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

namespace MessageStoreFunction.Functions
{
    public class QueueEndpoints
    {
        private readonly IQueueMessageFunction _queueService;

        public QueueEndpoints(IQueueMessageFunction queueService)
        {
            _queueService = queueService;
        }

        [Function("SendMessage")]
        public async Task<IActionResult> Send(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "queue/send")] HttpRequest req)
        {
            try
            {
                // Read the JSON payload from the request body stream
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestObjectResult("Request body cannot be empty.");
                }

                // Deserialize into your QueueMessage model
                var message = JsonSerializer.Deserialize<QueueMessage>(requestBody);

                if (message == null)
                {
                    return new BadRequestObjectResult("Invalid queue message payload.");
                }

                await _queueService.SendMessageAsync(message);
                return new OkObjectResult(new { Status = "Message queued successfully" });
            }
            catch (JsonException ex)
            {
                return new BadRequestObjectResult($"JSON deserialization error: {ex.Message}");
            }
        }

        [Function("ReceiveMessages")]
        public async Task<IActionResult> Receive(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queue/receive")] HttpRequest req)
        {
            int maxMessages = int.TryParse(req.Query["maxMessages"], out int max) ? max : 10;
            var messages = await _queueService.ReceiveMessagesAsync(maxMessages);
            return new OkObjectResult(messages);
        }

        [Function("PeekMessages")]
        public async Task<IActionResult> Peek(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queue/peek")] HttpRequest req)
        {
            int maxMessages = int.TryParse(req.Query["maxMessages"], out int max) ? max : 10;
            var messages = await _queueService.PeekMessagesAsync(maxMessages);
            return new OkObjectResult(messages);
        }

        [Function("DeleteMessage")]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "queue/delete")] HttpRequest req)
        {
            string? messageId = req.Query["messageId"];
            string? popReceipt = req.Query["popReceipt"];

            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt))
                return new BadRequestObjectResult("Both 'messageId' and 'popReceipt' are required.");

            await _queueService.DeleteMessageAsync(messageId, popReceipt);
            return new OkResult();
        }
    }
}