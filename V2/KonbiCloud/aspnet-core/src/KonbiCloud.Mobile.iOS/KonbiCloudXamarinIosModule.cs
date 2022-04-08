using Abp.Modules;
using Abp.Reflection.Extensions;

namespace KonbiCloud
{
    [DependsOn(typeof(KonbiCloudXamarinSharedModule))]
    public class KonbiCloudXamarinIosModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(KonbiCloudXamarinIosModule).GetAssembly());
        }
    }
}