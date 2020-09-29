using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface INetworkService
    {
        Task<bool> IsConnectedToInternet();
    }
}