using AmbientFuckery.Pocos;
using AmbientFuckery.Repositories;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientFuckery.Services
{
    public class ImageCurator
    {
        private readonly ImageManipulator imageManipulator;
        private readonly RedditImageFetcher redditImageFetcher;
        private readonly SubredditConfigRepository subredditConfigRepository;

        public ImageCurator(
            ImageManipulator imageManipulator,
            RedditImageFetcher redditImageFetcher,
            SubredditConfigRepository subredditConfigRepository
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
