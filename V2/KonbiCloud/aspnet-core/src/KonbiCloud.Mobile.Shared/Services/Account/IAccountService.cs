using System.Threading.Tasks;
using KonbiCloud.ApiClient.Models;

namespace KonbiCloud.Services.Account
{
    public interface IAccountService
    {
        AbpAuthenticateModel AbpAuthenticateModel { get; set; }
        AbpAuthenticateResultModel AuthenticateResultModel { get; set; }
        Task LoginUserAsync();
    }
}
