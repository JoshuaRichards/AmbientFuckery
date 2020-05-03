﻿using Newtonsoft.Json;
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
        [JsonProperty("after")]
        public string After { get; set; }
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
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("over_18")]
        public bool Over18 { get; set; }
    }
}
