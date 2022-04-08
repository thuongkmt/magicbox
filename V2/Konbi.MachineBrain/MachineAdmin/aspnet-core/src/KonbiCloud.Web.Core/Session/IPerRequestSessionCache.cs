using System.Threading.Tasks;
using KonbiCloud.Sessions.Dto;

namespace KonbiCloud.Web.Session
{
    public interface IPerRequestSessionCache
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformationsAsync();
    }
}
