using Konbini.RfidFridge.Domain.DTO;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Data;
using Konbini.RfidFridge.Service.Lib;
using Konbini.RfidFridge.Service.Util;
using System.Diagnostics;
using System.Timers;
using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain;

namespace Konbini.RfidFridge.Service.Core
{
    public class FridgeInterface
    {
        #region Private Properties
        private UIntPtr readerHandler = UIntPtr.Zero;
        private UIntPtr InvenParamSpecList = UIntPtr.Zero;
        private bool InventoryFlg = false;
        private Thread inventoryThrd = null;
        private Thread _checkDoorStatus;
        private bool _oldDoorFlg;
        public bool IsTransacting
        {
            get => isTransacting;
            set
            {
                isTransacting = value;
                GlobalAppData.IsTransacting = value;
            }
        }
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private List<TagData> NotTopupTags = new List<TagData>();
        private bool doorThreadIsRunning;
        private bool doorThreadFlag;
        private bool DEV_MODE = System.Diagnostics.Debugger.IsAttached;
        private MachineStatus currentMachineStatus;
        private System.Timers.Timer testPublishInventoryTimer = new System.Timers.Timer();
        private bool IsDebug { get; set; }
        private int InventoryDelay { get; set; }
        private Guid TransactionImageGuid { get; set; }

        private bool RemoveTagIdChecksum { get; set; }
        #endregion

        #region Public Properties
        public List<string> ComPorts { get; set; }
        public int InventoryAdditionalDelay { get; set; }
        public string LockComport { get; set; }
        public string InventoryComport { get; set; }
        public FridgeReaderVersion InventoryReaderVersion { get; set; }
        public double CurrentCredit { get; set; }
        public List<int> AntennaList { get; set; }
        public double TotalOrder { get; set; }
        public bool IsTestTransaction { get; set; }
        public List<string> TransactionImages { get; set; }

        public static List<string> OldTagIds = new List<string>();
        public List<string> TakedOutItems = new List<string>();
        public List<InventoryDto> Inventories = new List<InventoryDto>();
        public static List<InventoryDto> OrderList = new List<InventoryDto>();
        public static List<InventoryDto> OldProducts = new List<InventoryDto>();
        public static List<InventoryDto> CurrentInventory = new List<InventoryDto>();
        public static List<TagData> TagIds = new List<TagData>();
        public static List<StateTagIdDto> RestockRemovedTags = new List<StateTagIdDto>();

        public static List<TagData> MissedTagIds = new List<TagData>();
        public static List<TagData> PaymentTagsId = new List<TagData>();
        public static List<TagData> TakenOutTags = new List<TagData>();

        public static List<TagData> OnTransactionTagsId = new List<TagData>();

        public FridgeReaderMode CurrentInventoryMode;
        public byte CurrentInventoryMask;

        public PaymentType SelectedPaymentMode { get; set; }
        #endregion

        #region Events
        public Action<List<InventoryDto>, int, List<string>> OnTransactionReadyForPayment { get; set; }

        public Action<List<InventoryDto>, int> OnTransactionFinished { get; set; }
        public Action<MachineStatus> OnMachineStatusChange { get; set; }
        public MachineStatus CurrentMachineStatus
        {
            get => currentMachineStatus;
            set
            {
                currentMachineStatus = value;
                OnMachineStatusChange?.Invoke(value);
                GlobalAppData.CurrentMachineStatus = value;
                //  MachineStatusService.NotifyChangeToMachineStatus(value);
            }
        }
        public Action<DoorState> OnDoorStateChange { get; set; }
        public DoorState CurrentDoorState { get; set; }
        public Action<List<string>, TagChangeEvent> OnTagChange { get; set; }
        public Action<List<InventoryDto>> OnReportInventory { get; set; }
        public Action ReloadInventoryCallback { get; set; }

        public Action<OrderDto> OnTxnOrderChanged { get; set; }
        public List<InventoryDto> GetCurrentInventory()
        {
            return CurrentInventory;
        }
        #endregion

        #region Constructor
        private LogService LogService;
        private StompService StompService;
        private RabbitMqService RabbitMqService;
        private IInventoryService InventoryService;
        private FridgeLockInterface FridgeLockInterface;
        private CameraInterface CameraInterface;
        private CustomerUINotificationService CustomerUINotificationService;
        // private MachineStatusService MachineStatusService;
        private SlackService SlackService;
        private UnstableTagService UnstableTagService;
        private DeviceCheckingService DeviceCheckingService;

        public FridgeInterface(LogService logService, StompService stompService, RabbitMqService rabbitMqService,
            IInventoryService inventoryService, FridgeLockInterface fridgeLockInterface, CameraInterface cameraInterface,
            CustomerUINotificationService customerUINotificationService,
            SlackService slackService,
            UnstableTagService unstableTagService,
            DeviceCheckingService deviceCheckingService
            )
        {
            LogService = logService;
            StompService = stompService;
            RabbitMqService = rabbitMqService;
            InventoryService = inventoryService;
            FridgeLockInterface = fridgeLockInterface;
            CameraInterface = cameraInterface;
            CustomerUINotificationService = customerUINotificationService;
            SlackService = slackService;
            UnstableTagService = unstableTagService;
            DeviceCheckingService = deviceCheckingService;
        }

