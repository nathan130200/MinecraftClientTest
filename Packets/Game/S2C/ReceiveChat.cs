using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x02, Direction.In, ProtocolState.Play)]
public class ReceiveChat : IPacket, IPacketDeserializer
{
    public string JsonData { private set; get; }
    public Type ChatType { private set; get; }

    public enum Type
    {
        ChatBox,
        SystemMessage,
        AboveHotbar
    }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        JsonData = stream.ReadString();
        ChatType = (Type)stream.ReadByte();

        Console.WriteLine(" [CHAT] ({0}) {1}", ChatType, JsonData);
    }
}
