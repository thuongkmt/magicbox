using Abp.Dependency;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Common
{
    public class DetailLogService : IDetailLogService
    {
        private readonly Serilog.ILogger detailLogger; 
        public DetailLogService(IHostingEnvironment env)
        {
            var baseDir = env.ContentRootPath;
            var path = System.IO.Path.Combine(baseDir, @"App_Data\Logs\");
            //var desktopPath = @"\App_Data\Logs\";
            detailLogger =
                new LoggerConfiguration()
                    //.Enrich.WithExceptionDetails()
                    .WriteTo.RollingFile(path + "log-detail-{Date}.txt", shared: true)
                    .CreateLogger();
        }
        public void Log(string message)
        {
            detailLogger.Information(message);
        }
    }
}
