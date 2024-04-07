using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Login;

[Packet(0x00, Direction.Out, ProtocolState.Login)]
public class LoginStart : IPacket, IPacketSerializer
{
    public string Username { private get; set; }
    public Uuid UniqueId { private get; set; }

    void IPacketSerializer.Serialize(BinaryStream stream)
    {
        stream.WriteString(Username);
    }
}
