using System;
using System.Diagnostics.CodeAnalysis;

[assembly: CLSCompliant(true)]

namespace Interstellar
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Class without implementation, used only by ExpressionVisitor")]
    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Class without implementation, used only by ExpressionVisitor")]
    public class Query<T>
    {
        public Query<T> From<TTable>(Func<TTable, TTable> alias) => this;

        public Query<T> FromQuery<TResult>(Func<Query<TResult>, Query<TResult>> query) => this;

        public Query<T> Select<TTable, TValue>(Func<TTable, TValue> field) => this;
        public Query<T> Select<TTable, TValue>(Func<TTable, TValue> field, Func<T, TValue> alias) => this;

        public Query<T> Select<TValue>(TValue value) => this;
        public Query<T> Select<TValue>(TValue value, Func<T, TValue> alias) => this;
        public Query<T> Select<TValue>(Func<TValue> expression) => this;
        public Query<T> Select<TValue>(Func<TValue> expression, Func<T, TValue> alias) => this;

        public Query<T> Join<T1, T2>(Func<T1, T2, bool> expression) => this;
        public Query<T> LeftJoin<T1, T2>(Func<T1, T2, bool> expression) => this;
        public Query<T> RightJoin<T1, T2>(Func<T1, T2, bool> expression) => this;
        public Query<T> FullJoin<T1, T2>(Func<T1, T2, bool> expression) => this;

        public Query<T> Where<TTable>(Func<TTable, bool> expression) => this;
        public Query<T> Where<T1, T2>(Func<T1, T2, bool> expression) => this;
        public Query<T> Where<T1, T2, T3>(Func<T1, T2, T3, bool> expression) => this;

        public Query<T> Having<TTable>(Func<TTable, bool> expression) => this;
        public Query<T> Having<T1, T2>(Func<T1, T2, bool> expression) => this;
        public Query<T> Having<T1, T2, T3>(Func<T1, T2, T3, bool> expression) => this;

        public Query<T> GroupBy<TTable>(Func<TTable, object> field) => this;

        public Query<T> OrderBy<TTable>(Func<TTable, object> field) => this;
        public Query<T> OrderByDesc<TTable>(Func<TTable, object> field) => this;
    }
}
