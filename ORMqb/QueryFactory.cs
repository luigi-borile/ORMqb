using ORMqb.Compilation;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ORMqb
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

        public Task<int> StatementAsync(Expression<Func<Statement, Statement>> expression)
        {
            CompileResult compileResult = Compiler.Compile(expression);

            throw new NotImplementedException();
            //return Executor.GetManyAsync<T>(compileResult);
        }

        public Task<int> ExecAsync<T>(T storedProcedure)
        {
            CompileResult compileResult = Compiler.CompileSp(storedProcedure);

            return Executor.ExecAsync(compileResult);
        }

        protected IQueryCompiler Compiler { get; }
        protected IQueryExecutor Executor { get; }
    }
}
