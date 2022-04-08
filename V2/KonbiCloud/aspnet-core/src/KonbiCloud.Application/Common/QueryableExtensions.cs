using Abp.Domain.Repositories;
using KonbiCloud.Machines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KonbiCloud.Common
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> RestrictedByMachineUser<T>(this IQueryable<T> query,IRepository<UserMachine,Guid> userMachineRepository, long userId) where T:IHasMachine
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            var userMachineQuery = userMachineRepository.GetAll().Where(x => x.UserId == userId).Select(x => x.MachineId).Distinct();
            return query.Where(x => userMachineQuery.Any(id => x.Machine.Id == id));
        }

        public static IQueryable<Machine> RestrictedByUser(this IQueryable<Machine> query, IRepository<UserMachine, Guid> userMachineRepository, long userId) 
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            
            var userMachineQuery = userMachineRepository.GetAll().Where(x => x.UserId == userId).Select(x => x.MachineId).Distinct();
            return query.Where(x => userMachineQuery.Any(id => x.Id == id));
        }
    }
}
