using Abp.Authorization;
using Abp.Timing;
using Common.TruePayment;
using KonbiCloud.Configuration;
using KonbiCloud.Sessions;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Payments
{
    [AbpAllowAnonymous]
    public class TruePaymentService : KonbiCloudAppServiceBase, ITruePaymentService
    {
        public string ClientSecret { get; set; }
        public string ClientId { get; set; }
        private string ApiKey { get; set; }
        private string ApiUrl { get; set; }
        private string PaymentUrl { get; set; }
        private string MerchantId { get; set; }
        private const string ContentSignaturePreFix = "digest-alg=RSA-SHA; key-id=KEY:RSA:rsf.org; data=";
        private string TmnPublicKey { get; set; }
        private const string ContentSignature = "Content-Signature";
        private const string TimeStamp = "TimeStamp";
        private string KbnPrivateKeyPath { get; set; }
        private string DepositAmount { get; set; }

        public CustomSession CustomSession { get; set; }

        public TruePaymentService(IHostingEnvironment env)
        {
            var appConfiguration = env.GetAppConfiguration();
            ApiUrl = appConfiguration["Abp:TrueMoney:ApiUrl"];
            ClientId = appConfiguration["Abp:TrueMoney:ClientId"];
            ClientSecret = appConfiguration["Abp:TrueMoney:ClientSecret"];
            KbnPrivateKeyPath = appConfiguration["Abp:TrueMoney:KbnPrivateKeyPath"];
            TmnPublicKey = appConfiguration["Abp:TrueMoney:TmnPublicKey"];
            PaymentUrl = appConfiguration["Abp:TrueMoney:PaymentUrl"];
            DepositAmount = appConfiguration["Abp:TrueMoney:DepositAmount"];
        }

        public async Task<OtpResponse> AuthenticateUser(AccountRequest input)
        {
            //Reset Session data
            CustomSession.TrueAccessToken = "";
            CustomSession.TrueRefreshToken = "";
            CustomSession.TrueTokenExpire = 0;

            input.ClientId = ClientId;
            input.ClientSecret = ClientSecret;
            var tmResponse = new OtpResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    var timeout = 15;

                    client.Timeout = TimeSpan.FromSeconds(timeout);

                    var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(input, Formatting.None));
                    Logger.Info($"True Money add AuthenticateUser data: {stringPayload}");

                    var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                    var url = $"{ApiUrl}oauth2/auth-apply";
                    var httpResponse = await client.PostAsync(url, httpContent);

                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    tmResponse = JsonConvert.DeserializeObject<OtpResponse>(responseContent);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        CustomSession.UserMobile = input.MobileNumber;
                        Logger.Info($"True Money AuthenticateUser API response\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }
                    else
                    {
                        Logger.Info($"True Money: can not verify AuthenticateUser response\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }

                    return tmResponse;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error when call True Money AuthenticateUser API\r\n {ex.Message}\r\n {ex.StackTrace}");
                return tmResponse;
            }
        }

        public async Task<TokenResponse> SubmitOtp(TokenRequest input)
        {
            input.ClientId = ClientId;
            var tmResponse = new TokenResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    var timeout = 15;

                    client.Timeout = TimeSpan.FromSeconds(timeout);

                    var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(input, Formatting.None));
                    Logger.Info($"True Money add SubmitOtp data: {stringPayload}");

                    var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                    var url = $"{ApiUrl}oauth2/tokens";
                    var httpResponse = await client.PostAsync(url, httpContent);

                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    tmResponse = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        Logger.Info($"True Money SubmitOtp API response\r\n{JsonConvert.SerializeObject(tmResponse)}");

                        //Start deposit monney from customer account
                        //1. Get payment code
                        var payment = await GetPaymentCode();
                        if(payment.PaymentCode == null)
                        {
                            tmResponse.Status.Message = "Cannot get payment code";
                            return tmResponse;
                        }
                        //2. Add deposit payment
                        var paymetnRequest = new PaymentRequest
                        {
                            Isv_payment_ref = GetPaymentRef(),
                            Merchant_id = MerchantId,
                            Request_amount = DepositAmount,
                            Payment_code = payment.PaymentCode,
                            Description = "Test payment",
                            Data = new PaymentRequest.Metadata
                            {
                                ShopId = "ShopID 001",
                                UserMobile = CustomSession.UserMobile
                            }
                        };
                        var paymentResult = await Payment(paymetnRequest);
                        if(paymentResult.PaymentId == null)
                        {
                            tmResponse.Status.Message = "Cannot deposit money";
                            return tmResponse;
                        }
                        CustomSession.TrueDepositPaymentId = paymentResult.PaymentId;
                    }
                    else
                    {
                        Logger.Info($"True Money: can not verify SubmitOtp response\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }

                    return tmResponse;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error when call True Money SubmitOtp API\r\n {ex.Message}\r\n {ex.StackTrace}");
                return tmResponse;
            }
        }

        public async Task<PaymentCodeResponse> GetPaymentCode()
        {
            var tmResponse = new PaymentCodeResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    var timeout = 15;
                    client.Timeout = TimeSpan.FromSeconds(timeout);

                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {CustomSession.TrueAccessToken}");

                    var url = $"{ApiUrl}payment/codes?type=online-merchant";

                    var httpResponse = await client.GetAsync(ApiUrl);

                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    tmResponse = JsonConvert.DeserializeObject<PaymentCodeResponse>(responseContent);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        Logger.Info($"True Money GetPaymentCode API response\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }
                    else
                    {
                        Logger.Info($"True Money: can not verify GetPaymentCode response\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }

                    return tmResponse;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error when call True Money GetPaymentCode API\r\n {ex.Message}\r\n {ex.StackTrace}");
                return tmResponse;
            }
        }

        public async Task<PaymentResponse> Payment(PaymentRequest input)
        {
            var tmResponse = new PaymentResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);

                    AddParamsToHeader(client, input.Isv_payment_ref);
                    var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(input, Formatting.None));
                    Logger.Info($"True Money add Payment data: {stringPayload}");
                    var signedContent = SignWithSystemPrivateKey(input.Isv_payment_ref + stringPayload);
                    Logger.Info($"True Money add Payment - signed content: {signedContent}");
                    client.DefaultRequestHeaders.Add(ContentSignature, ContentSignaturePreFix + signedContent);

                    var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                    var url = $"{PaymentUrl}payments";
                    var httpResponse = await client.PostAsync(url, httpContent);

                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    var isValid = VerifyResponse(httpResponse, responseContent);
                    if (isValid)
                    {
                        tmResponse = JsonConvert.DeserializeObject<PaymentResponse>(responseContent);
                        Logger.Info($"True Money Payment API response\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }
                    else
                    {
                        Logger.Info($"True Money: can not verify Payment response\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }

                    return tmResponse;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error when call True Money Payment API\r\n {ex.Message}\r\n {ex.StackTrace}");
                return tmResponse;
            }
        }

        public async Task<RefundResponse> Refund(RefundRequest input)
        {
            var tmResponse = new RefundResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    var timeStamp = GetEpochTime();
                    AddParamsToHeader(client, timeStamp);

                    client.Timeout = TimeSpan.FromSeconds(15);

                    //input = new RefundRequest
                    //{
                    //    Isv_refund_ref = "C" + GetPaymentRef(), // add C prefix to refund reference number.
                    //    Request_amount = amount.ToString()
                    //};
                    var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(input, Formatting.None));
                    Logger.Info($"True Money refund body: {stringPayload}");
                    var signedContent = SignWithSystemPrivateKey(timeStamp + stringPayload);
                    client.DefaultRequestHeaders.Add("content-signature", ContentSignaturePreFix + signedContent);
                    Logger.Info($"True Money refund - signed content: {signedContent}");
                    var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                    var httpResponse = await client.PostAsync($"{ApiUrl}/payments/{CustomSession.TrueDepositPaymentId}/refunds", httpContent);

                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var isValid = VerifyResponse(httpResponse, responseContent);
                    if (isValid)
                    {
                        tmResponse = JsonConvert.DeserializeObject<RefundResponse>(responseContent);
                        Logger.Info($"True Money Refund API\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }
                    else
                    {
                        Logger.Info($"True Money: can not verify RefundByPaymentId response\r\n{JsonConvert.SerializeObject(tmResponse)}");
                    }

                    return tmResponse;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"True Money Error when call True Money Refund API\r\n {ex.Message}\r\n {ex.StackTrace}");
                return tmResponse;
            }
        }

        private string GetPaymentRef()
        {
            var dateNow = Clock.Now;
            dateNow = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute, dateNow.Second, DateTimeKind.Utc);
            return $"01{dateNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.ToString()}";
        }

        private string SignWithSystemPrivateKey(string data)
        {
            Logger.Info("SignWithSystemPrivateKey");
            var privateKey = GetAsymmetricPrivateKey();
            var rsaKeyParameters = new RsaKeyParameters(privateKey.IsPrivate, ((RsaPrivateCrtKeyParameters)privateKey).Modulus, ((RsaPrivateCrtKeyParameters)privateKey).Exponent);
            var sig = SignerUtilities.GetSigner("SHA256WITHRSA");
            sig.Init(true, rsaKeyParameters);
            var bytes = Encoding.UTF8.GetBytes(data);
            sig.BlockUpdate(bytes, 0, bytes.Length);
            var signature = sig.GenerateSignature();
            return Convert.ToBase64String(signature);
        }

        private AsymmetricKeyParameter GetAsymmetricPrivateKey()
        {
            Logger.Info("GetAsymmetricPrivateKey");
            AsymmetricCipherKeyPair keyPair;
            using (var reader = File.OpenText(KbnPrivateKeyPath))
            {
                keyPair = (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
            }
            return keyPair.Private;
        }

        private bool VerifyResponse(HttpResponseMessage responseMessage, string responseContent)
        {
            Logger.Info("VerifyResponse");
            RsaKeyParameters key = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(TmnPublicKey));
            RSAParameters param = new RSAParameters
            {
                Modulus = key.Modulus.ToByteArrayUnsigned(),
                Exponent = key.Exponent.ToByteArrayUnsigned()
            };

            string timeStamp = string.Empty;
            string contentSignature = string.Empty;
            IEnumerable<string> values;
            if (responseMessage.Headers.TryGetValues(TimeStamp, out values))
            {
                timeStamp = values.First();
            }

            if (responseMessage.Headers.TryGetValues(ContentSignature, out values))
            {
                contentSignature = values.First().Replace(ContentSignaturePreFix, "");
            }

            var buffer = Encoding.UTF8.GetBytes(timeStamp + responseContent);

            using (var csp = new RSACryptoServiceProvider())
            {
                csp.ImportParameters(param);
                return csp.VerifyData(buffer, CryptoConfig.MapNameToOID("SHA256"), Convert.FromBase64String(contentSignature));
            }
        }

        private void AddParamsToHeader(HttpClient client, string timeStamp)
        {
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
            client.DefaultRequestHeaders.Add("timestamp", timeStamp);
            Logger.Info($"True Money timestamp: {timeStamp}");
        }

        private string GetEpochTime()
        {
            var dateNow = Clock.Now;
            dateNow = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute, dateNow.Second, DateTimeKind.Utc);
            return dateNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.ToString();
        }
    }
}
