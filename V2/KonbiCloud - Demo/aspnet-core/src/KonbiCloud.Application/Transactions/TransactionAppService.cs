using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.CloudSync;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Enums;
using KonbiCloud.Machines;
using KonbiCloud.Plate;
using KonbiCloud.Transactions.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KonbiCloud.Transactions
{
    [AbpAuthorize(AppPermissions.Pages_Transactions)]
    public class TransactionAppService : KonbiCloudAppServiceBase, ITransactionAppService
    {
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IRepository<Employees.Employee, Guid> _employeeRepository;
        private readonly IRepository<Disc, Guid> _discRepository;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService detailLogService;
        private const string defaultImage = "assets/common/images/ic_nophoto.jpg";
        private const string ServerRootAddress = "App:ServerRootAddress";

        public TransactionAppService(IRepository<DetailTransaction, long> transactionRepository,
                                     IRepository<Machine, Guid> machineRepository,
                                     IRepository<Session, Guid> sessionRepository,
                                     IRepository<Employees.Employee, Guid> employeeRepository,
                                     IRepository<Disc, Guid> discRepository,
                                     IHostingEnvironment env,
                                     IDetailLogService detailLog)
        {
            _transactionRepository = transactionRepository;
            _machineRepository = machineRepository;
            _sessionRepository = sessionRepository;
            _employeeRepository = employeeRepository;
            _discRepository = discRepository;
            _appConfiguration = env.GetAppConfiguration();
            this.detailLogService = detailLog;
        }

        [AbpAuthorize(AppPermissions.Pages_Transactions)]
        //[AbpAllowAnonymous]
        public async Task<PagedResultDto<TransactionDto>> GetAllTransactions(TransactionInput input)
        {
            detailLogService.Log("GetAllTransactions");
            try
            {
                var transactions = _transactionRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SessionFilter), e => e.SessionId.ToString().Equals(input.SessionFilter))
                    .WhereIf(input.DateFilter != null, e => e.PaymentTime.Date == input.DateFilter.Value.Date);

                if (input.TransactionType == 1)
                {
                    transactions = transactions.Where(e => e.Status == TransactionStatus.Success);
                }
                else
                {
                    transactions = transactions.Where(e => e.Status != TransactionStatus.Success);
                    transactions = transactions.WhereIf(!string.IsNullOrWhiteSpace(input.StateFilter), e => e.Status.ToString().Equals(input.StateFilter));
                }

                transactions = transactions
                   .Include(x => x.Machine)
                   .Include(x => x.Session)
                   .Include(x => x.Dishes)
                   .Include("Dishes.Disc");

                var totalCount = await transactions.CountAsync();

                var tranLists= await transactions.OrderBy(input.Sorting ?? "PaymentTime desc")
                    .PageBy(input)
                    .ToListAsync();

                var pathImage = Path.Combine(_appConfiguration[ServerRootAddress], Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);
                var list = new List<TransactionDto>();
                foreach (var x in tranLists)
                {
                    var newTran = new TransactionDto()
                    {
                        Id = x.Id,
                        TranCode = x.TranCode.ToString(),
                        Buyer = x.Buyer,
                        PaymentTime = x.PaymentTime,
                        Amount = x.Amount,
                        PlatesQuantity = x.Dishes == null ? 0 : x.Dishes.Count,
                        States = x.Status.ToString(),
                        Dishes = ObjectMapper.Map<ICollection<DishTransactionDto>>(x.Dishes),
                        Machine = x.Machine == null ? null :  x.Machine.Name,
                        Session = x.Session == null ? null : x.Session.Name,
                        TransactionId = x.Machine == null ? x.TransactionId : $"{x.Machine.Name}_{x.TransactionId}",
                        BeginTranImage = x.BeginTranImage,
                        EndTranImage = x.EndTranImage
                    };
                    if (string.IsNullOrEmpty(x.BeginTranImage))
                    {
                        newTran.BeginTranImage = defaultImage;
                    }
                    else
                    {
                        newTran.BeginTranImage = Path.Combine(pathImage, x.BeginTranImage);
                    }
                    if (string.IsNullOrEmpty(x.EndTranImage))
                    {
                        newTran.EndTranImage = defaultImage;
                    }
                    else
                    {
                        newTran.EndTranImage = Path.Combine(pathImage, x.EndTranImage);
                    }
                    list.Add(newTran);
                }

                return new PagedResultDto<TransactionDto>(totalCount, list);
            }
            catch (UserFriendlyException ue)
            {
                throw ue;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new PagedResultDto<TransactionDto>(0, new List<TransactionDto>());
            }
        }

        [AbpAllowAnonymous]
        public async Task<List<long>> AddTransactions(IList<DetailTransaction> trans)
        {
            try
            {
                var successTrans = new List<long>();
                IQueryable<Machine> machinesQuery;
                IQueryable<Session> sessionsQuery;
                IQueryable<Disc> dishesQuery;
                IQueryable<DetailTransaction> existTransQuery;
                Machine machine = null;
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
                {
                    var mId = trans[0].MachineId;
                    machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == mId);
                    if (machine == null)
                    {
                        Logger.Error($"Sync Transaction: MachineId: {mId} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        Logger.Error($"Sync Transaction: Machine with id: {mId} is deleted");
                        return null;
                    }

                    machinesQuery = _machineRepository.GetAll();
                    sessionsQuery = _sessionRepository.GetAll();
                    dishesQuery =  _discRepository.GetAll();
                    existTransQuery = _transactionRepository.GetAll();
                    
                    foreach (var tran in trans)
                    {
                        Logger.Error($"Sync Transaction: {tran.ToString()}");
                        tran.TransactionId = tran.Id.ToString();
                        var oldId = tran.Id;
                        tran.Id = 0;

                        //transaction existed
                        if (await existTransQuery.AnyAsync(x => x.TransactionId == tran.TransactionId)) continue;

                        if (tran.MachineId != null)
                        {
                            if (!(await machinesQuery.AnyAsync(x => x.Id == tran.MachineId)))
                            {
                                tran.MachineId = null;
                            }
                        }
                        if (tran.SessionId != null)
                        {
                            if (!(await sessionsQuery.AnyAsync(x => x.Id == tran.SessionId)))
                            {
                                tran.SessionId = null;
                            }
                        }

                        //Save image
                        ExtractTransactionImage(tran.BeginTranImageByte);
                        ExtractTransactionImage(tran.EndTranImageByte);

                        tran.TenantId = machine?.TenantId;
                        await _transactionRepository.InsertAsync(tran);
                        successTrans.Add(oldId);
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                    return successTrans;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return null;
            }
        }
        private void ExtractTransactionImage(byte[] imgBytes)
        {
            try
            {
                using (var zipStream = new MemoryStream(imgBytes))
                {
                    using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, true))
                    {
                        var tranImgPath = Path.Combine(_appConfiguration[ServerRootAddress], Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);
                        archive.ExtractToDirectory(tranImgPath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error when decompress transaction images");
                Logger.Error(ex.Message, ex);
            }
        }
    }
}