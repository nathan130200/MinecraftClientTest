using Minecraft.Abstractions;
using Minecraft.Entities;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x01, Direction.In, ProtocolState.Play)]
internal class JoinGame : IPacket, IPacketDeserializer
{
    public int EntityId { get; private set; }
    public GameMode GameMode { get; private set; }
    public WorldType Dimension { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public bool IsHardcore { get; private set; }
    public byte ScoreboardMaxPlayers { get; private set; }
    public string LevelType { get; private set; }
    public bool ReducedDebugInfo { get; private set; }

    const byte HARDCORE_FLAG = 0b100;

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        EntityId = stream.ReadInt32();
        GameMode = (GameMode)stream.ReadByte();
        Dimension = (WorldType)(sbyte)stream.ReadByte();

        {
            var diff = stream.ReadByte();
            IsHardcore = (diff & HARDCORE_FLAG) == HARDCORE_FLAG;
            Difficulty = (Difficulty)(diff & ~HARDCORE_FLAG);
        }

        ScoreboardMaxPlayers = stream.ReadByte();
        LevelType = stream.ReadString();
        ReducedDebugInfo = stream.ReadBoolean();
    }
}
