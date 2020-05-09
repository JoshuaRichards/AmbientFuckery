using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using AmbientFuckery.Tools;
using ImageMagick;

namespace AmbientFuckery.Services
{
    public class ImageManipulator : IImageManipulator
    {
        public IMagickImageInfo ParseImage(ImageData image)
        {
            using var stream = new AsyncEnumerableStream(image.Stream);
            var info = new MagickImageInfo(stream);
            return info;
        }
    }
}
