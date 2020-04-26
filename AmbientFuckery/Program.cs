using System.Net.Http;
using System.Threading.Tasks;
using AmbientFuckery.Repositories;
using AmbientFuckery.Services;

namespace AmbientFuckery
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var imageFetcher = new RedditImageFetcher(new HttpClient());
            var photosManager = new GooglePhotosManager(new HttpClient());
            var imageCurator = new ImageCurator(new ImageManipulator(), imageFetcher, new SubredditConfigRepository());

            //var albumId = await photoUploader.CreateAlbumAsync();

            var images = imageCurator.GetImagesAsync();

            await photosManager.NukeAlbum();
            await photosManager.UploadImages(images);
        }
    }
}
