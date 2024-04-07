using Minecraft.Abstractions;
using Minecraft.Entities;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x07, Direction.In, ProtocolState.Play)]
public class SetRespawn : IPacket, IPacketDeserializer
{
    public WorldType Dimension { get; set; }
    public Difficulty Difficulty { get; set; }
    public GameMode GameMode { get; set; }
    public string LevelType { get; set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        Dimension = (WorldType)(sbyte)stream.ReadByte();
        Difficulty = (Difficulty)stream.ReadByte();
        GameMode = (GameMode)stream.ReadByte();
        LevelType = stream.ReadString();
    }
}