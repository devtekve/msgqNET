using msgqNET.interfaces;
using NetMQ;

namespace msgqNET.implementations.zmq;

public class ZmqMessage : IMessage
{
    public ZmqMessage()
    {
    }

    public ZmqMessage(Msg msg)
    {
        Init(msg);
    }
    
    public ZmqMessage(string data)
    {
        Init(System.Text.Encoding.UTF8.GetBytes(data), data.Length);
    }

    NetMQMessage _netMqMessage = new();

    public void Init(Msg msg)
    {
        _netMqMessage.Clear();
        _netMqMessage.Append(msg.ToArray());
    }

    public void Init(int size)
    {
        _netMqMessage.Clear();
        _netMqMessage.Append(new byte[size]);
    }

    public void Init(byte[] data, int size)
    {
        _netMqMessage.Clear();
        var content = new byte[size];
        Array.Copy(data, content, size);
        _netMqMessage.Append(content);
    }

    public void Close()
    {
        _netMqMessage.Clear();
    }

    public int GetSize()
    {
        return _netMqMessage?.First.BufferSize ?? 0;
    }

    public byte[] GetData()
    {
        return _netMqMessage?.First.ToByteArray() ?? [];
    }
}