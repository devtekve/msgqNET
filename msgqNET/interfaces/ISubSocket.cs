namespace msgqNET.interfaces;

public interface ISubSocket
{
    bool Connect(IContext context, string endpoint, string address = "127.0.0.1", bool conflate = false, bool checkEndpoint = true);
    void SetTimeout(TimeSpan timeout);
    IMessage? Receive(bool nonBlocking = false);
    IntPtr GetRawSocket();
}