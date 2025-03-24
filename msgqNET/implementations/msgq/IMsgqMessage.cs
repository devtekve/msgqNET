using msgqNET.interfaces;

namespace msgqNET.implementations.msgq;

public interface IMsgqMessage : IMessage
{
    void TakeOwnership(byte[] data, long size);
}