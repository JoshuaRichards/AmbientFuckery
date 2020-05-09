using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using System.Collections.Generic;
using System.Linq;

namespace AmbientFuckery.Services
{
    public class ImageCurator : IImageCurator
    {
        private readonly IImageManipulator imageManipulator;
        private readonly IRedditImageFetcher redditImageFetcher;
        private readonly ISubredditConfigRepository subredditConfigRepository;

        public ImageCurator(
            IImageManipulator imageManipulator,
            IRedditImageFetcher redditImageFetcher,
            ISubredditConfigRepository subredditConfigRepository
        )
        {
            this.imageManipulator = imageManipulator;
            this.redditImageFetcher = redditImageFetcher;
            this.subredditConfigRepository = subredditConfigRepository;
        }

        public async IAsyncEnumerable<ImageData> GetImagesAsync()
        {
            var subredditConfigs = subredditConfigRepository.GetSubredditConfigs();

            foreach (var subredditConfig in subredditConfigs)
            {
                var images = redditImageFetcher.GetImagesAsync(subredditConfig.SubredditName)
                    .Where(i => subredditConfig.AllowNsfw || !i.IsNsfw)
                    .Where(i => IsBigEnough(i, subredditConfig))
                    .TakeWhile(i => i.Score >= subredditConfig.MinScore)
                    .Take(subredditConfig.MaxFetch);

                await foreach (var image in images) yield return image;
            }
        }

        private bool IsBigEnough(ImageData imageData, SubredditConfig subredditConfig)
        {
            var image = imageManipulator.ParseImage(imageData);

            double width = image.Width;
            double height = image.Height;
            double aspectRatio = width / height;

            return aspectRatio >= subredditConfig.MinAspectRatio &&
                height >= subredditConfig.MinHeight;
        }
    }
}
