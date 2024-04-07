using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Minecraft;

public class Uuid
{
    private readonly byte[] _buffer;

    public static Uuid Nil => new();

    Uuid()
    {

    }

    internal Uuid(byte[] buffer)
        => _buffer = buffer;

    public Uuid(long msb, long lsb)
    {
        _buffer = new byte[16];
        BinaryPrimitives.WriteInt64BigEndian(_buffer.AsSpan(..8), msb);
        BinaryPrimitives.WriteInt64BigEndian(_buffer.AsSpan(8..), lsb);
    }

    public byte Version
        => (byte)((_buffer[6] >> 4) & 0x0f);

    public byte Variant
    {
        get
        {
            var bits = ((byte)(_buffer[8] >> 5) & 0x07);

            if ((bits & 0x04) == 0) return 0;
            else if ((bits & 0x02) == 0) return 1;
            else if ((bits & 0x01) == 0) return 2;
            else return 3;
        }
    }

    public byte[] ToByteArray()
        => [.. _buffer];

    public static Uuid GenerateVersion3(string name)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(name));
        hash[6] = (byte)((hash[6] & 0x0f) | 0x30);
        hash[8] = (byte)((hash[8] & 0x3f) | 0x80);
        return new(hash);
    }

    public override string ToString()
        => ToString(false);

    static readonly int[] DashIndices = [8, 13, 18, 23];

    public static Uuid Parse(string s)
    {
        if (s.Contains('-'))
            s = s.Replace("-", "");

        return new(Convert.FromHexString(s));
    }

    public string ToString(bool dashes)
    {
        var result = Convert.ToHexString(_buffer).ToLower();

        if (dashes)
        {
            foreach (var index in DashIndices)
                result = result.Insert(index, "-");
        }

        return result;
    }
}