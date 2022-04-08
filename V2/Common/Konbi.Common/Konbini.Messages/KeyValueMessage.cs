using MessagePack;
using System;
using Konbini.Messages.Enums;
using Newtonsoft.Json;

namespace Konbini.Messages
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
        public string KeyLabel { get;set; }

        [Key(4)]
        public int? TenantId { get; set; }

        //[Key(4)]
        //public string OtherInfo { get; set; }
    }
}
