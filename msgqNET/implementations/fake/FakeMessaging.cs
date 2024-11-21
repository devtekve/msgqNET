using msgqNET.interfaces;

namespace msgqNET.implementations.fake;

public static class FakeMessaging
{
    public static IContext CreateContext()
    {
        return new FakeContext();
    }

    public static ISubSocket CreateSubSocket()
    {
        return new FakeSubSocket();
    }

    public static IPubSocket CreatePubSocket()
    {
        return new FakePubSocket();
    }

    public static IPoller CreatePoller()
    {
        return new FakePoller();
    }
}