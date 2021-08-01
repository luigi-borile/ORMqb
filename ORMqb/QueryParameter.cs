using System;
using System.Data;

namespace ORMqb
{
    public sealed class QueryParameter
    {
        public QueryParameter(string name, object value)
            : this(name, value, null, null)
        { }

        public QueryParameter(string name, object value, int? size, ParameterDirection? direction)
        {
            (Name, Value, Size, Direction) = (name, value, size, direction ?? ParameterDirection.Input);
        }

        public string Name { get; }

        public object Value { get; }
        public Action<object?>? SetOutputValue { get; set; }

        public int? Size { get; }

        public ParameterDirection Direction { get; }
    }
}