        string CurrentPath;
        public void Init(string path)
        {
            try
            {
                CurrentPath = path;
                ComPorts = SerialPort.GetPortNames().ToList();
                var result = 0;
                if (InventoryReaderVersion == FridgeReaderVersion.V1)
                {
                    result = RFIDLIB.rfidlib_reader_v1.RDR_LoadReaderDrivers($"{path}\\Drivers");
                }
                else
                {
                    result = RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers($"{path}\\Drivers");
                }
                LogService.LogInfo($"Load driver result: {result}");

                ConnectToHardware();
                InitInventoryData();

                StartRecordTag();
                StartRecordLock();
                CurrentCredit = 1000;
                TotalOrder = 0;

                //IsDebug = true;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }
        #endregion

        #region Transaction
        public void StartTransaction(PaymentType paymentType = PaymentType.NONE)
        {


            SelectedPaymentMode = paymentType;

            TransactionImageGuid = Guid.NewGuid();
            TransactionImages = new List<string>();
            var imageFile = $"{TransactionAction.START}_{TransactionImageGuid}";
            CameraInterface.CaptureImage(imageFile);
            TransactionImages.Add(imageFile);

            LogService.LogInfo("Start Transaction");
            LogService.LogInfo("Payment Type: " + SelectedPaymentMode.ToString());

            //if (CurrentMachineStatus != MachineStatus.IDLE)
            //{
            //    LogService.LogInfo("StartTransaction IDLE");
            //    CurrentMachineStatus = MachineStatus.IDLE;
            //}

            if (!InventoryFlg)
            {
                this.inventoryThrd?.Abort();
                this.StartRecordTag();
            }

            OrderList = new List<InventoryDto>();

            HandleOpenDoor(MachineStatus.TRANSACTION_START);

            IsTransacting = true;

            //StartRecordLock();
        }

        private void HandleOpenDoor(MachineStatus machineStatus)
        {
            var code = OpenDoor();
            var openTryTime = 0;
            if (code == 0)
            {
                do
                {
                    FridgeLockInterface.GetDoorStatus(ref isDoorOpen, ref isAlarm, ref isPassing, ref temperature);
                    Thread.Sleep(300);
                    if (++openTryTime >= 50)
                    {
                        LogService.LogInfo("Timeout to open the door when start transaction!");
                        break;
                    }
                }
                while (!isDoorOpen);

                if (isDoorOpen)
                {
                    CurrentMachineStatus = machineStatus;
                    //IsTransacting = true;
                    CustomerUINotificationService.OpenDoor();
                }
                else
                {
                    LogService.LogInfo("Door stucked");
                    SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "Door stucked");
                    CurrentMachineStatus = MachineStatus.FAIL_TO_OPEN_THE_DOOR;
                    IsTransacting = false;
                }
            }
            else
            {
                var msg = $"Fail to open the door when start transaction! | Code {code}";
                LogService.LogInfo(msg);
                SlackService.SendAlert(RfidFridgeSetting.Machine.Name, msg);
                CurrentMachineStatus = MachineStatus.FAIL_TO_OPEN_THE_DOOR;
                IsTransacting = false;
            }
        }

        public void ConfirmTransaction()
        {
            CurrentMachineStatus = MachineStatus.TRANSACTION_CONFIRM;
        }

        public void ReopenDoor()
        {
            //CurrentMachineStatus = MachineStatus.TRANSACTION_REOPENDOOR;
            HandleOpenDoor(MachineStatus.TRANSACTION_REOPENDOOR);
        }


        public Action<List<InventoryDto>, int, List<string>> OnPreauthTxnFinish { get; set; }

        public void EndPreauthTransaction()
        {
            LogService.LogInfo("Preauth Finished, doing Charge and Refund...");
            Thread.Sleep((InventoryDelay * 2) + InventoryAdditionalDelay);
            var imageFile = $"{TransactionAction.END}_{TransactionImageGuid}";
            CameraInterface.CaptureImage(imageFile);
            TransactionImages.Add(imageFile);

            IsTransacting = false;
            var a = OrderList.Sum(x => x.Price);
            var roundedAmount = Math.Round(a, 2);
            var amount = int.Parse((roundedAmount * 100).ToString());
            LogService.LogInfo("Amount: " + amount);
        }

        public void EndTransaction()
        {
            LogService.LogInfo("Ending Transaction!!!");
            LogService.LogInfo($"Inventory Delay: {InventoryDelay}");
            LogService.LogInfo($"Additional Inventory Delay: {InventoryAdditionalDelay}");

            Thread.Sleep((InventoryDelay * 2) + InventoryAdditionalDelay);

            var imageFile = $"{TransactionAction.END}_{TransactionImageGuid}";
            CameraInterface.CaptureImage(imageFile);
            TransactionImages.Add(imageFile);

            IsTransacting = false;

            var a = OrderList.Sum(x => x.Price);

            var roundedAmount = Math.Round(a, 2);
            var amount = int.Parse((roundedAmount * 100).ToString());
            GlobalAppData.CurrentTransactionAmount = amount;
            LogService.LogInfo("Amount: " + amount);
            LogService.LogInfo("SelectedPaymentMode: " + SelectedPaymentMode);

            if (RfidFridgeSetting.System.Payment.Magic.TerminalType == "IM30")
            {
                if (SelectedPaymentMode == PaymentType.MAGIC)
                {
                    OnPreauthTxnFinish?.Invoke(OrderList, amount, TransactionImages);
                }
                else
                {
                    this.OnTransactionReadyForPayment?.Invoke(OrderList, amount, TransactionImages);
                }
            }
            else
            {
                this.OnTransactionReadyForPayment?.Invoke(OrderList, amount, TransactionImages);
            }
        }
        #endregion

        #region Door & Lock
        public void StartRecordLock()
        {
            do
            {
                doorThreadIsRunning = false;
                Thread.Sleep(200);
            }

            while (doorThreadIsRunning);
            doorThreadFlag = true;
            _checkDoorStatus?.Abort();
            _checkDoorStatus = new Thread(GetDoorStatus);
            _checkDoorStatus.Start();
        }
        public int OpenDoor()
        {
            int iret = FridgeLockInterface.OpenDoor();
            if (iret == 0)
            {
                LogService.LogInfo("Open the door successfully.");
                //OnDoorStateChange?.Invoke(DoorState.OPEN);
                CurrentDoorState = DoorState.OPEN;
            }
            else
            {
                LogService.LogInfo("Failed to open the door | " + iret);
            }
            return iret;
        }


        private bool isDoorOpen = false;
        private bool isAlarm = false;
        private bool isPassing = false;
        private float temperature = 0.0f;
        private bool isTransacting;

        private void GetDoorStatus()
        {
            LogService.LogInfo("Get Door Status Thread");
            while (doorThreadFlag)
            {
                doorThreadIsRunning = true;

                //if (IsTransacting)
                //{

                FridgeLockInterface.GetDoorStatus(ref isDoorOpen, ref isAlarm, ref isPassing, ref temperature);

                if (_oldDoorFlg != isDoorOpen)
                {
                    CurrentDoorState = isDoorOpen == true ? DoorState.OPEN : DoorState.CLOSE;
                    OnDoorStateChange?.Invoke(CurrentDoorState);
                }

                if (isAlarm)
                {
                    LogService.LogInfo("DOOR ALARM!!");
                }

                Debug($"IsTransacting: {IsTransacting}, isDoorOpen: {isDoorOpen}");
                if ((IsTransacting && isDoorOpen == false))
                {
                    // EndTransaction();
                    ConfirmTransaction();
                }

                _oldDoorFlg = isDoorOpen;

                //}
                Thread.Sleep(300);
            }

        }
        #endregion

        #region Fridge
        private void ConnectInventory(string com = "", Action onSuccess = null)
        {
            try
            {
                bool.TryParse(RfidFridgeSetting.System.Inventory.RemoveTagIdChecksum, out bool removeTagIdCheckSum);
                RemoveTagIdChecksum = removeTagIdCheckSum;

                var invenComport = "";
                if (string.IsNullOrEmpty(com))
                {
                    invenComport = InventoryComport;
                }
                else
                {
                    invenComport = com;
                }

                DeviceCheckingService.AddToChecklist(Domain.Enums.DeviceChecking.DeviceName.RFID_READER, invenComport);

                var readerName = InventoryReaderVersion == FridgeReaderVersion.V1 ? "RD5100" : "RD5100F";
                string GetConnString(string comport)
                {
                    var connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerName + ";" +
                                  RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_COM + ";" +
                                  RFIDLIB.rfidlib_def.CONNSTR_NAME_COMNAME + "=" + comport + ";" +
                                  RFIDLIB.rfidlib_def.CONNSTR_NAME_COMBARUD + "=" + RfidFridgeSetting.System.Comport.RfidReaderBaurate + ";" +
                                  RFIDLIB.rfidlib_def.CONNSTR_NAME_COMFRAME + "=" + RfidFridgeSetting.System.Comport.RfidReaderFrame + ";" +
                                  RFIDLIB.rfidlib_def.CONNSTR_NAME_BUSADDR + "=" + "255";
                    return connstr;
                }

                bool Connnect(string comport)
                {
                    LogService.LogInfo("Detect inventory: " + comport);
                    LogService.LogInfo("Antenna Setting: " + string.Join(",", AntennaList));
                    LogService.LogInfo("Reader version: " + InventoryReaderVersion);

                    int iret = 0;
                    if (InventoryReaderVersion == FridgeReaderVersion.V1)
                    {
                        iret = RFIDLIB.rfidlib_reader_v1.RDR_Open(GetConnString(invenComport), ref readerHandler);
                    }
                    else
                    {
                        iret = RFIDLIB.rfidlib_reader.RDR_Open(GetConnString(invenComport), ref readerHandler);
                    }
                    if (iret == 0)
                    {
                        DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.RFID_READER, Domain.Enums.DeviceChecking.DeviceStatus.OK);
                        LogService.LogInfo($"Found inventory at {invenComport}");
                        onSuccess?.Invoke();
                        if (InventoryReaderVersion == FridgeReaderVersion.V1)
                        {
                            InvenParamSpecList = RFIDLIB.rfidlib_reader_v1.RDR_CreateInvenParamSpecList();
                            RFIDLIB.rfidlib_aip_iso15693.ISO15693_CreateInvenParam(InvenParamSpecList, 0, 0, 0, 0);
                        }

                        return true;
                    }
                    else
                    {
                        DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.RFID_READER, Domain.Enums.DeviceChecking.DeviceStatus.ERROR, iret.ToString());
                        LogService.LogInfo($"Connect to inventory error: {iret}");
                    }
                    return false;
                }

                Connnect(invenComport);
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }
        private void ConnectToHardware()
        {
            DeviceCheckingService.AddToChecklist(Domain.Enums.DeviceChecking.DeviceName.LOCK_CONTROLLER, LockComport);
            LogService.LogInfo("Start detect Lock at: " + LockComport);
            var result = FridgeLockInterface.Connect(LockComport);
            LogService.LogInfo("Result: " + result);

            if (result == 0)
            {
                LogService.LogInfo("Lock is ok");
                DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.LOCK_CONTROLLER, Domain.Enums.DeviceChecking.DeviceStatus.OK);
                bool isDoorOpen = false;
                bool isAlarm = false;
                bool isPassing = false;
                float temperature = 0.0f;
                FridgeLockInterface.GetDoorStatus(ref isDoorOpen, ref isAlarm, ref isPassing, ref temperature);

                if (isDoorOpen)
                {
                    CurrentDoorState = DoorState.OPEN;
                }
                else
                {
                    CurrentDoorState = DoorState.CLOSE;
                }
                //OnDoorStateChange?.Invoke(CurrentDoorState);

                LogService.LogInfo("Door status: " + CurrentDoorState);
            }
            else
            {
                DeviceCheckingService.UpdateStatus(Domain.Enums.DeviceChecking.DeviceName.LOCK_CONTROLLER, Domain.Enums.DeviceChecking.DeviceStatus.ERROR, result.ToString());
            }
            this.ConnectInventory();
        }
        private void StartRecordTag()
        {
            Debug("Start record tag");
            if (inventoryThrd != null && this.inventoryThrd.IsAlive)
            {
                Debug("Start record tag | Kill old thread");
                this.inventoryThrd?.Abort();
            }
            // Set Default inventory mode is Storage
            if (InventoryReaderVersion == FridgeReaderVersion.V1)
            {
                inventoryThrd = new Thread(InventoryProc);
            }
            else
            {
                SetInventoryMode(FridgeReaderMode.STORAGE, miss: false, exist: true, change: false, abnormal: false);
                inventoryThrd = new Thread(InventoryProcV2);

                // SetInventoryModeBaseOnState();
            }
            Debug("Beginnnn record tag");
            inventoryThrd?.Start();
        }


