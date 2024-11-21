using msgqNET.interfaces;

namespace msgqNET.implementations.fake;

public class FakeMessage : IMessage
{
    private byte[] _data = [];

    public void Init(int size) => _data = new byte[size];

    public void Init(byte[] data, int size)
    {
        _data = new byte[size];
        Array.Copy(data, _data, size);
    }

    public void Close() => _data = [];

    public int GetSize() => _data.Length;

    public byte[] GetData() => _data;

    public void Dispose()
    {
        Close();
    }
}