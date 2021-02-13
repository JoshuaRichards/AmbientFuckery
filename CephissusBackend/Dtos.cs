using CephissusBackend.Entities;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace CephissusBackend.Dtos
{
    public class AuthResponse
    {
        public string RedirectUrl { get; set; }
        public bool Authenticated { get; set; }
        public User User { get; set; }
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
        [JsonProperty("id_token")]
        public string IdToken { get; set; }
    }

    public class GoogleIdToken
    {
        [JsonProperty("sub")]
        public string Sub { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("picture")]
        public string Picture { get; set; }
    }

    public class ConfigResponse
    {
        public ICollection<ManagedAlbum> ManagedAlbums { get; set; }
    }
}