        public void ReconnectInventory()
        {
            LogService.LogInfo("Retrying to reconnect to inventory");
            InventoryFlg = false;
            //RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter(readerHandler);
            while (inventoryThrd.IsAlive)
            {
                LogService.LogInfo("Stopping inventory...");
                Thread.Sleep(1000);
            }
            LogService.LogInfo("Inventory is stopped.");

            //Thread.Sleep(1000);

            RFIDLIB.rfidlib_reader.RDR_Close(readerHandler);
            readerHandler = UIntPtr.Zero;

            LogService.LogInfo("Reconnecting inventory.");

            ConnectInventory();
            StartRecordTag();

            LogService.LogInfo("Reconnecting inventory | Finished");

        }
        private void SetInventoryMode(FridgeReaderMode fridgeReaderMode, bool miss, bool exist, bool change, bool abnormal)
        {
            //LogService.LogInfo($"Inventory mode: {fridgeReaderMode} | MISS: {miss}, EXIST: {exist}, CHANGE: {change}, ANNORMAL: {abnormal}");
            CurrentInventoryMode = fridgeReaderMode;
            Byte mask = 0;
            if (miss)
            {
                mask |= 0x01;
            }
            if (exist)
            {
                mask |= 0x02;
            }
            if (change)
            {
                mask |= 0x04;
            }
            if (abnormal)
            {
                mask |= 0x08;
            }

            CurrentInventoryMask = mask;
        }

