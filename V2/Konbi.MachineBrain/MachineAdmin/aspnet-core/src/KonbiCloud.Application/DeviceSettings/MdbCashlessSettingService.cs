using System;
using System.Collections.Generic;
using System.Text;
using Abp.Authorization;
using KonbiCloud.MultiTenancy.Payments;
using KonbiCloud.Payments;

namespace KonbiCloud.DeviceSettings
{
    public class MdbCashlessSettingService: KonbiCloudAppServiceBase,IMdbCashlessSettingService
    {
        private readonly IPaymentDeviceService paymentService;

        public MdbCashlessSettingService(IPaymentDeviceService paymentService)
        {
            this.paymentService = paymentService;
        }

        [AbpAllowAnonymous]
        public void EnableSale001()
        {
            var code="532C8EF6-B7AA-4546-A124-47CC24BED863";
            paymentService.EnablePayments();
        }
    }
}
