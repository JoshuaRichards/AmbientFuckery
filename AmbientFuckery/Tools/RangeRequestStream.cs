using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AmbientFuckery.Tools
{
    public class RangeRequestStream : Stream
    {
        private const int MIN_CACHED_REQUEST_COUNT = 8192;

        private readonly HttpClient httpClient;
        private readonly string url;
        private readonly int length;

        public override long Length => length;
        public override long Position
        {
            get => position;
            set
            {
                if (value > int.MaxValue) throw new ArgumentOutOfRangeException();

                position = (int)value;
            }
        }

        private readonly List<byte> cache = new List<byte>();
        private int position = 0;

        public RangeRequestStream(HttpClient httpClient, string url, int length)
        {
            this.httpClient = httpClient;
            this.url = url;
            this.length = length;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (position >= length || count == 0) return 0;
            if (position + count > length) count = length - position;

            int bytesReadFromCache = ReadFromCache(buffer, offset, count);
            if (bytesReadFromCache == count) return bytesReadFromCache;

            count -= bytesReadFromCache;
            offset += bytesReadFromCache;

            int bytesReadFromHttp = ReadFromHttpAsync(buffer, offset, count).Result;

            return bytesReadFromCache + bytesReadFromHttp;
        }

        private async Task<int> ReadFromHttpAsync(byte[] buffer, int offset, int count)
        {
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            bool shouldCache = cache.Count == position;
            int requestCount = shouldCache ? Math.Max(count, MIN_CACHED_REQUEST_COUNT) : count;

            int to = Math.Min(position + requestCount - 1, length - 1);
            request.Headers.Range = new RangeHeaderValue(position, to);

            using HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            int ret = Math.Min(bytes.Length, count);

            Array.Copy(bytes, 0, buffer, offset, ret);
            if (shouldCache) cache.AddRange(bytes);

            position += ret;

            return ret;
        }

        private int ReadFromCache(byte[] buffer, int offset, int count)
        {
            if (position >= cache.Count) return 0;

            int bytesRead = 0;
            for (int i = position; i < cache.Count && bytesRead < count; i++, bytesRead++)
            {
                buffer[offset++] = cache[i];
            }

            position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset > int.MaxValue) throw new InvalidOperationException();

            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = (int)offset;
                    break;
                case SeekOrigin.Current:
                    position += (int)offset;
                    break;
                case SeekOrigin.End:
                    position = length + (int)offset - 1;
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (position < 0 || position > Length - 1)
            {
                throw new InvalidOperationException();
            }

            return position;
        }

        #region boring stuff
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        #endregion
    }
}
