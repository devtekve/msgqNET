using System.Collections.Concurrent;
using msgqNET.interfaces;

namespace msgqNET.implementations.fake;

public class FakeSubSocket : ISubSocket
{
    private static readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> GlobalMessageQueues = new();

    private string _endpoint;
    private string _address;
    private ConcurrentQueue<byte[]> _messageQueue;
    private TimeSpan _timeout = TimeSpan.FromMilliseconds(500); // default timeout
    private bool _disposed = false;

    public bool Connect(IContext context, string endpoint, string address = "127.0.0.1", bool conflate = false, bool checkEndpoint = true)
    {
        _endpoint = endpoint;
        _address = address;
        _messageQueue = GlobalMessageQueues.GetOrAdd(endpoint, _ => new ConcurrentQueue<byte[]>());
        return true;
    }

    public void SetTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
    }

    public IMessage Receive(bool nonBlocking = false)
    {
        if (nonBlocking)
        {
            if (_messageQueue.TryDequeue(out byte[] data))
            {
                var message = new FakeMessage();
                message.Init(data, data.Length);
                return message;
            }

            return null;
        }

        // Blocking receive with timeout
        var startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalMilliseconds < _timeout.TotalMilliseconds)
        {
            if (_messageQueue.TryDequeue(out byte[] data))
            {
                var message = new FakeMessage();
                message.Init(data, data.Length);
                return message;
            }

            Thread.Sleep(10);
        }

        return null;
    }

    public IntPtr GetRawSocket() => IntPtr.Zero;

    // Static method to simulate sending messages for testing
    public static void SimulateSend(string endpoint, byte[] message)
    {
        if (GlobalMessageQueues.TryGetValue(endpoint, out var queue))
        {
            queue.Enqueue(message);
        }
    }
}