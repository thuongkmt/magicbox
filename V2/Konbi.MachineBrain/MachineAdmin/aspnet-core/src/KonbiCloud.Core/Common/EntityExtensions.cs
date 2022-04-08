using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Common
{
    public static class EntityExtensions
    {
        public static T MarkSync<T>(this T entity) where T : ISyncEntity
        {
            entity.IsSynced = true;
            entity.SyncDate=DateTime.Now;
            return entity;
        }
    }
}
