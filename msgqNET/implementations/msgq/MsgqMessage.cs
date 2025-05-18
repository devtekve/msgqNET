namespace msgqNET.implementations.msgq;

public class MsgqMessage : IMsgqMessage
{
    private long _size;
    private byte[] _data;

    // Constructor to initialize with size
    public MsgqMessage(long size)
    {
        _size = size;
        _data = new byte[size];
    }

    // Constructor to initialize with data and size
    public MsgqMessage(byte[] data, long size)
    {
        _size = size;
        _data = new byte[size];
        Array.Copy(data, _data, size);
    }

    public void TakeOwnership(byte[] data, long size)
    {
        _size = size;
        _data = data;
    }

    public void Close()
    {
        // Release the data and reset size
        _data = [];
        _size = 0;
    }

    public long GetSize() => _size;

    public byte[] GetData() => _data;
}