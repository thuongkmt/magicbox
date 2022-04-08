using Abp.AspNetCore.Mvc.ViewComponents;

namespace KonbiCloud.Web.Public.Views
{
    public abstract class KonbiCloudViewComponent : AbpViewComponent
    {
        protected KonbiCloudViewComponent()
        {
            LocalizationSourceName = KonbiCloudConsts.LocalizationSourceName;
        }
    }
}