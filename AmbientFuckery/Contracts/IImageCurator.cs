using AmbientFuckery.Pocos;
using System.Collections.Generic;

namespace AmbientFuckery.Contracts
{
    public interface IImageCurator
    {
        IAsyncEnumerable<ImageData> GetImagesAsync();
    }
}