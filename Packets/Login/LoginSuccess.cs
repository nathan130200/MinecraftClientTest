using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Login;

[Packet(0x02, Direction.In, ProtocolState.Login)]
public class LoginSuccess : IPacket, IPacketDeserializer
{
    public Uuid UniqueId { get; private set; }
    public string Username { get; private set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        UniqueId = Uuid.Parse(stream.ReadString());
        Username = stream.ReadString();
    }
}