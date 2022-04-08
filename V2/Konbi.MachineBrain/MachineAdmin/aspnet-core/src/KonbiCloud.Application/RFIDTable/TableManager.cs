using Abp.Domain.Repositories;
using KonbiCloud.Machines;
using KonbiCloud.MenuSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abp.ObjectMapping;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Abp.Application.Services;
using KonbiCloud.RFIDTable.Cache;
using Abp.Runtime.Caching;
using KonbiCloud.Sessions;
using Abp.Dependency;

using KonbiBrain.Common.Messages.Payment;
using KonbiCloud.Enums;
using KonbiCloud.Common;
using System.Threading;
using KonbiBrain.Common.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Commands.RFIDTable;
using KonbiCloud.SignalR;
using System.Diagnostics;
using Newtonsoft.Json;
using Abp.Configuration;
using KonbiCloud.Configuration;
using NsqSharp;
using KonbiBrain.Messages;
using KonbiBrain.Common.Messages.Camera;

namespace KonbiCloud.RFIDTable
{
    public class TableManager : ITableManager, IHandler
    {
        public static string TableDeviceSettingGroup = "TableDeviceSettingGroup";
        public static string PaymentDeviceGroup = "PaymentDevice";
        public static string CustomerUIGroup = "CustomerUI";
        private readonly ICacheManager cacheManager;
        private IRfidTableSignalRMessageCommunicator signalRCommunicator;
        private readonly ISettingManager _settingManager;
        private int scanningStableAfterMiliseconds;
        private readonly IMessageCommunicator messageCommunicator;
        private readonly ITableSettingsManager tableSettingsManager;
        private readonly IPaymentManager paymentManager;
        private HashSet<ClientInfo> clients = new HashSet<ClientInfo>();
        public HashSet<ClientInfo> Clients
        {
            get { return clients; }
        }

        private TransactionInfo transaction;

        public bool OnSale
        {
            get
            {
                return !Clients.Any(el => el.Group == TableManager.TableDeviceSettingGroup);
            }
        }
        public TransactionInfo Transaction
        {
            get { return transaction; }
            set
            {
                lock (transactionLock)
                {
                    transaction = value;
                }
            }
        }



        private object transactionLock = new object();
        private readonly IMessageProducerService nsqProducerService;
        private readonly Consumer consumer;




        public TableManager(
            IRfidTableSignalRMessageCommunicator signalRCommunicator,
            ICacheManager cacheManager,
            ITableSettingsManager tableSettingsManager,
            IPaymentManager paymentManager,
            IMessageCommunicator messageCommunicator,
            ISettingManager settingManager,
            IMessageProducerService nsqProducerService
            )
        {
            this.tableSettingsManager = tableSettingsManager;
            this.tableSettingsManager.DeviceFeedBack += TableSettingsManager_DeviceFeedBackAsync;
            this.cacheManager = cacheManager;
            this.signalRCommunicator = signalRCommunicator;
            this.paymentManager = paymentManager;
            this.paymentManager.DeviceFeedBack += PaymentManager_DeviceFeedBack;
            this.messageCommunicator = messageCommunicator;
            this._settingManager = settingManager;
            this.nsqProducerService = nsqProducerService;
            consumer = new Consumer(NsqTopics.CAMERA_RESPONSE_TOPIC, NsqConstants.NsqDefaultChannel);
            consumer.AddHandler(this);
            //consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);

            scanningStableAfterMiliseconds = _settingManager.GetSettingValue<int>(AppSettingNames.ScanningStableAfterMiliseconds);
            if (scanningStableAfterMiliseconds <= 0)
            {
                scanningStableAfterMiliseconds = 2000;
            }

        }

