using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using Minecraft.DataTypes;

namespace Minecraft.IO;

public delegate TResult SpanFunc<T, TResult>(ReadOnlySpan<T> span);

public class BinaryStream : MemoryStream
{
    public BinaryStream()
    {

    }

    public BinaryStream(byte[] buffer)
    {
        Write(buffer);
        Position = 0;
    }

    public void Reset()
    {
        SetLength(0);
        Position = 0;
    }

    public new byte ReadByte()
    {
        var b = base.ReadByte();

        if (b == -1)
            throw new EndOfStreamException();

        return (byte)b;
    }

    #region 7Bit Encoder Helpers

    public static int GetVarIntSize(int value)
    {
        var numBytes = 0;

        do
        {
            value >>>= 7;
            numBytes++;
        }
        while (value > 0);

        return numBytes;
    }

    public static int GetVarLongSize(long value)
    {
        var numBytes = 0;

        do
        {
            value >>>= 7;
            numBytes++;
        }
        while (value > 0);

        return numBytes;
    }

    #endregion

    #region VarInt & VarLong

    public int ReadVarInt()
    {
        int value = 0, position = 0;

        while (true)
        {
            var currentByte = ReadByte();
            value |= (currentByte & 127) << position;

            if ((currentByte & 128) == 0)
                break;

            position += 7;

            if (position >= 32)
                throw new IOException("VarInt is too big!");
        }

        return value;
    }

    public long ReadVarLong()
    {
        long value = 0;
        int position = 0;

        while (true)
        {
            var currentByte = ReadByte();
            value |= (long)(currentByte & 127) << position;

            if ((currentByte & 128) == 0)
                break;

            position += 7;

            if (position >= 32)
                throw new IOException("VarInt is too big!");
        }

        return value;
    }

    public void WriteVarInt(int value)
    {
        while (true)
        {
            if ((value & ~127) == 0)
            {
                WriteByte((byte)value);
                return;
            }

            WriteByte((byte)((value & 127) | 128));
            value >>>= 7;
        }
    }

    public void WriteVarLong(long value)
    {
        while (true)
        {
            if ((value & ~127) == 0)
            {
                WriteByte((byte)value);
                return;
            }

            WriteByte((byte)((value & 127) | 128));
            value >>>= 7;
        }
    }

    #endregion

    #region String

    public void WriteString(string value)
    {
        var buf = Encoding.UTF8.GetBytes(value);
        WriteVarInt(buf.Length);
        Write(buf);
    }

    public string ReadString(int size = -1)
    {
        var buf = new byte[size < 0 ? ReadVarInt() : size];
        Read(buf);
        return Encoding.UTF8.GetString(buf);
    }

    #endregion

    #region Primitives

    T ReadValue<T>(SpanFunc<byte, T> cb)
    {
        var buf = new byte[Unsafe.SizeOf<T>()];

        try
        {
            Read(buf);
            return cb(buf);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    void WriteValue<T>(T value, SpanAction<byte, T> cb)
    {
        var buf = new byte[Unsafe.SizeOf<T>()];

        try
        {
            cb(buf, value);
            Write(buf);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public ushort ReadUInt16() => ReadValue(BinaryPrimitives.ReadUInt16BigEndian);
    public uint ReadUInt32() => ReadValue(BinaryPrimitives.ReadUInt32BigEndian);
    public ulong ReadUInt64() => ReadValue(BinaryPrimitives.ReadUInt64BigEndian);
    public short ReadInt16() => ReadValue(BinaryPrimitives.ReadInt16BigEndian);
    public int ReadInt32() => ReadValue(BinaryPrimitives.ReadInt32BigEndian);
    public long ReadInt64() => ReadValue(BinaryPrimitives.ReadInt64BigEndian);
    public float ReadFloat() => ReadValue(BinaryPrimitives.ReadSingleBigEndian);
    public double ReadDouble() => ReadValue(BinaryPrimitives.ReadDoubleBigEndian);

    public void WriteUInt16(ushort value) => WriteValue(value, BinaryPrimitives.WriteUInt16BigEndian);
    public void WriteUInt32(uint value) => WriteValue(value, BinaryPrimitives.WriteUInt32BigEndian);
    public void WriteUInt64(ulong value) => WriteValue(value, BinaryPrimitives.WriteUInt64BigEndian);
    public void WriteInt16(short value) => WriteValue(value, BinaryPrimitives.WriteInt16BigEndian);
    public void WriteInt32(int value) => WriteValue(value, BinaryPrimitives.WriteInt32BigEndian);
    public void WriteInt64(long value) => WriteValue(value, BinaryPrimitives.WriteInt64BigEndian);
    public void WriteFloat(float value) => WriteValue(value, BinaryPrimitives.WriteSingleBigEndian);
    public void WriteDouble(double value) => WriteValue(value, BinaryPrimitives.WriteDoubleBigEndian);

    #endregion

    #region Minecraft Specific

    public bool ReadBoolean() => ReadByte() == 0x01;
    public void WriteBoolean(bool value) => WriteByte((byte)(value ? 0x01 : 0x00));

    public byte[] ReadUInt8Array(int sizeHint = -1)
    {
        if (sizeHint < 0)
            sizeHint = ReadVarInt();

        var buf = new byte[sizeHint];
        Read(buf);
        return buf;
    }

    public void WriteUInt8Array(byte[] buffer)
    {
        WriteVarInt(buffer.Length);
        Write(buffer);
    }

    public Uuid ReadUuid()
    {
        var buf = new byte[16];
        Read(buf);
        return new Uuid(buf);
    }

    public void WriteUuid(Uuid value)
        => Write(value.ToByteArray());

    static class Constants
    {
        public const int M1P25 = 1 << 25;
        public const int M1P26 = 1 << 26;
        public const int M1P11 = 1 << 11;
        public const int M1P12 = 1 << 12;
    }

    public Vec3<int> ReadVec3i()
    {
        var val = ReadInt64();

        var x = (int)(val >> 38);
        var y = (int)(val << 52 >> 52);
        var z = (int)(val << 26 >> 38);

        if (x >= Constants.M1P25)
            x -= Constants.M1P26;

        if (y >= Constants.M1P11)
            y -= Constants.M1P12;

        if (z >= Constants.M1P25)
            z -= Constants.M1P26;

        return new(x, y, z);
    }

    public void WriteVec3i(Vec3<int> value)
    {
        long pos
            = ((value.X & 0x3FFFFFF) << 38)
            | ((value.Z & 0x3FFFFFF) << 12)
            | (value.Y & 0xfff);

        WriteInt64(pos);
    }

    public Vec3<double> ReadVec3d()
    {
        var x = ReadDouble();
        var y = ReadDouble();
        var z = ReadDouble();
        return new(x, y, z);
    }

    public void WriteVec3d(Vec3<double> value)
    {
        WriteDouble(value.X);
        WriteDouble(value.Y);
        WriteDouble(value.Z);
    }

    #endregion
}