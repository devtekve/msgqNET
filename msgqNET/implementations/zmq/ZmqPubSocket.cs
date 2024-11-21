using System.Diagnostics;
using msgqNET.interfaces;
using NetMQ;
using NetMQ.Sockets;

namespace msgqNET.implementations.zmq;

public class ZmqPubSocket : IZmqSocket, IPubSocket
{
    private readonly PublisherSocket _socket = new();
    public bool Connect(IContext context, string endpoint, bool checkEndpoint = true)
    {
        var fullEndpoint = checkEndpoint ? $"tcp://*:{IZmqSocket.GetPort(endpoint)}" : $"tcp://*:{endpoint}";
        Console.WriteLine($"Publisher socket binding... [{fullEndpoint}]");
        _socket.Bind(fullEndpoint);
        return true;
    }

    public bool SendMessage(IMessage message)
    {
        
        if (message.GetSize() == 0) return false;
        _socket.SendFrame(message.GetData());
        return true;
    }

    public bool Send(byte[] data, int size)
    {
        _socket.SendFrame(data);
        return true;
    }

    public bool AllReadersUpdated()
    {
        Debug.Assert(false);
        return false;
    }
}