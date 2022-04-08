using System.Collections.Generic;
using KonbiCloud.Authorization.Permissions.Dto;

namespace KonbiCloud.Authorization.Users.Dto
{
    public class GetUserPermissionsForEditOutput
    {
        public List<FlatPermissionDto> Permissions { get; set; }

        public List<string> GrantedPermissionNames { get; set; }
    }
}