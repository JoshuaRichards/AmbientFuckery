using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using System.Collections.Generic;

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
                var count = 0;

                var images = redditImageFetcher.GetImagesAsync(subredditConfig.SubredditName);
                await foreach (var imageData in images)
                {
                    if (imageData.Score < subredditConfig.MinScore) break;

                    using var image = imageManipulator.ParseImage(imageData);

                    double width = image.Width;
                    double height = image.Height;
                    double aspectRatio = width / height;
                    if (aspectRatio < subredditConfig.MinAspectRatio) continue;
                    if (height < subredditConfig.MinHeight) continue;

                    yield return imageData;
                    if (++count >= subredditConfig.MaxFetch) break;
                }
            }
        }
    }
}
