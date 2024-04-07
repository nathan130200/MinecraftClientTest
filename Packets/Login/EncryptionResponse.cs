using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Login;

[Packet(0x01, Direction.Out, ProtocolState.Login)]
public class EncryptionResponse : IPacket, IPacketSerializer
{
    public byte[] SharedSecret { private get; set; }
    public byte[] VerifyToken { private get; set; }

    void IPacketSerializer.Serialize(BinaryStream stream)
    {
        stream.WriteUInt8Array(SharedSecret);
        stream.WriteUInt8Array(VerifyToken);
    }
}
