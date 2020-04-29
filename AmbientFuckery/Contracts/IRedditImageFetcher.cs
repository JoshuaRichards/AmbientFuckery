using AmbientFuckery.Pocos;
using System.Collections.Generic;

namespace AmbientFuckery.Contracts
{
    public interface IRedditImageFetcher
    {
        IAsyncEnumerable<ImageData> GetImagesAsync(string subreddit);
    }
}