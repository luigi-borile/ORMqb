using ORMqb.Schema;
using Xunit;

namespace ORMqb.SqlServer.Tests.Compiler
{
    public class FromClause : TestCompilerBase
    {
        [Fact]
        public void Table()
        {
            const string tableName = "dbo.Table";
            var schemaBuilder = new DbSchemaBuilder();
            schemaBuilder.SchemaFor<Table1>(b => b
                .Source(tableName)
                .Column(c => c.Field1, "Field1"));

            CompileResult result = GetResult<Empty>(schemaBuilder, q => q
                .From<Table1>(t => t));

            Assert.Equal($" FROM {tableName} AS [t]", result.Sql);
        }

        [Fact]
        public void SubQuery()
        {
            const string tableName = "dbo.Table";
            var schemaBuilder = new DbSchemaBuilder();
            schemaBuilder.SchemaFor<Table1>(b => b
                .Source(tableName)
                .Column(c => c.Field1, "Field1"));

            CompileResult result = GetResult<Empty>(schemaBuilder, q => q
                .FromQuery<Empty>(t => t.Select(1).From<Table1>(t2 => t2)));

            Assert.Matches(@" FROM \( .* \) AS \[t\]", result.Sql);
        }
    }
}
