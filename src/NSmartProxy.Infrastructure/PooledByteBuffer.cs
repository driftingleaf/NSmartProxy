using System;
using System.Buffers;

namespace NSmartProxy.Infrastructure
{
    public sealed class PooledByteBuffer : IDisposable
    {
        private readonly ArrayPool<byte> _pool;
        private bool _disposed;

        public byte[] Buffer { get; }
        public int Length { get; }

        public ReadOnlyMemory<byte> Memory => Buffer.AsMemory(0, Length);

        public PooledByteBuffer(byte[] buffer, int length, ArrayPool<byte> pool = null)
        {
            Buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            Length = length;
            _pool = pool ?? ArrayPool<byte>.Shared;
        }

        public static PooledByteBuffer Rent(int minimumLength)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(minimumLength);
            return new PooledByteBuffer(buffer, minimumLength);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pool.Return(Buffer);
        }
    }
}
