using Minecraft.Abstractions;
using Minecraft.IO;

namespace Minecraft.Packets.Game.S2C;

[Packet(0x03, Direction.In, ProtocolState.Play)]
public class ServerTimeUpdate : IPacket, IPacketDeserializer
{
    public TimeSpan WorldAge { private set; get; }
    public TimeSpan TimeOfDay { private set; get; }

    void IPacketDeserializer.Deserialize(BinaryStream stream)
    {
        long val1, val2;

        {
            val1 = stream.ReadInt64();
            WorldAge = TimeSpan.FromSeconds(val1 / 20d);
        }

        {
            double timeRatio = (val2 = stream.ReadInt64()) / (double)24000;

            if (timeRatio >= 1)
                timeRatio -= 1;

            TimeOfDay = TimeSpan.FromHours(timeRatio * 24);
        }

        //Console.WriteLine(" [SERVER TIME] WORLD={0} ({1}); TOD={2} ({3})", WorldAge, val1, TimeOfDay, val2);
    }
}
