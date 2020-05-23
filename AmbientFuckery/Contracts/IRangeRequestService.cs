using AmbientFuckery.Pocos;
using System.Threading.Tasks;

namespace AmbientFuckery.Contracts
{
    public interface IRangeRequestService
    {
        Task<ImageData> GetImageDataAsync(string url);
    }
}