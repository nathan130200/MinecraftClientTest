
using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets;

[Packet(0x00, Direction.Out, ProtocolState.Handshake)]
public class HandshakeOut : IPacket, IPacketSerializer
{
    public int ProtocolVersion { get; set; }
    public string Hostname { get; set; }
    public ushort Port { get; set; }
    public ProtocolState NextState { get; set; }

    void IPacketSerializer.Serialize(BinaryStream s)
    {
        s.WriteVarInt(ProtocolVersion);
        s.WriteString(Hostname);
        s.WriteUInt16(Port);
        s.WriteVarInt((int)NextState);
    }
}
