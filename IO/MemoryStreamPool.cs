namespace Minecraft.IO;

public class MemoryStreamPool
{
    public class MemoryStreamPoolImpl : BinaryStream
    {
        public MemoryStreamPoolImpl()
        {

        }

        protected internal DateTimeOffset _lastAccess;

        public new void Dispose()
        {
            _lastAccess = DateTimeOffset.Now;
            SetLength(0);
            Position = 0;
            Return(this);
        }

        public byte[] ToArrayAndReset()
        {
            var result = base.ToArray();
            SetLength(0);
            Position = 0;
            return result;
        }

        protected internal void DisposeInternal()
             => base.Dispose();
    }

    static readonly List<MemoryStreamPoolImpl> s_Pool = [];

    static MemoryStreamPool()
    {
        _ = CleanupUnusedStreams();
    }

    static async Task CleanupUnusedStreams()
    {
        var interval = TimeSpan.FromSeconds(30);

        while (true)
        {
            await Task.Delay(interval);

            lock (s_Pool)
            {
                foreach (var item in s_Pool.ToArray())
                {
                    if (DateTimeOffset.Now - item._lastAccess >= interval)
                    {
                        item.DisposeInternal();
                        s_Pool.Remove(item);
                    }
                }
            }
        }
    }

    public static MemoryStreamPoolImpl Rent(byte[] buffer)
    {
        var self = Rent();
        self.Write(buffer);
        self.Position = 0;
        return self;
    }

    public static MemoryStreamPoolImpl Rent()
    {
        MemoryStreamPoolImpl ms;

        lock (s_Pool)
        {
            if (s_Pool.Count > 0)
            {
                ms = s_Pool[0];
                s_Pool.RemoveAt(0);
            }
            else
            {
                ms = new();
            }
        }

        return ms;
    }

    static void Return(MemoryStreamPoolImpl ms)
    {
        lock (s_Pool)
        {
            s_Pool.Add(ms);
        }
    }
}