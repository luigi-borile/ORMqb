using System;
using System.Diagnostics.CodeAnalysis;

namespace ORMqb
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Class without implementation, used only by ExpressionVisitor")]
    [SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "Class without implementation, used only by ExpressionVisitor")]
    public class Statement : Query<Empty>
    {
        public Query<Empty> Into<TTable>() => this;
        public Query<Empty> Insert<TTable, TValue>(Func<TTable, TValue> field) => this;
        public Query<Empty> Value<TValue>(TValue value) => this;
        public Query<Empty> Value<TValue>(Func<TValue> value) => this;
        public Query<Empty> Delete<TTable>(Func<TTable, TTable> alias) => this;
        public Query<Empty> Update<TTable>(Func<TTable, TTable> alias) => this;
        public Query<Empty> Set<TTable, TValue>(Func<TTable, TValue> updateField, TValue value) => this;
        public Query<Empty> Set<T1, T2, TValue>(Func<T1, TValue> updateField, Func<T2, TValue> valueField) => this;
    }
}
