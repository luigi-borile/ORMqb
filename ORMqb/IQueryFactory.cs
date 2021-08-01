using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ORMqb
{
    public interface IQueryFactory
    {
        Task<T> GetAsync<T>(Expression<Func<Query<T>, Query<T>>> expression);

        Task<IEnumerable<T>> GetManyAsync<T>(Expression<Func<Query<T>, Query<T>>> expression);

        Task<int> StatementAsync(Expression<Func<Statement, Statement>> expression);

        //Task<int> BulkInsertAsync<T>(IEnumerable<T> data);

        Task<int> ExecAsync<T>(T storedProcedure);
    }
}
