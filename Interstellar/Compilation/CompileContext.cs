using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interstellar.Compilation
{
    internal sealed class CompileContext
    {
        private readonly StringBuilder _selectSql = new();
        private readonly StringBuilder _fromSql = new();
        private readonly StringBuilder _joinSql = new();
        private readonly StringBuilder _whereSql = new();
        private readonly StringBuilder _havingSql = new();
        private readonly StringBuilder _groupBySql = new();
        private readonly StringBuilder _orderBySql = new();
        private readonly List<ClauseType> _addedClauses = new();
        private List<QueryParameter>? _parameters;
        private SqlClause? _clause;

        public CompileContext()
        { }

        public CompileContext(IEnumerable<QueryParameter>? parameters)
        {
            SetParameters(parameters);
        }

        public List<QueryParameter> Parameters => _parameters ??= new();

        public bool FirstAppend { get; private set; }

        public SqlClause? Clause
        {
            get => _clause;
            set
            {
                if (value != null)
                {
                    if (_addedClauses.Contains(value.Type))
                    {
                        if (!value.AllowMultiple)
                        {
                            throw new QueryCompilerException($"Multiple {value.Type} clauses are not allowed");
                        }
                        FirstAppend = false;
                    }
                    else
                    {
                        _addedClauses.Add(value.Type);
                        FirstAppend = true;
                    }
                }

                _clause = value;
            }
        }

        public SqlFunction? Function { get; set; }

        public StringBuilder Sql
        {
            get
            {
                if (Clause is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(Sql)} before setting {nameof(Clause)}");
                }

                return Clause.Type switch
                {
                    ClauseType.Select => _selectSql,
                    ClauseType.From or ClauseType.FromQuery => _fromSql,
                    ClauseType.Join or ClauseType.LeftJoin or
                    ClauseType.RightJoin or ClauseType.FullJoin => _joinSql,
                    ClauseType.Where => _whereSql,
                    ClauseType.Having => _havingSql,
                    ClauseType.GroupBy => _groupBySql,
                    ClauseType.OrderBy => _orderBySql,
                    _ => throw new NotImplementedException()
                };
            }
        }

        public CompileResult GetResult() => new CompileResult(GetSql(), Parameters);

        public CompileContext NewWithParameters() =>
            new CompileContext(_parameters);

        public void Restore(CompileContext context) =>
            SetParameters(context._parameters);

        private void SetParameters(IEnumerable<QueryParameter>? parameters)
        {
            if (parameters != null && parameters.Any())
            {
                _parameters = new List<QueryParameter>(parameters);
            }
        }

        private string GetSql()
        {
            var sb = new StringBuilder();

            if (_selectSql.Length > 0)
            {
                sb.Append(_selectSql.ToString());
            }
            if (_fromSql.Length > 0)
            {
                sb.Append(' ');
                sb.Append(_fromSql.ToString());
            }
            if (_joinSql.Length > 0)
            {
                sb.Append(' ');
                sb.Append(_joinSql.ToString());
            }
            if (_whereSql.Length > 0)
            {
                sb.Append(' ');
                sb.Append(_whereSql.ToString());
            }
            if (_groupBySql.Length > 0)
            {
                sb.Append(' ');
                sb.Append(_groupBySql.ToString());
            }
            if (_havingSql.Length > 0)
            {
                sb.Append(' ');
                sb.Append(_havingSql.ToString());
            }
            if (_orderBySql.Length > 0)
            {
                sb.Append(' ');
                sb.Append(_orderBySql.ToString());
            }

            return sb.ToString();
        }
    }
}
