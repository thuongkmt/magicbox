using Konbini.RfidFridge.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Konbini.RfidFridge.Data;
using System.Data.Entity;

namespace Konbini.RfidFridge.Service.Base
{
    public class EntityService<T> : IEntityService<T> where T : BaseEntity
    {

        public T Create(T entity)
        {
            if (entity == null)

            {
                throw new ArgumentNullException("entity");
            }
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                var result = _dbset.Add(entity);
                _context.SaveChanges();
                return result;
            }
        }

        public async Task<T> CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                var result = _dbset.Add(entity);
                await _context.SaveChangesAsync();
                return result;
            }
        }

        public void Delete(T entity)
        {
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                if (entity == null) throw new ArgumentNullException("entity");
                _dbset.Attach(entity);
                _dbset.Remove(entity);
                _context.SaveChanges();
            }
        }

        public IList<T> FetchAll()
        {
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                return _dbset.ToList();
            }
        }

        public IList<T> FetchAll(string include)
        {
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                return _dbset.Include(include).ToList();
            }
        }

        public IQueryable<T> QueryAll(RfidFridgeDataContext context)
        {

            var _dbset = context.Set<T>();
            return _dbset.AsQueryable().AsNoTracking();

        }

        public T Find(object id)
        {
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                return _dbset.Find(id);
            }
        }

        public IList<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                return _dbset.Where(predicate).ToList();
            }
        }

        public virtual void Update(T entity)
        {
            using (var _context = new RfidFridgeDataContext())
            {
                if (entity == null) throw new ArgumentNullException("entity");
                _context.Set<T>().Attach(entity);
                _context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
            }
        }

        public T SingleOrDefault()
        {
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                if (_dbset.Count() >= 2)
                    throw new Exception("This method is only valid for entity with Single record");

                return _dbset.FirstOrDefault();
            }
        }

        public T Single()
        {
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                var count = _dbset.Count();
                if (count > 1)
                    throw new Exception("This method is only valid for entity with only Single record");

                return _dbset.FirstOrDefault();
            }
        }
        public void Clear()
        {
            using (var _context = new RfidFridgeDataContext())
            {
                var _dbset = _context.Set<T>();
                var list = _dbset.ToList();
                _dbset.RemoveRange(list);
                _context.SaveChanges();
            }
        }

    }
}