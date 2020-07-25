using CephissusBackend.Dtos;
using System;
using System.Threading.Tasks;

namespace CephissusBackend.Contracts
{
    public interface IAuthService
    {
        string GetAuthRedirect();
        Task<Guid> OauthCallbackAsync(string code);
    }
}
