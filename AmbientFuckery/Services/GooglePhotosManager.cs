using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace AmbientFuckery.Services
{
    public class GooglePhotosManager : IGooglePhotosManager
    {
        private readonly IGoogleHttpClient googleHttpClient;

        public GooglePhotosManager(
            IGoogleHttpClient httpClient
        )
        {
            this.googleHttpClient = httpClient;
        }

        public async Task<string> CreateAlbumAsync()
        {
            var request = @"{
                ""album"": {
                    ""title"": ""Ambient Fuckery""
                }
            }";
            var content = new StringContent(request, Encoding.UTF8, "application/json");

            var response = await googleHttpClient.PostAsync(
                "https://photoslibrary.googleapis.com/v1/albums", content
            );
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync())
                .Value<string>("id");
        }

        private string GetAlbumId()
        {
            return Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_ALBUM_ID");
        }

        public async Task NukeAlbumAsync()
        {
            var albumId = GetAlbumId();

            var buffer = new List<string>();
            var bufferSize = 50;
            await foreach (var id in GetMediaItemIdsAsync(albumId))
            {
                buffer.Add(id);
                if (buffer.Count >= bufferSize)
                {
                    await RemoveMediaItemsFromAlbumAsync(albumId, buffer);
                    buffer.Clear();
                }
            }
            if (buffer.Any())
            {
                await RemoveMediaItemsFromAlbumAsync(albumId, buffer);
            }
        }

        private async IAsyncEnumerable<string> GetMediaItemIdsAsync(string albumId)
        {
            var request = new Dictionary<string, object>
            {
                { "albumId", albumId },
                { "pageSize", 100 },
            };
            while (true)
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"
                );
                var url = "https://photoslibrary.googleapis.com/v1/mediaItems:search";
                var response = await googleHttpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                var mediaItemIds = json
                    .Value<JArray>("mediaItems")
                    ?.Select(o => o.Value<string>("id"))
                    ?? new string[0];
                foreach (var id in mediaItemIds)
                {
                    yield return id;
                }

                var nextPageToken = json.Value<string>("nextPageToken");
                if (string.IsNullOrEmpty(nextPageToken)) break;
                request["pageToken"] = nextPageToken;
            }
        }

        private async Task RemoveMediaItemsFromAlbumAsync(string albumId, IEnumerable<string> ids)
        {
            var request = new Dictionary<string, object>
            {
                { "mediaItemIds", ids }
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"
            );
            var url = $"https://photoslibrary.googleapis.com/v1/albums/{albumId}:batchRemoveMediaItems";
            var response = await googleHttpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UploadImages(IAsyncEnumerable<ImageData> images)
        {
            var uploads = new List<(string uploadToken, string description)>();
            await foreach (var image in images)
            {
                var content = new ByteArrayContent(image.Bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Headers.Add("X-Goog-Upload-Content-Type", image.ContentType);
                content.Headers.Add("X-Goog-Upload-Protocol", "raw");

                var response = await googleHttpClient.PostAsync(
                    "https://photoslibrary.googleapis.com/v1/uploads", content
                );

                response.EnsureSuccessStatusCode();
                uploads.Add((await response.Content.ReadAsStringAsync(), image.Description));
            }

            var albumId = GetAlbumId();
            var requestBody = new Dictionary<string, object>
            {
                { "albumId", albumId },
                {
                    "newMediaItems",
                    uploads.Select(u => new Dictionary<string, object>
                    {
                        { "description", u.description },
                        {
                            "simpleMediaItem",
                            new Dictionary<string, object>
                            {
                                { "uploadToken", u.uploadToken }
                            }
                        }
                    })
                }
            };

            var createContent = new StringContent(JsonConvert.SerializeObject(requestBody));
            createContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var createResponse = await googleHttpClient.PostAsync(
                "https://photoslibrary.googleapis.com/v1/mediaItems:batchCreate", createContent
            );
            createResponse.EnsureSuccessStatusCode();
        }
    }
}
