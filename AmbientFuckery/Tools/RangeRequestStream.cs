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

            var bytesReadFromCache = ReadFromCache(buffer, offset, count);
            if (bytesReadFromCache == count) return bytesReadFromCache;

            position += bytesReadFromCache;
            count -= bytesReadFromCache;
            offset += bytesReadFromCache;

            var bytesReadFromHttp = ReadFromHttpAsync(buffer, offset, count).Result;
            if (bytesReadFromHttp == 0) return bytesReadFromCache;

            var bytesRead = bytesReadFromCache + bytesReadFromHttp;
            var shouldCache = cache.Count == position;
            position += bytesReadFromHttp;
            if (!shouldCache) return bytesRead;

            var upperBound = offset + count;
            while (offset < upperBound)
            {
                cache.Add(buffer[offset++]);
            }

            return bytesRead;
        }

        private async Task<int> ReadFromHttpAsync(byte[] buffer, int offset, int count)
        {
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };
            var to = Math.Min(position + count - 1, length - 1);
            request.Headers.Range = new RangeHeaderValue(position, to);

            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var bytesRead = await stream.ReadAsync(buffer, offset, count);
            return bytesRead;
        }

        private int ReadFromCache(byte[] buffer, int offset, int count)
        {
            if (Position >= cache.Count) return 0;

            int bytesRead = 0;
            for (int i = position; i < cache.Count && bytesRead < count; i++, bytesRead++)
            {
                buffer[offset++] = cache[i];
            }
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

            return Position;
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
