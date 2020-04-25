using Newtonsoft.Json;
using RedditDtos;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AmbientFuckery
{
    public class RedditImageFetcher
    {
        private readonly HttpClient httpClient;

        public RedditImageFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async IAsyncEnumerable<ImageData> GetImagesAsync()
        {
            foreach (var subreddit in new[] { "earthporn", "spaceporn", "wallpaper", "wallpapers" })
            {
                int max = 30;
                int count = 0;
                int minScore = 100;

                await foreach (var submission in GetSubmissionsAsync(subreddit))
                {
                    if (submission.Score < minScore) break;

                    var image = await DownloadImageAsync(submission.Url);
                    if (image == null) continue;

                    image.Description = $"https://reddit.com{submission.Permalink}";

                    yield return image;
                    if (++count >= max) break;
                }
            }
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

        private async Task<ImageData> DownloadImageAsync(string url)
        {
            var allowedContentTypes = new HashSet<string> { "image/jpeg", "image/png" };
            try
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (string.IsNullOrEmpty(contentType)) return null;
                if (!allowedContentTypes.Contains(contentType)) return null;

                return new ImageData
                {
                    Bytes = await response.Content.ReadAsByteArrayAsync(),
                    ContentType = contentType,
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public class ImageData
    {
        public string Description { get; set; }
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
    }
}
