using KonbiCloud.Products;
using KonbiCloud.Transactions;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using KonbiCloud.Transactions.Dtos;
using KonbiCloud.Dto;
using Abp.Application.Services.Dto;
using KonbiCloud.Authorization;
using Abp.Extensions;
using Abp.Authorization;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Transactions
{
    [AbpAuthorize(AppPermissions.Pages_TransactionDetails)]
    public class TransactionDetailsAppService : KonbiCloudAppServiceBase, ITransactionDetailsAppService
    {
        private readonly IRepository<TransactionDetail, Guid> _transactionDetailRepository;
        private readonly IRepository<DetailTransaction, long> _lookup_detailTransactionRepository;
        private readonly IRepository<Product, Guid> _lookup_productRepository;

        public TransactionDetailsAppService(IRepository<TransactionDetail, Guid> transactionDetailRepository, 
            IRepository<DetailTransaction, long> lookup_detailTransactionRepository, 
            IRepository<Product, Guid> lookup_productRepository)
        {
            _transactionDetailRepository = transactionDetailRepository;
            _lookup_detailTransactionRepository = lookup_detailTransactionRepository;
            _lookup_productRepository = lookup_productRepository;

        }

        public async Task<PagedResultDto<TransactionDetailDto>> GetTransactionDetailForView(long TransactionId)
        {
            var transDetailList = _transactionDetailRepository.GetAll()
                    .Include(e => e.Product)
                    .Where(s => s.TransactionId == TransactionId).ToList();

            int totalCount = transDetailList.Count();

            var list = new List<TransactionDetailDto>();


            foreach (var x in transDetailList)
            {
                var newTran = new TransactionDetailDto()
                {

                    TagId = x.TagId,
                    Price = x.Price,
                    TopupId = x.TopupId,
                    LocalInventoryId = x.LocalInventoryId,
                    ProductName = x.Product.Name
                };

                list.Add(newTran);
            }

            return new PagedResultDto<TransactionDetailDto>(totalCount, ObjectMapper.Map<List<TransactionDetailDto>>(list));

        }
    }
}