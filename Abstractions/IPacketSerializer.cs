using Minecraft.IO;

namespace Minecraft.Abstractions;

public interface IPacketSerializer
{
    void Serialize(BinaryStream stream);
}
