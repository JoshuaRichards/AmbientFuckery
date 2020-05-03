using AmbientFuckery.Pocos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientFuckery.Contracts
{
    public interface IGooglePhotosManager
    {
        Task ReplaceImagesAsync(IAsyncEnumerable<ImageData> images);
    }
}