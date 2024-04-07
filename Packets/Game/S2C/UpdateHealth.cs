using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x06, Direction.In, ProtocolState.Play)]
public class UpdateHealth : IPacket, IPacketDeserializer
{
    public float Health { get; private set; }
    public int FoodLevel { get; private set; }
    public float Saturation { get; private set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        Health = stream.ReadFloat();
        FoodLevel = stream.ReadVarInt();
        Saturation = stream.ReadFloat();
    }
}

//[Packet(0x026, Direction.In, ProtocolState.Play)]
public class MapChunkBulk : IPacket, IPacketDeserializer
{
    public bool HasSkyLight { private set; get; }
    public int ChunkColumnCount { private set; get; }
    public IEnumerable<ChunkMetadataInfo> ChunkMetadata { private set; get; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {

    }

    public record ChunkMetadataInfo(int X, int Z, ushort LayerMask);
    public record ChunkInfo
    {

    }
}