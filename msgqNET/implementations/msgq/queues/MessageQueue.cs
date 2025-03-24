using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace msgqNET.implementations.msgq.queues;

public abstract class MessageQueue
{
    private static readonly int HeaderSize = Marshal.SizeOf<MsgQueueHeader>();
    protected const int MaxReaders = 15;
    private MemoryMappedFile memoryMappedFile;
    protected MemoryMappedViewAccessor accessor;
    private string ShmPath => OperatingSystem.IsLinux() ? "/dev/shm/" : "/tmp/";

    public string Path { get; private set; }
    public int Size { get; private set; }
    public byte[] Data { get; private set; }

    protected struct MsgQueueHeader
    {
        public long NumReaders;
        public long WritePointer;
        public long WriterUid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxReaders)]
        public long[] ReadPointers;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxReaders)]
        public long[] ReadValids;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxReaders)]
        public long[] ReaderUIDs;

        public MsgQueueHeader(int numReaders)
        {
            NumReaders = 0;
            WritePointer = 0;
            ReadPointers = new long[numReaders];
            ReadValids = new long[numReaders];
            ReaderUIDs = new long[numReaders];
        }
    }

    protected MsgQueueHeader Header;

    protected MessageQueue(string path, int size)
    {
        if (size < 0)
        {
            Console.WriteLine($"Size must be between 0 and {int.MaxValue}");
        }

#if RELEASE
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            RegisterSigUsr2Handler();
#endif

        try
        {
            Path = path;
            Size = size;

            // Construct full shared memory path
            var fullPath = ShmPath + path; // Adjust for platform-specific paths

            // Create or open the memory-mapped file
            memoryMappedFile = MemoryMappedFile.CreateFromFile(fullPath, FileMode.OpenOrCreate, null, size);

            // Create a view accessor for the entire file
            accessor = memoryMappedFile.CreateViewAccessor();

            // Initialize or read the header
            Header = ReadHeader();

            // Map the data portion
            Data = new byte[size];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize message queue: {ex.Message}");
        }
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

    protected void WriteHeader()
    {
        // Serialize the header to bytes
        var headerBytes = new byte[HeaderSize];
        var handle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
        Marshal.StructureToPtr(Header, handle.AddrOfPinnedObject(), false);
        handle.Free();

        // Write the header back to the memory-mapped file
        accessor.WriteArray(0, headerBytes, 0, HeaderSize);
    }

    public void Close()
    {
        accessor?.Dispose();
        memoryMappedFile?.Dispose();
    }

    /// <summary>
    /// Registers a handler for the SIGUSR2 signal.
    /// </summary>
    [UnsupportedOSPlatform(nameof(OSPlatform.Windows))]
    public static void RegisterSigUsr2Handler()
    {
        throw new NotImplementedException("sigusr2 handling is yet implemented. Maybe .net doesn't support it.");
        // PosixSignalRegistration.Create(PosixSignal.SIGUSR2, context =>
        // {
        //     Console.WriteLine("SIGUSR2 received. Handling the signal...");
        //     // Implement your signal handling logic here
        //     context.Cancel = true; // Prevents default termination
        // });
    }

    protected static long GetUid() => ((long)Environment.ProcessId << 32) | (uint)Environment.CurrentManagedThreadId;
}