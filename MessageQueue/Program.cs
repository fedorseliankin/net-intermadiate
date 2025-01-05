using MessageQueue;
public class Program
{
    public static async Task Main(string[] args)
    {
        var consumer = new RabbitMqConsumer();
        Console.WriteLine("Consumer running. Press [enter] to exit.");

        // Application runs until the user stops it manually
        Console.ReadLine();
    }
}