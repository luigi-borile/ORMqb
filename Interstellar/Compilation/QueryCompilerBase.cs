using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Interstellar.Compilation
{
    public abstract class QueryCompilerBase : ExpressionVisitor, IQueryCompiler
    {
        protected readonly StringBuilder _selectSql = new();
        protected readonly StringBuilder _fromSql = new();
        protected readonly StringBuilder _joinSql = new();
        protected readonly StringBuilder _whereSql = new();
        protected Type _queryType;
        protected static readonly IEnumerable<string> _clauses = Enum.GetNames(typeof(Clause));

        public QueryCompilerBase(ISchemaProvider schemaProvider)
        {
            SchemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            Parameters = new List<QueryParameter>();
        }

        protected ISchemaProvider SchemaProvider { get; }

        protected StringBuilder Sql => CurrentClause switch
        {
            Clause.Select => _selectSql,
            Clause.From => _fromSql,
            Clause.Join => _joinSql,
            Clause.Where => _whereSql
        };

        protected List<QueryParameter> Parameters { get; }

        protected Clause CurrentClause { get; set; }

        protected abstract string GetSql();

        public CompileResult Compile(Expression query)
        {
            Visit(query);

            return new CompileResult(GetSql(), Parameters);
        }
    }
}
