using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

public class Program
{
    static async Task Main(string[] args)
    {
        var tasks = new List<Task<HttpResponseMessage>>();
        var httpClient = new HttpClient();
        for (int i = 0; i < 10; i++)
        {
            var CartId = "1230303" + i.ToString();
            var body = new
            {
                EventId = "example",
                SeatId = "seat-id-12",
                PriceOptionId  = "1",
                CartId,
            };
            tasks.Add(httpClient.PostAsync(
                "https://localhost:7154/api/orders/carts/1230303", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")));
        }

        var responses = await Task.WhenAll(tasks);

        var successCount = responses.Count(response => response.IsSuccessStatusCode);
        Console.WriteLine($"Number of successful responses: {successCount}");
    }
}