using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Core;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Konbini.RfidFridge.Service.Util
{
    public class CustomerUINotificationService
    {
        private RabbitMqService RabbitMqService;
        private LogService LogService;
        private AudioService AudioService;

        public CustomerUINotificationService(
            RabbitMqService rabbitMqService,
            LogService logService,
            AudioService audioService
            )
        {
            this.RabbitMqService = rabbitMqService;
            this.LogService = logService;
            this.AudioService = audioService;
        }

        public void PublishScreenBaseOnStatus(MachineStatus status)
        {
            // Handle message
            var message = string.Empty;
            switch (status)
            {
                case MachineStatus.STOPSALE:
                case MachineStatus.MANUAL_STOPSALE:
                case MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC:
                case MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC_TRACING:
                case MachineStatus.UNLOADING_PRODUCT:
                    message = RfidFridgeSetting.CustomerUI.Messages.StopSale;
                    break;
                case MachineStatus.VALIDATE_CARD_FAILED:
                    message = RfidFridgeSetting.CustomerUI.Messages.InvalidCard;
                    break;
                case MachineStatus.STOPSALE_DUE_TO_PAYMENT:
                    message = RfidFridgeSetting.System.Payment.Magic.StopSaleDueToPaymentMessage;
                    break;
                case MachineStatus.PREAUTH_MAKE_PAYMENT:
                    message = RfidFridgeSetting.CustomerUI.Messages.PreauthMakePayment.Replace("{amount}".ToString(), (GlobalAppData.CurrentTransactionAmount / 100m).ToString("C"));
                    break;
                case MachineStatus.PREAUTH_REFUND:
                    int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out int minBalance);
                    message = RfidFridgeSetting.CustomerUI.Messages.PreauthRefund.Replace("{amount}".ToString(), (minBalance / 100m).ToString("C"));
                    break;
            }
            RabbitMqService.PublishMachineStatus(status, message);
        }

        public void WaitForUiStart(MachineStatus status)
        {
            while (true)
            {
                var current = GlobalAppData.CurrentMachineStatus;
                Console.WriteLine(current);
                if (current == status)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
        }

        public void StartUi()
        {

            // UI deo start thi bo may start
            try
            {
                var count = Process.GetProcessesByName("Rfid-Fridge").Count() + Process.GetProcessesByName("Electron").Count();
                if (count == 0)
                {
                    var patch = "C:\\MagicBox\\magicbox_deployment\\Modules\\CustomerUI\\Rfid-Fridge.exe"; //RfidFridgeSetting.CustomerUI.Patch;

                    //"D:\\Konbini\\SRC\\NewMB\\Magicbox-CSharp\\V2\\Konbi.MachineBrain\\CustomerUI\\Konbini.RfidFridge.Web.Client\\release-builds\\Rfid-Fridge-win32-ia32\\Rfid-Fridge.exe";
                    LogService.LogInfo("Manual Start Customer UI");
                    Process.Start(patch);
                }
                else
                {
                    LogService.LogInfo("Custommer UI already started");
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        public void OpenDoor()
        {
            Task.Run(() =>
            {
                DismissDialog();

                //var msg = RfidFridgeSetting.CustomerUI.Messages.OpenDoorDialogMessage;
                //var message = new DialogMessageDTO()
                //{
                //    Message = msg,
                //    Timeout = 5
                //};
                //SendDialogNotification(message);

                //SendNotification(RfidFridgeSetting.CustomerUI.Messages.OpenDoor);
                //PlaySound(RfidFridgeSetting.CustomerUI.Sounds.OpenDoor);
                //Thread.Sleep(5000);
                //SendNotification(RfidFridgeSetting.CustomerUI.Messages.TakeItem);
                //PlaySound(RfidFridgeSetting.CustomerUI.Sounds.TakeItem);
            });
        }

        public void InvalidCard(VALIDATE_ERROR_TYPE error, int ezLinkBalance, string generalMessage = null)
        {
            SendNotification(RfidFridgeSetting.CustomerUI.Messages.InvalidCard);
            if (error == VALIDATE_ERROR_TYPE.EZLINK_BALANCE_NOT_ENOUGHT)
            {
                int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out int minBalance);
                var msg = RfidFridgeSetting.CustomerUI.Messages.EzlinkInsufficientBalanceDialogMessage
                    .Replace("{balance}".ToString(), (ezLinkBalance / 100m).ToString("C"))
                    .Replace("{min}".ToString(), (minBalance / 100m).ToString("C"));
                var message = new DialogMessageDTO()
                {
                    Message = msg,
                    Timeout = 5
                };
                SendDialogNotification(message);
            }

            if (error == VALIDATE_ERROR_TYPE.CANT_READ_CARD)
            {
                var msg = RfidFridgeSetting.CustomerUI.Messages.CantReadCardDialogMessage;
                var message = new DialogMessageDTO()
                {
                    Message = msg,
                    Timeout = 5
                };
                SendDialogNotification(message);
            }

            if (error == VALIDATE_ERROR_TYPE.GENERAL_ERROR)
            {
                var msg = Convert.ToString(generalMessage);

                if (msg.Contains("{min}"))
                {
                    int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out int minBalance);
                    msg = msg.Replace("{min}".ToString(), (minBalance / 100m).ToString("C"));
                }
                var message = new DialogMessageDTO()
                {
                    Message = msg,
                    Timeout = 5
                };
                SendDialogNotification(message);
            }
        }



        public void TapCard()
        {
            SendNotification(RfidFridgeSetting.CustomerUI.Messages.TapCard);
        }

        public void FailToOpenTheDoor()
        {
            SendNotification("CAN'T OPEN THE DOOR");
        }

        public void ValidateCard()
        {
            SendNotification(RfidFridgeSetting.CustomerUI.Messages.ValidateCard);
            PlaySound(RfidFridgeSetting.CustomerUI.Sounds.ValidateCard);
            var msg = RfidFridgeSetting.CustomerUI.Messages.ValidatingDialogMessage;
            var message = new DialogMessageDTO()
            {
                Message = msg,
                Timeout = 60
            };
            SendDialogNotification(message);
        }

        public void DoNotTakeMoreItem(OrderDto order, int reserveBalance)
        {
            if (order.Amount > reserveBalance)
            {
                var msg = RfidFridgeSetting.CustomerUI.Messages.DotNotTakeMoreItem?.Replace("{balance}".ToString(), (reserveBalance / 100m).ToString("C"));
                var message = new DialogMessageDTO()
                {
                    Message = msg,
                    Timeout = 10
                };
                SendDialogNotification(message);
            }
        }
        public void CardIsBlacklist()
        {
            //SendNotification(RfidFridgeSetting.CustomerUI.Messages.BlackList);
            //PlaySound(RfidFridgeSetting.CustomerUI.Sounds.ValidateCard);
            var msg = RfidFridgeSetting.CustomerUI.Messages.BlackListDialogMessage;
            var message = new DialogMessageDTO()
            {
                Message = msg,
                Timeout = 10
            };
            SendDialogNotification(message);
        }


        public void DoorOpenTooLong()
        {
            // SendNotification(RfidFridgeSetting.CustomerUI.Messages.DoorOpenTooLong);
            PlaySound(RfidFridgeSetting.CustomerUI.Sounds.DoorOpenTooLong);

            var msg = RfidFridgeSetting.CustomerUI.Messages.DoorOpenTooLongDialogMessage;
            var message = new DialogMessageDTO()
            {
                Message = msg,
                Timeout = 30
            };
            SendDialogNotification(message);
        }

        public void PleaseWait()
        {
            SendNotification(RfidFridgeSetting.CustomerUI.Messages.PleaseWait);
            PlaySound(RfidFridgeSetting.CustomerUI.Sounds.PleaseWait);
        }
        public void SendNotification(string message)
        {
            try
            {
                LogService.LogInfo("Send Notification: " + message);
                if (!string.IsNullOrEmpty(message))
                {
                    RabbitMqService.PublishUIMessage(message);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }

        public void DismissDialog()
        {
            var message = new DialogMessageDTO()
            {
                Message = string.Empty,
                Timeout = 0
            };
            SendDialogNotification(message);
        }

        public void SendDialogNotification(DialogMessageDTO message)
        {
            try
            {
                var json = JsonConvert.SerializeObject(message);
                LogService.LogInfo("Send Dialog Notification: " + json);

                if (message != null)
                {
                    RabbitMqService.PublishUIDialogMessage(message);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }

        public void StopSound()
        {
            try
            {
                LogService.LogInfo("Stop Sound");
                AudioService.StopSound();
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }

        public void PlaySound(string sound)
        {
            try
            {
                LogService.LogInfo("Playsound: " + sound);
                if (!string.IsNullOrEmpty(sound))
                {
                    AudioService.PlaySound(sound);
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }

        #region GrabPay
        public void SendDialogNotification(string msg, int time = 30)
        {
            Task.Run(() =>
            {
                var message = new DialogMessageDTO()
                {
                    Message = msg,
                    Timeout = time
                };
                SendDialogNotification(message);
            });
        }
        #endregion
    }
}
