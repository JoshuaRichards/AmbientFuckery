using System;
using System.IO;

namespace AmbientFuckery.Pocos
{
    public class ImageData
    {
        public string Description { get; set; }
        public Stream Stream { get; set; }
        public string ContentType { get; set; }
        public int Score { get; set; }
        public bool IsNsfw { get; set; }
    }

    public class ImageUpload
    {
        public string UploadToken { get; set; }
        public string Description { get; set; }
    }

    public class SubredditConfig
    {
        public string SubredditName { get; set; }
        public int MinScore { get; set; }
        public int MaxFetch { get; set; }
        public double MinAspectRatio { get; set; }
        public int MinHeight { get; set; }
        public bool AllowNsfw { get; set; }
    }

    public class GoogleCredentials
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
    }

    public class Album
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
