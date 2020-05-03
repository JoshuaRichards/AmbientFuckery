using System.Threading.Tasks;

namespace AmbientFuckery.Contracts
{
    public interface IAlbumService
    {
        Task<string> GetAlbumIdAsync();
    }
}