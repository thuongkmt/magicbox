using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.PlateMenu.Dtos;
using System.Threading.Tasks;

namespace KonbiCloud.PlateMenus
{
    public interface IPlateMenusAppService : IApplicationService 
    {
        Task<PagedResultDto<PlateMenuDto>> GetAllPlateMenus(Dtos.PlateMenusInput input);

        Task<bool> UpdatePrice(Dtos.PlateMenusInput input);

        Task<bool> UpdatePriceStrategy(Dtos.PlateMenusInput input);
    }
}