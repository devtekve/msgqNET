using Microsoft.Extensions.DependencyInjection;
using msgqNET;
using msgqNET.implementations.msgq.queues;
using msgqNET.interfaces;
using TestApp;

// Set up the dependency injection container

internal class Program
{
    public static void Main(string endpoint = "backupManagerSP")
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IEventProcessor, EventProcessor>()
            .AddMessaging()
            .BuildServiceProvider();

// Get the event processor from the service provider
        var messagingFactory = serviceProvider.GetRequiredService<IMessagingFactory>();
        var context = messagingFactory.CreateContext();


        var subsocket = messagingFactory.CreateSubSocket(context, endpoint, "127.0.0.1");
        subsocket.SetTimeout(TimeSpan.FromSeconds(30));
//
        long counter = 0;
// Event processing loop
        while (true)
        {
            Console.WriteLine($"Processing event {counter++}");
            // pubsocket.SendMessage(new FakeMessage());
            // await Task.Delay(1000); // Sleep for 1 second
            var x = subsocket.Receive();
            HexDumper.PrintHexDump(x?.GetData() ?? []);
            Console.WriteLine();
            Thread.Sleep(1);
        }




// var pubsocket = messagingFactory.CreatePubSocket(context, "example-endpoint");
// var subsocket = messagingFactory.CreateSubSocket(context, "example-endpoint");
//
// var publisherTask = Task.Run(async () =>
// {
//     long counter = 0;
//     // Event processing loop
//     while (true)
//     {
//         pubsocket.SendMessage(new ZmqMessage($"hello world [{counter++}]"));
//         await Task.Delay(3000);
//     }
// });
//
//
// subsocket.SetTimeout(TimeSpan.FromSeconds(5));
// while (true)
// {
//     HexDumper.PrintHexDump(subsocket.Receive()?.GetData() ?? []);
// }

        var pubqueue = new MessageQueuePublisher("backupManagerSP", 10486144);
        var queue = new MessageQueueSubscriber("backupManagerSP", 10486144);
        var queue2 = new MessageQueueSubscriber("backupManagerSP", 10486144);

// Perform operations...

        pubqueue.Close();
        queue.Close();
        queue2.Close();
    }
}


