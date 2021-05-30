using System;
using System.Diagnostics.CodeAnalysis;

namespace Interstellar
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Class without implementation, used only by ExpressionVisitor")]
    public class SqlFunctions
    {
        public static int Count(Func<object> expression) => default;

        public static bool Exists(Func<Query<bool>, Query<bool>> action) => default;
    }
}
