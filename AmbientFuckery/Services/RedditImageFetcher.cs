﻿using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using AmbientFuckery.Tools;
using Newtonsoft.Json;
using RedditDtos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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
                var stream = new RangeRequestStream(httpClient, url);
                if (!(await stream.CanConnectAsync())) return null;
                var contentType = await stream.GetContentTypeAsync();
                if (string.IsNullOrEmpty(contentType)) return null;
                if (!allowedContentTypes.Contains(contentType)) return null;

                return new ImageData
                {
                    Stream = stream,
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