        public void SetInventoryModeBaseOnState()
        {
            switch (CurrentMachineStatus)
            {
                case MachineStatus.IDLE:
                    SetInventoryMode(FridgeReaderMode.STORAGE, miss: false, exist: true, change: false, abnormal: false);
                    break;
                case MachineStatus.TRANSACTION_START:
                case MachineStatus.TRANSACTION_REOPENDOOR:
                    RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter(readerHandler);
                    OnTransactionTagsId.Clear();
                    SetInventoryMode(FridgeReaderMode.NORMAL, miss: true, exist: false, change: true, abnormal: false);
                    // SetInventoryMode(FridgeReaderMode.STORAGE, miss: false, exist: true, change: false, abnormal: false);
                    break;
                case MachineStatus.TRANSACTION_CONFIRM:
                    //SetInventoryMode(FridgeReaderMode.PAYMENT, miss: true, exist: false, change: false, abnormal: false);
                    break;
                case MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC:
                    //SetInventoryMode(FridgeReaderMode.ABNORMAL, miss: false, exist: false, change: false, abnormal: true);
                    SetInventoryMode(FridgeReaderMode.STORAGE, miss: false, exist: true, change: false, abnormal: false);
                    break;
                case MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC_TRACING:
                case MachineStatus.UNLOADING_PRODUCT:
                    SetInventoryMode(FridgeReaderMode.NORMAL, miss: true, exist: false, change: false, abnormal: false);
                    break;
            }
        }

        private void InventoryProcV2()
        {
            InventoryFlg = true;
            while (InventoryFlg)
            {
                try
                {
                    //var tags = new List<TagData>();
                    var watch = new Stopwatch();
                    watch.Start();
                    SetInventoryModeBaseOnState();
                    OldTagIds = TagIds.Select(x => x.TagId).ToList();
                    int iret = InventoryV2((byte)CurrentInventoryMode, CurrentInventoryMask, TagIds);
                    OnTransactionTagsId.AddRange(TagIds);
                    OnTransactionTagsId.Distinct();

                    ProcessTags();
                    InventoryDelay = int.Parse(Math.Round(watch.Elapsed.TotalMilliseconds).ToString());
                    watch.Stop();
                    if (iret != 0)
                    {
                        SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "RFID Error | Code: " + iret);
                        if (iret == -27)
                        {
                            ReconnectInventory();
                        }
                        if (iret == -39)
                        {
                            LogService.LogInfo("Please do shelf storage first(Inventory with mode=0x00).");
                        }
                        else if (iret == -17)
                        {
                            LogService.LogInfo("It's failure in the operation.In order to get the remaining tags You can try to inventory with mode=0x01/0x02 and mask=0x02.");
                        }
                        else
                        {
                            LogService.LogInfo("There are some errors in the connection.Please reconnect the RFID reader | Code: " + iret);
                        }
                        break;
                    }
                    Debug($"CurrentInventoryMode: {CurrentInventoryMode} | Mask: {CurrentInventoryMask} | Tags: {TagIds.Count} | Delay: {InventoryDelay}");
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            }
        }

        private void ProcessTags()
        {
            switch (CurrentMachineStatus)
            {
                case MachineStatus.IDLE:
                case MachineStatus.RESTOCKING_PRODUCT:
                    PublishCurrentInventory();
                    break;
                case MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC:
                    PublishUnstableInventory();
                    break;
                case MachineStatus.UNSTABLE_TAGS_DIAGNOSTIC_TRACING:
                case MachineStatus.UNLOADING_PRODUCT:
                    PublishMissedInventory();
                    break;
                case MachineStatus.TRANSACTION_START:
                    #region Start
                    #endregion
                    break;
                case MachineStatus.TRANSACTION_CONFIRM:

                    #region Start
                    if (IsTransacting)
                    {
                        MissedTagIds = TagIds.Where(x => x.Mode == FridgeReaderMode.NORMAL && x.Type == FridgeReaderTagType.MISS).ToList();
                        OrderList = new List<InventoryDto>();
                        LogService.LogReaderInfo($"Missed tags: {string.Join(",", MissedTagIds.Select(x => x.TagId))}");
                        var orderItems = MissedTagIds.Select(x => x.TagId).ToList();
                        foreach (var item in orderItems)
                        {
                            var product = this.Inventories.FirstOrDefault(x => x.TagId == item);
                            if (product != null)
                            {
                                OrderList.Add(product);
                            }
                        }
                        TotalOrder = OrderList.Sum(x => x.Price);

                        var order = new OrderDto
                        {
                            Amount = TotalOrder,//.ToString("C"),
                            Inventories = OrderList
                        };
                        RabbitMqService.PublishOrder(order);
                        OnTxnOrderChanged?.Invoke(order);
                    }
                    #endregion
                    break;

            }
        }

