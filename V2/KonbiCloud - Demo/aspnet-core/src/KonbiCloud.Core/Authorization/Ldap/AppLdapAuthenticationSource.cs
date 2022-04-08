using Abp.Zero.Ldap.Authentication;
using Abp.Zero.Ldap.Configuration;
using KonbiCloud.Authorization.Users;
using KonbiCloud.MultiTenancy;

namespace KonbiCloud.Authorization.Ldap
{
    public class AppLdapAuthenticationSource : LdapAuthenticationSource<Tenant, User>
    {
        public AppLdapAuthenticationSource(ILdapSettings settings, IAbpZeroLdapModuleConfig ldapModuleConfig)
            : base(settings, ldapModuleConfig)
        {
        }
    }
}