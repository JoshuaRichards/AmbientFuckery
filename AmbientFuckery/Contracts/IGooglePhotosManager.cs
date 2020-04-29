using AmbientFuckery.Pocos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientFuckery.Contracts
{
    public interface IGooglePhotosManager
    {
        Task<string> CreateAlbumAsync();
        Task NukeAlbumAsync();
        Task UploadImages(IAsyncEnumerable<ImageData> images);
    }
}