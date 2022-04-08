using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinePay
{
    [MessagePackObject]
    public class KeyValueMessage
    {
        private MessageKeys _key;
        private object _value;

        [Key(0)]
        public Guid MachineId { get; set; }

        [Key(1)]
        public MessageKeys Key
        {
            get => _key;
            set
            {
                _key = value;
                KeyLabel = _key.ToString();
            }
        }


        [IgnoreMember]
        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                JsonValue = JsonConvert.SerializeObject(_value);
            }
        }

        [Key(2)]
        public string JsonValue { get; set; }

        [Key(3)]
        public string KeyLabel { get; set; }

        [Key(4)]
        public int? TenantId { get; set; }

        //[Key(4)]
        //public string OtherInfo { get; set; }
    }

    public enum MessageKeys
    {
        TestKey = 1,
        MachineStatus = 10,
        Transaction = 100,
        Product = 200,
        Inventory = 300,
        UpdateInventoryList = 301,
        Topup = 400,
        TemperatureLogs = 401,
        ProductRfidTags = 402,
        UpdateTagState = 403,
        ProductCategory = 404,
        ProductCategoryRelations = 405,
        ProductRfidTagsRealtime = 406,
        ProductMachinePrice = 407,
        Restock = 408,
        ManuallySyncProduct = 409,
        ManuallySyncProductCategory = 410,
        AlertConfiguration = 411,
        SyncInventoriesForActiveTopupMessageHandler = 412,
        LinePayOpenLock = 413,
        LinePayCloseLock = 414
    }
}
