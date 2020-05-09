using AmbientFuckery.Pocos;
using ImageMagick;

namespace AmbientFuckery.Contracts
{
    public interface IImageManipulator
    {
        IMagickImageInfo ParseImage(ImageData image);
    }
}