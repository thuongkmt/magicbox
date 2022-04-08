using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Common;
using KonbiCloud.TemperatureLogs;
using KonbiCloud.Transactions;
using KonbiCloud.Transactions.Dtos;
using System;
using System.Linq;

namespace KonbiCloud.BackgroundJobs
{
    public class SendEmailJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IDetailLogService _detailLogService;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private bool isRunning;
        private ITransactionAppService _transactionAppService;

        public SendEmailJob(
            AbpTimer timer,
            IRepository<TemperatureLog> temperatureLogsRepository,
            IDetailLogService detailLogService,
            IEmailSender emailSender,
             IUnitOfWorkManager unitOfWorkManager,
             ITransactionAppService transactionAppService
        ) : base(timer)
        {
            Timer.Period = 5000;
            //Timer.Period = 60 * 1000;
            _detailLogService = detailLogService;
            _emailSender = emailSender;
            _unitOfWorkManager = unitOfWorkManager;
            _transactionAppService = transactionAppService;
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {

                    if (isRunning)
                    {
                        return;
                    }
                    isRunning = true;
                    var a = new TransactionInput();
                    var sida = _transactionAppService.GetAllTransactions(a);

                    //using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                    //{
                    //    _emailSender.Send(
                    //        to: "manh@konbinisg.com",
                    //        subject: "You have a new task!",
                    //        body: $"A new task is assigned for you: <b>SIDA</b>",
                    //        isBodyHtml: true

                    //    );
                    //}

                    isRunning = false;
                }
            }
            catch (Exception ex)
            {
                _detailLogService.Log($"Error when clear temperature logs: " + ex.Message);
            }
        }
    }
}
