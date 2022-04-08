using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using System;
using System.Linq;
using KonbiCloud.TemperatureLogs;
using System.Net.Http;
using System.Net.Http.Headers;
using KonbiCloud.Common;
using Abp.Net.Mail;
using KonbiCloud.Configuration;
using Abp.Configuration;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace KonbiCloud.BackgroundJobs
{
    public class StopSaleMessageService : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<TemperatureLog> _temperatureLogsRepository;
        private readonly IStopSaleSlackService _stopSaleSlackService;
        private readonly IEmailSender _emailSender;
        private readonly ISettingManager _settingManager;

        private int stopSaleTimeSpan = 60;
        private const int stopSaleTemperature = -999;
        private string BASE_URL { get; set; }
        private string baseDir { get; set; }
        private readonly Serilog.ILogger logger;


        public StopSaleMessageService(
            AbpTimer timer,
            IRepository<TemperatureLog> temperatureLogsRepository,
            IStopSaleSlackService stopSaleSlackService,
            IEmailSender emailSender,
            IHostingEnvironment env,
            ISettingManager settingManager
        )
       : base(timer)
        {
            Timer.Period = 60 * 1000; //1 minute
            _temperatureLogsRepository = temperatureLogsRepository;
            _stopSaleSlackService = stopSaleSlackService;
            _emailSender = emailSender;
            _settingManager = settingManager;
            BASE_URL = "http://localhost:9000";
            baseDir = env.ContentRootPath;
            var path = System.IO.Path.Combine(baseDir, @"App_Data\Logs\");
            logger =
                new LoggerConfiguration()
                    .WriteTo.RollingFile(path + "log-stopsale-{Date}.txt", shared: true)
                    .CreateLogger();
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            bool _enableStopSale;
            bool.TryParse(_settingManager.GetSettingValue(AppSettingNames.EnableStopSale), out _enableStopSale);
            int.TryParse(_settingManager.GetSettingValue(AppSettingNames.StopSaleTimeSpan), out stopSaleTimeSpan);

            if (!_enableStopSale)
            {
                Log($"This machine is disabled stop sale");
                return;
            }
            try
            {
                Log($"Run stop sales job");

                int? chilledTemperature = 10;

                chilledTemperature = SettingManager.GetSettingValue<int>(AppSettingNames.NormalTemperature);

                Log($"Stop sale temperature = " + chilledTemperature);

                if (chilledTemperature == stopSaleTemperature)
                {
                    Log($"Job stop, setting temperature is stop sales temperature");
                    return;
                }

                //Check if the temperature is abnormal continuously in 10 minutes
                var startTime = DateTime.Now.AddMinutes(-1 * stopSaleTimeSpan);

                var stopSaleLogs = _temperatureLogsRepository.GetAll().Where(x => (x.CreationTime >= startTime && x.CreationTime <= DateTime.Now));

                var abnormalTmp = _temperatureLogsRepository.GetAll().Where(x => (x.CreationTime >= startTime && x.CreationTime <= DateTime.Now) && x.Temperature <= chilledTemperature);

                var client = new HttpClient { BaseAddress = new Uri(BASE_URL) };

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync("/api/machine/status").Result;

                dynamic res;
                using (var content = response.Content)

                {
                    var result = content.ReadAsStringAsync();
                    res = result.Result.Replace("\"", "");
                }

                if (res == MachineStatus.MANUAL_STOPSALE.ToString() ||
                    res == MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC.ToString() ||
                    res == MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC_TRACING.ToString() ||
                    res == MachineStatus.UNLOADING_PRODUCT.ToString())
                {
                    Log($"Machine is in manual stop sale state and can not auto resume");
                    return;
                }

                //If machine status is IDLE, send StopSale message
                if (!abnormalTmp.Any() && stopSaleLogs.Count() >= (stopSaleTimeSpan/2))
                {
                    if (res == MachineStatus.IDLE.ToString())
                    {
                        Log($"Send stop sale message to machine");

                        var updateStopSaleStatus = client.PostAsync("/api/machine/setstatus/STOPSALE", null);
                        Log($"Change stop sale status result = " + updateStopSaleStatus.Result);

                        //Send slack message
                        var machineName = SettingManager.GetSettingValue(AppSettingNames.MachineName);
                        var message = SettingManager.GetSettingValue(AppSettingNames.StopSaleMessage);

                        _stopSaleSlackService.SendAlert(machineName, message);
                    }
                }
                else
                {
                    if (res == MachineStatus.STOPSALE.ToString())
                    {
                        //SHasStopSaleend message to resume sales after the temperature is back to normal
                        Log($"Send resume sale message to machine");

                        var updateIdleStatus = client.PostAsync("/api/machine/setstatus/IDLE", null);
                        Log($"Resume sale result = " + updateIdleStatus.Result);
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        public void Log(string message)
        {
            logger.Information(message);
        }
        public enum MachineStatus
        {
            NONE = -1,
            IDLE,
            TRANSACTION_START,
            TRANSACTION_FAILED,
            TRANSACTION_DONE,
            STOPSALE,
            MANUAL_STOPSALE,
            TRANSACTION_DOOR_CLOSED,
            VALIDATE_CARD_FAILED,
            STOPSALE_DUE_TO_PAYMENT,
            TRANSACTION_WAITTING_FOR_PAYMENT,
            FAIL_TO_OPEN_THE_DOOR,
            UNSTABLE_TAGS_DIAGNOSTIC,
            UNSTABLE_TAGS_DIAGNOSTIC_TRACING,
            UNLOADING_PRODUCT
        }
    }
}