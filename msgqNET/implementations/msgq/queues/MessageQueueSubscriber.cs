namespace msgqNET.implementations.msgq.queues;

public class MessageQueueSubscriber : MessageQueue
{
    private static readonly long Uid = GetUid();
    private long ReaderId { get; set; }
    private bool ReadConflate { get; set; }

    // Expose the underlying buffer and pointers through properties.
    // Assume Header is an object that holds the shared state.
    private long WritePointer => Header.WritePointer;

    private long ReadPointer
    {
        get => Header.ReadPointers[ReaderId];
        set => Header.ReadPointers[ReaderId] = value;
    }

    private bool ReadValid
    {
        get => Header.ReadValids[ReaderId] != 0;
        set => Header.ReadValids[ReaderId] = value ? 1 : 0;
    }

    private long ReaderUid
    {
        get => Header.ReaderUIDs[ReaderId];
        set => Header.ReaderUIDs[ReaderId] = value;
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

    public MsgqMessage? Read(bool nonBlocking = false)
    {
        do
        {
            lock (_syncLock)
            {
                // Read the message
                var readPointer = new Packed64(ReadPointer);
                var writePointer = new Packed64(WritePointer);

                if (readPointer == writePointer)
                {
                    Console.WriteLine($"No message to read because read pointer [{ReadPointer}] is equal to write pointer [{WritePointer}].");
                    continue;
                }

                // Get size
                var size = BitConverter.ToInt64(Data, readPointer.ReadPointer);

                // Check if valid
                if (!ReadValid)
                {
                    Console.WriteLine("Read pointer is invalid. Resetting reader.");
                    ResetReader();
                    continue;
                }

                if (size == -1)
                {
                    Console.WriteLine("Size is -1. Skipping.");
                    readPointer.CycleCounter++;
                    continue;
                }

                // // crashing is better than passing garbage data to the consumer
                // // the size will have weird value if it was overwritten by data accidentally
                // assert((uint64_t)size < q->size);
                // assert(size > 0);

                // uint32_t new_read_pointer = ALIGN(read_pointer + sizeof(std::int64_t) + size);
                var newReadPointer = new Packed64(readPointer.ReadPointer + sizeof(long) + size);
                if (ReadConflate && writePointer != newReadPointer)
                {
                    Console.WriteLine("Read conflate is enabled. Skipping.");
                    ReadPointer = newReadPointer.Value;
                    continue;
                }

                var spn = new Span<byte>(Data, readPointer.ReadPointer + sizeof(long), (int)size);
                var msg = new MsgqMessage(spn.ToArray(), size);

                ReadPointer = newReadPointer.Value;
                if (ReadValid) return msg;

                msg.Close();
                ResetReader();
            }
        } while (!nonBlocking);

        return null;
    }
}