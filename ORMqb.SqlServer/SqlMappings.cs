using ORMqb.Compilation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ORMqb.SqlServer
{
    internal static class SqlMappings
    {
        internal static ConcurrentDictionary<string, SqlClause> Clauses { get; } =
            new(new KeyValuePair<string, SqlClause>[]
            {
                new("Select", new SqlClause(ClauseType.Select, "SELECT", true, false, ",")),
                new("From", new SqlClause(ClauseType.From, "FROM", false, false)),
                new("FromQuery", new SqlClause(ClauseType.FromQuery, "FROM", false, true, pre: "(", post: ")")),
                new("Join", new SqlClause(ClauseType.Join, "INNER JOIN", true, false)),
                new("LeftJoin", new SqlClause(ClauseType.LeftJoin, "LEFT JOIN", true, false)),
                new("RightJoin", new SqlClause(ClauseType.RightJoin, "RIGHT JOIN", true, false)),
                new("FullJoin", new SqlClause(ClauseType.FullJoin, "FULL JOIN", true, false)),
                new("Where", new SqlClause(ClauseType.Where, "WHERE", true, false, "AND")),
                new("Having", new SqlClause(ClauseType.Having, "HAVING", true, false, "AND")),
                new("GroupBy", new SqlClause(ClauseType.GroupBy, "GROUP BY", true, false, ",")),
                new("OrderBy", new SqlClause(ClauseType.OrderBy, "ORDER BY", true, false, ",")),
                new("OrderByDesc", new SqlClause(ClauseType.OrderBy, "ORDER BY", true, false, ",", post: "DESC"))
            });

        internal static ConcurrentDictionary<string, SqlFunction> Functions { get; } =
            new(new KeyValuePair<string, SqlFunction>[]
            {
                new("Exists", new SqlFunction("EXISTS", true)),
                new("Count", new SqlFunction("COUNT", false)),
                new("Average", new SqlFunction("AVG", false)),
                new("Sum", new SqlFunction("SUM", false)),
                new("Min", new SqlFunction("MIN", false)),
                new("Max", new SqlFunction("MAX", false)),
            });

        internal static IReadOnlyDictionary<ExpressionType, string> Operands { get; } =
            new Dictionary<ExpressionType, string>
            {
                { ExpressionType.Add, "+" },
                { ExpressionType.AddChecked, "+" },
                { ExpressionType.AndAlso, "AND" },
                { ExpressionType.Decrement, "-" },
                { ExpressionType.Divide, "/" },
                { ExpressionType.Equal, "=" },
                { ExpressionType.GreaterThan, ">" },
                { ExpressionType.GreaterThanOrEqual, ">=" },
                { ExpressionType.Increment, "+" },
                { ExpressionType.LessThan, "<" },
                { ExpressionType.LessThanOrEqual, "<=" },
                { ExpressionType.Modulo, "%" },
                { ExpressionType.Multiply, "*" },
                { ExpressionType.MultiplyChecked, "*" },
                { ExpressionType.Not, "NOT" },
                { ExpressionType.NotEqual, "<>" },
                { ExpressionType.OrElse, "OR" },
                { ExpressionType.Subtract, "-" },
                { ExpressionType.SubtractChecked, "-" }
                //{ ExpressionType.Coalesce, "ISNULL({0}, {1})" },
                //{ ExpressionType.Conditional, "IIF({0}, {1}, {2})" },
                //{ ExpressionType.Convert, "CONVERT({0}, {1})" },
                //{ ExpressionType.ConvertChecked, "CONVERT({0}, {1})" },
                //{ ExpressionType.TypeAs, "CONVERT({0}, {1})" },
            };
    }
}
