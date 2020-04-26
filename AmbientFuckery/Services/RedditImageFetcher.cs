using AmbientFuckery.Pocos;
using Newtonsoft.Json;
using RedditDtos;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AmbientFuckery.Services
{
    public class RedditImageFetcher
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
                var image = await DownloadImageAsync(submission.Url);
                if (image == null) continue;

                image.Description = $"https://reddit.com{submission.Permalink}";
                image.Score = submission.Score;

                yield return image;
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

}
