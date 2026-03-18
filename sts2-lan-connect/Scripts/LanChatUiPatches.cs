using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace Sts2LanConnect.Scripts;

internal static class LanChatUiPatches
{
    public static void EnsureLobbyChatPanel(Control anchor)
    {
        EnsureChatPanel(anchor, "lobby");
    }

    public static void EnsureRunChatPanel(Control anchor)
    {
        EnsureChatPanel(anchor, "run");
    }

    private static void EnsureChatPanel(Control anchor, string source)
    {
        Control root = ResolveRoot(anchor);
        if (root.GetNodeOrNull<LanChatPanel>(LanConnectConstants.ChatPanelName) != null)
        {
            return;
        }

        LanChatPanel panel = new();
        root.AddChild(panel);
        root.MoveChild(panel, root.GetChildCount() - 1);
        Log.Info($"sts2_lan_connect attached chat panel via {source}; root={root.GetPath()}");
    }

    private static Control ResolveRoot(Control anchor)
    {
        Control current = anchor;
        while (current.GetParent() is Control parent)
        {
            current = parent;
        }

        return current;
    }
}
