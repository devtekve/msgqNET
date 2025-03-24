namespace msgqNET.implementations.msgq.queues;

public class MessageQueueSubscriber : MessageQueue
{
    private static readonly long Uid = GetUid();
    public long ReaderId { get; private set; }
    public bool ReadConflate { get; private set; }

    // Expose the underlying buffer and pointers through properties.
    // Assume Header is an object that holds the shared state.
    public long WritePointer => Header.WritePointer;

    public long ReadPointer
    {
        get => Header.ReadPointers[ReaderId];
        private set => Header.ReadPointers[ReaderId] = value;
    }
    
    public bool ReadValid
    {
        get => Header.ReadValids[ReaderId] != 0;
        private set => Header.ReadValids[ReaderId] = value ? 1 : 0;
    }
    
    public long ReaderUid
    {
        get => Header.ReaderUIDs[ReaderId];
        private set => Header.ReaderUIDs[ReaderId] = value;
    }

    private readonly object _syncLock = new();

    public MessageQueueSubscriber(string path, int size) : base(path, size) => InitializeSubscriber();

    private void InitializeSubscriber()
    {
        if (accessor == null)
            throw new InvalidOperationException("Message queue not initialized");
        
        while (true)
        {
            var curNumReaders = Header.NumReaders;
            var newNumReaders = curNumReaders + 1;

            // No more slots available. Reset all subscribers to kick out inactive ones
            if (newNumReaders > MaxReaders)
            {
                KickAllReaders();
                continue;
            }

            // Use atomic compare and exchange to handle race condition
            if (Interlocked.CompareExchange(ref Header.NumReaders, newNumReaders, curNumReaders) != curNumReaders)
                continue;

            ReaderId = curNumReaders;
            // Start with read_valid = false
            ReadValid = false; // false
            ReadPointer = 0;
            ReaderUid = Uid;

            WriteHeader();
            break;
        }

        ResetReader();
    }

    /// <summary>
    /// This method feels like madness. I am a subscriber. Why am I writing to the header (other than my own fields)?
    /// </summary>
    private void KickAllReaders()
    {
        Header.NumReaders = 0;

        for (var i = 0; i < MaxReaders; i++)
        {
            Header.ReadValids[i] = 0; // false
            Header.ReaderUIDs[i] = 0;

            var oldUid = Header.ReaderUIDs[i];

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
    }


    public void ResetReader()
    {
        lock (_syncLock)
        {
            // Sync read pointer with write pointer on reset
            Header.ReadPointers[ReaderId] = Header.WritePointer;
            Header.ReadValids[ReaderId] = 0; // mark as invalid, to be revalidated externally if needed
            WriteHeader();
        }
    }

    // private static T Align<T>(T value) where T : INumber<T>, IBitwiseOperators<T, T, T>
    // {
    //     // Constants
    //     var seven = T.CreateChecked(7);
    //     // Use the bitwise operator directly through the ~ operator
    //     var mask = ~seven;
    //
    //     // (value + 7) & ~7
    //     return (value + seven) & mask;
    // }
    
    // private IMsgqMessage? ReadMessage()
    // {
    //     while (ReaderUid != Uid)
    //     {
    //         Console.WriteLine("Reader was evicted. Reconnecting...");
    //         InitializeSubscriber();
    //     }
    //
    //     while (!ReadValid)
    //     {
    //         Console.WriteLine("Reader is not valid. Resetting...");
    //         ResetReader();
    //     }
    //    
    //     
    // }
}