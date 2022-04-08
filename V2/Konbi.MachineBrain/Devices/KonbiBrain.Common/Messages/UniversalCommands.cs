using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Messages
{
    public class UniversalCommands<T>: UniversalCommands
    {
        public new T CommandObject { get; set; }
    }
    public class UniversalCommands : IUniversalCommands
    {

        public string Command { get; set; }
        public Guid CommandId { get; set; }
        public UniversalCommands()
        {
            this.CommandId = Guid.NewGuid();
        }
        public UniversalCommands(Guid id)
        {
            this.CommandId = id;
        }
        public UniversalCommands(string command)
        {
            this.Command = command;
            PublishedDate = DateTime.Now;
        }

        public dynamic CommandObject { get; set; }
        public bool IsTimeout()
        {
            var time = (DateTime.Now - PublishedDate).TotalSeconds;
            if (time >= 10) return true;
            return false;
        }
        public DateTime PublishedDate { get; set; }
    }
    public interface IUniversalCommands
    {
        string Command { get; set; }
        Guid CommandId { get; set; }
    }
    public static class UniversalCommandConstants
    {
        //Rfid Table
        public const string RfidTableDetectPlates = "DetectedPlates";

        //Payment device
        public const string DisablePaymentCommand = "DisablePaymentCommand";
        public static string EnablePaymentCommand = "EnablePaymentCommand";
    }
}

