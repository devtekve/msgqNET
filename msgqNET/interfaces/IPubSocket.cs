namespace msgqNET.interfaces;

public interface IPubSocket
{
    bool Connect(IContext context, string endpoint, bool checkEndpoint = true);
    bool SendMessage(IMessage message);
    bool Send(byte[] data, int size);
    bool AllReadersUpdated();
}