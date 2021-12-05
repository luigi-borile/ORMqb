using System;
using System.Collections.Generic;

namespace ORMqb
{
    public sealed class CompileResult
    {
        public CompileResult(string sql)
            : this(sql, null)
        { }

        public CompileResult(string sql, IReadOnlyList<QueryParameter>? parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            Sql = sql;
            Parameters = parameters;
            HasParameters = parameters != null && parameters.Count > 0;
        }

        public string Sql { get; }

        public IReadOnlyList<QueryParameter>? Parameters { get; }

        public bool HasParameters { get; }
    }
}
