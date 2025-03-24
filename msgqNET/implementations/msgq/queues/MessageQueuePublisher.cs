namespace msgqNET.implementations.msgq.queues;

public class MessageQueuePublisher : MessageQueue
{
    public MessageQueuePublisher(string path, int size) : base(path, size)
    {
        if (accessor == null)
            throw new InvalidOperationException("Message queue not initialized");

        long uid = GetUid();
        Header.WriterUid = uid;
        Header.NumReaders = 0;
        for (var i = 0; i < MaxReaders; i++)
        {
            Header.ReadValids[i] = 0; // false
            Header.ReaderUIDs[i] = 0;
        }

        WriterUidLocal = uid;
        WriteHeader();
    }

    public long WriterUidLocal { get; private set; }
    public bool ReadConflate { get; private set; }
}