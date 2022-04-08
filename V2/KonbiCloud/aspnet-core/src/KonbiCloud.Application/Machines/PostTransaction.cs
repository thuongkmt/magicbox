using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Machines
{
    public class PostTransaction
    {
        private static HttpClient httpClient = new HttpClient();
        public async void TestPostTransaction()
        {
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(new
            {
                transactionID = 0,
                machineId = "string",
                machineLogicalId = "sgsgsf",
                paymentTime = "2018-08-13T11:52:33.743Z",
                totalValue=100,
                sumCollected=100,
                changeIssued = 10,
                productSKU="23131",
                paymentMethod="awdasd",
                productName= "productName"
            }));

            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var httpResponse = await httpClient.PostAsync(@"https://cpvendingbackend.azurewebsites.net/api/services/app/Product/TestAddPostOutSide", httpContent);

            // If the response contains content we want to read it!
            if (httpResponse.Content != null && httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                //var result = JsonConvert.DeserializeObject<RestApiResult>(responseContent);
                //if (result.success && result.result)
                //{
                //    machineError.IsSynced = true;
                //    ctx.MachineErrors.Attach(machineError);
                //    ctx.Entry(machineError).State = EntityState.Modified;
                //    await ctx.SaveChangesAsync();
                //}
            }
        }
    }
}