        private void TableSettingsManager_DeviceFeedBackAsync(object sender, CommandEventArgs e)
        {
            if (e.Command.Command == UniversalCommandConstants.RfidTableConfiguration)
            {
                //var cmd = e.Command as ConfigCommand;
                //if (cmd != null)
                //{
                //    if(cmd.CommandObject.Action == "settingsResult")
                //    {
                //        signalRCommunicator.UpdateTableSettings(cmd.CommandObject.selectedComPort, cmd.CommandObject.ComPortAvaliable, cmd.CommandObject.IsServiceRunning);
                //    }

                //}

            }
            else if (e.Command.Command == UniversalCommandConstants.RfidTableDetectPlates)
            {
                var cmd = JsonConvert.DeserializeObject<UniversalCommands<DetectPlatesCommandPayload>>(e.CommandStr);
                var mess = "{\"type\":\"RFIDTable_DetectedDisc\",\"data\":";
                mess += JsonConvert.SerializeObject(cmd.CommandObject);
                mess += "}";// "]}";
                messageCommunicator.SendRfidTableMessageToAllClient(new GeneralMessage { Message = mess });

                if (OnSale)
                {
                    cmd = JsonConvert.DeserializeObject<UniversalCommands<DetectPlatesCommandPayload>>(e.CommandStr);
                    // processing transaction and notify customer UI
                    ProcessTransactionAsync(cmd.CommandObject.Plates.Select(el => new PlateReadingInput() { UID = el.UID, UType = el.UType }).ToList()).ContinueWith(taskResult =>
                    {
                        if (taskResult.IsCompleted && taskResult.IsFaulted == false && taskResult.IsCanceled == false)
                            signalRCommunicator.UpdateTransactionInfo(taskResult.Result);
                    });
                }

                else
                {
                    // notify Admin about detecting dishes
                    signalRCommunicator.SendAdminDetectedPlates(cmd.CommandObject.Plates);

                }
            }
        }

        private void PaymentManager_DeviceFeedBack(object sender, CommandEventArgs e)
        {
            if (e.Command.Command == UniversalCommandConstants.EnablePaymentCommand)
            {
                var cmd = e.Command as NsqEnablePaymentResponseCommand;
                if (cmd.TransactionId == Transaction.Id && (transaction.PaymentState == PaymentState.ActivatingPayment || transaction.PaymentState == PaymentState.Init))
                {

                    Transaction.PaymentState = cmd.Code == 0 ? PaymentState.ActivatedPaymentSuccess : PaymentState.ActivatedPaymentError;
                    if(Transaction.PaymentState == PaymentState.ActivatedPaymentSuccess)
                    {
                        SendMessageToCamera(true);
                    }
                }
                //SendMessageToCamera(true);
            }
            if (e.Command.Command == UniversalCommandConstants.PaymentDeviceResponse)
            {
                var cmd = e.Command as NsqPaymentCallbackResponseCommand;
                if (cmd != null)
                {
                    switch (cmd.Response.State)
                    {

                        case PaymentState.InProgress:
                            {
                                Transaction.CustomerMessage = cmd.Response.Message;
                            }
                            break;
                        case PaymentState.Success:
                            {
                                Transaction.PaymentState = PaymentState.Success;
                                Transaction.CustomerMessage = "PAID";
                                //Send message to take end transaction picture
                                SendMessageToCamera();
                            }
                            break;
                        case PaymentState.Failure:
                            {
                                Transaction.PaymentState = PaymentState.Failure;
                                Transaction.CustomerMessage = cmd.Response.Message;
                                //Send message to take end transaction picture
                                SendMessageToCamera();
                            }
                            break;
                    }
                    NotifyOnTransactionChanged();
                }
            }
        }

        private void SendMessageToCamera(bool isPaymentStart = false)
        {
            var cmd = new NsqCameraRequestCommand()
            {
                IsPaymentStart = isPaymentStart
            };
            nsqProducerService.SendNsqCommand(NsqTopics.CAMERA_REQUEST_TOPIC, cmd);
        }

