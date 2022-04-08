using Konbini.RfidFridge.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Konbini.RfidFridge.Data;

namespace Konbini.RfidFridge.Service.Base
{
    using System.Data.Entity;

    public interface IEntityService<T> where T : BaseEntity
    {
        /// <summary>
        /// Create new entity object and return the object after created.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Object</returns>
        T Create(T entity);

        Task<T> CreateAsync(T entity);

        /// <summary>
        /// Update an existed from database.
        /// </summary>
        /// <param name="entity"></param>
        void Update(T entity);

        /// <summary>
        /// Delete an object from database.
        /// </summary>
        /// <param name="entity"></param>
        void Delete(T entity);

        /// <summary>
        /// Find object from database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Return the object if it's exist from database, return null if not found.</returns>
        T Find(object id);

        /// <summary>
        /// Fetch objects by condition 
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>Return objects that match with the condition. Return null if not found.</returns>
        IList<T> FindBy(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Fetch all entity objects.
        /// </summary>
        /// <returns>Return all objects from database</returns>
        IList<T> FetchAll();
        IList<T> FetchAll(string path);

        /// <summary>
        /// Get Single entity
        /// </summary>
        /// <returns></returns>
        T SingleOrDefault();

        T Single();

        IQueryable<T> QueryAll(RfidFridgeDataContext context);

        void Clear();
    }
}
