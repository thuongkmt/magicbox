using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;

namespace KonbiCloud
{
    [DependsOn(typeof(KonbiCloudClientModule), typeof(AbpAutoMapperModule))]
    public class KonbiCloudXamarinSharedModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Localization.IsEnabled = false;
            Configuration.BackgroundJobs.IsJobExecutionEnabled = false;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(KonbiCloudXamarinSharedModule).GetAssembly());
        }
    }
}