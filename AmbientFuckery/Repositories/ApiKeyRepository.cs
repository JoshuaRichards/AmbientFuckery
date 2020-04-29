using AmbientFuckery.Contracts;
using System;

namespace AmbientFuckery.Repositories
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        public string GetClientId()
        {
            return Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_CLIENT_ID");
        }

        public string GetClientSecret()
        {
            return Environment.GetEnvironmentVariable("AMBIENT_FUCKERY_CLIENT_SECRET");
        }
    }
}
