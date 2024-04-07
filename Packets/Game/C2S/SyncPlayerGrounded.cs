using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.C2S;

[Packet(0x03, Direction.Out, ProtocolState.Play)]
public class SyncPlayerGrounded : IPacket, IPacketSerializer
{
    public bool Grounded { private get; set; }

    void IPacketSerializer.Serialize(BinaryStream stream)
    {
        OnSerialize(stream);
        stream.WriteBoolean(Grounded);
    }

    protected virtual void OnSerialize(BinaryStream stream)
    {

    }
}