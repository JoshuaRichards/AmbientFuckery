using System.Net.Http;
using System.Threading.Tasks;

namespace AmbientFuckery.Contracts
{
    public interface IGoogleHttpClient
    {
        Task<string> GetStringAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content);
    }
}