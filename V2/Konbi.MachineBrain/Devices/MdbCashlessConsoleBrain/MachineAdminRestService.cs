using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MdbCashlessBrain
{
    public class MachineAdminRestService
    {
        private readonly string serverUrl;
        private readonly IConfigurationRoot configurationRoot;
        public MachineAdminRestService(IConfigurationRoot configurationRoot)
        {
            this.configurationRoot = configurationRoot;
            serverUrl = configurationRoot["App:ServerUrl"] + "/api/services/app";
        }
        public async Task<string> GetPort()
        {
            using (var client = new HttpClient())
            {
                var url = serverUrl + "/CommonService/GetSetting?settingName=MdbCashlessPort";
                var response = await client.GetAsync(url);
                var value = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(value);
                return obj.result; ;
            }
        }
    }
}
