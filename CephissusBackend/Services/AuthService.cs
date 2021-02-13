using CephissusBackend.Contracts;
using CephissusBackend.Dtos;
using CephissusBackend.Entities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CephissusBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserRepository _userRepository;

        public AuthService(
            HttpClient httpClient,
            IUserRepository userRepository
        )
        {
            _httpClient = httpClient;
            _userRepository = userRepository;
        }

        public string GetAuthRedirect()
        {
            var clientId = Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_CLIENT_ID");
            var redirectUri = "https://localhost:4242/auth/oauthcallback";
            var scope = "https://www.googleapis.com/auth/photoslibrary openid profile email";

            var url = new StringBuilder();
            url.Append("https://accounts.google.com/o/oauth2/v2/auth?");
            url.Append($"scope={HttpUtility.UrlEncode(scope)}&");
            url.Append($"response_type=code&");
            url.Append($"redirect_uri={HttpUtility.UrlEncode(redirectUri)}&");
            url.Append($"client_id={HttpUtility.UrlEncode(clientId)}");

            return url.ToString();
        }

        public async Task<Guid> OauthCallbackAsync(string code)
        {
            var response = await ExchangeCodeAsync(code);
            var userId = await UpdateUserAsync(response);

            return userId;
        }

        private async Task<Guid> UpdateUserAsync(GoogleTokenResponse tokenResponse)
        {
            var handler = new JwtSecurityTokenHandler();
            var idToken = handler.ReadJwtToken(tokenResponse.IdToken);

            var id = _userRepository.LookupUserId(idToken.Subject);
            if (id != null)
            {
                await _userRepository.UpdateTokensAsync(
                    id.Value, tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.Scope
                );
                return id.Value;
            }

            var user = CreateUser(tokenResponse, idToken);
            await _userRepository.AddUser(user);
            return user.Id;
        }

        private User CreateUser(GoogleTokenResponse tokenResponse, JwtSecurityToken idToken)
        {
            var name = GetClaimValue(idToken, "name");
            var picture = GetClaimValue(idToken, "picture");
            var email = GetClaimValue(idToken, "email");

            return new User
            {
                Id = Guid.NewGuid(),
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                Scope = tokenResponse.Scope,
                Sub = idToken.Subject,
                DisplayName = name,
                ProfilePic = picture,
                Email = email,
            };
        }

        private string GetClaimValue(JwtSecurityToken token, string claimType)
        {
            return token.Claims.First(c => c.Type == claimType).Value;
        }

        private async Task<GoogleTokenResponse> ExchangeCodeAsync(string code)
        {
            var clientId = Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_CLIENT_SECRET");
            var redirectUri = "https://localhost:4242/auth/oauthcallback";

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
