﻿using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System;

namespace AmbientFuckery.Services
{
    public class ImageManipulator : IImageManipulator
    {
        public IImage ParseImage(ImageData image)
        {
            var img = Image.Load(image.Bytes, GetDecoder(image.ContentType));
            return img;
        }

        private IImageDecoder GetDecoder(string contentType)
        {
            switch (contentType)
            {
                case "image/jpeg":
                    return new JpegDecoder();
                case "image/png":
                    return new PngDecoder();
                default:
                    throw new NotImplementedException($"unhandled content type: {contentType}");
            }
        }
    }
}
