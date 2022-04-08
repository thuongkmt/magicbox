using Konbini.Messages.Enums;
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
        public UniversalCommands()
        {
        }
        public UniversalCommands(string command) : base(command)
        {
            
        }
    }
    public class UniversalCommands : IUniversalCommands
    {
        public Guid CommandId { get;  set; }
        public string Command { get; set; }
        public UniversalCommands()
        {
            PublishedDate = DateTime.UtcNow;
        }

        public UniversalCommands(string command)
        {
            this.Command = command;
            PublishedDate = DateTime.UtcNow;

        }

        public dynamic CommandObject { get; set; }

        public bool IsTimeout()
        {
            var time = (DateTime.Now - PublishedDate).TotalSeconds;
            if (time >= 10) return true;
            return false;
        }

        public DateTime PublishedDate { get; set; }
        public CommandState CommandState { get; set; }
    }
    public interface IUniversalCommands
    {
        string Command { get; set; }
        Guid CommandId { get; set; }
        CommandState CommandState { get; set; }
    }

    public static class UniversalCommandConstants
    {
        public const string RfidTableDetectPlates = "DetectedPlates";

        public const string PaymentDeviceResponse = "PaymentDeviceResponse";
        public const string MdbCashlessReponse = "MdbCashlessReponse";
        public const string PaymentRequest = "PaymentRequest";

        public static string PaymentACKCommand = "PaymentACKCommand";
        public const string DisablePaymentCommand = "DisablePaymentCommand";
        public const string EnablePaymentCommand = "EnablePaymentCommand";

        public const string RfidTableConfiguration = "RfidTableConfiguration";

        public const string ACKResponse = "ACKResponse";
        public const string CameraRequest = "CameraRequest";
        public const string CameraResponse = "CameraResponse";
    }
}

