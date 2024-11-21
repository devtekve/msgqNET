using msgqNET.interfaces;

namespace msgqNET.implementations.fake;

public class FakePubSocket : IPubSocket
{
    private string _endpoint;
    private bool _disposed = false;

    public bool Connect(IContext context, string endpoint, bool checkEndpoint = true)
    {
        _endpoint = endpoint;
        return true;
    }

    public bool SendMessage(IMessage message)
    {
        if (message == null) return false;
            
        // Send to all subscribers using the static method
        FakeSubSocket.SimulateSend(_endpoint, message.GetData());
        return true;
    }

    public bool Send(byte[] data, int size)
    {
        // Send to all subscribers using the static method
        FakeSubSocket.SimulateSend(_endpoint, data);
        return true;
    }

    public bool AllReadersUpdated()
    {
        // In a fake implementation, always return true
        return true;
    }
}