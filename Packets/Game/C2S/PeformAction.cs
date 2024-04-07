using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.C2S;

[Packet(0x16, Direction.Out, ProtocolState.Play)]
public class PeformAction : IPacket, IPacketSerializer
{
    public Type ActionType { set; private get; }

    public enum Type
    {
        Respawn,
        GetStats,
        FirstOpenInventory
    }

    void IPacketSerializer.Serialize(BinaryStream stream)
    {
        Console.WriteLine(" [PEFORM ACTION] type={0}", ActionType);

        stream.WriteVarInt((int)ActionType);
    }
}
