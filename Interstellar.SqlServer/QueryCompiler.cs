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
            else if (CurrentClause == Clause.Select)
            {
                if (CurrentParameterName == "alias")
                //if (node.Parameters[0].Type == ResultType)
                {
                    Sql.Append(" AS ");
                }
                else if (Sql.Length > 7) //contains only select clause
                {
                    Sql.Append(", ");
                }
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
            Visit(node.Expression);

            string columnName = SchemaProvider.DbSchema.GetColumnName(node.Member);
            Sql.Append(columnName);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (CurrentParameterName != "alias")
            {
                Sql.AppendFormat("[{0}].", node.Name);
            }

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

        protected static LambdaExpression StripQuotes(Expression expression)
        {
            Expression e = expression;
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)expression).Operand;
            }
            return (LambdaExpression)e;
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

        protected override void PreAppendClause()
        {
            if (!SqlMappings.Clauses.TryGetValue(CurrentClause, out string sqlClause))
            {
                throw new QueryCompilerException($"Clause {CurrentClause} not mapped");
            }

            bool append = true;

            switch (CurrentClause)
            {
                case Clause.Select:
                case Clause.Where:
                    if (Sql.Length > 0)
                    {
                        append = false;
                    }
                    break;
                case Clause.From:
                case Clause.FromQuery:
                    if (Sql.Length > 0)
                    {
                        throw new QueryCompilerException("FROM clause specied more then once");
                    }
                    break;
            }

            if (append)
            {
                Sql.AppendFormat("{0} ", sqlClause);

                if (CurrentClause == Clause.FromQuery ||
                    CurrentClause == Clause.Exists)
                {
                    Sql.Append('(');
                }
            }
        }

        protected override void PostAppendClause()
        {
            switch (CurrentClause)
            {
                case Clause.FromQuery:
                    Sql.AppendFormat(") AS [{0}]", QueryAlias);
                    break;
                case Clause.Exists:
                    Sql.Append(')');
                    break;
            }
        }
    }
}
