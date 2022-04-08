using Konbini.RfidFridge.Domain;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.DTO.GrabPay;
using Konbini.RfidFridge.Domain.DTO.Tera;
using Konbini.RfidFridge.Domain.Entities;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Core;
using Konbini.RfidFridge.Service.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Data
{
    public class TransactionService : ITransactionService
    {
        private WebApiService WebApiService;
        private LogService LogService;

        public TransactionService(WebApiService webApiService, LogService logService)
        {
            this.WebApiService = webApiService;
            this.LogService = logService;
        }
        public void Add(List<InventoryDto> items, TransactionStatus status, object saleResponse, PaymentType paymentType, CardPaymentType type, List<string> transactionImages)
        {
            this.LogService.LogInfo("ADD TRANSACTION");
            var tryTime = 0;

            try
            {
            Retry:
                tryTime++;
                var token = WebApiService.GetToken();

                this.LogService.LogMachineAdminApi("Add transaction | Status = " + status);

                var client = new HttpClient
                {
                    BaseAddress = new Uri(WebApiService.GetUrl())
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var paymentDetail = string.Empty;
                var cashlessDetail = new CashlessDetailDto();
                if (paymentType == PaymentType.MAGIC)
                {
                    if (status == TransactionStatus.Success)
                    {
                        if (saleResponse != null)
                        {
                            var iucResponse = (IucApprovedResponse)saleResponse;
                            cashlessDetail.Aid = iucResponse.Aid;
                            cashlessDetail.Amount = iucResponse.Amount;
                            cashlessDetail.AppLabel = iucResponse.AppLabel;
                            cashlessDetail.ApproveCode = iucResponse.ApproveCode;
                            cashlessDetail.Batch = iucResponse.Batch;
                            cashlessDetail.CardLabel = iucResponse.CardLabel;
                            cashlessDetail.CardNumber = iucResponse.CardNumber;
                            cashlessDetail.EntryMode = iucResponse.EntryMode;
                            cashlessDetail.Invoice = iucResponse.Invoice;
                            cashlessDetail.Mid = iucResponse.Mid;
                            cashlessDetail.Rrn = iucResponse.Rrn;
                            cashlessDetail.Tc = iucResponse.Tc;
                            cashlessDetail.Tid = iucResponse.Tid;
                        }
                    }
                    if (status == TransactionStatus.Error)
                    {
                        if (saleResponse != null)
                        {
                            var iucResponse = (SaleResponse)saleResponse;
                            paymentDetail = iucResponse.ToString();
                        }
                    }
                }

                if (paymentType == PaymentType.NAYAX)
                {
                   // cashlessDetail.Amount = (decimal)items.Sum(x => x.Price);
                }

                if (paymentType == PaymentType.QR)
                {
                    if (type == CardPaymentType.GRABPAY)
                    {
                        if (status == TransactionStatus.Error)
                        {
                            if (saleResponse != null)
                            {
                                paymentDetail = ((GrabPayMbChargeResponse)saleResponse).Message;
                            }
                        }
                        else
                        {
                            var r = ((GrabPayMbChargeResponse)saleResponse);
                            cashlessDetail.Amount = (decimal)items.Sum(x => x.Price);
                            cashlessDetail.CardLabel = type.ToString();
                            cashlessDetail.CardNumber = $"TXN:{r.TransactionId}, RFD:{r.RefundId}";
                        }
                    }

                    if (type == CardPaymentType.WALLET)
                    {
                        if (status == TransactionStatus.Error)
                        {
                            if (saleResponse != null)
                            {
                               // paymentDetail = ((GrabPayMbChargeResponse)saleResponse).Message;
                            }
                        }
                        else
                        {
                            //var r = ((TereWalletMbChargeResponse)saleResponse);
                            //cashlessDetail.Amount = (decimal)items.Sum(x => x.Price);
                            //cashlessDetail.CardLabel = type.ToString();
                            //cashlessDetail.CardNumber = $"User ID:{r.UserId}, Status:{r.Message}";
                        }

                        var r = ((TereWalletMbChargeResponse)saleResponse);
                        cashlessDetail.Amount = (decimal)items.Sum(x => x.Price);
                        cashlessDetail.CardLabel = type.ToString();
                        cashlessDetail.CardNumber = $"User ID:{r.UserId}, Status:{r.Message}";
                    }

                    if (type == CardPaymentType.CREDITCARD_WALLET)
                    {
                        if (status == TransactionStatus.Error)
                        {
                            if (saleResponse != null)
                            {
                                // paymentDetail = ((GrabPayMbChargeResponse)saleResponse).Message;
                            }
                        }
                        else
                        {
                            //var r = ((TereWalletMbChargeResponse)saleResponse);
                            //cashlessDetail.Amount = (decimal)items.Sum(x => x.Price);
                            //cashlessDetail.CardLabel = type.ToString();
                            //cashlessDetail.CardNumber = $"User ID:{r.UserId}, Status:{r.Message}";
                        }

                        var r = ((CreditCardWalletResponse)saleResponse);
                        cashlessDetail.Amount = (decimal)items.Sum(x => x.Price);
                        cashlessDetail.CardLabel = "OMISE";
                        cashlessDetail.CardNumber = $"Transaction ID:{r.TransactionId}";
                    }

                }

                var transaction = new TransactionDto
                {
                    Inventories = items,
                    CashlessDetail = cashlessDetail,
                    PaymentDetail = paymentDetail,
                    Status = status
                };


                var tDto = JsonConvert.SerializeObject(transaction);
                this.LogService.LogMachineAdminApi("Add transaction | DTO = " + tDto);

                var response = client.PostAsJsonAsync($"api/services/app/Transaction/AddTransaction", transaction).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsAsync<dynamic>().Result;
                    LogService.LogMachineAdminApi($"Result: {json}");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (tryTime <= 3)
                    {
                        Thread.Sleep(1000);
                        this.WebApiService.RenewToken();
                        goto Retry;
                    }
                    else
                    {
                        this.LogService.LogMachineAdminApi("Failed to get Token!");
                    }
                }
                else
                {
                    LogService.LogMachineAdminApi($"ERROR: {response}");
                }
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex);
                this.LogService.LogMachineAdminApi(ex.ToString());
            }
        }
    }
}
