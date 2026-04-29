using System;
using System.IO;
using System.Buffers;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NSmartProxy.Shared;

namespace NSmartProxy.Infrastructure
{
    public static class StreamExtension
    {
        public static async Task WriteAndFlushAsync(this Stream stream, byte[] buffer, int offset = 0, int count = 0)
        {
            //不让赋值默认值，只能暂时给个0
            if (count == 0) count = buffer.Length;
            await stream.WriteAndFlushAsync(buffer.AsMemory(offset, count));
        }

        public static async Task WriteAndFlushAsync(this Stream stream, ReadOnlyMemory<byte> buffer)
        {
            await stream.WriteAsync(buffer);
            await stream.FlushAsync();
        }

        public static Task WriteByteAndFlushAsync(this Stream stream, byte value)
        {
            stream.WriteByte(value);
            return stream.FlushAsync();
        }


        /// <summary>
        /// 带超时的readasync,timeout 毫秒
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="TimeOut"></param>
        /// <returns></returns>
        public static async Task<UdpReceiveResult?> ReceiveAsync(this UdpClient client, int timeOut)
        {
            try
            {
                return await client.ReceiveAsync().WaitAsync(TimeSpan.FromMilliseconds(timeOut));
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        /// <summary>
        /// 带超时的readasync,timeout 毫秒
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="TimeOut"></param>
        /// <returns></returns>
        public static async Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count, int timeOut)
        {
            return await stream.ReadAsync(buffer.AsMemory(offset, count), timeOut);
        }

        public static async Task<int> ReadAsync(this Stream stream, Memory<byte> buffer, int timeOut)
        {
            try
            {
                return await stream.ReadAsync(buffer).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeOut));
            }
            catch (TimeoutException)
            {
                return -1;
            }
        }

        /// <summary>
        /// 读取接下来N字节的定长数据，如果服务端没有发那么多信息，
        /// 可能会出现读不全的情况，也有可能出现阻塞超时的情况
        /// </summary>
        public static async Task<int> ReadNextSTLengthBytes(this Stream stream, byte[] buffer)
        {
            return await stream.ReadNextSTLengthBytes(buffer.AsMemory());
        }

        public static async Task<int> ReadNextSTLengthBytes(this Stream stream, Memory<byte> buffer)
        {
            int totalReceivedBytes = 0;
            while (totalReceivedBytes < buffer.Length)
            {
                int receivedBytes = await stream.ReadAsyncEx(buffer[totalReceivedBytes..]);
                if (receivedBytes <= 0) return -1;//没有接收满则断开返回-1
                totalReceivedBytes += receivedBytes;
            }
            return totalReceivedBytes;
        }

