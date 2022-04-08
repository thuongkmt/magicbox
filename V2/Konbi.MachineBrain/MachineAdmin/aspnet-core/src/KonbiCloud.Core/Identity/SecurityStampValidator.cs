using Abp.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using KonbiCloud.Authorization.Roles;
using KonbiCloud.Authorization.Users;
using KonbiCloud.MultiTenancy;

namespace KonbiCloud.Identity
{
    public class SecurityStampValidator : AbpSecurityStampValidator<Tenant, Role, User>
    {
        public SecurityStampValidator(
            IOptions<SecurityStampValidatorOptions> options, 
            SignInManager signInManager,
            ISystemClock systemClock) 
            : base(options, signInManager, systemClock)
        {
        }
    }
}