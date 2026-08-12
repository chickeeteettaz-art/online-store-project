using Microsoft.AspNetCore.Mvc;
using online_store_project.Models;
using online_store_project.Services.QueueServices;

namespace online_store_project.Controllers
{
    public class QueuesController : Controller
    {
        private readonly QueueService _queueService;

        public QueuesController(QueueService queueService)
        {
            _queueService = queueService;
        }

        // GET: /Queue
        // Shows messages currently in the queue (using Peek – does not remove them)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var messages = await _queueService.PeekMessagesAsync(maxMessages: 32);
            return View(messages);
        }

        // GET: /Queue/Send
        [HttpGet]
        public IActionResult Send()
        {
            return View(new QueueMessage());
        }

        // POST: /Queue/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(QueueMessage message)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(message);

                message.CreatedAt = DateTime.UtcNow;
                await _queueService.SendMessageAsync(message);

                TempData["SuccessMessage"] = "Message successfully sent to the queue.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to send message: {ex.Message}");
                return View(message);
            }
        }

        // POST: /Queue/Receive
        // Receives messages (makes them invisible) and shows them so the admin can process/delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive()
        {
            try
            {
                var messages = await _queueService.ReceiveMessagesAsync(maxMessages: 10);
                return View("Received", messages);   // a separate view to process received messages
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not receive messages: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Queue/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string messageId, string popReceipt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(popReceipt))
                {
                    TempData["ErrorMessage"] = "MessageId and PopReceipt are required.";
                    return RedirectToAction(nameof(Index));
                }

                await _queueService.DeleteMessageAsync(messageId, popReceipt);
                TempData["SuccessMessage"] = "Message deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not delete message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Optional: Quick test endpoint that sends a sample message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTestMessage()
        {
            var testMessage = new QueueMessage
            {
                Type = "Test",
                OrderId = Guid.NewGuid().ToString()[..8],
                CustomerId = "TEST-CUSTOMER",
                ProductId = "TEST-PRODUCT",
                Quantity = 1,
                TotalPrice = 100,
                AdditionalInfo = "This is a test message"
            };

            await _queueService.SendMessageAsync(testMessage);
            TempData["SuccessMessage"] = "Test message sent to the queue.";
            return RedirectToAction(nameof(Index));
        }
    }
}