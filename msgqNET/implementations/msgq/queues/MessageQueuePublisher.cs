namespace msgqNET.implementations.msgq.queues;

public class MessageQueuePublisher : MessageQueue
{


    public MessageQueuePublisher(string path, int size)
    {
        Initialize(path, size);
        InitPublisher();
    }

    public long WriterUidLocal { get; private set; }
    public bool ReadConflate { get; private set; }

    private void InitPublisher()
    {
        
        if (accessor == null)
            throw new InvalidOperationException("Message queue not initialized");

        long uid = GetUid();
        header.WriterUid = uid;
        header.NumReaders = 0;
        for (var i = 0; i < MaxReaders; i++)
        {
            header.ReadValids[i] = 0; // false
            header.ReaderUIDs[i] = 0;
        }
        
        WriterUidLocal = uid;
        WriteHeader();
    }
}