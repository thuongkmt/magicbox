using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.CustomerKonbiWallet.Dtos;
using System.Threading.Tasks;

namespace KonbiCloud.CustomerKonbiWallet
{
    public interface ICustomerKonbiWalletService : IApplicationService
    {
        Task<PagedResultDto<CustomerWallet>> GetAll(GetAllCustomersInput input);
    }
}
