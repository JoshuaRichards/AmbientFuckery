using AmbientFuckery.Contracts;
using AmbientFuckery.Pocos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AmbientFuckery.Repositories
{
    public class AlbumService : IAlbumService
    {
        private readonly IAmbientFuckeryDatabase ambientFuckeryDatabase;
        private readonly IGoogleHttpClient googleHttpClient;

        public AlbumService(
            IAmbientFuckeryDatabase ambientFuckeryDatabase,
            IGoogleHttpClient googleHttpClient
        )
        {
            this.ambientFuckeryDatabase = ambientFuckeryDatabase;
            this.googleHttpClient = googleHttpClient;
        }

        public async Task<string> GetAlbumIdAsync()
        {
            var album = ambientFuckeryDatabase.Query<Album>().SingleOrDefault();
            if (album != null) return album.Id;

            album = await CreateAlbumAsync();
            ambientFuckeryDatabase.Insert(album);
            return album.Id;
        }

        public async Task<Album> CreateAlbumAsync()
        {
            const string title = "Ambient Fuckery";

            var request = new { album = new { title } };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await googleHttpClient.PostAsync(
                "https://photoslibrary.googleapis.com/v1/albums", content
            );
            response.EnsureSuccessStatusCode();

            var id = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync())
                .Value<string>("id");
            return new Album
            {
                Id = id,
                Name = title,
            };
        }
    }
}
