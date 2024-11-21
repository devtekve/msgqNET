namespace msgqNET.implementations.zmq;

public interface IZmqSocket
{
    protected internal static int GetPort(string endpoint)
    {
        // Create a hash from the endpoint string
        var hashValue = endpoint.GetHashCode();

        // Define the port range
        const int startPort = 8023;
        const int maxPort = 65535;

        // Calculate the port using modulo operation
        var port = startPort + Math.Abs(hashValue % (maxPort - startPort));

        return port;
    }
}