using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game;

[Packet(0x00, Direction.Both, ProtocolState.Play)]
public class KeepAlive : IPacket, IPacketDeserializer, IPacketSerializer
{
    public int QueryId { get; set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        QueryId = stream.ReadVarInt();
    }

    void IPacketSerializer.Serialize(BinaryStream stream)
    {
        stream.WriteVarInt(QueryId);
    }
}