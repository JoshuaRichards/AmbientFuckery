using AmbientFuckery.Contracts;
using Autofac;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AmbientFuckery
{
    public class Program
    {
        private static IContainer container;

        public static async Task Main(string[] args)
        {
            Init();

            var imageCurator = container.Resolve<IImageCurator>();
            var photosManager = container.Resolve<IGooglePhotosManager>();

            var images = imageCurator.GetImagesAsync();
            await photosManager.NukeAlbumAsync();
            await photosManager.UploadImages(images);
        }

        private static void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AsImplementedInterfaces();

            builder.Register(_ => new HttpClient());

            container = builder.Build();
        }
    }
}
