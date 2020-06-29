using CephissusBackend.Dtos;
using System.Threading.Tasks;

namespace CephissusBackend.Contracts
{
    public interface IAuthService
    {
        string GetAuthRedirect();
        Task<GoogleTokenResponse> OauthCallback(string code);
    }
}
