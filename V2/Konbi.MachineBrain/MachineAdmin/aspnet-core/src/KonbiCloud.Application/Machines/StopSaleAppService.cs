using Abp.UI;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using static KonbiCloud.BackgroundJobs.StopSaleMessageService;

namespace KonbiCloud.Machines
{
    public class StopSaleAppService : KonbiCloudAppServiceBase, IStopSaleAppService
    {
        private string BASE_URL { get; set; }
        private string baseDir { get; set; }
        public StopSaleAppService(IHostingEnvironment env)
        {
            BASE_URL = "http://localhost:9000";
            baseDir = env.ContentRootPath;
            var path = System.IO.Path.Combine(baseDir, @"App_Data\Logs\");
            logger =
                new LoggerConfiguration()
                    .WriteTo.RollingFile(path + "log-stopsale-{Date}.txt", shared: true)
                    .CreateLogger();
        }

        private readonly Serilog.ILogger logger;

        public void ChangeMachineStatus(MachineStatus status)
        {
            var client = new HttpClient { BaseAddress = new Uri(BASE_URL) };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                client.PostAsync("/api/machine/setstatus/" + status.ToString(), null);
                Log($"Change machine state to : " + status.ToString());
            }
            catch(Exception ex)
            {
                if(status == MachineStatus.MANUAL_STOPSALE)
                {
                    throw new UserFriendlyException("Error when disable stop sale");
                }
                else if(status == MachineStatus.IDLE)
                {
                    throw new UserFriendlyException("Error when enable stop sale");
                }

                Log($"Error when change machine state: " + ex.Message);
            }
        }

        public void Log(string message)
        {
            logger.Information(message);
        }
    }
}
