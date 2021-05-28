using Interstellar.Compilation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Interstellar
{
    public class QueryFactory : IQueryFactory
    {
        public QueryFactory(IQueryCompiler compiler, IQueryExecutor executor)
        {
            Compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            Executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public Task<T> GetAsync<T>(Expression<Func<Query<T>, Query<T>>> expression)
        {
            CompileResult compileResult = Compiler.Compile(expression);

            return Executor.GetAsync<T>(compileResult);
        }

        public Task<IEnumerable<T>> GetManyAsync<T>(Expression<Func<Query<T>, Query<T>>> expression)
        {
            CompileResult compileResult = Compiler.Compile(expression);

            return Executor.GetManyAsync<T>(compileResult);
        }

        protected IQueryCompiler Compiler { get; }
        protected IQueryExecutor Executor { get; }
    }
}
