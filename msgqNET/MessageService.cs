using msgqNET.interfaces;

namespace msgqNET;

public class MessageService(IMessagingFactory messagingFactory)
{
    public void ProcessMessages()
    {
        // Create context
        var context = messagingFactory.CreateContext();
        
        // Create and connect a sub socket
        var subSocket = messagingFactory.CreateSubSocket(
            context, 
            "example-endpoint"
        );

        // Create a poller
        var poller = messagingFactory.CreatePoller();
        poller.RegisterSocket(subSocket);
    }
}