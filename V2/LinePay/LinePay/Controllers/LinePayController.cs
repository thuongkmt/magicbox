using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LinePayCSharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LinePay.Controllers
{
    [Route("api/[controller]")]
    public class LinePayController : Controller
    {
        private LinePayClient client;
        private IConfiguration configuration { get; set; }
        
        public LinePayController()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            configuration = builder.Build();

            client = new LinePayClient(
                configuration["LinePay:ChannelId"],
                configuration["LinePay:ChannelSecret"],
                bool.Parse(configuration["LinePay:IsSandbox"]));
        }

        [HttpGet]
        [Route("reserve")]
        public async Task<IActionResult> Reserve()
        {
            var reserve = new Reserve()
            {
                //ProductImageUrl = "http://konbinisg.com/image/konbini/logo.png",
                ProductName = $"{configuration["LinePay:MessageDeposit"]}",
                Amount = int.Parse(configuration["LinePay:AmountDeposit"]),
                Currency = Currency.THB,
                OrderId = Guid.NewGuid().ToString(),
                ConfirmUrl = $"{configuration["LinePay:LocalUri"]}/api/linepay/confirm",
                CancelUrl = $"{configuration["LinePay:LocalUri"]}/api/linepay/cancel",
                Capture = false,
                PayType = PayType.PREAPPROVED
            };

            var response = await client.ReserveAsync(reserve);
            
            CacheService.Cache.Add(response.Info.TransactionId, reserve);

            return Redirect(response.Info.PaymentUrl.Web);
        }

        [HttpGet]
        [Route("confirm")]
        public async Task<IActionResult> Confirm()
        {
            try
            {
                var transactionId = Int64.Parse(HttpContext.Request.Query["transactionId"]);
                var reserve = CacheService.Cache[transactionId] as Reserve;

                var confirm = new Confirm()
                {
                    Amount = reserve.Amount,
                    Currency = reserve.Currency
                };

                var response = await client.ConfirmAsync(transactionId, confirm);

                // Check return Succcess.
                if (response.ReturnCode == "0000")
                {
                    var regkey = response.Info.RegKey;

                    // Open Lock. => Using rabbitmq send to device.
                    HttpClient _client = new HttpClient();
                    string _url = $"{configuration["LinePay:ServerUri"]}/api/services/app/LinePay/GetConfirmLinePay?machineId={configuration["LinePay:MachineId"]}&transactionId={transactionId}&regkey={regkey}";
                    var _response = await _client.GetAsync(_url);
                    System.IO.File.WriteAllText(@"C:\Test.txt", "response: " + _response.StatusCode);
                    if (_response.IsSuccessStatusCode != true)
                    {
                        return Redirect("http://localhost:5000/error-linepay");
                    }

                    return Redirect("http://localhost:5000/waiting?transactionId=" + transactionId + "&regkey=" + regkey);
                }
                else
                {
                    return Redirect("http://localhost:5000/error-linepay");
                }
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(@"C:\Test.txt", "Error: " + ex.Message);
                return null;
            }
        }

        [HttpGet]
        [Route("waiting")]
        public async Task<Waiting> WaitingAsync(Int64 transactionId)
        {
            Waiting result = new Waiting();
            while (true)
            {
                var reserve_ = CacheService.Cache[transactionId] as Reserve;
                if ((bool)reserve_.Capture)
                {
                    result.Capture = true;
                    return await Task.FromResult<Waiting>(result);
                }
            }
        }

        [HttpGet]
        [Route("success")]
        public async Task<LinePayFinishDto> SuccessAsync(Int64 transactionId)
        {
            LinePayFinishDto result = new LinePayFinishDto();
            var reserve = CacheService.Cache[transactionId] as Reserve;
            result.amount = reserve.Amount;
            result.productName = reserve.ProductName;
            return await Task.FromResult<LinePayFinishDto>(result);
        }
    }

    public class Waiting
    {
        public bool Capture { get; set; }
    }
}
