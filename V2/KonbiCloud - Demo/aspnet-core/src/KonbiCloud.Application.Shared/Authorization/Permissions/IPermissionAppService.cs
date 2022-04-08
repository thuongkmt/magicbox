using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization.Permissions.Dto;

namespace KonbiCloud.Authorization.Permissions
{
    public interface IPermissionAppService : IApplicationService
    {
        ListResultDto<FlatPermissionWithLevelDto> GetAllPermissions();
    }
}
