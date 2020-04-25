using Newtonsoft.Json;
using RedditDtos;
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

        public async Task<IEnumerable<ImageData>> GetImagesAsync()
        {
            var url = "https://www.reddit.com/r/earthporn/top/.json?t=day";

            var response = await httpClient.GetStringAsync(url);
            var submissions = JsonConvert.DeserializeObject<Listing>(response)
                .Data.Children.Select(x => x.Data).ToList();

            var ret = new List<ImageData>();
            foreach (var submission in submissions)
            {
                var bytes = await GetBytesAsync(submission.Url);
                if (bytes == null) continue;

                ret.Add(new ImageData
                {
                    Bytes = bytes,
                    Description = $"reddit.com{submission.Permalink}",
                });
            }
            return ret;
        }

        private async Task<byte[]> GetBytesAsync(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;
                if (response.Content.Headers.ContentType.MediaType != "image/jpeg") return null;

                return await response.Content.ReadAsByteArrayAsync();
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
    }
}
