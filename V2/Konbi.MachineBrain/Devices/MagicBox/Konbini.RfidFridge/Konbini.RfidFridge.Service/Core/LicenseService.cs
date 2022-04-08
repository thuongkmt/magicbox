using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slack.Webhooks;
using Konbini.RfidFridge.Service.Helper;
using Microsoft.Extensions.Options;
using Konbini.License;

namespace Konbini.RfidFridge.Service.Core
{
    public class LicenseService
    {
        private readonly LogService _logger;

        public LicenseService(LogService logger)
        {
            _logger = logger;
        }

        public bool IsValid()
        {
            var result = false;

            KeyManager km = new KeyManager(ComputerInfo.GetComputerId());
            LicenseInfo lic = new LicenseInfo();
            _logger.LogInfo($"Checking license | ID: {ComputerInfo.GetComputerId()}");
            //Get license information from license file
            int value = km.LoadSuretyFile(string.Format(@"{0}\License.lic", AppDomain.CurrentDomain.BaseDirectory), ref lic);
            string productKey = lic.ProductKey;

            //Check valid
            if (km.ValidKey(ref productKey))
            {
                KeyValuesClass kv = new KeyValuesClass();
                if (km.DisassembleKey(productKey, ref kv))
                {
                    var licenseType = string.Empty;
                    if (kv.Type == LicenseType.TRIAL)
                    {
                        var days = (kv.Expiration - DateTime.Now.Date).Days;
                        licenseType = string.Format("{0} days", days);
                        result = days > 0 ? true : false;
                    }
                    else
                    {
                        licenseType = "Full";
                        result = true;
                    }
                    _logger.LogInfo($"License type: {licenseType}");
                }
            }
            else
            {
                _logger.LogInfo("Invalid license");
                result = false;
            }

            return result;
        }

        public void Registration(string productKey)
        {
            string computerId = ComputerInfo.GetComputerId();
            KeyManager km = new KeyManager(computerId);
            if (km.ValidKey(ref productKey))
            {
                KeyValuesClass kv = new KeyValuesClass();
                //Decrypt license key
                if (km.DisassembleKey(productKey, ref kv))
                {
                    LicenseInfo lic = new LicenseInfo();
                    lic.ProductKey = productKey;
                    lic.FullName = "Konbini";
                    if (kv.Type == LicenseType.TRIAL)
                    {
                        lic.Day = kv.Expiration.Day;
                        lic.Month = kv.Expiration.Month;
                        lic.Year = kv.Expiration.Year;
                    }
                    //Save license key to file
                    km.SaveSuretyFile(string.Format(@"{0}\License.lic", AppDomain.CurrentDomain.BaseDirectory), lic);
                    _logger.LogInfo("You have been successfully registered.");
                }
            }
            else
            {
                _logger.LogInfo("Your product key is invalid.");
            }
        }
    }
}
