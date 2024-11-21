using msgqNET.interfaces;
using static System.GC;

namespace msgqNET.implementations.fake;

public class FakeContext : IContext
{
    private bool _disposed = false;
    private Guid _contextId = Guid.NewGuid();

    public IntPtr GetRawContext() => IntPtr.Zero;
}
