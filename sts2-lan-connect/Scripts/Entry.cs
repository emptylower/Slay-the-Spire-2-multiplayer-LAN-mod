using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Sts2LanConnect.Scripts;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public static void Init()
    {
        LanConnectConfig.Load();
        LanChatSync.Reset();
        LanPlayerProfileSync.Reset();
        LanConnectRuntimeMonitor.Install();
        Log.Info("sts2_lan_connect initialized with runtime monitor.");
    }
}
