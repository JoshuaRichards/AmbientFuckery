using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using SixLabors.ImageSharp;

namespace AmbientFuckery.Services
{
    public class ImageManipulator : IImageManipulator
    {
        public IImageInfo ParseImage(ImageData image)
        {
            var stream = image.Stream;
            var info = Image.Identify(stream);
            return info;
        }
    }
}
