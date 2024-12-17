using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrderFunction
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var orderDetails = JsonSerializer.Deserialize<OrderDetails>(requestBody);

            string? storageConnectionString = Environment.GetEnvironmentVariable("BlobServiceContainer:ConnectionString");
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("BlobServiceContainer:FileContainerName");
            await containerClient.CreateIfNotExistsAsync();
        
            string blobName = $"order-{orderDetails?.OrderId}.json";
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(requestBody);

            return new OkObjectResult("Order request uploaded successfully.");
        }
    }
}
