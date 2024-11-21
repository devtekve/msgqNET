using Microsoft.Extensions.DependencyInjection;
using msgqNET.implementations.fake;
using msgqNET.implementations.zmq;
using msgqNET.interfaces;

namespace msgqNET;

public static class MessagingServiceExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services, 
        Action<MessagingOptions> configureOptions = null)
    {
        // Configure options
        var options = new MessagingOptions();
        configureOptions?.Invoke(options);

        // Register the factory
        services.AddTransient<IMessagingFactory, MessagingFactory>();

        // Register your specific implementations
        // You would replace these with your actual implementations
        services.AddTransient<IContext, ZmqContext>();
        services.AddTransient<ISubSocket, ZmqSubSocket>();
        services.AddTransient<IPubSocket, ZmqPubSocket>();
        services.AddTransient<IPoller, FakePoller>();

        return services;
    }
}

public class MessagingOptions
{
    // Add any configuration options you might need
    public bool SomeConfigurationFlag { get; set; }
}