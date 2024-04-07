using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.C2S;

[Packet(0x01, Direction.Out, ProtocolState.Play)]
public class SendChat : IPacket, IPacketSerializer
{
    public string Message { private get; set; }

    void IPacketSerializer.Serialize(BinaryStream stream)
    {
        stream.WriteString(Message);
    }
}