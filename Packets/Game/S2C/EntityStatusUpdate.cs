using Minecraft.Abstractions;
using Minecraft.Entities;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x1A, Direction.In, ProtocolState.Play)]
public class EntityStatusUpdate : IPacket, IPacketDeserializer
{
    public int EntityId { get; private set; }
    public EntityStatusType Status { get; private set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        EntityId = stream.ReadInt32();
        Status = (EntityStatusType)stream.ReadByte();

        Console.WriteLine(" [ENTITY STATUS] eid={0}; type={1}", EntityId, Status);
    }
}
