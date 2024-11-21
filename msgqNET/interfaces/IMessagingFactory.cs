namespace msgqNET.interfaces;

public interface IMessagingFactory
{
    IContext CreateContext();
    ISubSocket CreateSubSocket();
    ISubSocket CreateSubSocket(IContext context, string endpoint, string address = "127.0.0.1", bool conflate = false, bool checkEndpoint = true);
    IPubSocket CreatePubSocket();
    IPubSocket CreatePubSocket(IContext context, string endpoint, bool checkEndpoint = true);
    IPoller CreatePoller();
    IPoller CreatePoller(IEnumerable<ISubSocket> sockets);
}