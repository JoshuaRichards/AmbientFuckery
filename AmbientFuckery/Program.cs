using Newtonsoft.Json;
using RedditDtos;
using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace AmbientFuckery
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var imageFetcher = new RedditImageFetcher(new HttpClient());
            var photoUploader = new GooglePhotosUploader(new HttpClient());

            //var albumId = await photoUploader.CreateAlbumAsync();

            var images = await imageFetcher.GetImagesAsync();
            await photoUploader.UploadImages(images);
        }
    }
}
