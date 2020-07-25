using CephissusBackend.Contracts;
using CephissusBackend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CephissusBackend.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly CephissusContext _context;

        public UserRepository(CephissusContext context)
        {
            _context = context;
        }

        public User GetById(Guid id)
        {
            return _context.Users.First(u => u.Id == id);
        }

        public Guid? LookupUserId(string sub)
        {
            var user = _context.Users.FirstOrDefault(u => u.Sub == sub);
            if (user == null) return null;

            var id = user.Id;
            _context.Entry(user).State = EntityState.Detached;
            return id;
        }

        public async Task AddUser(User user)
        {
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateTokensAsync(Guid id, string accessToken, string refreshToken, string scope)
        {
            var user = new User
            {
                Id = id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Scope = scope,
            };

            _context.Attach(user);
            _context.Entry(user).Property(u => u.AccessToken).IsModified = true;
            _context.Entry(user).Property(u => u.RefreshToken).IsModified = true;
            _context.Entry(user).Property(u => u.Scope).IsModified = true;

            await _context.SaveChangesAsync();
        }
    }
}
