using Interstellar.Compilation;
using System.Linq;
using System.Linq.Expressions;

namespace Interstellar.SqlServer
{
    public class QueryCompiler : QueryCompilerBase
    {
        public QueryCompiler(ISchemaProvider schemaProvider)
            : base(schemaProvider)
        { }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (CurrentClause == Clause.From)
            {
                string tableSource = SchemaProvider.DbSchema.GetTableSource(node.Parameters[0].Type);
                Sql.AppendFormat("{0} AS [{1}]", tableSource, node.Parameters[0].Name);
                return node; //body of from clause is not necessary
            }
            else if (CurrentClause == Clause.Join)
            {
                string tableSource = SchemaProvider.DbSchema.GetTableSource(node.Parameters[1].Type);
                Sql.AppendFormat("{0} AS [{1}] ON ", tableSource, node.Parameters[1].Name);
            }

            return Visit(node.Body);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (CurrentParameterName == "alias")
            {
                Sql.Append(" AS ");
            }
            else
            {
                Visit(node.Expression);
            }

            string columnName = SchemaProvider.DbSchema.GetColumnName(node.Member);
            Sql.Append(columnName);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Sql.AppendFormat("[{0}].", node.Name);

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);

            if (SqlMappings.Operands.TryGetValue(node.NodeType, out string operand))
            {
                Sql.AppendFormat(" {0} ", operand);
            }
            else
            {
                throw new QueryCompilerException($"No SQL mapping found for node type '{node.NodeType}'");
            }

            Visit(node.Right);

            if (node.NodeType == ExpressionType.Equal)
            {
                Sql.Replace("= NULL", "IS NULL");
            }
            else if (node.NodeType == ExpressionType.NotEqual)
            {
                Sql.Replace("<> NULL", "IS NOT NULL");
            }

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (SqlMappings.Operands.TryGetValue(node.NodeType, out string str))
            {
                Sql.Append(str);
            }

            return base.VisitUnary(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            AddValue(node.Value);

            return node;
        }

        protected void AddValue(object value)
        {
            if (value is null)
            {
                Sql.Append("NULL");
            }
            else
            {
                QueryParameter p = Parameters.FirstOrDefault(p => p.Value.Equals(value));
                if (p is null)
                {
                    p = new QueryParameter($"@p{Parameters.Count + 1}", value);
                    Parameters.Add(p);
                }

                Sql.Append(p.Name);
            }
        }

        protected override void PreAppendClause(bool firstAppend)
        {
            bool append = true;
            string sqlClause;

            if (CurrentFunction is null)
            {
                if (!SqlMappings.Clauses.TryGetValue(CurrentClause, out sqlClause))
                {
                    throw new QueryCompilerException($"Clause {CurrentClause} not mapped");
                }

                switch (CurrentClause)
                {
                    case Clause.Select:
                    case Clause.Where:
                        if (!firstAppend)
                        {
                            append = false;
                        }
                        break;
                    case Clause.From:
                    case Clause.FromQuery:
                        if (!firstAppend)
                        {
                            throw new QueryCompilerException("FROM clause specied more then once");
                        }
                        break;
                }
            }
            else
            {
                if (!SqlMappings.Functions.TryGetValue(CurrentFunction.Value, out sqlClause))
                {
                    throw new QueryCompilerException($"Function {CurrentFunction} not mapped");
                }
            }

            if (append)
            {
                Sql.AppendFormat("{0} ", sqlClause);

                if (CurrentClause == Clause.FromQuery ||
                    CurrentFunction is not null)
                {
                    Sql.Append('(');
                }
            }

            if (!firstAppend && CurrentClause == Clause.Select)
            {
                Sql.Append(", ");
            }
        }

        protected override void PostAppendClause()
        {
            if (CurrentClause == Clause.FromQuery)
            {
                Sql.AppendFormat(") AS [{0}]", QueryAlias);
            }

            if (CurrentFunction is not null)
            {
                Sql.Append(')');
            }
        }
    }
}