        private void PublishMissedInventory()
        {
            // Current DB data
            var missedTagIds = TagIds.Where(x => x.Mode == FridgeReaderMode.NORMAL && x.Type == FridgeReaderTagType.MISS).ToList();
            var currentInventory = this.Inventories.Select(x => x.TagId).ToList();
            if (currentInventory.Count > 0)
            {
                NotTopupTags = missedTagIds.Where(tag => !currentInventory.Any(inven => inven == tag.TagId)).ToList();
            }
            else
            {
                NotTopupTags = missedTagIds;
            }

            // Mapping with reader data
            var inventoryItems = this.Inventories.Where(inven => missedTagIds.Any(tag => tag.TagId == inven.TagId)).OrderBy(inven => inven.Product.ProductName).ToList();
            // Set Antenna
            inventoryItems.ForEach(x => x.TrayLevel = GetTagInfo(x.TagId).Antenna);
            // Not topped up items
            inventoryItems.InsertRange(0, NotTopupTags.Select(tag => new InventoryDto("", tag.TagId, tag.Antenna, 0, new ProductDto() { ProductName = "TAG" })));

            // For topup
            var notTopupTagsDto = NotTopupTags.ToList().Select(tag => new TagIdDto(tag.TagId, tag.Antenna)).ToList();
            if (notTopupTagsDto != null && notTopupTagsDto.Count() > 0)
            {
                // For backend
                // StompService.PublishTagId(notTopupTagsDto);

                // For new 7inch UI
                // RabbitMqService.PublishTagId(notTopupTagsDto);
            }

            // For current inventory
            if (inventoryItems != null)
            {
                StompService.PublishMissedInventory(inventoryItems);

                RabbitMqService.PublishMissedInventory(inventoryItems);
            }
        }

        private void PublishUnstableInventory()
        {
            // Current DB data
            var currentInventory = this.Inventories.Select(x => x.TagId).ToList();
            var tagIds = UnstableTagService.UnstableInventories.Select(x => new TagData { TagId = x.Inventory.TagId }).ToList();
            if (currentInventory.Count > 0)
            {
                NotTopupTags = tagIds.Where(tag => !currentInventory.Any(inven => inven == tag.TagId)).ToList();
            }
            else
            {
                NotTopupTags = tagIds;
            }

            // Mapping with reader data
            var inventoryItems = this.Inventories.Where(inven => tagIds.Any(tag => tag.TagId == inven.TagId)).OrderBy(inven => inven.Product.ProductName).ToList();

            // Not topped up items
            inventoryItems.InsertRange(0, NotTopupTags.Select(tag => new InventoryDto("", tag.TagId, tag.Antenna, 0, new ProductDto() { ProductName = "TAG" })));

            // Set Antenna
            //inventoryItems.Where(x => x.TrayLevel != 0).ToList().ForEach(x => x.TrayLevel = GetTagInfo(x.TagId).Antenna);

            var unstableInventoryItems = UnstableTagService.UnstableInventories;
            foreach (var unstable in unstableInventoryItems)
            {
                var inventory = inventoryItems.FirstOrDefault(x => x.TagId == unstable.Inventory.TagId);
                if (inventory != null)
                {
                    unstable.Inventory.Product = inventory.Product;
                    unstable.Inventory.TagId = inventory.TagId;
                    unstable.Inventory.Price = inventory.Price;
                    unstable.Inventory.Id = inventory.Id;
                }

                var anten = GetTagInfo(unstable.Inventory.TagId).Antenna;
                if (anten != -1)
                {
                    unstable.Inventory.TrayLevel = anten;
                    //if (unstable.Inventory.TrayLevel == 0)
                    //{
                    //    unstable.Inventory.TrayLevel = anten;
                    //}
                    //else
                    //{

                    //}
                }


            }

            if (unstableInventoryItems != null && unstableInventoryItems.Count > 0)
            {
                StompService.PublishUnstableInventory(unstableInventoryItems);
            }

            var oldHashTag = string.Join("|", OldTagIds.OrderBy(tag => tag));
            var hashTag = string.Join("|", TagIds.Select(tag => tag.TagId).OrderBy(tag => tag));

            if (oldHashTag != hashTag)
            {
                OnReportInventory?.Invoke(inventoryItems);

                var total = new List<string>();
                total.AddRange(OldTagIds);
                total.AddRange(TagIds.Select(tag => tag.TagId));
                var sum = total.Distinct().ToList();

                var added = sum.Except(OldTagIds).ToList();
                var removed = sum.Except(TagIds.Select(tag => tag.TagId)).ToList();
                LogService.LogReaderInfo("Added: " + string.Join("|", added));
                LogService.LogReaderInfo("Removed: " + string.Join("|", removed));
                if (removed.Count > 0)
                {
                    LogService.LogReaderInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                    LogService.LogReaderInfo("Removed: " + string.Join("|", removed));
                    OnTagChange?.Invoke(removed, TagChangeEvent.REMOVED);
                }
                if (added.Count > 0)
                {
                    LogService.LogReaderInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                    LogService.LogReaderInfo("Added: " + string.Join("|", added));
                    OnTagChange?.Invoke(added, TagChangeEvent.ADDED);
                }

            }
        }

        public void CleanRestockData()
        {
            RestockRemovedTags = new List<StateTagIdDto>();
        }

