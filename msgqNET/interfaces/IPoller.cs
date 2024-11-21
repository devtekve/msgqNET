namespace msgqNET.interfaces;

public interface IPoller
{
    void RegisterSocket(ISubSocket socket);
    IEnumerable<ISubSocket> Poll(int timeout);
}