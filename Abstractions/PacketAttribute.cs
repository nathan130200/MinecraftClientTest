namespace Minecraft.Abstractions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
public sealed class PacketAttribute(int id, Direction direction, ProtocolState state) : Attribute
{
    public int Id { get; } = id;
    public Direction Direction { get; } = direction;
    public ProtocolState State { get; } = state;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class DynamicPacketAttribute(Direction direction, ProtocolState state) : Attribute
{
    public Direction Direction { get; } = direction;
    public ProtocolState State { get; } = state;
}
