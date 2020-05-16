using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using AmbientFuckery.Tools;
using SixLabors.ImageSharp;

namespace AmbientFuckery.Services
{
    public class ImageManipulator : IImageManipulator
    {
        //public IMagickImageInfo ParseImage(ImageData image)
        public IImageInfo ParseImage(ImageData image)
        {
            var stream = image.Stream;
            //var info = new MagickImageInfo(stream);
            var info = Image.Identify(stream);
            return info;
        }
    }
}
