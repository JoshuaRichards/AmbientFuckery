using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;

namespace CephissusBackend.Entities
{
    public class CephissusContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public CephissusContext(DbContextOptions<CephissusContext> options) : base(options)
        {
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public string Scope { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string ProfilePic { get; set; }
    }
}

