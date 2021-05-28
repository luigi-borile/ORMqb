using Interstellar.Compilation;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Interstellar.SqlServer
{
    public static class SqlMappings
    {
        public static IReadOnlyDictionary<Clause, string> Clauses { get; } =
            new Dictionary<Clause, string>
            {
                { Clause.Select, "SELECT" },
                { Clause.From, "FROM" },
                { Clause.Join, "INNER JOIN" },
                { Clause.Where, "WHERE" }
            };

        public static IReadOnlyDictionary<ExpressionType, string> Operands { get; } =
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
