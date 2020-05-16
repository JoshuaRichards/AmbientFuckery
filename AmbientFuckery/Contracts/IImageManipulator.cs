using AmbientFuckery.Pocos;
using SixLabors.ImageSharp;

namespace AmbientFuckery.Contracts
{
    public interface IImageManipulator
    {
        IImageInfo ParseImage(ImageData image);
    }
}