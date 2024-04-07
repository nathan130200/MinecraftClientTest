using System.Reflection;
using System.Runtime.CompilerServices;
using Minecraft.Abstractions;

namespace Minecraft.Registry;

public class PacketRegistry
{
    record PacketInfo(int Id, Direction Direction, ProtocolState State, Type Type, bool IsDynamic);

    static List<PacketInfo> _packets = [];
    static bool _initialized;

    [ModuleInitializer]
    internal static void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        var baseIface = typeof(IPacket);
        var ifaceIn = typeof(IPacketDeserializer);
        var ifaceOut = typeof(IPacketSerializer);

        var types = typeof(PacketRegistry).Assembly
            .GetTypes()
            .Where(xt => xt.IsAssignableTo(baseIface) && (xt.IsAssignableTo(ifaceIn)
                || xt.IsAssignableTo(ifaceOut)));

        foreach (var type in types)
        {
            var attrs = type.GetCustomAttributes();

            if (attrs.Any())
            {
                foreach (var rawAttr in attrs)
                {
                    if (rawAttr is DynamicPacketAttribute dynAttr)
                    {
                        _packets.Add(new(-1, dynAttr.Direction, dynAttr.State, type, true));
                        break;
                    }

                    if (rawAttr is PacketAttribute attr)
                        _packets.Add(new(attr.Id, attr.Direction, attr.State, type, false));
                }
            }
        }
    }

    public static int GetId<T>()
        => GetId(typeof(T));

    public static int GetId(Type type)
        => _packets.First(x => x.Type == type).Id;

    public static IPacket CreateNew(int id, Direction direction, ProtocolState state)
    {
        var type = _packets.FirstOrDefault(x => x.Id == id && x.State == state
            && x.Direction.HasFlag(direction))?.Type;

        if (type == null)
            return null;

        return (IPacket)Activator.CreateInstance(type);
    }

    public static P CreateNew<P>() where P : IPacket
    {
        var attr = typeof(P)
            .GetCustomAttribute<PacketAttribute>();

        return (P)CreateNew(attr.Id, attr.Direction, attr.State);
    }

    public static P CreateNew<P>(Action<P> callback) where P : IPacket
    {
        var result = CreateNew<P>();
        callback(result);
        return result;
    }

    public static Y CreateDynamic<Y>(Action<Y> builder) where Y : class, IPacket
    {
        var result = Activator.CreateInstance<Y>() as Y;
        builder(result);
        return result;
    }
}