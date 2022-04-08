using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MdbCashlessBrain
{
    public class MachineAdminRestService
    {
        private readonly string serverUrl;
        public MachineAdminRestService()
        {
            serverUrl = ConfigurationManager.AppSettings["ServerUrl"]+ "/api/services/app";
        }
        public async Task<string> GetPort()
        {
            using (var client = new HttpClient())
            {
                var url = serverUrl + "/CommonService/GetSetting?settingName=MdbCashlessPort";
                var response = await client.GetAsync(url);
                var value = await response.Content.ReadAsStringAsync();
                return value;
            }
        }
    }
}
