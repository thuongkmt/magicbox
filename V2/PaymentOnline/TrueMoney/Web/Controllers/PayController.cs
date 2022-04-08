using Common.TruePayment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LinePayCSharpWeb.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private IConfiguration configuration { get; set; }
        private string ApiUrl { get; set; }
        private HttpClient client;
        static private JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public PaymentController()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

            configuration = builder.Build();

            ApiUrl = configuration["LocalMachineUrl"];
            client = new HttpClient();
        }

        [HttpGet]
        [Route("login")]
        public async Task<IActionResult> Login(string phone)
        {
            try
            {
                if (string.IsNullOrEmpty(phone))
                    throw new ArgumentNullException("Phone is empty");

                var uri = $"{ApiUrl}/api/services/app/TruePaymentService/AuthenticateUser";
                var request = new HttpRequestMessage(HttpMethod.Post, uri);

                var acc = new AccountRequest
                {
                    MobileNumber = phone,
                    TmnAccount = phone
                };
                request.Content = new StringContent(JsonConvert.SerializeObject(acc, serializerSettings), Encoding.UTF8, "application/json");

                //HttpContext.Session.Set("AgreementLink", Encoding.ASCII.GetBytes("https://ext.truemoney.com/m/info/register/term_of_service.html?online_merchant"));

                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                var otpResponse = JsonConvert.DeserializeObject<OtpResponse>(content);

                HttpContext.Session.SetString("OtpResponse", JsonConvert.SerializeObject(otpResponse));

                if (otpResponse.LinkAgreement == null)
                {
                    //return Json(new { message = "Cannot veriry your account" });
                    return Json(new { agreementLink = "https://ext.truemoney.com/m/info/register/term_of_service.html?online_merchant" });
                }
                else
                {
                    return Json(new { agreementLink = otpResponse.LinkAgreement});
                }
            }
            catch
            {
                return Json(new { message = "Cannot veriry your account" });
            }
            
        }

        [HttpGet]
        [Route("submitotp")]
        public async Task<IActionResult> SubmitOtp(string otp)
        {
            try
            {
                if (string.IsNullOrEmpty(otp))
                    throw new ArgumentNullException("Otp is empty");

                var uri = $"{ApiUrl}/api/services/app/TruePaymentService/SubmitOtp";
                var request = new HttpRequestMessage(HttpMethod.Post, uri);

                var otpResponse = JsonConvert.DeserializeObject<OtpResponse>(HttpContext.Session.GetString("OtpResponse"));
                var acc = new TokenRequest
                {
                    OtpRef = otpResponse?.OtpRef,
                    OtpCode = otp,
                    AuthCode = otpResponse?.AuthCode
                };
                request.Content = new StringContent(JsonConvert.SerializeObject(acc, serializerSettings), Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                var tmResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                if (tmResponse.AccessToken == null)
                {
                    return NotFound("Error! Cannot verify OTP");
                }
                else if(tmResponse.Status.Message != null)
                {
                    return NotFound(tmResponse.Status.Message);
                }

                return Ok("Submit OTP code successfull!");
            }
            catch (Exception ex)
            {
                return NotFound("Error! Cannot verify OTP");
                //throw new Exception("Error");
            }

        }

    }
}
