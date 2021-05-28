using System;
using System.Diagnostics.CodeAnalysis;

namespace Interstellar
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Class without implementation, used only by ExpressionVisitor")]
    public sealed class Where
    {
        public bool Exists(Action<Query<bool>> action) => true;

        //public T Select<T>(Func<Query, Query> action) => default;
    }
}
