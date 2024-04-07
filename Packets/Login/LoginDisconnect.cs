using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Login;

[Packet(0x00, Direction.In, ProtocolState.Login)]
public class LoginDisconnect : IPacket, IPacketDeserializer
{
    public string Reason { get; private set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        Reason = stream.ReadString();
    }
}
