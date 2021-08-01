using System;
using System.Diagnostics.CodeAnalysis;

namespace ORMqb
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Class without implementation, used only by ExpressionVisitor")]
    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Class without implementation, used only by ExpressionVisitor")]
    public static class SqlFn
    {
        public static int Count<T>(Func<T> expression) => default;
        public static T Average<T>(Func<T> expression) => default!;
        public static T Sum<T>(Func<T> expression) => default!;
        public static T Min<T>(Func<T> expression) => default!;
        public static T Max<T>(Func<T> expression) => default!;

        public static bool Exists(Func<Query<bool>, Query<bool>> action) => default;
    }
}
