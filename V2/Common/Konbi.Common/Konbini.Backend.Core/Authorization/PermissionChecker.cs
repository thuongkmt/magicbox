using Abp.Authorization;
using KonbiCloud.Authorization.Roles;
using KonbiCloud.Authorization.Users;

namespace KonbiCloud.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {

        }
    }
}
