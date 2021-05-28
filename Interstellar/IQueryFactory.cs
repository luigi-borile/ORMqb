using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Interstellar
{
    public interface IQueryFactory
    {
        Task<T> GetAsync<T>(Expression<Func<Query<T>, Query<T>>> expression);

        Task<IEnumerable<T>> GetManyAsync<T>(Expression<Func<Query<T>, Query<T>>> expression);
    }
}
