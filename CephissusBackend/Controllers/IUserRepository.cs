using CephissusBackend.Entities;
using System;
using System.Threading.Tasks;

namespace CephissusBackend.Contracts
{
    public interface IUserRepository
    {
        Task AddUser(User user);
        User GetById(Guid id);
        Guid? LookupUserId(string sub);
        Task UpdateTokensAsync(Guid id, string accessToken, string refreshToken, string scope);
    }
}