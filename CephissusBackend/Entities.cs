﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
        [JsonIgnore]
        [Required]
        public string AccessToken { get; set; }
        [JsonIgnore]
        [Required]
        public string RefreshToken { get; set; }
        [JsonIgnore]
        [Required]
        public string Scope { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string ProfilePic { get; set; }
        [JsonIgnore]
        [Required]
        public string Sub { get; set; }
        [Required]
        public string Email { get; set; }
    }

    public class SubredditConfig
    {
        public Guid Id { get; set; }
        public string SubredditName { get; set; }
        public int MicScore { get; set; }
        public int MaxFetch { get; set; }
        public double MinAspectRatio { get; set; }
        public int MinHeight { get; set; }
        public bool AllowNsfw { get; set; }

        [JsonIgnore]
        public virtual User User { get; set; }
    }

}

