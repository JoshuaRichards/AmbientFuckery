using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace AmbientFuckery.Services
{
    public class GooglePhotosManager : IGooglePhotosManager
    {
        private readonly IGoogleHttpClient googleHttpClient;
        private readonly IAlbumService albumService;

        public GooglePhotosManager(
            IGoogleHttpClient googleHttpClient,
            IAlbumService albumService
        )
        {
            this.googleHttpClient = googleHttpClient;
            this.albumService = albumService;
        }

        public async Task ReplaceImagesAsync(IAsyncEnumerable<ImageData> images)
        {
            var albumId = await albumService.GetAlbumIdAsync();

            var uploads = await BatchUploadAsync(images);
            var existingItemIds = await GetMediaItemIdsAsync(albumId).ToListAsync();

            var newMediaItemIds = await AddUploadsToAlbumAsync(albumId, uploads);
            newMediaItemIds = new HashSet<string>(newMediaItemIds);

            var itemsToRemove = existingItemIds.Where(x => !newMediaItemIds.Contains(x));

            await RemoveMediaItemsFromAlbumAsync(albumId, itemsToRemove);
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
            const int pageSize = 50;

            var list = ids.ToList();
            while (list.Any())
            {
                var mediaItemIds = list.Take(pageSize);

                var request = new { mediaItemIds };
                var content = new StringContent(
                    JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"
                );
                var url = $"https://photoslibrary.googleapis.com/v1/albums/{albumId}:batchRemoveMediaItems";
                var response = await googleHttpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                list.RemoveRange(0, Math.Min(pageSize, list.Count));
            }
        }

        private async Task<IEnumerable<string>> AddUploadsToAlbumAsync(string albumId, IEnumerable<ImageUpload> uploads)
        {
            var requestBody = new
            {
                albumId,
                newMediaItems = uploads.Select(u => new
                {
                    description = u.Description,
                    simpleMediaItem = new
                    {
                        uploadToken = u.UploadToken,
                    },
                }),
            };

            var createContent = new StringContent(JsonConvert.SerializeObject(requestBody));
            createContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var createResponse = await googleHttpClient.PostAsync(
                "https://photoslibrary.googleapis.com/v1/mediaItems:batchCreate", createContent
            );
            createResponse.EnsureSuccessStatusCode();

            return JObject.Parse(await createResponse.Content.ReadAsStringAsync())
                .Value<JArray>("newMediaItemResults")
                .Select(o => o.Value<JObject>("mediaItem"))
                .Select(o => o.Value<string>("id"))
                .ToList();
        }

        private async Task<List<ImageUpload>> BatchUploadAsync(IAsyncEnumerable<ImageData> images)
        {
            var uploads = new List<ImageUpload>();
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

                var uploadToken = await response.Content.ReadAsStringAsync();
                uploads.Add(new ImageUpload
                {
                    UploadToken = uploadToken,
                    Description = image.Description,
                });
            }

            return uploads;
        }
    }
}
