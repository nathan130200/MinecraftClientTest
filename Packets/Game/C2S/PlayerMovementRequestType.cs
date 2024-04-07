namespace Minecraft.Packets.Game.C2S;

[Flags]
public enum PlayerMovementRequestType
{
    None,
    Position = 1 << 0,
    Look = 1 << 1,

    PositionAndLook = Position | Look
}