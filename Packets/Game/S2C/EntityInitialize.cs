using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x14, Direction.In, ProtocolState.Play)]
internal class EntityInitialize : IPacket, IPacketDeserializer
{
    public int EntityId { get; private set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        EntityId = stream.ReadVarInt();
    }
}
