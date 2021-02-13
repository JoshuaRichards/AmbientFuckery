using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CephissusBackend.Entities
{
    public class CephissusContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<SubredditConfig> SubredditConfigs { get; set; }
        public DbSet<ManagedAlbum> ManagedAlbums { get; set; }

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
        [Required]
        public string SubredditName { get; set; }
        public int MinScore { get; set; }
        public int MaxFetch { get; set; }
        public double MinAspectRatio { get; set; }
        public int MinHeight { get; set; }
        public bool AllowNsfw { get; set; }

        [JsonIgnore]
        [Required]
        public virtual User User { get; set; }
        [JsonIgnore]
        [Required]
        public virtual ManagedAlbum ManagedAlbum { get; set; }
    }

    public class ManagedAlbum
    {
        public Guid Id { get; set; }
        [Required]
        public string AlbumId { get; set; }
        [Required]
        public string AlbumName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Interval RefreshSchedule { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Interval SearchPeriod { get; set; }

        public virtual ICollection<SubredditConfig> SubredditConfigs { get; set; }
    }

    public enum Interval
    {
        Daily = 1,
        Weekly,
        Monthly,
    }

}

