namespace AmbientFuckery.Pocos
{
    public class ImageData
    {
        public string Description { get; set; }
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public int Score { get; set; }
    }

    public class SubredditConfig
    {
        public string SubredditName { get; set; }
        public int MinScore { get; set; }
        public int MaxFetch { get; set; }
        public double MinAspectRatio { get; set; }
        public int MinHeight { get; set; }
    }
}