        public async Task<SessionInfo> GetSessionInfoAsync()
        {
            var cacheItem = await GetSaleSessionCacheItemAsync();
            return cacheItem.SessionInfo ?? null;

        }
        CancellationTokenSource ts = new CancellationTokenSource();
        /// <summary>
        /// Generate transaction due to  plates of reading
        /// </summary>
        /// <param name="plates"></param>
        /// <returns></returns>
        public async Task<TransactionInfo> ProcessTransactionAsync(List<PlateReadingInput> plates)
        {
            // Lock transaction due to  payment device is activated.
            if (Transaction != null && Transaction.MenuItems != null && Transaction.MenuItems.Count > 0 && Transaction.PaymentState == PaymentState.ActivatedPaymentSuccess)
            {
                return Transaction;
            }


            Transaction = new TransactionInfo();
            Transaction.PaymentState = Konbini.Messages.Enums.PaymentState.Init;
            Transaction.MenuItems = new List<MenuItemInfo>();
            Transaction.Id = Guid.NewGuid();
            Transaction.PaymentType = paymentManager.PaymentType;
            NotifyOnTransactionChanged();

            Transaction.CustomerMessage = "Scanning...Please do not shift the tray";

            // retrieve session information. base on this to  build transaction lines.
            var cacheItem = await GetSaleSessionCacheItemAsync();
            Transaction.SessionId = cacheItem.SessionInfo != null ? cacheItem.SessionInfo.Id : Guid.Empty;
            if (cacheItem.MenuItems != null)
            {
                var inputPlates = cacheItem.Plates.Where(el => plates.Any(cond => cond.UType == el.Code && cond.UID == el.Uid)).ToList();
                var isContractor = cacheItem.Trays.Any(el => plates.Any(cond => cond.UType == el.Code));
                Transaction.Buyer = isContractor ? "Contractor" : "Staff";



                Transaction.MenuItems = inputPlates.Select(el =>
                {
                    var item = cacheItem.MenuItems.First(cond => el.Code == cond.Code);
                    return new MenuItemInfo()
                    {
                        Code = item.Code,
                        Desc = item.Desc,
                        Color = item.Color,
                        Price = isContractor ? item.PriceContractor : item.Price,
                        PlateId = item.PlateId,
                        Name = item.Name,
                        ImageUrl = item.ImageUrl,
                        Plate = new PlateInfo() { Code = el.Code, Uid = el.Uid }

                    };
                }).OrderBy(el => el.Name).ToList();


            }

            NotifyOnTransactionChanged();
            ts.Cancel();
            ts = new CancellationTokenSource();
            var isStatble = await IsStablizeReadingAsync(ts.Token);
            if (!isStatble)
                return Transaction;

            if (plates.Count() == 0)
                Transaction.CustomerMessage = string.Empty;

            //validate
            var errorMessage = ValidateReading(plates, cacheItem);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Transaction.PaymentState = PaymentState.Rejected;
                Transaction.CustomerMessage = errorMessage;
                return Transaction;
            }


            if (Transaction.Amount > 0 && Transaction.PaymentState == Konbini.Messages.Enums.PaymentState.Init)
            {
                // Activate Payment device
                Transaction.CustomerMessage = "Please wait...";
                NotifyOnTransactionChanged();
                var result = await paymentManager.ActivatePaymentAsync(Transaction);

                if (Transaction.PaymentState == PaymentState.Init)
                {
                    if (result == CommandState.Received)
                    {
                        Transaction.PaymentState = PaymentState.ActivatingPayment;
                    }
                    else if (result == CommandState.Cancelled)
                    {
                        //Transaction.PaymentState = PaymentState.ActivatingPayment;
                    }
                    else if(result == CommandState.TimeOut)
                    {
                        Transaction.PaymentState = PaymentState.Failure;
                        Transaction.CustomerMessage = "Timeout";
                    }
                }



            }
            return Transaction;
        }
        public async Task CancelTransactionAsync()
        {
            Transaction.CustomerMessage = "Cancelling transaction...";
            //var result = await paymentManager.CancelPaymentAsync(Transaction);
            // Harr coded ressult 
            var result = CommandState.Received;
            if (result == CommandState.Received)
            {
                Transaction = new TransactionInfo();
                Transaction.PaymentState = Konbini.Messages.Enums.PaymentState.Init;
                Transaction.MenuItems = new List<MenuItemInfo>();
                Transaction.Id = Guid.NewGuid();
                Transaction.PaymentType = paymentManager.PaymentType;
                NotifyOnTransactionChanged();
            }
           
            else if (result == CommandState.Cancelled)
            {
                Transaction.PaymentState = PaymentState.ActivatingPayment;
            }
            else
            {
                Transaction.PaymentState = PaymentState.ActivatedPaymentError;
            }
            NotifyOnTransactionChanged();
        }
        private DateTime lastReading = DateTime.MinValue;
        
