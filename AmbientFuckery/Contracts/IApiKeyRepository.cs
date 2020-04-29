using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientFuckery.Contracts
{
    public interface IApiKeyRepository
    {
        string GetClientId();
        string GetClientSecret();
    }
}
