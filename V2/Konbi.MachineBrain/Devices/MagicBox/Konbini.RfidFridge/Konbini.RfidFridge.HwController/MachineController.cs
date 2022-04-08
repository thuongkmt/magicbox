using Autofac;
using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Core;
using Konbini.RfidFridge.Service.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;

namespace Konbini.RfidFridge.HwController
{
    [RoutePrefix("api/machine")]
    public class MachineController : ApiController
    {
        private FridgeInterface FridgeInterface;
        private QrPaymentService QrPaymentService;
        private IFridgePayment FridgePayment;

        public string AppVersion => $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}({AppBuildDate})";
        public string AppBuildDate => $"{System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString("dd/MM")}";

        public MachineController()
        {
            FridgeInterface = AutofacConfig.CurrentContainer.Resolve<FridgeInterface>();
            QrPaymentService = AutofacConfig.CurrentContainer.Resolve<QrPaymentService>();
            FridgePayment = AutofacConfig.CurrentContainer.Resolve<IFridgePayment>();

        }

        [HttpGet, Route("config/{key}")]
        public object GetConfig(string key)
        {
            var settings = Assembly
                .GetAssembly(typeof(RfidFridgeSetting))
                .GetTypes();

            foreach (var setting in settings)
            {
                var properties = setting.GetProperties();
                if (properties.Length > 0)
                {
                    var settingKey = setting.FullName.Replace(setting.Namespace, string.Empty);
                    settingKey = settingKey.Substring(1, settingKey.Length - 1).Replace("+", ".");

                    foreach (var propertyInfo in properties)
                    {
                        var configKey = $"{settingKey}.{propertyInfo.Name}";
                        if (configKey == key)
                        {
                            var value = propertyInfo.GetValue(setting);
                            return value;
                        }
                    }
                }
            }
            return "";
        }

        [HttpPost, Route("open")]
        public dynamic Open()
        {
            var result = FridgeInterface.OpenDoor();

            MachineApiData responseData = null;
            if (result != 0)
            {
                var errMess = $"Error code: {result}";
                responseData = new MachineApiData { Success = false, Errors = new List<string>() { errMess } };
            }
            else
            {
                responseData = new MachineApiData { Success = true, Errors = new List<string>() };
            }
            return responseData;
        }

        [HttpPost, Route("reopen")]
        public dynamic ReOpen()
        {
            MachineApiData responseData;
            try
            {
                FridgeInterface.ReopenDoor();

                responseData = new MachineApiData { Success = true, Errors = new List<string>() };
            }
            catch (Exception ex)
            {
                responseData = new MachineApiData { Success = false, Errors = new List<string>() { ex.ToString() } };
                return responseData;
            }
            return responseData;
        }

        [HttpPost, Route("endtransaction")]
        public dynamic EndTransaction()
        {
            MachineApiData responseData;
            try
            {
                
                FridgeInterface.EndTransaction();
                responseData = new MachineApiData { Success = true, Errors = new List<string>() };
            }
            catch (Exception ex)
            {
                responseData = new MachineApiData { Success = false, Errors = new List<string>() { ex.ToString() } };
                return responseData;
            }
            return responseData;
        }

        [HttpGet, Route("status")]
        public dynamic GetStatus()
        {
            FridgeInterface = AutofacConfig.CurrentContainer.Resolve<FridgeInterface>();
            return FridgeInterface.CurrentMachineStatus.ToString();
        }

        [HttpPost, Route("setstatus/{status}")]
        public dynamic SetStatus(string status)
        {
            MachineApiData responseData;
            try
            {
                FridgeInterface = AutofacConfig.CurrentContainer.Resolve<FridgeInterface>();
                var machineStatus = (MachineStatus)Enum.Parse(typeof(MachineStatus), status, true);
                FridgeInterface.CurrentMachineStatus = machineStatus;
                responseData = new MachineApiData { Success = true, Errors = new List<string>() };
            }
            catch (Exception ex)
            {
                responseData = new MachineApiData { Success = false, Errors = new List<string>() { ex.ToString() } };
                return responseData;
            }
            return responseData;
        }

        [HttpPost, Route("inventory/reload")]
        public dynamic ReloadInventory()
        {
            MachineApiData responseData;
            try
            {
                FridgeInterface = AutofacConfig.CurrentContainer.Resolve<FridgeInterface>();
                FridgeInterface.InitInventoryData();

                responseData = new MachineApiData { Success = true, Errors = new List<string>() };
            }
            catch (Exception ex)
            {
                responseData = new MachineApiData { Success = false, Errors = new List<string>() { ex.ToString() } };
                return responseData;
            }
            return responseData;
        }


        [HttpPost, Route("inventory/cleanrestockdata")]
        public dynamic CleanRestockData()
        {
            MachineApiData responseData;
            try
            {
                FridgeInterface = AutofacConfig.CurrentContainer.Resolve<FridgeInterface>();
                FridgeInterface.CleanRestockData();

                responseData = new MachineApiData { Success = true, Errors = new List<string>() };
            }
            catch (Exception ex)
            {
                responseData = new MachineApiData { Success = false, Errors = new List<string>() { ex.ToString() } };
                return responseData;
            }
            return responseData;
        }

        [HttpGet, Route("inventory/paymenttags")]
        public dynamic PaymentTags()
        {
            FridgeInterface = AutofacConfig.CurrentContainer.Resolve<FridgeInterface>();
            var tags = FridgeInterface.PaymentTagsId;
            return tags;
        }

        public IEnumerable<string> GetTest()
        {
            FridgeInterface = AutofacConfig.CurrentContainer.Resolve<FridgeInterface>();
            return new string[]
            {
                FridgeInterface.InventoryComport,
                FridgeInterface.LockComport,
            };
        }

        bool isProcessing;
        [HttpPost, Route("authqr/validate/{code}")]
        public bool Validate(string code)
        {
            //if(isProcessing)
            //{
            //    return false;
            //}
            if (FridgeInterface.IsTransacting)
            {
                return false;
            }
            var result = QrPaymentService.Validate(code);
            return result;
        }


        [HttpPost, Route("customer/action/pressstart")]
        public dynamic CustomerPressStartButton()
        {
            MachineApiData responseData;
            try
            {
                FridgePayment.CustomerAction(CustomerAction.PRESS_START_BUTTON);
                responseData = new MachineApiData { Success = true, Errors = new List<string>() };
            }
            catch (Exception ex)
            {
                responseData = new MachineApiData { Success = false, Errors = new List<string>() { ex.ToString() } };
                return responseData;
            }
            return responseData;
        }

        [HttpGet, Route("version")]
        public string GetSoftwareVersion()
        {
            return AppVersion;
        }

    }

  

    public class MachineApiData
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}
