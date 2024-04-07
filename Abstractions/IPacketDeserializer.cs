using Minecraft.IO;

namespace Minecraft.Abstractions;

public interface IPacketDeserializer
{
    void Deserialize(BinaryStream stream);
}
