using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace Sts2LanConnect.Scripts;

internal sealed class LanChatMessage : INetMessage
{
    public string text = string.Empty;

    public bool ShouldBroadcast => true;

    public NetTransferMode Mode => NetTransferMode.Reliable;

    public LogLevel LogLevel => LogLevel.Debug;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(text);
    }

    public void Deserialize(PacketReader reader)
    {
        text = reader.ReadString();
    }
}
