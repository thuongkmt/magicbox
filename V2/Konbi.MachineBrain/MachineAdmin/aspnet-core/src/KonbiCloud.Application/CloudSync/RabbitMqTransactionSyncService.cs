using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiBrain.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Transactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Abp;
using Abp.Dependency;
using Abp.Domain.Repositories;
using KonbiCloud.Common;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;

namespace KonbiCloud.CloudSync
{
    public class RabbitMqTransactionSyncService : AbpServiceBase, ITransactionSyncService, ITransientDependency
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        private readonly ISendMessageToCloudService _sendMessageToCloudService;
        private readonly IRepository<DetailTransaction, long> _transactionRepository;

        public RabbitMqTransactionSyncService(ILogger logger, ISendMessageToCloudService sendMessageToCloudService, IRepository<DetailTransaction, long> transactionRepository)
        {
            _logger = logger;
            this._sendMessageToCloudService = sendMessageToCloudService;
            _transactionRepository = transactionRepository;
        }

        public async Task<RestApiGenericResult<long>> PushTransactionsToServer(IList<DetailTransaction> trans)
        {
            var result = new RestApiGenericResult<long>();
            if (SettingManager == null)
            {
                _logger?.Error($"Transaction Sync Service: SettingManager is null");
                return result;
            }
            try
            {
                foreach (var newTran in trans)
                {
                    var sending = new KeyValueMessage
                    {
                        Key = MessageKeys.Transaction,
                        MachineId = newTran.MachineId.Value,
                        Value = newTran
                    };
                    if (_sendMessageToCloudService.SendQueuedMsgToCloud(sending))
                    {
                        var tran = await _transactionRepository.SingleAsync(x => x.Id == newTran.Id);
                        tran = tran.MarkSync();
                        await _transactionRepository.UpdateAsync(tran);
                    }
                }

                return new RestApiGenericResult<long>() {success = true};
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
                _logger?.Error(e.StackTrace);
                return result;
            }
        }
    }
}
