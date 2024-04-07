using System.Numerics;
using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x08, Direction.In, ProtocolState.Play)]
public class PlayerMovementResponse : IPacket, IPacketDeserializer
{
    public PlayerMovementResponseType Type { get; set; }
    public Vector3 Position { get; set; }
    public Vector2 Look { get; set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        Position = new(
            (float)stream.ReadDouble(),
            (float)stream.ReadDouble(),
            (float)stream.ReadDouble()
        );

        Look = new(
            stream.ReadFloat(),
            stream.ReadFloat()
        );

        Type = (PlayerMovementResponseType)stream.ReadByte();
    }
}

[Flags]
public enum PlayerMovementResponseType
{
    RelativeX = 0x01,
    RelativeY = 0x02,
    RelativeZ = 0x04,
    RelativeRotationY = 0x08,
    RelativeRotationX = 0x10,
}

//[Packet(0x0c, Direction.In, ProtocolState.Play)]
public class SpawnPlayer : IPacket, IPacketDeserializer
{
    public int EntityId { get; set; }
    public Uuid UniqueId { get; set; }
    public Vector3 Position { get; set; }
    public (byte Yaw, byte Pitch) Rotation { get; set; }
    public sbyte CurrentItem { get; set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {

    }
}