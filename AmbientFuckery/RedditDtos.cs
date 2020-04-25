using Newtonsoft.Json;
using System.Collections.Generic;

namespace RedditDtos
{
    public class Listing
    {
        [JsonProperty("data")]
        public SubmissionList Data { get; set; }
    }

    public class SubmissionList
    {
        [JsonProperty("children")]
        public IEnumerable<Submission> Children { get; set; }
    }

    public class Submission
    {
        [JsonProperty("data")]
        public SubmissionData Data { get; set; }
    }

    public class SubmissionData
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
        [JsonProperty("permalink")]
        public string Permalink { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
    }
}
