using System.Runtime.InteropServices;

namespace msgqNET;

public static class MessagingConfiguration
{
    public static bool UseZmq()
    {
        var zmqEnv = Environment.GetEnvironmentVariable("ZMQ");
        var mustUseZmq = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        if (string.IsNullOrEmpty(zmqEnv) && !mustUseZmq) return false;
        var openPilotPrefix = Environment.GetEnvironmentVariable("OPENPILOT_PREFIX");

        if (!string.IsNullOrEmpty(openPilotPrefix))
            throw new InvalidOperationException("OPENPILOT_PREFIX not supported with ZMQ backend");

        return true;
    }

    public static bool UseFake() => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CEREAL_FAKE"));
}
