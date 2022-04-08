using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Authorization;
using KonbiCloud.Common.Dtos;

namespace KonbiCloud.Common
{
    [AbpAllowAnonymous]
    public class CommonService:KonbiCloudAppServiceBase
    {
        private readonly IMessageProducerService messageProducerService;

        public CommonService(IMessageProducerService messageProducerService)
        {
            this.messageProducerService = messageProducerService;
        }

        [AbpAllowAnonymous]
        public async Task<string> GetSetting(string settingName)
        {
            return await SettingManager.GetSettingValueAsync(settingName);
        }

        
        public bool PublishNsqCommand(PublishNsqCommandDto obj)
        {
            return messageProducerService.SendNsqCommand(obj.Topic, obj.CommandObj);
        }

        public List<GetComPortDto> GetComPorts()
        {
            var tempports = SerialPort.GetPortNames();
            return tempports.Select(x => new GetComPortDto
            {
                Port = x
            }).ToList();
        }

        public async  Task<string> ReadLog(ReadLogDto dto)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Logs\";
            var fileName = $"log-{dto.LogType}-{dto.Date.ToString("yyyyMMdd")}.txt";
            var txt = await System.IO.File.ReadAllTextAsync (desktopPath + fileName);
            txt = txt.Replace(System.Environment.NewLine, "<br/>");

            return txt;
        }

    }
}
