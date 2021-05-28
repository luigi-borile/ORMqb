using System;
using System.Diagnostics.CodeAnalysis;

namespace Interstellar
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Class without implementation, used only by ExpressionVisitor")]
    public sealed class Query<T>
    {
        public Query<T> From<TTable>(Func<TTable, TTable> alias) => this;

        public Query<T> FromQuery<TResult>(Func<Query<TResult>, Query<TResult>> query) => this;

        public Query<T> Select<TTable, TValue>(Func<TTable, TValue> expression) => this;
        public Query<T> Select<TTable, TValue>(Func<TTable, TValue> expression, Func<T, TValue> alias) => this;

        public Query<T> SelectValue<TValue>(Func<TValue> expression) => this;

        public Query<T> Join<T1, T2>(Func<T1, T2, bool> expression) => this;
        public Query<T> LeftJoin<T1, T2>(Func<T1, T2, bool> expression) => this;
        public Query<T> RightJoin<T1, T2>(Func<T1, T2, bool> expression) => this;
        public Query<T> FullJoin<T1, T2>(Func<T1, T2, bool> expression) => this;

        public Query<T> Where<TTable>(Func<Where, TTable, bool> expression) => this;

        public Query<T> Where<T1, T2>(Func<Where, T1, T2, bool> expression) => this;
        public Query<T> Where<T1, T2, T3>(Func<Where, T1, T2, T3, bool> expression) => this;

        public Query<T> Having<TTable>(Func<Where, T, bool> expression) => this;

        public Query<T> OrderBy<TTable, TValue>(Func<T, TValue> field) => this;

        public Query<T> GroupBy<TTable, TValue>(Func<T, TValue> field) => this;
    }
}
