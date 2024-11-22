namespace msgqNET.implementations.zmq;

public interface IZmqSocket
{
    private static ulong Fnv1AHash(string str)
    {
        const ulong fnvPrime = 0x100000001b3UL;
        var hashValue = 0xcbf29ce484222325UL;
    
        foreach (var c in str)
        {
            hashValue ^= (byte)c;
            hashValue *= fnvPrime;
        }
    
        return hashValue;
    }

    protected internal static int GetPort(string endpoint)
    {
        var hashValue = Fnv1AHash(endpoint);

        // Define the port range
        const int startPort = 8023;
        const int maxPort = 65535;

        // Map the hash value to the valid port range
        return startPort + (int)(hashValue % (maxPort - startPort));
    }
}