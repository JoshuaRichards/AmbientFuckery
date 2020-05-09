using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using AmbientFuckery.Tools;
using Newtonsoft.Json;
using RedditDtos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmbientFuckery.Services
{
    public class RedditImageFetcher : IRedditImageFetcher
    {
        private readonly HttpClient httpClient;

        public RedditImageFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async IAsyncEnumerable<ImageData> GetImagesAsync(string subreddit)
        {
            await foreach (var submission in GetSubmissionsAsync(subreddit))
            {
                var url = GetUrl(submission);
                var image = await GetImageDataAsync(url);
                if (image == null) continue;

                image.Description = $"https://reddit.com{submission.Permalink}";
                image.Score = submission.Score;
                image.IsNsfw = submission.Over18;

                yield return image;
            }
        }

        private string GetUrl(SubmissionData submission)
        {
            if (submission.Domain != "imgur.com") return submission.Url;
            if (Regex.IsMatch(submission.Url, @"\.\w{3,}$")) return submission.Url;

            return submission.Url + ".jpg";
        }

        private async IAsyncEnumerable<SubmissionData> GetSubmissionsAsync(string subreddit)
        {
            var baseUrl = $"https://www.reddit.com/r/{subreddit}/top/.json?t=day";
            var url = baseUrl;

            while (true)
            {
                var response = await httpClient.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<Listing>(response).Data;

                foreach (var submission in data.Children)
                {
                    yield return submission.Data;
                }

                if (data.After == null) break;

                url = $"{baseUrl}&after={data.After}";
            }
        }

        private async Task<ImageData> GetImageDataAsync(string url)
        {
            var allowedContentTypes = new HashSet<string> { "image/jpeg", "image/png" };
            try
            {
                var response = await httpClient.SendAsync(new HttpRequestMessage { RequestUri = new Uri(url), Method = HttpMethod.Head });
                if (!response.IsSuccessStatusCode) return null;
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (string.IsNullOrEmpty(contentType)) return null;
                if (!allowedContentTypes.Contains(contentType)) return null;

                var length = response.Content.Headers.ContentLength.Value;

                return new ImageData
                {
                    Stream = StreamImageAsync(url, length),
                    ContentType = contentType,
                };
            }
            catch
            {
                return null;
            }
        }

        private async IAsyncEnumerable<byte> StreamImageAsync(string url, long length)
        {
            const int bytesPerRequest = 10 * 1024;

            long pos = 0;
            while (pos < length - 1)
            {
                var to = Math.Min(pos + bytesPerRequest - 1, length - 1);
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get,
                };
                request.Headers.Range = new RangeHeaderValue(pos, to);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                pos += response.Content.Headers.ContentLength.Value;
                foreach (var b in await response.Content.ReadAsByteArrayAsync())
                {
                    yield return b;
                }
            }
        }
    }

}
