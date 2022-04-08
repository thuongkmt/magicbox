using Abp.Domain.Repositories;
using KonbiCloud.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Abp.ObjectMapping;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Abp.Application.Services;
using KonbiCloud.RFIDTable.Cache;
using Abp.Runtime.Caching;
using Abp.Timing;

namespace KonbiCloud.RFIDTable
{
    public class TableManager : ApplicationService, ITableManager
    {
        private readonly ICacheManager cacheManager;
        private readonly IRepository<Session, Guid> sessionRepository;

        public TableManager(IRepository<Session, Guid> sessionRepository, ICacheManager cacheManager)
        {
            this.sessionRepository = sessionRepository;
            this.cacheManager = cacheManager;
        }
        public async Task<SessionInfo> GetSessionInfo()
        {
            var cacheItem = GetSaleSessionCacheItem();
            return cacheItem.SessionInfo ?? null;

        }

        public TransactionInfo ProcessTransaction(IEnumerable<PlateReadingInput> plates)
        {
            TransactionInfo preTransaction = new TransactionInfo();
            preTransaction.MenuItems = new List<MenuItemInfo>();
            preTransaction.Id = Guid.NewGuid();
            var cacheItem = GetSaleSessionCacheItem();
            if(cacheItem.MenuItems!=null)
            {
                
                plates.ToList().ForEach(plate => {
                    if(cacheItem.MenuItems.Any(el=> el.Code == plate.UType))
                    {
                        preTransaction.MenuItems.Add(cacheItem.MenuItems.Where(el => el.Code == plate.UType).First());
                    }
                   
                });
            }
            return preTransaction;
        }

        private  SaleSessionCacheItem GetSaleSessionCacheItem()
        {
            var currentTime = Convert.ToInt32(string.Format("{0}{1}", Clock.Now.Hour, Clock.Now.Minute));
            var cacheItem = cacheManager.GetCache(SaleSessionCacheItem.CacheName).Get(SaleSessionCacheItem.CacheName,  () => {
                  return GetSaleSessionInternal();
             });
            if(cacheItem.SessionInfo ==null || (cacheItem.SessionInfo!=null && Convert.ToInt32(currentTime)> Convert.ToInt32(cacheItem.SessionInfo.ToHrs))){
                
                cacheManager.GetCache(SaleSessionCacheItem.CacheName).Clear();               
            }
          
            return cacheItem;
        }
        private  SaleSessionCacheItem GetSaleSessionInternal()
        {
            SaleSessionCacheItem cacheItem = new SaleSessionCacheItem();
            var currentTime = Convert.ToInt32(string.Format("{0}{1}", DateTime.Now.Hour, DateTime.Now.Minute));
            var s =  sessionRepository.GetAll().Where(session => currentTime > Convert.ToInt32(session.FromHrs) && currentTime <= Convert.ToInt32(session.ToHrs)).OrderBy(session => session.FromHrs)
              .FirstOrDefault();
            if (s != null)
            {

                cacheItem.SessionInfo = new SessionInfo() { Id = s.Id, Name = s.Name, FromHrs = s.FromHrs, ToHrs = s.ToHrs };
            }
            return cacheItem;
        }
    }
}
