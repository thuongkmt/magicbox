using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Konbini.RfidFridge.Common
{

    public class RfidFridgeSetting
    {
        public static class Machine
        {
            [DefaultValue("MAGIC BOX")]
            public static string Name { get; set; }

            [DefaultValue("00000000-0000-0000-0000-000000000000")]
            public static string Id { get; set; }

            [DefaultValue("Phone: 12345678 NEA: 12345678")]
            public static string BottomText { get; set; }

            [DefaultValue("C:\\MagicBox\\Advertising\\instructions.jpg")]
            public static string AdvsImage { get; set; }

            [DefaultValue("True")]
            public static string EnableStopSale { get; set; }

            [DefaultValue("60")]
            public static string StopSaleTimeSpan { get; set; }

        }

        public static class System
        {
            public static class Comport
            {
                [DefaultValue("COM14")]
                public static string Inventory { get; set; }

                [DefaultValue("COM12")]
                public static string Lock { get; set; }

                [DefaultValue("COM11")]
                public static string CashlessTerminal { get; set; }

                [DefaultValue("38400")]
                public static string RfidReaderBaurate { get; set; }

                [DefaultValue("8E1")]
                public static string RfidReaderFrame { get; set; }

                [DefaultValue("COM13")]
                public static string QrCodeReader { get; set; }
            }

            public static class Payment
            {
                [DefaultValue("NAYAX")]
                public static string Type { get; set; }
                public static class Nayax
                {
                    [DefaultValue("2000")]
                    public static string PreAuthAmount { get; set; }
                }
                public static class Payter
                {
                    [DefaultValue("2000")]
                    public static string PreAuthAmount { get; set; }
                }
                public static class Magic
                {
                    [DefaultValue("20000")]
                    public static string MinBalanceRequire { get; set; }

                    [DefaultValue("False")]
                    public static string StopSaleDueToPayment { get; set; }
                    [DefaultValue("False")]
                    public static string AutoEnableIfFailed { get; set; }

                    [DefaultValue("SOME THING WRONG WITH PAYMENT<br>PLEASE CALL HOT LINE:812345")]
                    public static string StopSaleDueToPaymentMessage { get; set; }
                    [DefaultValue("False")]
                    public static string EnableCreditCard { get; set; }
                    [DefaultValue("True")]
                    public static string EnableCpas { get; set; }

                    [DefaultValue("BL,DL,CO,CU")]
                    public static string RetryToChargeIfGotErrorCodes { get; set; }
                    [DefaultValue("IUC")]
                    public static string TerminalType { get; set; }

                    [DefaultValue("SLIM")]
                    public static string CardInsertType { get; set; }

                    [DefaultValue("COM16")]
                    public static string CardInsertComport { get; set; }
                }

                public static class Qr
                {

                    [DefaultValue("GRABPAY")]
                    public static string Type { get; set; }
                }

                [DefaultValue("True")]
                public static string EnableBlackList { get; set; }

                public static class GrabPay
                {
                    [DefaultValue("https://partner-api.stg-myteksi.com")]
                    public static string Host { get; set; }
                    [DefaultValue("e2020857-8fee-465e-a580-a597195bef66")]
                    public static string PartnerId { get; set; }
                    [DefaultValue("P_3K37OfelpWf29G")]
                    public static string PartnerSecret { get; set; }
                    [DefaultValue("de0df3f43a6640e78987c50ab465ed3d")]
                    public static string ClientId { get; set; }
                    [DefaultValue("AjNu5SWoBDYC2vPf")]
                    public static string ClientSecret { get; set; }
                    [DefaultValue("acfb214c-f464-421c-b8de-9064224bb764")]
                    public static string GrabId { get; set; }
                    [DefaultValue("6afaa0ff0211fff8a240fe83f")]
                    public static string TerminalId { get; set; }
                    [DefaultValue("2000")]
                    public static string ReserveAmount { get; set; }
                }

                public static class TeraWallet
                {
                    [DefaultValue("https://staging5.konbinisg.com/daisho")]
                    public static string Host { get; set; }

                    [DefaultValue("2000")]
                    public static string ReserveAmount { get; set; }
                }

                public static class CreditCardWallet
                {
                    [DefaultValue("https://s.ineedfood.today")]
                    public static string Host { get; set; }
                }
            }

            public static class Slack
            {
                [DefaultValue("#magic_box")]
                public static string InfoChannel { get; set; }

                [DefaultValue("#magic_box_alert")]
                public static string AlertChannel { get; set; }
                [DefaultValue("#mb_unstable")]
                public static string UnstableTagChannel { get; set; }

                [DefaultValue("KonbiWatchDog")]
                public static string Username { get; set; }

                [DefaultValue("https://hooks.slack.com/services/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")]
                public static string URL { get; set; }
            }

            public static class Temperature
            {
                [DefaultValue("1000")]
                public static string ReportInterval { get; set; }
                [DefaultValue("60000")]
                public static string ReportToCloudInterval { get; set; }

                [DefaultValue("5")]
                public static string NormalTemperature { get; set; }

                [DefaultValue("true")]
                public static string Enable { get; set; }

                [DefaultValue("0")]
                public static string SourceIndex { get; set; }
                [DefaultValue("COM7")]
                public static string Comport { get; set; }

                [DefaultValue("-3")]
                public static string Offset { get; set; }
            }

            public static class Inventory
            {
                [DefaultValue("2")]
                public static string ReaderVersion { get; set; }
                [DefaultValue("2000")]
                public static string Delay { get; set; }
                [DefaultValue("1,2,3,4,5,6,7,8,9,10")]
                public static string Antenna { get; set; }

                [DefaultValue("True")]
                public static string RemoveTagAfterSold { get; set; }

                [DefaultValue("6000")]
                public static string CartUIDelay { get; set; }

                [DefaultValue("True")]
                public static string RemoveTagIdChecksum { get; set; }
            }

            public static class Cloud
            {
                [DefaultValue("True")]
                public static string UseCloud { get; set; }

                [DefaultValue("http://localhost:22743")]
                public static string CloudApiUrl { get; set; }

                [DefaultValue("128.199.209.115")]
                public static string RabbitMqServer { get; set; }

                [DefaultValue("admin")]
                public static string RabbitMqUser { get; set; }

                [DefaultValue("konbini62")]
                public static string RabbitMqPassword { get; set; }
                [DefaultValue("0")]
                public static string TenantId { get; set; }

                [DefaultValue("false")]
                public static string SyncUseRabbitMq { get; set; }

                [DefaultValue("false")]
                public static string MapTagByPrefix { get; set; }
            }

            public static class CustomerCloud
            {
                [DefaultValue("https://customercloud.azurewebsites.net/")]
                public static string Url { get; set; }

                [DefaultValue("admin")]
                public static string Username { get; set; }

                [DefaultValue("123qwe")]
                public static string Password { get; set; }

                [DefaultValue("0")]
                public static string TenantId { get; set; }

                [DefaultValue("10000")]
                public static string QrMinbalanceRequire { get; set; }
            }

            public static class MachineStatus
            {
                [DefaultValue("True")]
                public static string Enable { get; set; }

                [DefaultValue("300000")]
                public static string PcHeartBeatInterval { get; set; }
            }

            public static class Camera
            {
                [DefaultValue("False")]
                public static string Enable { get; set; }

                [DefaultValue("0")]
                public static string CameraIndex { get; set; }
                [DefaultValue("1080")]
                public static string ImageWidth { get; set; }
                [DefaultValue("768")]
                public static string ImageHeight { get; set; }

                [DefaultValue("C:\\MagicBox\\TransactionImages")]
                public static string TransactionImagesFolder { get; set; }
            }
            public class DevCon
            {
                [DefaultValue("C:\\devcon.exe")]
                public static string Location { get; set; }

                public class Command
                {
                    [DefaultValue("restart @\"USBVCOM\\VID_0B00&PID_0057\\INST_0\"")]
                    public static string IucReset { get; set; }
                    [DefaultValue("restart @\"HID\\VID_23D8&PID_0285\"")]
                    public static string ResetCardHolder { get; set; }
                }
            }


        }

        public static class CustomerUI
        {

            [DefaultValue("true")]
            public static string EnableSound { get; set; }
            [DefaultValue("C:\\MagicBox\\Sounds")]
            public static string SoundFolder { get; set; }

            [DefaultValue("USD")]
            public static string Currency { get; set; }

            [DefaultValue("en-US")]
            public static string LanguageCode { get; set; }

            [DefaultValue("C:\\MagicBox\\magicbox_deployment\\Modules\\CustomerUI\\Rfid-Fridge.exe")]
            public static string Patch { get; set; }
            public static class Messages
            {
                [DefaultValue("PLEASE OPEN THE DOOR")]
                public static string OpenDoor { get; set; }

                [DefaultValue("PLEASE TAKE ITEM")]
                public static string TakeItem { get; set; }

                [DefaultValue("TAP/INSERT YOUR CARD TO BEGIN")]
                public static string TapCard { get; set; }

                [DefaultValue("DOOR OPEN TOO LONG")]
                public static string DoorOpenTooLong { get; set; }

                [DefaultValue("VALIDATING YOUR CARD...")]
                public static string ValidateCard { get; set; }

                [DefaultValue("PLEASE WAIT...")]
                public static string PleaseWait { get; set; }


                [DefaultValue("YOUR CARD IS BLOCKED")]
                public static string BlackList { get; set; }


                [DefaultValue("SALE STOP TEMPORARY<br>PLEASE COME BACK LATER")]
                public static string StopSale { get; set; }

                [DefaultValue("CANCELLED<br>YOU WILL NOT BE CHARGED")]
                public static string TransactionCancel { get; set; }

                [DefaultValue("YOU HAVE BEEN CHARGED: <span style='color: orange'>{amount}</span>")]
                public static string TransactionSuccess { get; set; }

                [DefaultValue("YOUR CARD IS INVALID!!!")]
                public static string InvalidCard { get; set; }

                [DefaultValue("INSUFFICIENT BALANCE<br>MINIMUM BALANCE REQUIRE: {min}<br>YOUR BALANCE: {balance}<br>PLEASE TAKE BACK YOUR CARD")]
                public static string EzlinkInsufficientBalanceDialogMessage { get; set; }

                [DefaultValue("UNABLE TO READ YOUR CARD BALANCE<BR>PLEASE TAKE BACK YOUR CARD")]
                public static string CantReadCardDialogMessage { get; set; }
                [DefaultValue("WE ARE VALIDATING YOUR CARD<BR><b>PLEASE DO NOT TAKE OUT YOUR CARD</b>")]
                public static string ValidatingDialogMessage { get; set; }

                [DefaultValue("YOUR CARD IS VALID!!!<BR>OPEN THE DOOR AND TAKE YOUR ITEM")]
                public static string OpenDoorDialogMessage { get; set; }

                [DefaultValue("YOUR CARD IS BLOCKED!!!<BR>CONTACT OUR HOTLINE FOR DETAIL")]
                public static string BlackListDialogMessage { get; set; }
                [DefaultValue("DOOR OPEN TOO LONG<BR>PLEASE CLOSE THE DOOR")]
                public static string DoorOpenTooLongDialogMessage { get; set; }

                [DefaultValue("PLEASE DO NOT TAKE MORE THAN <b>{balance}</b>")]
                public static string DotNotTakeMoreItem { get; set; }
                [DefaultValue("PLEASE INSERT CARD TO PAY <br><b>{amount}</b>")]
                public static string PreauthMakePayment { get; set; }
                [DefaultValue("PLEASE INSERT CARD TO <b><CLAIM</b> YOUR DEPOSIT OF <br><b>{amount}</b>")]
                public static string PreauthRefund { get; set; }

                // GrabPay
                [DefaultValue("Validating QR.")]
                public static string QRValidating { get; set; }
                [DefaultValue("QR check successful.")]
                public static string QRSuccess { get; set; }
                [DefaultValue("Invalid QR, please try again.")]
                public static string QRUnsuccess { get; set; }
                [DefaultValue("Transaction was successful.")]
                public static string QRStatusSuccess { get; set; }
                [DefaultValue("Transaction was declined")]
                public static string QRStatusFailed { get; set; }
                [DefaultValue("Transaction is processing")]
                public static string QRStatusUnknown { get; set; }
                [DefaultValue("Transaction is processing")]
                public static string QRStatusPending { get; set; }
                [DefaultValue("Failed to payout merchant")]
                public static string QRStatusBad_debt { get; set; }

                // GrabPay - 7.2 Logic Error.
                [DefaultValue("Server error.")]
                public static string GrabError5001 { get; set; }
                [DefaultValue("Database service error.")]
                public static string GrabError5003 { get; set; }
                [DefaultValue("Transaction not found.")]
                public static string GrabError4040 { get; set; }
                [DefaultValue("User does not existed.")]
                public static string GrabError4041 { get; set; }
                [DefaultValue("Transaction violating compliance or risk check.")]
                public static string GrabError4097 { get; set; }
                [DefaultValue("Transaction status is not supported to cancel.")]
                public static string GrabError40011 { get; set; }
                [DefaultValue("Partial refund is not allowed for this transaction.")]
                public static string GrabError40012 { get; set; }
                [DefaultValue("Consumer code is invalid.")]
                public static string GrabError40912 { get; set; }
                [DefaultValue("Consumer code is expired.")]
                public static string GrabError40913 { get; set; }


                // Tera
                [DefaultValue("Validating QR.")]
                public static string TeraQRValidating { get; set; }
                [DefaultValue("QR check successful.")]
                public static string TeraQRValidateSuccess { get; set; }
                [DefaultValue("Invalid QR, please try again.")]
                public static string TeraQRValidateFail { get; set; }
                [DefaultValue("Transaction was successful.")]
                public static string TeraTransactionSuccess { get; set; }
                [DefaultValue("Balance not enough.")]
                public static string TeraBalanceNotEnough { get; set; }

            }

            public static class Sounds
            {
                [DefaultValue("open_door.wav")]
                public static string OpenDoor { get; set; }

                [DefaultValue("take_item.wav")]
                public static string TakeItem { get; set; }

                [DefaultValue("door_open_too_long.wav")]
                public static string DoorOpenTooLong { get; set; }

                [DefaultValue("validate_card.wav")]
                public static string ValidateCard { get; set; }

                [DefaultValue("please_wait.wav")]
                public static string PleaseWait { get; set; }
            }

        }

        public static class Alert
        {
            [DefaultValue("60000")]
            public static string DoorOpenTimeOut { get; set; }

            [DefaultValue("10000")]
            public static string DoorOpenTooLongAlertRepeatTime { get; set; }

            public static class Messages
            {
                [DefaultValue("Door opened")]
                public static string DoorOpen { get; set; }

                [DefaultValue("Door closed")]
                public static string DoorClose { get; set; }

                [DefaultValue("Transaction completed | Status {status} | Amount {amount} | Products: {products}")]
                public static string TransactionCompleted { get; set; }

                [DefaultValue("Transaction error!! | Status {status} | Amount {amount} | Products: {products}")]
                public static string TransactionError { get; set; }

                [DefaultValue("Transaction cancelled")]
                public static string TransactionCancelled { get; set; }

                [DefaultValue("Door open too long!!!")]
                public static string DoorOpenTooLong { get; set; }

                [DefaultValue("Temperature is abnormal: {temp}")]
                public static string TemperatureIsAbnormal { get; set; }

                [DefaultValue("Unstable tags: {tags}")]
                public static string UnstableTag { get; set; }

                [DefaultValue("Machine stop sale!")]
                public static string StopSaleMessage { get; set; }
            }
        }
    }
}
