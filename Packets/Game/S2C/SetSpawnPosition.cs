using System.Numerics;
using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x05, Direction.In, ProtocolState.Play)]
public class SetSpawnPosition : IPacket, IPacketDeserializer
{
    public Vector3 Value { get; private set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        var pos = stream.ReadVec3i();
        Value = new(pos.X, pos.Y, pos.Z);
    }
}