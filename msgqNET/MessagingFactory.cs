using Microsoft.Extensions.DependencyInjection;
using msgqNET.interfaces;

namespace msgqNET;

public class MessagingFactory(IServiceProvider serviceProvider) : IMessagingFactory
{
    public IContext CreateContext()
    {
        return serviceProvider.GetRequiredService<IContext>();
    }

    public ISubSocket CreateSubSocket()
    {
        return serviceProvider.GetRequiredService<ISubSocket>();
    }

    public ISubSocket CreateSubSocket(
        IContext context,
        string endpoint,
        string address = "127.0.0.1",
        bool conflate = false,
        bool checkEndpoint = true)
    {
        var socket = CreateSubSocket();
        var result = socket.Connect(context, endpoint, address, conflate, checkEndpoint);

        return result ? socket : throw new Exception($"Failed to connect SubSocket to {endpoint}");
    }

    public IPubSocket CreatePubSocket()
    {
        return serviceProvider.GetRequiredService<IPubSocket>();
    }

    public IPubSocket CreatePubSocket(
        IContext context,
        string endpoint,
        bool checkEndpoint = true)
    {
        var socket = CreatePubSocket();
        var result = socket.Connect(context, endpoint, checkEndpoint);

        return result ? socket : throw new Exception($"Failed to bind PubSocket to {endpoint}");
    }

    public IPoller CreatePoller()
    {
        return serviceProvider.GetRequiredService<IPoller>();
    }

    public IPoller CreatePoller(IEnumerable<ISubSocket> sockets)
    {
        var poller = CreatePoller();

        foreach (var socket in sockets)
        {
            poller.RegisterSocket(socket);
        }

        return poller;
    }
}