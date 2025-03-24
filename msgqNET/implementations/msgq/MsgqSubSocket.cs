using System.Diagnostics;
using msgqNET.implementations.msgq.queues;
using msgqNET.interfaces;

namespace msgqNET.implementations.msgq;

public class MsgqSubSocket : ISubSocket
{
    private MessageQueueSubscriber? _queue;
    private const int Size = 10486144;
    private TimeSpan _timeout = TimeSpan.FromMilliseconds(500);

    public bool Connect(IContext context, string endpoint, string address = "127.0.0.1", bool conflate = false, bool checkEndpoint = true)
    {
        Debug.Assert(context is not null);
        Debug.Assert(address == "127.0.0.1");

        _queue = new MessageQueueSubscriber(endpoint, Size);
        return true;
    }

    public void SetTimeout(TimeSpan timeout) => _timeout = timeout;

    public IMessage? Receive(bool nonBlocking = false)
    {
        throw new NotImplementedException();
    }

    public IntPtr GetRawSocket()
    {
        throw new NotImplementedException();
    }
}