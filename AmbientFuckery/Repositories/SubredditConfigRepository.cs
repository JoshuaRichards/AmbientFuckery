using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using System;
using System.Collections.Generic;
using System.Text;

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
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                },
                new SubredditConfig
                {
                    SubredditName = "spaceporn",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                },
                new SubredditConfig
                {
                    SubredditName = "wallpapers",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                },
                new SubredditConfig
                {
                    SubredditName = "wallpaper",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                },
                new SubredditConfig
                {
                    SubredditName = "cityporn",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                },
                new SubredditConfig
                {
                    SubredditName = "cyberpunk",
                    MaxFetch = 30,
                    MinAspectRatio = 1.3d,
                    MinScore = 100,
                    MinHeight = 1080,
                },
            };
        }
    }
}
