using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.DTO
{
    public class MachineDTO : BaseAbpDTO<MachineDTO.Machine>
    {
        public  class Machine
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("tenantId")]
            public long TenantId { get; set; }

            [JsonProperty("tenantName")]
            public string TenantName { get; set; }

            [JsonProperty("cashlessTerminalId")]
            public object CashlessTerminalId { get; set; }

            [JsonProperty("registeredAzureIoT")]
            public bool RegisteredAzureIoT { get; set; }

            [JsonProperty("isDeleted")]
            public bool IsDeleted { get; set; }

            [JsonProperty("deleterUserId")]
            public object DeleterUserId { get; set; }

            [JsonProperty("deletionTime")]
            public object DeletionTime { get; set; }

            [JsonProperty("lastModificationTime")]
            public DateTimeOffset? LastModificationTime { get; set; }

            [JsonProperty("lastModifierUserId")]
            public long? LastModifierUserId { get; set; }

            [JsonProperty("creationTime")]
            public DateTimeOffset CreationTime { get; set; }

            [JsonProperty("creatorUserId")]
            public long CreatorUserId { get; set; }

            [JsonProperty("id")]
            public Guid Id { get; set; }
        }

    }


   
}
