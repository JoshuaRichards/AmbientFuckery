using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using System.Collections.Generic;

namespace AmbientFuckery.Repositories
{
    public class SubredditConfigRepository : ISubredditConfigRepository
    {
        public IEnumerable<SubredditConfig> GetSubredditConfigs()
        {
            return new[]
            {
                new SubredditConfig
                {
                    SubredditName = "earthporn",
                    MaxFetch = 30,
                    MinAspectRatio = 1.2d,
                    MinScore = 100,
                    MinHeight = 1000,
                    AllowNsfw = false,
                },
                new SubredditConfig
                {
                    SubredditName = "spaceporn",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                    AllowNsfw = false,
                },
                new SubredditConfig
                {
                    SubredditName = "wallpapers",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                    AllowNsfw = false,
                },
                new SubredditConfig
                {
                    SubredditName = "wallpaper",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                    AllowNsfw = false,
                },
                new SubredditConfig
                {
                    SubredditName = "cityporn",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                    AllowNsfw = false,
                },
            };
        }
    }
}
