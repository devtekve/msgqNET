using msgqNET.interfaces;
using NetMQ;
using NetMQ.Sockets;

namespace msgqNET.implementations.zmq;

public class ZmqSubSocket : IZmqSocket, ISubSocket
{
    private readonly SubscriberSocket _socket = new();
    private TimeSpan _timeout = TimeSpan.FromMilliseconds(500);

    public bool Connect(IContext context, string endpoint, string address = "127.0.0.1", bool conflate = false, bool checkEndpoint = true)
    {
        var fullEndpoint = checkEndpoint ? $"tcp://{address}:{IZmqSocket.GetPort(endpoint)}" : $"tcp://{address}:{endpoint}";
        Console.WriteLine($"Connecting to {fullEndpoint}");
        _socket.Connect(fullEndpoint);
        _socket.SubscribeToAnyTopic();
        return true;
    }

    public void SetTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
    }

    public IMessage? Receive(bool nonBlocking = false)
    {
        var bufferMsg = new Msg();
        bufferMsg.InitEmpty();
        return _socket.TryReceive(ref bufferMsg, nonBlocking ? TimeSpan.Zero : _timeout) ? new ZmqMessage(bufferMsg) : null;
    }

    public IntPtr GetRawSocket()
    {
        throw new NotImplementedException();
    }
}