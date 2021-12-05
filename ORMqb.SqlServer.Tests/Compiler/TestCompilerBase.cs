using ORMqb.Compilation.SqlServer;
using ORMqb.Schema;
using System;
using System.Linq.Expressions;

namespace ORMqb.SqlServer.Tests.Compiler
{
    public class TestCompilerBase
    {
        protected static CompileResult GetResult<T>(
            DbSchemaBuilder schemaBuilder,
            Expression<Func<Query<T>, Query<T>>> query)
        {
            var compiler = new QueryCompiler(schemaBuilder);
            return compiler.Compile(query);
        }
    }
}
