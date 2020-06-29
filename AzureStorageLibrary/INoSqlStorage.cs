using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageLibrary
{

    public enum ETableName
    {
        Products,
  
    }
    interface INoSqlStorage<TEntity>
    {

        Task<TEntity> Add(ETableName eTableName, TEntity entity);

        Task<TEntity> Get(ETableName eTableName, string rowKey, string partitionKey);

        Task Delete(ETableName eTableName, string rowKey, string partitionKey);

        Task<TEntity> Update(ETableName eTableName, TEntity entity);

        Task<List<TEntity>> All(ETableName eTableName);
        IQueryable<TEntity> Query(ETableName eTable, Expression<Func<TEntity, bool>> search);


    }
}
