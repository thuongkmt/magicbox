using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.MultiTenancy;
using Abp.Runtime;
using Abp.Runtime.Session;
using System.Linq;
using System.Security.Claims;

namespace KonbiCloud.Sessions
{
    public class CustomSession : ClaimsAbpSession, ITransientDependency
    {
        public CustomSession(
            IPrincipalAccessor principalAccessor,
            IMultiTenancyConfig multiTenancy,
            ITenantResolver tenantResolver,
            IAmbientScopeProvider<SessionOverride> sessionOverrideScopeProvider) :
            base(principalAccessor, multiTenancy, tenantResolver, sessionOverrideScopeProvider)
        {

        }

        public string TrueAccessToken
        {
            get
            {
                return GetData("TMN_AccessToken");
            }
            set
            {
                SetData("TMN_AccessToken", value);
            }
        }

        public string TrueRefreshToken
        {
            get
            {
                return GetData("TMN_RefreshToken");
            }
            set
            {
                SetData("TMN_RefreshToken", value);
            }
        }

        public int TrueTokenExpire
        {
            get
            {
                int.TryParse(GetData("TMN_TokenExpire"), out int val);
                return val;
            }
            set
            {
                SetData("TMN_TokenExpire", value.ToString());
            }
        }

        public string TruePaymentId
        {
            get
            {
                return GetData("TMN_PaymentId");
            }
            set
            {
                SetData("TMN_PaymentId", value);
            }
        }

        public string TrueDepositPaymentId
        {
            get
            {
                return GetData("TMN_DepositPaymentId");
            }
            set
            {
                SetData("TMN_DepositPaymentId", value);
            }
        }

        public string UserMobile
        {
            get
            {
                return GetData("TMN_UserMobile");
            }
            set
            {
                SetData("TMN_UserMobile", value);
            }
        }

        private string GetData(string name)
        {
            var tokenClaim = PrincipalAccessor.Principal?.Claims.FirstOrDefault(c => c.Type == name);
            if (string.IsNullOrEmpty(tokenClaim?.Value))
            {
                return null;
            }

            return tokenClaim.Value;
        }

        private void SetData(string name, string value)
        {
            PrincipalAccessor.Principal?.Identities.First().AddClaim(new Claim(name, value));

            var tokenClaim = PrincipalAccessor.Principal?.Claims.FirstOrDefault(c => c.Type == name);
            if (tokenClaim == null)
            {
                PrincipalAccessor.Principal?.Identities.First().AddClaim(new Claim(name, value));
            }
            else
            {
                PrincipalAccessor.Principal?.Identities.First().RemoveClaim(tokenClaim);
                PrincipalAccessor.Principal?.Identities.First().AddClaim(new Claim(name, value));
            }
        }
    }
}
