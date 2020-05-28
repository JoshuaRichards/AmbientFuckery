using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using AmbientFuckery.Tools;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AmbientFuckery.Services
{
    public class RangeRequestService : IRangeRequestService
    {
        private readonly HttpClient httpClient;

        public RangeRequestService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ImageData> GetImageDataAsync(string url)
        {
            using var head = await httpClient.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Head,
            });
            if (!head.IsSuccessStatusCode) return null;
            var length = head.Content.Headers.ContentLength;
            if (length == null) return null;
            if (length.Value > 1024 * 1024 * 100) return null;
            if (!head.Headers.AcceptRanges.Contains("bytes")) return null;

            var contentType = head.Content.Headers.ContentType.MediaType;
            var stream = new RangeRequestStream(httpClient, url, (int)length.Value);

            return new ImageData
            {
                Stream = stream,
                ContentType = contentType,
            };
        }
    }
}
