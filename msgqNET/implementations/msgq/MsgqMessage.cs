namespace msgqNET.implementations.msgq;

public class MsgqMessage : IMsgqMessage
{
    private int _size;
    private byte[] _data;

    // Constructor to initialize with size
    public MsgqMessage(int size)
    {
        _size = size;
        _data = new byte[size];
    }

    // Constructor to initialize with data and size
    public MsgqMessage(byte[] data, int size)
    {
        _size = size;
        _data = new byte[size];
        Array.Copy(data, _data, size);
    }

    public void TakeOwnership(byte[] data, int size)
    {
        _size = size;
        _data = data;
    }

    public int GetSize() => _size;

    public byte[] GetData() => _data;
}