        private void PublishCurrentInventory()
        {
            // Current DB data
            var currentInventory = this.Inventories.Select(x => x.TagId).ToList();
            if (currentInventory.Count > 0)
            {
                NotTopupTags = TagIds.Where(tag => !currentInventory.Any(inven => inven == tag.TagId)).ToList();
            }
            else
            {
                NotTopupTags = TagIds;
            }

            // Mapping with reader data
            var inventoryItems = this.Inventories.Where(inven => TagIds.Any(tag => tag.TagId == inven.TagId)).OrderBy(inven => inven.Product.ProductName).ToList();
            // Set Antenna
            inventoryItems.ForEach(x => x.TrayLevel = GetTagInfo(x.TagId).Antenna);
            // Not topped up items
            inventoryItems.InsertRange(0, NotTopupTags.Select(tag => new InventoryDto("", tag.TagId, tag.Antenna, 0, new ProductDto() { ProductName = "TAG" })));

            // Set to current inventory
            CurrentInventory = inventoryItems;

            // For topup
            var notTopupTagsDto = NotTopupTags.ToList().Select(tag => new TagIdDto(tag.TagId, tag.Antenna)).ToList();
            if (notTopupTagsDto != null && notTopupTagsDto.Count() >= 0)
            {
                StompService.PublishTagId(notTopupTagsDto);

                if (CurrentMachineStatus == MachineStatus.RESTOCKING_PRODUCT)
                {
                    var removedTags = new List<StateTagIdDto>();

                    var data = NotTopupTags.ToList().Select(tag => new StateTagIdDto(tag.TagId, tag.Antenna, TagChangeEvent.ADDED, new ProductDto() { ProductName = "TAG" })).ToList();
                    foreach (var r in RestockRemovedTags)
                    {
                        if (!TagIds.Any(x => x.TagId == r.Id))
                        {
                            if (this.Inventories.Any(x => x.TagId == r.Id))
                            {
                                r.Product = r.Product;
                                r.State = TagChangeEvent.REMOVED;
                                removedTags.Add(r);
                            }
                        }
                    }
                    data.AddRange(removedTags);
                    RabbitMqService.PublishTagId(data);
                    var totalAdded = data.Where(x => x.State == TagChangeEvent.ADDED).Count();
                    var totalRemoved = data.Where(x => x.State == TagChangeEvent.REMOVED).Count();
                    Console.WriteLine($"Total Added: {totalAdded} | Removed: {totalRemoved}");
                }
            }

            // For current inventory
            if (inventoryItems != null)
            {
                if (!IsTransacting)
                {
                    StompService.PublishInventory(inventoryItems);

                    var dashboard = new DashboardDto(inventoryItems);
                    StompService.PublishProductSummarize(dashboard.ProductSummarize);
                }
            }

            var oldHashTag = string.Join("|", OldTagIds.OrderBy(tag => tag));
            var hashTag = string.Join("|", TagIds.Select(tag => tag.TagId).OrderBy(tag => tag));

            if (oldHashTag != hashTag)
            {
                OnReportInventory?.Invoke(inventoryItems);

                var total = new List<string>();
                total.AddRange(OldTagIds);
                total.AddRange(TagIds.Select(tag => tag.TagId));
                var sum = total.Distinct().ToList();

                var added = sum.Except(OldTagIds).ToList();
                var removed = sum.Except(TagIds.Select(tag => tag.TagId)).ToList();
                LogService.LogReaderInfo("Added: " + string.Join("|", added));
                LogService.LogReaderInfo("Removed: " + string.Join("|", removed));


                if (removed.Count > 0)
                {
                    LogService.LogReaderInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                    LogService.LogReaderInfo("Removed: " + string.Join("|", removed));

                    LogService.LogInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                    LogService.LogInfo("Removed: " + string.Join("|", removed));

                    if (CurrentMachineStatus == MachineStatus.RESTOCKING_PRODUCT)
                    {
                        foreach (var r in removed)
                        {
                            var i = this.Inventories.FirstOrDefault(x => x.TagId.Trim() == r.Trim());
                            if (i == null)
                            {
                                RestockRemovedTags.Add(new StateTagIdDto(r, 0, TagChangeEvent.REMOVED, new ProductDto() { ProductName = "TAG" }));
                            }
                            else
                            {
                                RestockRemovedTags.Add(new StateTagIdDto(r, 0, TagChangeEvent.REMOVED, i?.Product));
                            }
                            RestockRemovedTags = RestockRemovedTags.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                        }
                    }

                    OnTagChange?.Invoke(removed, TagChangeEvent.REMOVED);
                }
                if (added.Count > 0)
                {
                    LogService.LogReaderInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                    LogService.LogReaderInfo("Added: " + string.Join("|", added));

                    LogService.LogInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                    LogService.LogInfo("Added: " + string.Join("|", added));

                    OnTagChange?.Invoke(added, TagChangeEvent.ADDED);
                }


                //if (oldHashTag.Length > hashTag.Length)
                //{
                //    var removed = OldTagIds.Except(TagIds.Select(x => x.TagId)).ToList();
                //    LogService.LogReaderInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                //    LogService.LogReaderInfo("Removed: " + string.Join("|", removed));
                //    OnTagChange?.Invoke(removed, TagChangeEvent.REMOVED);
                //}
                //if (oldHashTag.Length < hashTag.Length)
                //{
                //    var added = TagIds.Select(x => x.TagId).Except(OldTagIds).ToList();
                //    LogService.LogReaderInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                //    LogService.LogReaderInfo("Added: " + string.Join("|", added));
                //    OnTagChange?.Invoke(added, TagChangeEvent.ADDED);
                //}
            }
        }

