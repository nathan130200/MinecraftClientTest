using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Login;

[Packet(0x01, Direction.In, ProtocolState.Login)]
public class EncryptionRequest : IPacket, IPacketDeserializer
{
    public string ServerId { get; private set; }
    public byte[] PublicKey { get; private set; }
    public byte[] VerifyToken { get; private set; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        ServerId = stream.ReadString();
        PublicKey = stream.ReadUInt8Array();
        VerifyToken = stream.ReadUInt8Array();
    }
}
