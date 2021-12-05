using ORMqb.Schema;
using Xunit;

namespace ORMqb.SqlServer.Tests.Compiler
{
    public class SelectClause : TestCompilerBase
    {
        [Fact]
        public void SingleField()
        {
            const string field = "F1";
            var schemaBuilder = new DbSchemaBuilder();
            schemaBuilder.SchemaFor<Table1>(b => b
                .Source("dbo.Table")
                .Column(c => c.Field1, field));

            CompileResult result = GetResult<Result1>(schemaBuilder, q => q
                .Select<Table1, int>(t => t.Field1, r => r.Field1));

            Assert.Equal($"SELECT [t].[{field}] AS [{nameof(Result1.Field1)}]", result.Sql);
        }

        [Fact]
        public void MultipleFields()
        {
            const string field1 = "F1";
            const string field2 = "F2";
            var schemaBuilder = new DbSchemaBuilder();
            schemaBuilder.SchemaFor<Table1>(b => b
                .Source("dbo.Table")
                .Column(c => c.Field1, field1)
                .Column(c => c.Field2, field2));

            CompileResult result = GetResult<Result1>(schemaBuilder, q => q
                .Select<Table1, int>(t => t.Field1, r => r.Field1)
                .Select<Table1, int>(t => t.Field2, r => r.Field2));

            Assert.Equal($"SELECT [t].[{field1}] AS [{nameof(Result1.Field1)}], [t].[{field2}] AS [{nameof(Result1.Field2)}]", result.Sql);
        }

        [Fact]
        public void SingleValue()
        {
            const string paramName = "@p1";
            const int paramValue = 1;
            var schemaBuilder = new DbSchemaBuilder();
            schemaBuilder.SchemaFor<Table1>(b => b
                .Source("dbo.Table")
                .Column(c => c.Field1, "F1"));

            CompileResult result = GetResult<Result1>(schemaBuilder, q => q
                .Select(paramValue, r => r.Field1));

            Assert.Equal($"SELECT {paramName} AS [{nameof(Result1.Field1)}]", result.Sql);
            Assert.Single(result.Parameters, p => p.Name == paramName && p.Value.Equals(paramValue));
        }
    }
}