        private TagData GetTagInfo(string tagId)
        {
            var returnData = new TagData
            {
                Antenna = -1,
                TagId = string.Empty,
                Type = FridgeReaderTagType.EXIST,
                Mode = FridgeReaderMode.NONE
            };

            var d = TagIds.SingleOrDefault(tag => tag.TagId == tagId);
            if (d != null)
            {
                returnData = d;
            }

            return returnData;
        }
        private int InventoryV2(Byte mode, Byte mask, List<TagData> tags)
        {
            if (tags.Count > 0)
            {
                tags.Clear();
            }

            int iret = RFIDLIB.rfidlib_reader.RDR_FridgeInventory(readerHandler, mode, mask);

            if (iret != 0)
            {
                return iret;
            }

            UIntPtr TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(readerHandler, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
            while (TagDataReport != UIntPtr.Zero)
            {
                Byte tag_sta = 0;
                Byte ant_id = 0;
                Byte[] tag_data = new Byte[32];
                Byte nSize = (Byte)tag_data.Length;
                iret = RFIDLIB.rfidlib_reader.RDR_FridgeParseTagDataReport(TagDataReport, ref tag_sta, ref ant_id, tag_data, ref nSize);
                if (iret == 0)
                {
                    TagData tag = new TagData();
                    string strUid = BitConverter.ToString(tag_data, 0, (int)nSize).Replace("-", string.Empty);

                    if (RemoveTagIdChecksum)
                    {
                        strUid = strUid.Substring(8, strUid.Length - 8);
                    }

                    tag.TagId = strUid;
                    tag.Antenna = ant_id;
                    tag.Mode = (FridgeReaderMode)mode;
                    tag.Type = (FridgeReaderTagType)tag_sta;
                    tags.Add(tag);

                }
                TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(readerHandler, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
            }
            return 0;
        }

        private void InventoryProc()
        {
            try
            {
                InventoryFlg = true;
                var stopwatch = new Stopwatch();

                while (InventoryFlg)
                {
                    stopwatch.Reset();
                    stopwatch.Start();

                    OldTagIds = TagIds.Select(x => x.TagId).ToList();


                    int iret = Inventory(TagIds);
                    if (iret != 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    RFIDLIB.rfidlib_reader.RDR_CloseRFTransmitter(readerHandler);

                    if (IsTransacting)
                    {
                        var takedOut = OldTagIds.Except(TagIds.Select(x => x.TagId).ToList());
                        foreach (var item in takedOut)
                        {
                            var product = this.Inventories.FirstOrDefault(x => x.TagId == item);
                            if (product != null)
                            {
                                OrderList.Add(product);
                                TotalOrder = OrderList.Sum(x => x.Price);
                            }

                        }

                        var putBackItem = TagIds.Select(x => x.TagId).ToList().Except(OldTagIds);
                        foreach (var item in putBackItem)
                        {
                            var product = this.Inventories.FirstOrDefault(x => x.TagId == item);
                            OrderList.Remove(product);
                            TotalOrder = OrderList.Sum(x => x.Price);
                        }

                        var order = new OrderDto
                        {
                            Amount = TotalOrder,//.ToString("C"),
                            Inventories = OrderList
                        };

                        RabbitMqService.PublishOrder(order);
                    }

                    var currentInventory = this.Inventories.Select(x => x.TagId).ToList();

                    if (currentInventory.Count > 0)
                    {
                        NotTopupTags = TagIds.Where(x => !currentInventory.Any(y => y == x.TagId)).ToList();
                    }
                    else
                    {
                        NotTopupTags = TagIds;
                    }
                    var p = this.Inventories.Where(x => TagIds.Any(z => z.TagId == x.TagId)).OrderBy(x => x.Product.ProductName).ToList();
                    p.InsertRange(0, NotTopupTags.Select(inventory => new InventoryDto("", inventory.TagId, inventory.Antenna, 0, new ProductDto() { ProductName = "TAG" })));
                    var tags = NotTopupTags.ToList().Select(x => new TagIdDto(x.TagId, x.Antenna)).ToList();

                    if (tags != null && tags.Count() >= 0)
                    {
                        // Publish tag id
                        StompService.PublishTagId(tags);
                        Debug("PublishTagId: " + tags.Count);
                    }

                    // Set to current inventory
                    CurrentInventory = p;

                    if (p != null)
                    {
                        // Publish to customer screen;
                        RabbitMqService.PublishInventory(p);
                        Debug("PublishInventory: " + p.Count);

                        if (!IsTransacting)
                        {
                            // Publish to admin web
                            StompService.PublishInventory(p);

                            var dashboard = new DashboardDto(p);
                            StompService.PublishProductSummarize(dashboard.ProductSummarize);
                        }
                    }

                    var oldHashTag = string.Join("|", OldTagIds.OrderBy(x => x));
                    var hashTag = string.Join("|", TagIds.Select(x => x.TagId).OrderBy(x => x));

                    if (oldHashTag != hashTag)
                    {
                        OnReportInventory?.Invoke(p);
                        if (oldHashTag.Length > hashTag.Length)
                        {
                            var removed = OldTagIds.Except(TagIds.Select(x => x.TagId)).ToList();
                            LogService.LogReaderInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                            LogService.LogReaderInfo("Removed: " + string.Join("|", removed));
                            OnTagChange?.Invoke(removed, TagChangeEvent.REMOVED);
                        }
                        if (oldHashTag.Length < hashTag.Length)
                        {
                            var added = TagIds.Select(x => x.TagId).Except(OldTagIds).ToList();
                            LogService.LogReaderInfo("Inventory: " + string.Join("|", TagIds.Select(tag => tag.TagId)));
                            LogService.LogReaderInfo("Added: " + string.Join("|", added));
                            OnTagChange?.Invoke(added, TagChangeEvent.ADDED);
                        }
                    }

                    Thread.Sleep(100);
                    InventoryDelay = int.Parse(Math.Round(stopwatch.Elapsed.TotalMilliseconds).ToString());
                    Debug("Delay: " + InventoryDelay);
                    stopwatch.Stop();
                }

                InventoryFlg = false;
                inventoryThrd = null;
            }
            catch (Exception ex)
            {
                InventoryFlg = false;
                LogService.LogError(ex);
            }
        }
        private int Inventory(List<TagData> uids)
        {
            try
            {
                if (uids.Count > 0)
                {
                    uids.Clear();
                }

                List<Byte> antennaList = new List<byte>();

                foreach (var anten in AntennaList)
                {
                    antennaList.Add((Byte)anten);
                }
                var s = antennaList.ToArray();

                int iret = RFIDLIB.rfidlib_reader_v1.RDR_TagInventory(readerHandler, RFIDLIB.rfidlib_def.AI_TYPE_NEW, (byte)antennaList.Count, antennaList.ToArray(), InvenParamSpecList);

                if (iret != 0)
                {
                    return iret;
                }

                UIntPtr TagDataReport = RFIDLIB.rfidlib_reader_v1.RDR_GetTagDataReport(readerHandler, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
                while (TagDataReport != UIntPtr.Zero)
                {
                    UInt32 aip_id = 0;
                    UInt32 tag_id = 0;
                    UInt32 ant_id = 0;
                    Byte dsfid = 0;
                    Byte[] uid = new Byte[8];
                    iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, ref dsfid, uid);
                    if (iret == 0)
                    {
                        string strUid = BitConverter.ToString(uid, 0, (int)8).Replace("-", string.Empty);
                        if (!uids.Select(c => c.TagId).Contains(strUid))
                        {
                            uids.Add(new TagData { TagId = strUid, Antenna = (int)ant_id });
                            //Console.WriteLine(strUid);
                        }

                    }
                    TagDataReport = RFIDLIB.rfidlib_reader_v1.RDR_GetTagDataReport(readerHandler, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //first
                }

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }

            return 0;
        }
        #endregion

        #region Data
        public void InitInventoryData()
        {
            this.Inventories = ReadFromDb();
            LogService.LogInfo("Reaload inventories: " + Inventories.Count);
            ReloadInventoryCallback?.Invoke();
        }
        private List<InventoryDto> ReadFromDb()
        {
            var inventories = new List<InventoryDto>();
            inventories = InventoryService.GetAllFromWebApi().Result;
            return inventories;
        }
        #endregion

        #region Test

        public void TestSubCmd()
        {
            StompService.SubCommand();
        }
        public void TestPublishInventory()
        {
            var inven = new List<InventoryDto>();
            inven.Add(new InventoryDto("08d69615-6720-98f2-2d7c-38455fdc6063", "E00403500BDAF703", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302947", "ITEM 1", "", 1.2)));
            inven.Add(new InventoryDto("08d69615-679b-5522-7aa0-374ef019a53e", "E00403500BDAF704", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302947", "ITEM 2", "", 1.3)));


            RabbitMqService.PublishInventory(inven);
        }
        public void TestPublishMachineStatus(MachineStatus status)
        {
            CurrentMachineStatus = status;
            //RabbitMqService.PublishMachineStatus(status);
            RabbitMqService.PublishMachineStatus(status, "test");
        }
        public void TestPublishOrder()
        {
            var inven = new List<InventoryDto>();
            inven.Add(new InventoryDto("08d69615-6720-98f2-2d7c-38455fdc6063", "E00403500BDAF703", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302947", "ITEM 1", "", 1.2)));
            inven.Add(new InventoryDto("08d69615-679b-5522-7aa0-374ef019a53e", "E00403500BDAF704", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302947", "ITEM 2", "", 1.3)));

            var order = new OrderDto
            {
                Amount = 20,//.ToString("C"),
                Inventories = inven
            };

            RabbitMqService.PublishOrder(order);
        }
        public void TestPublishInventory_Web()
        {
            testPublishInventoryTimer.Interval = 1000;
            testPublishInventoryTimer.Elapsed += TestPublishInventoryTimer_Elapsed;
            testPublishInventoryTimer.Enabled = true;
            testPublishInventoryTimer.Start();
        }

        private void TestPublishInventoryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var inven = new List<InventoryDto>();
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f631", "E00403500BDAF701", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 1", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f632", "E00403500BDAF702", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 1", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f633", "E00403500BDAF703", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f634", "E00403500BDAF704", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302942", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f635", "E00403500BDAF705", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302942", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("", "E00403500BDAF701", 1, 2.2, new ProductDto("", "TAG", "", 1.2)));


            StompService.PublishInventory(inven);
        }

        public void TestPublishProductSummarize_web()
        {
            var inven = new List<InventoryDto>();
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f631", "E00403500BDAF701", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 1", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f632", "E00403500BDAF702", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 1", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f633", "E00403500BDAF703", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f634", "E00403500BDAF704", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302942", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f635", "E00403500BDAF705", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302942", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("", "E00403500BDAF701", 1, 2.2, new ProductDto("", "TAG", "", 1.2)));

            var dashboard = new DashboardDto(inven);


            StompService.PublishProductSummarize(dashboard.ProductSummarize);
        }

        public void TestPublishDashboard_web()
        {
            var inven = new List<InventoryDto>();
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f631", "E00403500BDAF701", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 1", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f632", "E00403500BDAF702", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 1", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f633", "E00403500BDAF703", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302941", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f634", "E00403500BDAF704", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302942", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("08d6a761-8018-071d-e48d-220db316f635", "E00403500BDAF705", 1, 2.2, new ProductDto("08d6932f-a5be-c06a-eb3e-c96482302942", "ITEM 2", "", 1.2)));
            inven.Add(new InventoryDto("", "E00403500BDAF701", 1, 2.2, new ProductDto("", "TAG", "", 1.2)));

            var dashboard = new DashboardDto(inven);
            dashboard.Temperature1 = 12.3;
            dashboard.Temperature2 = 14.4;

            StompService.PublishDashboard(dashboard);
        }

        public void TestPublishTag_Web()
        {
            var tags = new List<TagIdDto>();
            tags.Add(new TagIdDto() { Id = "E00403500BDAF703", TrayLevel = 1 });
            tags.Add(new TagIdDto() { Id = "E00403500BDAFB64", TrayLevel = 2 });
            tags.Add(new TagIdDto() { Id = "E00403500BDAFCA7", TrayLevel = 3 });

            StompService.PublishTagId(tags);

        }
        public void ListTagId2()
        {
            LogService.LogInfo("========TAGS========");

            foreach (var tag in OldTagIds)
            {
                LogService.LogInfo(tag);
            }
        }
        public void ListNotTopupTagId()
        {
            LogService.LogInfo("========NOT TOPUP TAGS========");

            foreach (var tag in this.NotTopupTags)
            {
                LogService.LogInfo(tag.TagId + " " + tag.Antenna);
            }
        }


        public void SetDebug()
        {
            IsDebug = !IsDebug;
        }

        private void Debug(string message)
        {
            if (IsDebug)
                Console.WriteLine(message);
        }
        #endregion

    }
    public class TagData
    {
        public string TagId = string.Empty;
        public int Antenna = 0;
        public FridgeReaderTagType Type = 0;
        public FridgeReaderMode Mode;
    }
}
