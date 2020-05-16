using System;
using System.Collections.Generic;
using System.IO;
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

        public override long Length => GetLengthAsync().Result;
        public override long Position { get; set; } = 0;

        public long? length = null;

        private HttpResponseMessage _head = null;

        public RangeRequestStream(HttpClient httpClient, string url)
        {
            this.httpClient = httpClient;
            this.url = url;
        }

        public async Task<string> GetContentTypeAsync()
        {
            var head = await GetHeadAsync();
            return head.Content.Headers.ContentType.MediaType;
        }

        public async Task<bool> CanConnectAsync()
        {
            var head = await GetHeadAsync();
            return head.IsSuccessStatusCode;
        }

        private async Task<long> GetLengthAsync()
        {
            var head = await GetHeadAsync();
            length = head.Content.Headers.ContentLength;
            return length.Value;
        }

        private async Task<HttpResponseMessage> GetHeadAsync()
        {
            if (_head != null) return _head;

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Head,
                RequestUri = new Uri(url),
            };

            return _head = await httpClient.SendAsync(request);
        }

        private async Task<Stream> GetRangeAsync(int count)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };
            var to = Math.Min(Position + count - 1, (await GetLengthAsync()) - 1);
            request.Headers.Range = new RangeHeaderValue(Position, to);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position >= Length || count == 0) return 0;

            var rangeStream = GetRangeAsync(count).Result;
            var bytesRead = rangeStream.Read(buffer, offset, count);
            Position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                Position = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                Position += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                Position = Length + offset - 1;
            }
            else
            {
                throw new NotSupportedException();
            }

            if (Position < 0 || Position > Length - 1)
            {
                throw new InvalidOperationException();
            }

            return Position;
        }

        public bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            disposed = true;
            _head?.Dispose();

            base.Dispose(disposing);
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
