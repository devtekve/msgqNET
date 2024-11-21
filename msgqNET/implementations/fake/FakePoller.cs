using msgqNET.interfaces;

namespace msgqNET.implementations.fake;

public class FakePoller : IPoller
{
    private List<ISubSocket> _sockets = new();
    private bool _disposed;

    public void RegisterSocket(ISubSocket socket)
    {
        _sockets.Add(socket);
    }

    public IEnumerable<ISubSocket> Poll(int timeout)
    {
        // In this fake implementation, return sockets with messages
        return _sockets.Where(socket => 
        {
            var message = socket.Receive(true);
            return message != null;
        });
    }
    
    public void Dispose()
    {
        _sockets.Clear();
        _disposed = true;
    }
}