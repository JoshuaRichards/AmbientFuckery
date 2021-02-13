using CephissusBackend.Dtos;
using CephissusBackend.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CephissusBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        [Route("values")]
        [HttpGet]
        public ActionResult<object> GetValues()
        {
            var rng = new Random();

            return Enumerable.Range(1, 20).Select(_ => rng.Next()).ToList();
        }

        [HttpGet]
        public ActionResult<ConfigResponse> Config()
        {
            return new ConfigResponse
            {
                ManagedAlbums = new List<ManagedAlbum>
                {
                    new ManagedAlbum
                    {
                        Id = Guid.NewGuid(),
                        AlbumId = "alsdfkjasdflkj",
                        AlbumName = "Ambient Fuckery",
                        RefreshSchedule = Interval.Daily,
                        SearchPeriod = Interval.Daily,
                        SubredditConfigs = new List<SubredditConfig>
                        {
                            new SubredditConfig
                            {
                                Id = Guid.NewGuid(),
                                AllowNsfw = false,
                                MaxFetch = 30,
                                MinAspectRatio = 1.2,
                                MinHeight = 1000,
                                MinScore = 100,
                                SubredditName = "wallpaper",
                            },
                            new SubredditConfig
                            {
                                Id = Guid.NewGuid(),
                                AllowNsfw = false,
                                MaxFetch = 30,
                                MinAspectRatio = 1.2,
                                MinHeight = 1000,
                                MinScore = 100,
                                SubredditName = "wallpapers",
                            },
                            new SubredditConfig
                            {
                                Id = Guid.NewGuid(),
                                AllowNsfw = false,
                                MaxFetch = 30,
                                MinAspectRatio = 1.2,
                                MinHeight = 1000,
                                MinScore = 100,
                                SubredditName = "space",
                            },
                        }
                    },
                    new ManagedAlbum
                    {
                        Id = Guid.NewGuid(),
                        AlbumId = "adl;sfkjasdflkj",
                        AlbumName = "Ambient Fuckery 2",
                        RefreshSchedule = Interval.Weekly,
                        SearchPeriod = Interval.Weekly,
                        SubredditConfigs = new List<SubredditConfig>
                        {
                            new SubredditConfig
                            {
                                Id = Guid.NewGuid(),
                                AllowNsfw = false,
                                MaxFetch = 30,
                                MinAspectRatio = 1.2,
                                MinHeight = 1000,
                                MinScore = 100,
                                SubredditName = "wallpaper",
                            },
                            new SubredditConfig
                            {
                                Id = Guid.NewGuid(),
                                AllowNsfw = false,
                                MaxFetch = 30,
                                MinAspectRatio = 1.2,
                                MinHeight = 1000,
                                MinScore = 100,
                                SubredditName = "wallpapers",
                            },
                            new SubredditConfig
                            {
                                Id = Guid.NewGuid(),
                                AllowNsfw = false,
                                MaxFetch = 30,
                                MinAspectRatio = 1.2,
                                MinHeight = 1000,
                                MinScore = 100,
                                SubredditName = "space",
                            },
                        }
                    },
                }
            };
        }
    }
}
