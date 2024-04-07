using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using Minecraft.Abstractions;
using Minecraft.Entities;
using Minecraft.IO;
using Minecraft.Packets;
using Minecraft.Packets.Game;
using Minecraft.Packets.Game.C2S;
using Minecraft.Packets.Game.S2C;
using Minecraft.Packets.Login;
using Minecraft.Registry;

namespace Minecraft.Net;

public readonly record struct CompressionInfo(bool Enabled,
    int MaxUncompressedSize);

public sealed class Client : IDisposable
{
    internal Socket _socket;
    internal Stream _stream;
    internal volatile ProtocolState _state;
    internal volatile bool _disposed;
    internal DnsEndPoint _endpoint;
    internal ConcurrentQueue<(byte[] Buffer, TaskCompletionSource Callback)> _sendQueue = [];
    internal CompressionInfo _compression = new(false, -1);
    internal AesStream _aesStream;
    internal bool _isEncrypted;

    internal bool Connected
        => _state != ProtocolState.Disconnect && !_disposed;

    public string Username { get; init; }
    public Uuid UniqueId { get; init; }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _stream?.Dispose();
        _stream = null;

        _socket?.Dispose();
        _socket = null;

        if (_isEncrypted)
        {
            _aesStream?.Dispose();
            _aesStream = null;
        }

