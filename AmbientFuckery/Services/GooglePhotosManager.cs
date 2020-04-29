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

using static System.Web.HttpUtility;

namespace AmbientFuckery.Services
{
    public class GooglePhotosManager : IGooglePhotosManager
    {
        private readonly HttpClient httpClient;
        private readonly IApiKeyRepository apiKeyRepository;

        public GooglePhotosManager(
            HttpClient httpClient,
            IApiKeyRepository apiKeyRepository
        )
        {
            this.httpClient = httpClient;
            this.apiKeyRepository = apiKeyRepository;
        }

        public async Task<string> CreateAlbumAsync()
        {
            await GetAuthTokenAsync();

            var request = @"{
                ""album"": {
                    ""title"": ""Ambient Fuckery""
                }
            }";
            var content = new StringContent(request, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(
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
            await GetAuthTokenAsync();
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
                var response = await httpClient.PostAsync(url, content);
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
            await GetAuthTokenAsync();
            var request = new Dictionary<string, object>
            {
                { "mediaItemIds", ids }
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"
            );
            var url = $"https://photoslibrary.googleapis.com/v1/albums/{albumId}:batchRemoveMediaItems";
            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UploadImages(IAsyncEnumerable<ImageData> images)
        {
            await GetAuthTokenAsync();

            var uploads = new List<(string uploadToken, string description)>();
            await foreach (var image in images)
            {
                var content = new ByteArrayContent(image.Bytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Headers.Add("X-Goog-Upload-Content-Type", image.ContentType);
                content.Headers.Add("X-Goog-Upload-Protocol", "raw");

                var response = await httpClient.PostAsync(
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

            var createResponse = await httpClient.PostAsync(
                "https://photoslibrary.googleapis.com/v1/mediaItems:batchCreate", createContent
            );
            createResponse.EnsureSuccessStatusCode();
        }

        private bool authTokenLoaded = false;
        private async Task GetAuthTokenAsync()
        {
            if (authTokenLoaded) return;
            var clientId = apiKeyRepository.GetClientId();
            var redirectUri = "http://localhost:6969";
            var scope = "https://www.googleapis.com/auth/photoslibrary";

            var url = new StringBuilder();
            url.Append("https://accounts.google.com/o/oauth2/v2/auth?");
            url.Append($"scope={UrlEncode(scope)}&");
            url.Append($"response_type=code&");
            url.Append($"redirect_uri={UrlEncode(redirectUri)}&");
            url.Append($"client_id={UrlEncode(clientId)}");

            string code;
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(redirectUri + "/");

                Console.WriteLine("Launching browser. Please authenticate.");
                Process.Start("cmd", "/c start " + url.ToString().Replace("&", "^&"));

                listener.Start();
                var context = await listener.GetContextAsync();

                code = context.Request.QueryString["code"];
                if (string.IsNullOrEmpty(code)) throw new Exception("Authentication failed");

                var response = "Access granted. You can close this browser now.";
                var bytes = Encoding.UTF8.GetBytes(response);

                context.Response.StatusCode = 200;
                context.Response.ContentLength64 = bytes.Length;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Close();

                listener.Stop();
            }

            var clientSecret = apiKeyRepository.GetClientSecret();
            url = new StringBuilder();
            url.Append("https://oauth2.googleapis.com/token?");
            url.Append($"client_id={UrlEncode(clientId)}&");
            url.Append($"client_secret={UrlEncode(clientSecret)}&");
            url.Append($"redirect_uri={UrlEncode(redirectUri)}&");
            url.Append($"code={UrlEncode(code)}&");
            url.Append($"grant_type=authorization_code");

            var authResponse = await httpClient.PostAsync(url.ToString(), null);
            var responseString = await authResponse.Content.ReadAsStringAsync();

            var token = JsonConvert.DeserializeObject<JObject>(responseString)
                .Value<string>("access_token");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            authTokenLoaded = true;
        }
    }
}
