using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Newtonsoft.Json;

namespace KonbiCloud.Machines
{
    public class Device : FullAuditedEntity<Guid>
    {
        private List<DeviceCustomInfo> customInfoes;

        public string Code { get; set; }
        public string Port { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }//Normal, Abnormal
        public string State { get; set; }//State (current): e.g Magicbox - Door (Open/Close),...

        [JsonIgnore]
        public virtual Machine Machine { get; set; }

        public string CustonInfoesJson { get; private set; }

        [JsonIgnore, NotMapped]
        public List<DeviceCustomInfo> CustomInfoes
        {
            get
            {
                return customInfoes;
            }
            set
            {
                CustonInfoesJson = JsonConvert.SerializeObject(value);
                customInfoes = value;
            }
        }
    }

    public class DeviceCustomInfo
    {
        public string Property { get; set; }
        public string Value { get; set; }

    }

}
