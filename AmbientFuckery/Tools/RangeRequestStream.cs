using Nito.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AmbientFuckery.Tools
{
    public class RangeRequestStream : Stream
    {
        private const int MIN_CACHED_REQUEST_COUNT = 8192;

        private readonly HttpClient httpClient;
        private readonly string url;
        private readonly int length;

        private readonly List<byte> cache = new List<byte>();
        private int position = 0;

        public RangeRequestStream(HttpClient httpClient, string url, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
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

            int bytesReadFromHttp = AsyncToSync(() => ReadFromHttpAsync(buffer, offset, count));

            return bytesReadFromCache + bytesReadFromHttp;
        }

        private T AsyncToSync<T>(Func<Task<T>> f)
        {
            var task = Task.Run(async () => await f());
            return task.WaitAndUnwrapException();
        }

        private async Task<int> ReadFromHttpAsync(byte[] buffer, int offset, int count)
        {
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            int from = cache.Count;
            int requestCount = position + count - from;
            requestCount = Math.Max(requestCount, MIN_CACHED_REQUEST_COUNT);
            int resultOffset = position - from;

            int to = Math.Min(from + requestCount - 1, length - 1);
            request.Headers.Range = new RangeHeaderValue(from, to);

            using HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            int bytesToReturn = Math.Min(bytes.Length - resultOffset, count);

            Array.Copy(bytes, resultOffset, buffer, offset, bytesToReturn);
            cache.AddRange(bytes);

            position += bytesToReturn;

            return bytesToReturn;
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
        public override long Length => length;
        public override long Position
        {
            get => position;
            set
            {
                if (value >= length || value < 0) throw new ArgumentOutOfRangeException();

                position = (int)value;
            }
        }
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        #endregion
    }
}
