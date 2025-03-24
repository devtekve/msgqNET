using System.Diagnostics;
using msgqNET.interfaces;

namespace msgqNET.implementations.msgq;

public class MsgqSubSocket : ISubSocket
{
    public bool Connect(IContext context, string endpoint, string address = "127.0.0.1", bool conflate = false, bool checkEndpoint = true)
    {
        Debug.Assert(context is not null);
        Debug.Assert(address == "127.0.0.1");
        
        throw new NotImplementedException();
    }

    public void SetTimeout(TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public IMessage? Receive(bool nonBlocking = false)
    {
        throw new NotImplementedException();
    }

    public IntPtr GetRawSocket()
    {
        throw new NotImplementedException();
    }
}