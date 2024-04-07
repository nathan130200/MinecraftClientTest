using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x12, Direction.In, ProtocolState.Play)]
internal class SetEntityVelocity : IPacket, IPacketDeserializer
{
    public int EntityID { get; private set; }
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        EntityID = stream.ReadVarInt();
        X = 8000f / stream.ReadInt16();
        Y = 8000f / stream.ReadInt16();
        Z = 8000f / stream.ReadInt16();
    }
}
