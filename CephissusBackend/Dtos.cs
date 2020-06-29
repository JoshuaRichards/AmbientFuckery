using Newtonsoft.Json;

namespace CephissusBackend.Dtos
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string RedirectUrl { get; set; }
        public bool Authenticated { get; set; }
    }

    public class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
