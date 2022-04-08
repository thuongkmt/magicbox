using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using KonbiCloud.MultiTenancy.Accounting.Dto;

namespace KonbiCloud.MultiTenancy.Accounting
{
    public interface IInvoiceAppService
    {
        Task<InvoiceDto> GetInvoiceInfo(EntityDto<long> input);

        Task CreateInvoice(CreateInvoiceDto input);
    }
}
