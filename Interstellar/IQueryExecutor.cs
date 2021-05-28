﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interstellar
{
    public interface IQueryExecutor
    {
        void BeginTransaction();

        Task BeginTransactionAsync();

        void CommitTransaction();

        void RollbackTransaction();

        Task<TResult> GetAsync<TResult>(CompileResult compileResult);

        Task<IEnumerable<TResult>> GetManyAsync<TResult>(CompileResult compileResult);
    }
}
