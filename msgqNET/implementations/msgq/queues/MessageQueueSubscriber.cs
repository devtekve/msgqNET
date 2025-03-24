namespace msgqNET.implementations.msgq.queues;

public class MessageQueueSubscriber : MessageQueue
{
    public int ReaderId { get; private set; }
    public long ReaderUidLocal { get; private set; }
    public bool ReadConflate { get; private set; }
    
    public MessageQueueSubscriber(string path, int size)
    {
        Initialize(path, size);
        InitSubscriber();
    }

    private void InitSubscriber()
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

                    long oldUid = header.ReaderUIDs[i];
                    header.ReaderUIDs[i] = 0;
                    
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
                header.ReaderUIDs[ReaderId] = uid;

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
}