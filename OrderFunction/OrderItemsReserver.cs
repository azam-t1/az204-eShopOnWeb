using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OrderFunction;

public class OrderItemsReserver(ILogger<OrderItemsReserver> logger)
{
    [Function("OrderItemsReserver")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
    {
        logger.LogInformation("OrderItemsReserver function processed a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var orderDetails = JsonSerializer.Deserialize<OrderDetails>(requestBody);

        string? storageConnectionString = Environment.GetEnvironmentVariable("BlobServiceContainer_ConnectionString");
        BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("BlobServiceContainer_FileContainerName");
        await containerClient.CreateIfNotExistsAsync();
        
        string blobName = $"order-{orderDetails?.OrderId}.json";
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(requestBody);

        return new OkObjectResult("Order request uploaded successfully.");
    }
}

public class OrderDetails
{
    public int OrderId { get; set; }
    public List<OrderItem> Items { get; set; }
}

public class OrderItem
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}
