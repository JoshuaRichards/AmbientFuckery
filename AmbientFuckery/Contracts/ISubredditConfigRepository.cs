using AmbientFuckery.Pocos;
using System.Collections.Generic;

namespace AmbientFuckery.Contracts
{
    public interface ISubredditConfigRepository
    {
        IEnumerable<SubredditConfig> GetSubredditConfigs();
    }
}