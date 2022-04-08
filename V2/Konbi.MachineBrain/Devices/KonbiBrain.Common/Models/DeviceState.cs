using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Konbi.Common.Models
{
    public class DeviceState: PropertyChangedBase
    {
        private bool isConnected;
        private DeviceStates state;
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
            set
            {
                if (isConnected != value) { 
                    isConnected = value;
                    NotifyOfPropertyChange(() => IsConnected);
                }
            }
        }
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceStates State
        {
            get
            {
                return state;
            }
            set
            {
                if (state != value) { 
                    state = value;
                    NotifyOfPropertyChange(() => State);
                }
            }
        }
        [JsonProperty]
        public string[] ErrorMessages { get; set; }
        public override string ToString()
        {
            return string.Format("Name: {0}, IsConnected: {1}, State:{2}, Errors: {3}", Name, IsConnected, State.ToString(), ErrorMessages!=null? string.Join(", ", ErrorMessages):string.Empty);
        }
    }
    public enum DeviceStates
    {
        Unknown,
        Normal,
        Error,
        Initializing
    }
}
