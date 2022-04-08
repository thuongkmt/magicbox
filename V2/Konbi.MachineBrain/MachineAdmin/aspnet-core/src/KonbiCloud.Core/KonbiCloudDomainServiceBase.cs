using Abp.Domain.Services;

namespace KonbiCloud
{
    public abstract class KonbiCloudDomainServiceBase : DomainService
    {
        /* Add your common members for all your domain services. */

        protected KonbiCloudDomainServiceBase()
        {
            LocalizationSourceName = KonbiCloudConsts.LocalizationSourceName;
        }
    }
}
