using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Configuration;
using KonbiCloud.CustomerKonbiWallet.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WooCommerceNET.WooCommerce.v2;

namespace KonbiCloud.CustomerKonbiWallet
{

    public class CustomerKonbiWalletService : KonbiCloudAppServiceBase, ICustomerKonbiWalletService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger _logger;

        private string _wooCommerceUrl;
        private string _wooConsumerKey;
        private string _wooConsumerSecret;

        public CustomerKonbiWalletService(
            ILogger logger,
            ISettingManager settingManager)
        {
            _logger = logger;
            _wooCommerceUrl = settingManager.GetSettingValue(AppSettingNames.WooUrl) + "/wp-json/wc/v2/";
            _wooConsumerKey = settingManager.GetSettingValue(AppSettingNames.WooConsumerKey);
            _wooConsumerSecret = settingManager.GetSettingValue(AppSettingNames.WooConsumerSecret);
        }

        //[RequiresFeature("CustomerKonbiWalletFeature")]
        public async Task<PagedResultDto<CustomerWallet>> GetAll(GetAllCustomersInput input)
        {
            PagedResultDto<CustomerWallet> result = new PagedResultDto<CustomerWallet>();
            try
            {
                var page = input.SkipCount == 0 ? 1 : input.SkipCount / input.MaxResultCount + 1;
                var per_page = input.MaxResultCount;
                // PerPage Woo REST API not support > 100.
                //var per_page = input.MaxResultCount > 100 ? 100 : input.MaxResultCount;
                _wooCommerceUrl += "customers?consumer_key=" + _wooConsumerKey + "&consumer_secret=" + _wooConsumerSecret + "&page=" + page + "&per_page=" + per_page;
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), _wooCommerceUrl))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "*");
                    var response = await _httpClient.SendAsync(request);

                    var Total = int.Parse(response.Headers.GetValues("X-WP-Total").ToList()[0]);
                    var TotalPages = response.Headers.GetValues("X-WP-TotalPages").ToList()[0];

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = "";
                        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                        {
                            content = sr.ReadToEnd();
                        }
                        List<CustomerWallet> customers = JsonConvert.DeserializeObject<List<CustomerWallet>>(content);

                        result.TotalCount = Total;
                        //result.TotalCount = customers.Count;

                        // Filter Customer.
                        var filteredList = customers
                            .WhereIf(!String.IsNullOrEmpty(input.Filter), 
                            customer => (customer.username != null && customer.username.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                            || (customer.email != null && customer.email.Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase))
                            || ((customer.first_name + " " + customer.last_name) != null && (customer.first_name + " " + customer.last_name).Contains(input.Filter.Trim(), StringComparison.OrdinalIgnoreCase)))
                            .WhereIf(!String.IsNullOrEmpty(input.UserNameFilter), customer => customer.username != null && customer.username.ToLower() == input.UserNameFilter.ToLower().Trim())
                            .WhereIf(!String.IsNullOrEmpty(input.EmailFilter), customer => customer.email != null && customer.email.ToLower() == input.EmailFilter.ToLower().Trim())
                            .WhereIf(!String.IsNullOrEmpty(input.CustomerFilter), customer => (customer.first_name + " " + customer.last_name) != null && ((customer.first_name + " " + customer.last_name).ToLower()).Contains(input.CustomerFilter.ToLower().Trim()));

                        // Order and Page.
                        List<CustomerWallet> customersPage = filteredList.ToList();
                        //List<CustomerWallet> customersPage = filteredList
                        //    .OrderBy(customer => customer.username).ToList();
                        //.Skip(input.SkipCount)
                        //.Take(input.MaxResultCount).ToList();                                               

                        foreach (var item in customersPage)
                        {
                            var findItem = item.meta_data.Find(x => x.key == "wc_last_active");
                            var lastActive = findItem != null ? item.meta_data.Find(x => x.key == "wc_last_active").value : null;
                            if (lastActive != null)
                            {
                                DateTime dateTimeTemp = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                                dateTimeTemp = dateTimeTemp.AddSeconds(long.Parse(lastActive.ToString()));
                                item.last_active = dateTimeTemp;
                            }
                        }

                        result.Items = SortCustomer(customersPage, input.Sorting);
                    }
                    else
                    {
                        _logger.Error("Error Get Customer From KonbiWallet => Response.StatusCode: " + response.StatusCode);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Customer From KonbiWallet: ", ex);
                return result;
            }
        }

        private List<CustomerWallet> SortCustomer(List<CustomerWallet> customers, string orderby)
        {
            string fieldName = orderby.Split()[0];
            string type = orderby.Split()[1];
            switch (fieldName)
            {
                case "customer":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.customer).ToList();
                    else
                        return customers.OrderByDescending(x => x.customer).ToList();

                case "username":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.username).ToList();
                    else
                        return customers.OrderByDescending(x => x.username).ToList();

                case "email":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.email).ToList();
                    else
                        return customers.OrderByDescending(x => x.email).ToList();

                case "orders_count":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.orders_count).ToList();
                    else
                        return customers.OrderByDescending(x => x.orders_count).ToList();

                case "total_spent":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.total_spent).ToList();
                    else
                        return customers.OrderByDescending(x => x.total_spent).ToList();

                case "total_topup":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.total_topup).ToList();
                    else
                        return customers.OrderByDescending(x => x.total_topup).ToList();

                case "wallet_balance":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.wallet_balance).ToList();
                    else
                        return customers.OrderByDescending(x => x.wallet_balance).ToList();

                case "sign_up":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.sign_up).ToList();
                    else
                        return customers.OrderByDescending(x => x.sign_up).ToList();

                case "last_active":
                    if (type.ToLower() == "asc")
                        return customers.OrderBy(x => x.last_active).ToList();
                    else
                        return customers.OrderByDescending(x => x.last_active).ToList();

                default:
                    return customers.OrderBy(x => x.username).ToList();
            }
        }

        public async Task<PagedResultDto<WalletTransaction>> GetOrdersByCustomer(GetOrdersByCustomerInput input)
        {
            PagedResultDto<WalletTransaction> result = new PagedResultDto<WalletTransaction>();

            try
            {
                var page = input.SkipCount == 0 ? 1 : input.MaxResultCount / input.SkipCount;
                // PerPage Woo REST API not support > 100.
                var per_page = input.MaxResultCount > 100 ? 100 : input.MaxResultCount;
                _wooCommerceUrl += "wallet_transactions/" + input.CustomerId;
                _wooCommerceUrl = _wooCommerceUrl.Replace("/wc/", "/wp/");
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), _wooCommerceUrl))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "*");
                    var response = await _httpClient.SendAsync(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = "";
                        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                        {
                            content = sr.ReadToEnd();
                        }
                        List<WalletTransaction> orders = JsonConvert.DeserializeObject<List<WalletTransaction>>(content);

                        result.TotalCount = orders.Count;

                        // Order and Page.
                        List<WalletTransaction> ordersPage = orders
                            .OrderByDescending(order => order.transaction_id)
                            .Skip(input.SkipCount)
                            .Take(input.MaxResultCount).ToList();

                        result.Items = ordersPage;
                    }
                    else
                    {
                        _logger.Error("Error Get Orders By Customer From KonbiWallet => Response.StatusCode: " + response.StatusCode);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Error Get Orders By Customer From KonbiWallet: ", ex);
                return result;
            }
        }
    }
}
