using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interstellar.Compilation
{
    public sealed class CompileContext
    {
        private readonly StringBuilder _selectSql = new();
        private readonly StringBuilder _fromSql = new();
        private readonly StringBuilder _joinSql = new();
        private readonly StringBuilder _whereSql = new();
        private List<QueryParameter>? _parameters;

        public CompileContext()
        { }

        public CompileContext(IEnumerable<QueryParameter>? parameters)
        {
            SetParameters(parameters);
        }

        public List<QueryParameter> Parameters => _parameters ??= new();

        public Clause CurrentClause { get; set; }

        public StringBuilder Sql => CurrentClause switch
        {
            Clause.Select => _selectSql,
            Clause.From or Clause.FromQuery => _fromSql,
            Clause.Join or Clause.LeftJoin or
            Clause.RightJoin or Clause.FullJoin => _joinSql,
            Clause.Where or Clause.Exists => _whereSql
        };

        public CompileResult GetResult() => new CompileResult(GetSql(), Parameters);

        public CompileContext NewWithParameters() =>
            new CompileContext(_parameters);

        public void Restore(CompileContext context) =>
            SetParameters(context._parameters);

        private void SetParameters(IEnumerable<QueryParameter>? parameters)
        {
            if (parameters != null && parameters.Count() > 0)
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

            return sb.ToString();
        }
    }
}