        private int readingStage = 0;
        public async Task<bool> IsStablizeReadingAsync(CancellationToken token)
        {
            lastReading = DateTime.Now;
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(10, token);
                if (DateTime.Now.Subtract(lastReading).TotalMilliseconds > scanningStableAfterMiliseconds)
                    break;
            }
            return !token.IsCancellationRequested;
        }
        private string ValidateReading(List<PlateReadingInput> plates, SaleSessionCacheItem cacheItem)
        {
            var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
            return tableAppService.Validate(plates, cacheItem);
        }

        private async Task<SaleSessionCacheItem> GetSaleSessionCacheItemAsync()
        {
            var currentTime = Convert.ToInt32(string.Format("{0:00}{1:00}", DateTime.Now.Hour, DateTime.Now.Minute));
            var isCachedDataIsClear = false;
            var cacheItem = await cacheManager.GetCache(SaleSessionCacheItem.CacheName).Get(SaleSessionCacheItem.CacheName, async () =>
            {
                var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
                isCachedDataIsClear = true;
                return await tableAppService.GetSaleSessionInternalAsync();
            });
            if (cacheItem.SessionInfo == null || (cacheItem.SessionInfo != null && Convert.ToInt32(currentTime) > Convert.ToInt32(cacheItem.SessionInfo.ToHrs)))
            {

                cacheManager.GetCache(SaleSessionCacheItem.CacheName).Clear();
            }
            if (isCachedDataIsClear)
            {
                signalRCommunicator.UpdateSessionInfo(cacheItem.SessionInfo);
            }

            return cacheItem;
        }


        public void NotifyOnTransactionChanged()
        {
            signalRCommunicator.UpdateTransactionInfo(Transaction);
        }

        public async Task GenerateSaleTransactionAsync()
        {
            //TODO store transaction to database
            // prevent generating transaction if Amount = 0 or PlatCount = 0
            if (Transaction.Amount == 0 || Transaction.PlateCount == 0)
                return;
            var tableAppService = IocManager.Instance.Resolve<ITableAppService>();
            await tableAppService.GenerateTransactionAsync(Transaction);

        }

        public async Task GetTableDeviceSettingsAsync()
        {
            startWatchingOnProcessForServiceappearance();
            await tableSettingsManager.GetSettingsAsync();

        }

        private void startWatchingOnProcessForServiceappearance()
        {
            new Thread(new ThreadStart(watchOnThread)).Start();
        }
        private void watchOnThread()
        {
            while (!OnSale)
            {

                var tableProcess = Process.GetProcessesByName(tableSettingsManager.TableProcessName);
                if (tableProcess != null && tableProcess.Length > 0)
                {
                    tableSettingsManager.IsServiceRunning = true;
                }
                else
                {
                    tableSettingsManager.IsServiceRunning = false;
                }
                Thread.Sleep(500);

            }
        }

        public void HandleMessage(IMessage message)
        {
            var msg = Encoding.UTF8.GetString(message.Body);
            var cmd = JsonConvert.DeserializeObject<NsqCameraResponseCommand>(msg);
            if (cmd.Command == UniversalCommandConstants.CameraResponse)
            {
                if(Transaction != null)
                {
                    Transaction.BeginTranImage = cmd.BeginImage;
                    Transaction.EndTranImage = cmd.EndImage;
                }

                //Save Transaction
                GenerateSaleTransactionAsync();
            }
        }

        public void LogFailedMessage(IMessage message)
        {
            
        }
    }
    public class ClientInfo
    {
        public string ConnectionId { get; set; }
        public string Group { get; set; }
    }
}