        public static async Task<bool> TryReadExactlyAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            return await stream.TryReadExactlyAsync(buffer.AsMemory(offset, count));
        }

        public static async Task<bool> TryReadExactlyAsync(this Stream stream, Memory<byte> buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int read = await stream.ReadAsyncEx(buffer[totalRead..]);
                if (read <= 0)
                {
                    return false;
                }

                totalRead += read;
            }

            return true;
        }

        public static Task<bool> TryReadExactlyAsync(this Stream stream, byte[] buffer)
        {
            return stream.TryReadExactlyAsync(buffer.AsMemory());
        }

        public static async Task<int> ReadAsyncEx(this Stream stream, byte[] buffer, int offset, int count)
        {
            return await stream.ReadAsync(buffer.AsMemory(offset, count), Global.DefaultConnectTimeout);
        }

        public static async Task<int> ReadAsyncEx(this Stream stream, Memory<byte> buffer)
        {
            return await stream.ReadAsync(buffer, Global.DefaultConnectTimeout);
        }

        public static async Task<int> ReadAsyncEx(this Stream stream, byte[] buffer)
        {
            return await stream.ReadAsyncEx(buffer.AsMemory());
        }

        public static Stream ProcessSSL(this Stream clientStream, X509Certificate cert)
        {
            try
            {
                SslStream sslStream = new SslStream(clientStream);
                sslStream.AuthenticateAsServer(cert, false, SslProtocols.Tls12, true);
                sslStream.ReadTimeout = 10000;
                sslStream.WriteTimeout = 10000;
                return sslStream;
            }
            catch (Exception)
            {
                clientStream.Close();
                throw;
            }

            //return null;
        }

        public static async Task WriteAsync(this Stream stream, byte[] bytes)
        {
            await stream.WriteAsync(bytes.AsMemory());
        }

        /// <summary>
        /// 写入字符串（ASCII）
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task WriteDLengthBytes(this Stream stream, string asciiStr)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(asciiStr);
            Span<byte> header = stackalloc byte[2];
            StringUtil.WriteIntTo2Bytes(header, bytes.Length);
            stream.Write(header);
            await stream.WriteAsync(bytes.AsMemory());
        }
        /// <summary>
        /// 写入动态长度的字节，头两字节存放长度
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task WriteDLengthBytes(this Stream stream, byte[] bytes)
        {
            Span<byte> header = stackalloc byte[2];
            StringUtil.WriteIntTo2Bytes(header, bytes.Length);
            stream.Write(header);
            await stream.WriteAsync(bytes.AsMemory());
        }

        /// <summary>
        /// 写入动态长度的字节，头四字节存放长度
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task WriteQLengthBytes(this Stream stream, byte[] bytes,int forceLength = -1)
        {
            Span<byte> header = stackalloc byte[4];
            if (forceLength > 0)
            {
                StringUtil.WriteIntTo4Bytes(header, forceLength);
                stream.Write(header);
                await stream.WriteAsync(bytes.AsMemory(0, forceLength));
            }
            else
            {
                StringUtil.WriteIntTo4Bytes(header, bytes.Length);
                stream.Write(header);
                await stream.WriteAsync(bytes.AsMemory());
            }
            
        }

        /// <summary>
        /// 读取动态长度的字节，头两字节存放长度
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadNextDLengthBytes(this Stream stream)
        {
            byte[] bt2 = ArrayPool<byte>.Shared.Rent(2);
            try
            {
                if (!await stream.TryReadExactlyAsync(bt2.AsMemory(0, 2)))
                {
                    return null;
                }

                int length = StringUtil.DoubleBytesToInt(bt2.AsSpan(0, 2));
                var bytes = new byte[length];
                return await stream.TryReadExactlyAsync(bytes.AsMemory()) ? bytes : null;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bt2);
            }
        }

        /// <summary>
        /// 读取动态长度的字节,头四字节存放长度,这种方式最大支持2G的数据
        /// 这种比ReadNextDLengthBytes支持更大数据，但是读取时候会造成中断，需要多次读取
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadNextQLengthBytes(this Stream stream)
        {
            byte[] bt2 = ArrayPool<byte>.Shared.Rent(4);
            try
            {
                if (!await stream.TryReadExactlyAsync(bt2.AsMemory(0, 4)))
                {
                    return Array.Empty<byte>();
                }

                int length = StringUtil.ReadInt32(bt2.AsSpan(0, 4));
                var bytes = new byte[length];
                return await stream.TryReadExactlyAsync(bytes.AsMemory()) ? bytes : Array.Empty<byte>();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bt2);
            }
        }

        public static async Task<PooledByteBuffer> ReadNextQLengthBytesRented(this Stream stream)
        {
            byte[] bt2 = ArrayPool<byte>.Shared.Rent(4);
            try
            {
                if (!await stream.TryReadExactlyAsync(bt2.AsMemory(0, 4)))
                {
                    return null;
                }

                int length = StringUtil.ReadInt32(bt2.AsSpan(0, 4));
                if (length <= 0)
                {
                    return null;
                }

                byte[] bytes = ArrayPool<byte>.Shared.Rent(length);
                if (await stream.TryReadExactlyAsync(bytes.AsMemory(0, length)))
                {
                    return new PooledByteBuffer(bytes, length);
                }

                ArrayPool<byte>.Shared.Return(bytes);
                return null;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bt2);
            }
        }

    }
}