        _state = ProtocolState.Disconnect;
    }

    public async Task Connect(string host, ushort port = 25565)
    {
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        await _socket.ConnectAsync(_endpoint = new(host, port));
        _ = StartAsync();
    }

    public ValueTask Disconnect()
    {
        _state = ProtocolState.Disconnect;
        Dispose();
        return default;
    }

    public async Task Send(IPacket packet)
    {
        int id, size;

        if (packet is not IPacketSerializer impl)
            return;

        var tcs = new TaskCompletionSource();

        try
        {
            using (var stm = MemoryStreamPool.Rent())
            {
                impl.Serialize(stm);

                var data = stm.ToArrayAndReset();

                if (packet is IDynamicPacket dyn)
                    id = dyn.GetId();
                else
                    id = PacketRegistry.GetId(packet.GetType());

                stm.WriteVarInt(data.Length + BinaryStream.GetVarIntSize(id));
                stm.WriteVarInt(id);
                stm.Write(data);

                size = (int)stm.Length;

                _sendQueue.Enqueue((stm.ToArray(), tcs));
            }

            await tcs.Task;
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: " + e);
            throw;
        }

        //Console.WriteLine(" [OUT] ID={0:X2} ({0}), SIZE={1} (TYPE: {2})",
        //    id, size, packet?.GetType()?.FullName ?? "<?>");

        if (packet != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("<{0}>", packet.GetType().Name);
            Console.ResetColor();
        }
    }

    internal Task Send<TPacket>(Action<TPacket> callback) where TPacket : IPacket
        => Send(PacketRegistry.CreateNew(callback));

    async Task<(int PacketId, IPacket Packet)> ReadNextPacketAsync()
    {
        try
        {
            var stream = _isEncrypted ? _aesStream : _stream;

            if (!_compression.Enabled)
            {
                var length = ReadVarInt();
                var id = ReadVarInt();
                length -= BinaryStream.GetVarIntSize(id);

                var packet = PacketRegistry.CreateNew(id, Direction.In, _state);

                //Console.WriteLine(" [IN] ID={0:X2} ({0}), SIZE: {1} (TYPE: {2})", id, length,
                //    packet?.GetType()?.FullName ?? "<?>");

                if (packet is not IPacketDeserializer serializer)
                {
                    for (int i = 0; i < length; i++)
                        _ = stream.ReadByte();

                    return (id, null);
                }

                var buffer = new byte[length];
                await stream.ReadExactlyAsync(buffer);

                using (var stm = MemoryStreamPool.Rent(buffer))
                {
                    serializer.Deserialize(stm);
                    return (id, packet);
                }

                int ReadVarInt()
                {
                    int value = 0, position = 0;

                    while (true)
                    {
                        var self = stream.ReadByte();

                        if (self == -1)
                            throw new EndOfStreamException();

                        var currentByte = (byte)self;

                        value |= (currentByte & 127) << position;

                        if ((currentByte & 128) == 0)
                            break;

                        position += 7;

                        if (position >= 32)
                            throw new IOException("VarInt is too big!");
                    }

                    return value;
                }
            }
            else
            {
                throw null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Dispose();
            throw;
        }
    }

    async Task ProcessQueueAsync()
    {
        while (!_disposed)
        {
            if (!_sendQueue.TryDequeue(out var entry))
            {
                await Task.Delay(1);
                continue;
            }

            var (buffer, tcs) = entry;

            try
            {
                await _stream.WriteAsync(buffer);
                tcs.TrySetResult();
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
                break;
            }
        }

        Dispose();
    }

    byte[] _sharedSecret;

    async Task StartAsync()
    {
        _stream = new NetworkStream(_socket, true);

        _ = ProcessQueueAsync();

        _state = ProtocolState.Handshake;

        await Send<HandshakeOut>(v =>
        {
            v.ProtocolVersion = 47;
            v.Hostname = _endpoint.Host;
            v.Port = (ushort)_endpoint.Port;
            v.NextState = ProtocolState.Login;
        });

        _state = ProtocolState.Login;

        await Send<LoginStart>(v => v.Username = Username);

        var (id, packet) = await ReadNextPacketAsync();

        if (packet == null)
        {
            Console.WriteLine("Unable to read next packet!");
            return;
        }

        Console.WriteLine("Packet {0} deserialized. [type={1}]", id, packet.GetType());

        if (packet is EncryptionRequest request)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(request.PublicKey, out _);

            var sharedSecret = RandomNumberGenerator.GetBytes(16);
            var verifyToken = rsa.Encrypt(request.VerifyToken, RSAEncryptionPadding.Pkcs1);

            await Send<EncryptionResponse>(v =>
            {
                v.SharedSecret = sharedSecret;
                v.VerifyToken = verifyToken;
            });

            (id, packet) = await ReadNextPacketAsync();

            if (id == 0)
            {
                Console.WriteLine("LOGIN: DISCONNECT");
                Dispose();
                return;
            }

            _sharedSecret = sharedSecret;
            goto _onLoginSuccess;
        }

    _onLoginSuccess:
        if (packet is LoginSuccess login)
        {
            _state = ProtocolState.Play;

            m_UserName = login.Username;
            m_uniqueId = login.UniqueId;

            if (_sharedSecret != null)
            {
                _aesStream = new(_stream, _sharedSecret);
                _isEncrypted = true;
            }

            Console.WriteLine("LOGIN: SUCCESS ({0})", _isEncrypted ? "ENCRYPTED" : "NOT ENCRYPTED");

            await ProcessGamePackets();
        }
        else
        {
            Console.WriteLine("LOGIN: UNKNOWN PACKET [ID={0}]", id);
            Dispose();
            return;
        }
    }

    int m_entityID;
    string m_UserName;
    Uuid m_uniqueId;
    float m_health, m_saturation;
    int m_food;
    bool m_isAlive = false;

    Stopwatch m_Timer;
    Vector3 m_Pos;
    Vector2 m_Look;

    Task _respawnTask;
    bool m_bMoved = true, m_bRotated = true, m_Grounded = false;

    async Task UpdatePlayer()
    {
        while (_state == ProtocolState.Play)
        {
            await SyncPlayer();
            await Task.Delay(16);
        }
    }

    ValueTask TriggerRespawn()
    {
        if (!m_isAlive && (_respawnTask == null || _respawnTask.IsCompleted))
            _respawnTask = DelayedRespawn(1.25f);

        return default;
    }

    bool m_StartPos;
    Vector3 m_SpawnPos;

    async Task ProcessGamePackets()
    {
        _ = Task.Run(UpdatePlayer);

        while (_state == ProtocolState.Play)
        {
            await Task.Delay(1);

            var (id, packet) = await ReadNextPacketAsync();

            if (id == -1)
            {
                Console.WriteLine("Unknown packet received! Closing connection...");
                _state = ProtocolState.Disconnect;
                Dispose();
                return;
            }

            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine("{1}<PACKET|{0:X2}>", id, new string(' ', Console.WindowWidth - 12));

            if (packet != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("<{0}>", packet.GetType().Name);
                Console.ResetColor();
            }

            if (packet is KeepAlive)
            {
                await Send(packet);
                continue;
            }

            if (packet is SetRespawn)
            {
                _ = TriggerRespawn();
            }

            if (packet is PlayerMovementResponse resp)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(" [SERVER: SYNC PLAYER]: {0} | POS: {1}; LOOK: {2}", resp.Type, resp.Position, resp.Look);
                Console.ResetColor();

                m_Pos = resp.Position;
                m_Look = resp.Look;
            }

            if (packet is EntityInitialize ie)
            {
                if (ie.EntityId == m_entityID)
                    _ = TriggerRespawn();
            }

            if (packet is SetSpawnPosition sp)
            {
                m_SpawnPos = sp.Value;
            }

            if (packet is JoinGame join)
            {
                m_entityID = join.EntityId;
                _ = TriggerRespawn();
            }

            if (packet is UpdateHealth uh)
            {
                m_health = uh.Health;
                m_food = uh.FoodLevel;
                m_saturation = uh.Saturation;

                Console.Title = string.Format("health={0}; food={1}; saturation={2}", uh.Health,
                    uh.FoodLevel, uh.Saturation);

                m_isAlive = m_health > 0;
            }

            if (packet is EntityStatusUpdate esu)
            {
                if (esu.EntityId == m_entityID && esu.Status == EntityStatusType.LivingEntityDead)
                {
                    m_isAlive = false;
                    _ = TriggerRespawn();
                }
            }

            if (packet is ReceiveChat rc)
            {
                if (rc.JsonData.Contains("FRNathan13") && rc.JsonData.Contains("respawn()"))
                    _ = Send<PeformAction>(x => x.ActionType = PeformAction.Type.Respawn);
            }

            if (packet is SetEntityVelocity vel)
            {
                if (vel.EntityID == m_entityID)
                {
                    m_Pos.X += vel.X;
                    m_Pos.Y += vel.Y;
                    m_Pos.Z += vel.Z;
                    m_bMoved = true;
                }
            }
        }
    }

    public void UpdateMovement(int x, int z, int y)
    {
        m_Pos.X += x * 0.16f;
        m_Pos.Y += y * 0.16f;
        m_Pos.Z += z * 0.16f;
    }

    async Task SyncPlayer()
    {
        var packet = new PlayerMovementRequest
        {
            Grounded = m_Grounded
        };

        if (m_bMoved || m_bRotated)
        {
            if (m_bMoved)
            {
                packet.Position = m_Pos;
                packet.Type |= PlayerMovementRequestType.Position;
            }
            else if (m_bMoved)
            {
                packet.Type |= PlayerMovementRequestType.Look;
                packet.Look = m_Look;
            }
        }

        await Send(packet);

        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine(" [SYNC PLAYER] POS: {0}; LOOK: {1}",
            m_Pos, m_Look);

        Console.ResetColor();

        m_bMoved = !m_bMoved;
        m_bRotated = false;
    }

    async Task DelayedRespawn(float time)
    {
        if (m_isAlive)
            return;

        await Task.Delay(TimeSpan.FromSeconds(time));

        await Send<PeformAction>(v => v.ActionType
            = PeformAction.Type.Respawn);

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("=-=-=-=-=-=-=- RESPAWN =-=-=-=-=-=-=-");

        //m_isAlive = true;
    }
}
