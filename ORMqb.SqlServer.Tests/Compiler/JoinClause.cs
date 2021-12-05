using ORMqb.Schema;
using Xunit;

namespace ORMqb.SqlServer.Tests.Compiler
{
    public class JoinClause : TestCompilerBase
    {
        [Fact]
        public void InnerSingleField()
        {
            const string table1 = "dbo.Table1";
            const string table2 = "dbo.Table2";
            const string field1 = "F1";
            var schemaBuilder = new DbSchemaBuilder();
            schemaBuilder
                .SchemaFor<Table1>(b => b
                    .Source(table1)
                    .Column(c => c.Field1, field1))
                .SchemaFor<Table2>(b => b
                .Source(table2)
                .Column(c => c.Field1, field1));

            CompileResult result = GetResult<Empty>(schemaBuilder, q => q
                .Join<Table1, Table2>((t1, t2) => t1.Field1 == t2.Field1));

            Assert.Equal($" INNER JOIN {table2} AS [t2] ON ([t1].[{field1}] = [t2].[{field1}])", result.Sql);
        }

        [Fact]
        public void InnerMultipleFields()
        {
            const string table1 = "dbo.Table1";
            const string table2 = "dbo.Table2";
            const string field1 = "F1";
            const string field2 = "F2";
            var schemaBuilder = new DbSchemaBuilder();
            schemaBuilder
                .SchemaFor<Table1>(b => b
                    .Source(table1)
                    .Column(c => c.Field1, field1)
                    .Column(c => c.Field2, field2))
                .SchemaFor<Table2>(b => b
                .Source(table2)
                .Column(c => c.Field1, field1)
                .Column(c => c.Field2, field2));

            CompileResult result = GetResult<Empty>(schemaBuilder, q => q
                .Join<Table1, Table2>((t1, t2) => t1.Field1 == t2.Field1));

            Assert.Equal($" INNER JOIN {table2} AS [t2] ON ([t1].[{field1}] = [t2].[{field1}] AND [t1].[{field2}] = [t2].[{field2}])", result.Sql);
        }
    }
}
