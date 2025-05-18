namespace msgqNET.interfaces;

public interface IMessage
{
    // void Init(int size);
    // void Init(byte[] data, int size);
    void Close();
    long GetSize();
    byte[] GetData();
}