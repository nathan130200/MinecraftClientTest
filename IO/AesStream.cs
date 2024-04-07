using System.Diagnostics;
using System.Security.Cryptography;

namespace Minecraft.IO;

public class AesStream : Stream
{
    private Aes _aes;
    private CryptoStream _encryptStream, _decryptStream;
    private ICryptoTransform _encryptor, _decryptor;

    private Stream _baseStream;
    private readonly bool _leaveOpen;

    public AesStream(Stream baseStream, byte[] key, bool leaveOpen = true)
    {
        _leaveOpen = leaveOpen;
        _baseStream = baseStream;

        Debug.Assert(key.Length == 16);

        _aes = Aes.Create();
        _aes.Mode = CipherMode.CFB;
        _aes.Key = key;
        _aes.IV = GC.AllocateUninitializedArray<byte>(16);
        _aes.FeedbackSize = 8;

        _encryptStream = new CryptoStream(_baseStream, _encryptor = _aes.CreateEncryptor(), CryptoStreamMode.Write, true);
        _decryptStream = new CryptoStream(_baseStream, _decryptor = _aes.CreateDecryptor(), CryptoStreamMode.Read, true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _encryptStream?.Dispose();
            _encryptStream = null;

            _decryptStream?.Dispose();
            _decryptStream = null;

            _encryptor?.Dispose();
            _encryptor = null;

            _decryptor?.Dispose();
            _decryptor = null;

            _aes.Dispose();
            _aes = null;

            if (!_leaveOpen)
                _baseStream?.Dispose();

            _baseStream = null;
        }

        base.Dispose(disposing);
    }

    public override bool CanRead => _decryptStream.CanRead;
    public override bool CanWrite => _encryptStream.CanWrite;

    public override int Read(byte[] buffer, int offset, int count)
        => _decryptStream.Read(buffer, offset, count);

    public override void Write(byte[] buffer, int offset, int count)
        => _encryptStream.Write(buffer, offset, count);

    public override void Flush() => _baseStream.Flush();
    public override long Seek(long offset, SeekOrigin origin) => _baseStream.Seek(offset, origin);
    public override void SetLength(long value) => _baseStream.SetLength(value);

    public override bool CanSeek => _baseStream.CanSeek;
    public override long Length => _baseStream.Length;
    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }
}