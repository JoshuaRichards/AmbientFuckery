using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using static System.Web.HttpUtility;

namespace AmbientFuckery.Services
{
    public class GoogleHttpClient : IGoogleHttpClient
    {
        private readonly HttpClient httpClient;
        private readonly IAmbientFuckeryDatabase database;
        private readonly IApiKeyRepository apiKeyRepository;

        public GoogleHttpClient(
            HttpClient httpClient,
            IAmbientFuckeryDatabase database,
            IApiKeyRepository apiKeyRepository
        )
        {
            this.httpClient = httpClient;
            this.database = database;
            this.apiKeyRepository = apiKeyRepository;
        }

        public async Task<string> GetStringAsync(string url)
        {
            await BlessHttpClient();
            return await httpClient.GetStringAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            await BlessHttpClient();
            return await httpClient.PostAsync(url, content);
        }

        private async Task BlessHttpClient()
        {
            var credentials = await GetCredentialsAsync();

            if (credentials.AccessTokenExpiry < DateTime.Now)
            {
                await RefreshCredentialsAsync(credentials);
                database.Update(credentials);
            }

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", credentials.AccessToken);
        }

        private async Task RefreshCredentialsAsync(GoogleCredentials credentials)
        {
            var clientId = apiKeyRepository.GetClientId();
            var clientSecret = apiKeyRepository.GetClientSecret();

            var url = new StringBuilder();
            url.Append("https://oauth2.googleapis.com/token?");
            url.Append($"client_id={clientId}&");
            url.Append($"client_secret={clientSecret}&");
            url.Append($"refresh_token={UrlEncode(credentials.RefreshToken)}&");
            url.Append($"grant_type=refresh_token");

            var responseMessage = await httpClient.PostAsync(url.ToString(), null);
            responseMessage.EnsureSuccessStatusCode();
            var response = JsonConvert.DeserializeObject<JObject>(
                await responseMessage.Content.ReadAsStringAsync()
            );

            var accessToken = response.Value<string>("access_token");
            var expirySeconds = response.Value<int>("expires_in");
            var expiry = DateTime.Now + TimeSpan.FromSeconds(expirySeconds - 5);

            credentials.AccessToken = accessToken;
            credentials.AccessTokenExpiry = expiry;
        }

        private async Task<GoogleCredentials> GetCredentialsAsync()
        {
            var credentials = database.Query<GoogleCredentials>().FirstOrDefault();
            if (credentials != null) return credentials;

            credentials = await PerformOAuthAsync();
            database.Insert(credentials);

            return credentials;
        }

        private async Task<GoogleCredentials> PerformOAuthAsync()
        {
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

                var message = "Access granted. You can close this browser now.";
                var bytes = Encoding.UTF8.GetBytes(message);

                context.Response.StatusCode = 200;
                context.Response.ContentLength64 = bytes.Length;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Close();

                listener.Stop();
                Console.WriteLine("OAuth response received.");
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

            var response = JsonConvert.DeserializeObject<JObject>(responseString);

            var accessToken = response.Value<string>("access_token");
            var refreshToken = response.Value<string>("refresh_token");

            var expirySeconds = response.Value<int>("expires_in");
            var expiry = DateTime.Now + TimeSpan.FromSeconds(expirySeconds - 5);

            return new GoogleCredentials
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = expiry,
            };
        }
    }
}
