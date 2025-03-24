using System.Diagnostics;
using msgqNET.implementations.msgq.queues;
using msgqNET.interfaces;

namespace msgqNET.implementations.msgq;

public class MsgqSubSocket : ISubSocket
{
    private MessageQueueSubscriber _socket;
    private const int size = 10486144;
    public bool Connect(IContext context, string endpoint, string address = "127.0.0.1", bool conflate = false, bool checkEndpoint = true)
    {
        Debug.Assert(context is not null);
        Debug.Assert(address == "127.0.0.1");
        
        _socket = new MessageQueueSubscriber(endpoint, size);
        return true;
    }

    public void SetTimeout(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public IMessage? Receive(bool nonBlocking = false)
    {
        var x = _socket.Data; //DUMMY EXAMPLE NOT YET IMPLEMENTED
        throw new NotImplementedException();
    }

    public IntPtr GetRawSocket()
    {
        throw new NotImplementedException();
    }
}