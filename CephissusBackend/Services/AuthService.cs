using CephissusBackend.Contracts;
using CephissusBackend.Dtos;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CephissusBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetAuthRedirect()
        {
            var clientId = Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_CLIENT_ID");
            var redirectUri = "https://localhost:6969/auth/oauthcallback";
            var scope = "https://www.googleapis.com/auth/photoslibrary";

            var url = new StringBuilder();
            url.Append("https://accounts.google.com/o/oauth2/v2/auth?");
            url.Append($"scope={HttpUtility.UrlEncode(scope)}&");
            url.Append($"response_type=code&");
            url.Append($"redirect_uri={HttpUtility.UrlEncode(redirectUri)}&");
            url.Append($"client_id={HttpUtility.UrlEncode(clientId)}");

            return url.ToString();
        }

        public async Task<GoogleTokenResponse> OauthCallback(string code)
        {
            var clientId = Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_CLIENT_SECRET");
            var redirectUri = "https://localhost:6969/auth/oauthcallback";

            var url = new StringBuilder();
            url.Append("https://oauth2.googleapis.com/token?");
            url.Append($"client_id={HttpUtility.UrlEncode(clientId)}&");
            url.Append($"client_secret={HttpUtility.UrlEncode(clientSecret)}&");
            url.Append($"redirect_uri={HttpUtility.UrlEncode(redirectUri)}&");
            url.Append($"code={HttpUtility.UrlEncode(code)}&");
            url.Append($"grant_type=authorization_code");

            var httpResponse = await _httpClient.PostAsync(url.ToString(), null);
            httpResponse.EnsureSuccessStatusCode();

            var rawResponse = await httpResponse.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<GoogleTokenResponse>(rawResponse);

            return response;
        }
    }
}
