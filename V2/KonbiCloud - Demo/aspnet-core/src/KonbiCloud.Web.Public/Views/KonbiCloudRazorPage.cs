using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace KonbiCloud.Web.Public.Views
{
    public abstract class KonbiCloudRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }

        protected KonbiCloudRazorPage()
        {
            LocalizationSourceName = KonbiCloudConsts.LocalizationSourceName;
        }
    }
}
