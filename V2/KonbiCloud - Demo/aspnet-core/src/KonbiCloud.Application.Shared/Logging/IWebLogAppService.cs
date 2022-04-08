using Abp.Application.Services;
using KonbiCloud.Dto;
using KonbiCloud.Logging.Dto;

namespace KonbiCloud.Logging
{
    public interface IWebLogAppService : IApplicationService
    {
        GetLatestWebLogsOutput GetLatestWebLogs();

        FileDto DownloadWebLogs();
    }
}
