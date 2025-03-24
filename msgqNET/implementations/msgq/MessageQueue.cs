using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace msgqNET.implementations.msgq;



public class MessageQueue
{
    private const int HeaderSize = 128; // Example size for metadata header (adjust as needed)
    private MemoryMappedFile memoryMappedFile;
    private MemoryMappedViewAccessor accessor;
    private string ShmPath => OperatingSystem.IsLinux() ? "/dev/shm/" : "/tmp/";

    public string Path { get; private set; }
    public int Size { get; private set; }
    public byte[] Data { get; private set; }

    private struct MsgQueueHeader
    {
        public long NumReaders;
        public long WritePointer;
        public long[] ReadPointers;
        public long[] ReadValids;
        public long[] ReadUIDs;

        public MsgQueueHeader(int numReaders)
        {
            NumReaders = 0;
            WritePointer = 0;
            ReadPointers = new long[numReaders];
            ReadValids = new long[numReaders];
            ReadUIDs = new long[numReaders];
        }
    }

    private MsgQueueHeader header;

    public int Initialize(string path, int size)
    {
        if (size < 0 || size > int.MaxValue)
        {
            Console.WriteLine("Size must be between 0 and 2^32-1.");
            return -1;
        }

        try
        {
            Path = path;
            Size = size;

            // Construct full shared memory path
            var fullPath = ShmPath + path; // Adjust for platform-specific paths

            // Create or open the memory-mapped file
            memoryMappedFile = MemoryMappedFile.CreateFromFile(fullPath, FileMode.OpenOrCreate, null, 1024);

            // Create a view accessor for the entire file
            accessor = memoryMappedFile.CreateViewAccessor();

            // Initialize or read the header
            header = ReadHeader();

            // Map the data portion
            Data = new byte[size];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize message queue: {ex.Message}");
            return -1;
        }

        return 0;
    }

    private MsgQueueHeader ReadHeader()
    {
        // Read the header structure from the memory-mapped file
        var headerBytes = new byte[HeaderSize];
        accessor.ReadArray(0, headerBytes, 0, HeaderSize);

        // Deserialize the header
        var handle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
        var result = (MsgQueueHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(MsgQueueHeader));
        handle.Free();

        return result;
    }

    private void WriteHeader()
    {
        // Serialize the header to bytes
        var headerBytes = new byte[HeaderSize];
        var handle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
        Marshal.StructureToPtr(header, handle.AddrOfPinnedObject(), false);
        handle.Free();

        // Write the header back to the memory-mapped file
        accessor.WriteArray(0, headerBytes, 0, HeaderSize);
    }

    public void Close()
    {
        accessor?.Dispose();
        memoryMappedFile?.Dispose();
    }
}