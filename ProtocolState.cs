namespace Minecraft;

public enum ProtocolState
{
    None = -1,
    Handshake,
    Status,
    Login,
    Play,
    Disconnect
}
