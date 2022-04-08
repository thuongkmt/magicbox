using Abp.Modules;
using Abp.Reflection.Extensions;

namespace KonbiCloud
{
    [DependsOn(typeof(KonbiCloudXamarinSharedModule))]
    public class KonbiCloudXamarinAndroidModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(KonbiCloudXamarinAndroidModule).GetAssembly());
        }
    }
}