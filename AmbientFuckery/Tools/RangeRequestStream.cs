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

        public override long Length { get; }
        public override long Position { get; set; } = 0;

        public RangeRequestStream(HttpClient httpClient, string url, long length)
        {
            this.httpClient = httpClient;
            this.url = url;

            Length = length;
        }

        private async Task<HttpResponseMessage> GetRangeAsync(int count)
        {
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };
            var to = Math.Min(Position + count - 1, Length - 1);
            request.Headers.Range = new RangeHeaderValue(Position, to);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return response;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position >= Length || count == 0) return 0;

            using var response = GetRangeAsync(count).Result;
            using var rangeStream = response.Content.ReadAsStreamAsync().Result;
            var bytesRead = rangeStream.Read(buffer, offset, count);
            Position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset - 1;
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (Position < 0 || Position > Length - 1)
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
