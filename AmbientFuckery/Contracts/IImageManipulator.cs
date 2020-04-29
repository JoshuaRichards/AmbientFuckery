using AmbientFuckery.Pocos;
using SixLabors.ImageSharp;

namespace AmbientFuckery.Contracts
{
    public interface IImageManipulator
    {
        IImage ParseImage(ImageData image);
    }
}