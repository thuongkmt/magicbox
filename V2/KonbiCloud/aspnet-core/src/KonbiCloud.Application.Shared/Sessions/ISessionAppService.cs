using System.Threading.Tasks;
using Abp.Application.Services;
using KonbiCloud.Sessions.Dto;

namespace KonbiCloud.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();

        Task<UpdateUserSignInTokenOutput> UpdateUserSignInToken();
    }
}
