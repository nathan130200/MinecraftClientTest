using System.Numerics;
using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.C2S;

[DynamicPacket(Direction.Out, ProtocolState.Play)]
public class PlayerMovementRequest : IPacket, IPacketSerializer, IDynamicPacket
{
    public PlayerMovementRequestType Type { get; set; }
    public Vector3 Position { get; set; }
    public Vector2 Look { get; set; }
    public bool Grounded { get; set; }

    void IPacketSerializer.Serialize(BinaryStream stream)
    {
        if (Type.HasFlag(PlayerMovementRequestType.Position))
        {
            stream.WriteDouble(Position.X);
            stream.WriteDouble(Position.Y);
            stream.WriteDouble(Position.Z);
        }

        if (Type.HasFlag(PlayerMovementRequestType.Look))
        {
            stream.WriteFloat(Look.X);
            stream.WriteFloat(Look.Y);
        }

        stream.WriteBoolean(Grounded);
    }

    int IDynamicPacket.GetId()
    {
        if (Type == PlayerMovementRequestType.PositionAndLook)
            return 0x06;
        else if (Type == PlayerMovementRequestType.Look)
            return 0x05;
        else if (Type == PlayerMovementRequestType.Position)
            return 0x04;
        else
            return 0x03;
    }
}
