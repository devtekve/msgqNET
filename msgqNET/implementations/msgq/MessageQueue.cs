using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace msgqNET.implementations.msgq;

public class MessageQueueSubscriber : MessageQueue
{
    public int ReaderId { get; private set; }
    public long ReaderUidLocal { get; private set; }
    public bool ReadConflage { get; private set; }

    public void InitSubscriber()
    {
        if (accessor == null)
            throw new InvalidOperationException("Message queue not initialized");

        long uid = GetUid();

        while (true)
        {
            long curNumReaders = header.NumReaders;
            long newNumReaders = curNumReaders + 1;

            // No more slots available. Reset all subscribers to kick out inactive ones
            if (newNumReaders > MaxReaders)
            {
                header.NumReaders = 0;

                for (int i = 0; i < MaxReaders; i++)
                {
                    header.ReadValids[i] = 0; // false

                    long oldUid = header.ReadUIDs[i];
                    header.ReadUIDs[i] = 0;
                    
#if RELEASE
                    // Wake up reader if they are in a poll
                    if (oldUid != 0)
                    {
                        throw new NotImplementedException("ThreadSignal is not implemented because SIGUSR2 handling is not implemented.");
                        // SignalThread((int)(oldUid & 0xFFFFFFFF));
                    }
#endif
                }

                WriteHeader();
                continue;
            }

            // Use atomic compare and exchange to handle race condition
            long originalValue = curNumReaders;
            if (Interlocked.CompareExchange(ref header.NumReaders, newNumReaders, curNumReaders) == originalValue)
            {
                ReaderId = (int)curNumReaders;
                ReaderUidLocal = uid;

                // Start with read_valid = false
                header.ReadValids[ReaderId] = 0; // false
                header.ReadPointers[ReaderId] = 0;
                header.ReadUIDs[ReaderId] = uid;

                WriteHeader();
                break;
            }
        }

        ResetReader();
    }

    private void ResetReader()
    {
        // On first read, sync read pointer with write pointer
        header.ReadValids[ReaderId] = 0; // false
        WriteHeader();
    }

    private static long GetUid() => ((long)Environment.ProcessId << 32) | (uint)Environment.CurrentManagedThreadId;
}

public  class MessageQueue
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

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxReaders)]
        public long[] ReadPointers;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxReaders)]
        public long[] ReadValids;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxReaders)]
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

    protected MsgQueueHeader header;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="size">Size in bytes</param>
    /// <returns></returns>
    public int Initialize(string path, int size)
    {
        if (size < 0)
        {
            Console.WriteLine($"Size must be between 0 and {int.MaxValue}");
            return -1;
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

    protected void WriteHeader()
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
}