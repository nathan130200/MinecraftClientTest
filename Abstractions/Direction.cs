namespace Minecraft.Abstractions;

[Flags]
public enum Direction
{
    In = 1 << 0,
    Out = 1 << 1,
    Both = In | Out
}
