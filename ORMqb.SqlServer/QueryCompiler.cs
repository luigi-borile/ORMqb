using ORMqb.SqlServer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

[assembly: CLSCompliant(true)]

namespace ORMqb.Compilation.SqlServer
{
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "All visit methods have not null parameter")]
    public class QueryCompiler : QueryCompilerBase
    {
        public QueryCompiler(ISchemaProvider schemaProvider)
            : base(schemaProvider)
        { }

        protected override IDictionary<string, SqlClause> ClauseMappings { get; } = SqlMappings.Clauses;

        protected override IDictionary<string, SqlFunction> FunctionMappings { get; } = SqlMappings.Functions;

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (CurrentClause.Type == ClauseType.From)
            {
                string tableSource = SchemaProvider.DbSchema.GetTableSource(node.Parameters[0].Type);
                Sql.AppendFormat(CultureInfo.InvariantCulture, "{0} AS [{1}]", tableSource, node.Parameters[0].Name);
                return node; //body of from clause is not necessary
            }
            else if (CurrentClause.Type == ClauseType.Join)
            {
                string tableSource = SchemaProvider.DbSchema.GetTableSource(node.Parameters[1].Type);
                Sql.AppendFormat(CultureInfo.InvariantCulture, "{0} AS [{1}] ON ", tableSource, node.Parameters[1].Name);
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
            Sql.AppendFormat(CultureInfo.InvariantCulture, "[{0}]", columnName);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Sql.AppendFormat(CultureInfo.InvariantCulture, "[{0}].", node.Name);

            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Sql.Append('(');
            Visit(node.Left);

            string operand = GetOperand(node);
            Sql.AppendFormat(CultureInfo.InvariantCulture, " {0} ", operand);

            Visit(node.Right);
            Sql.Append(')');

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.Method is null || !node.Method.IsSpecialName)
            {
                string operand = GetOperand(node.NodeType);
                Sql.AppendFormat(CultureInfo.InvariantCulture, "{0} ", operand);
            }

            return base.VisitUnary(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            AddValue(node.Value);

            return node;
        }

        protected static string GetOperand(ExpressionType type)
        {
            if (SqlMappings.Operands.TryGetValue(type, out string operand))
            {
                return operand;
            }
            else
            {
                throw new QueryCompilerException($"No SQL mapping found for node type '{type}'");
            }
        }

        protected static string GetOperand(BinaryExpression node)
        {
            //handles special null equality: IS operator instead of =
            if ((node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual) &&
                node.Right.NodeType == ExpressionType.Constant && ((ConstantExpression)node.Right).Value is null)
            {
                return node.NodeType == ExpressionType.Equal ? "IS" : "IS NOT";
            }
            else
            {
                return GetOperand(node.NodeType);
            }
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
            if (CurrentFunction is null)
            {
                if (!firstAppend && CurrentClause.Separator is not null)
                {
                    Sql.AppendFormat(CultureInfo.InvariantCulture, "{0} ", CurrentClause.Separator);
                }
                else
                {
                    Sql.AppendFormat(CultureInfo.InvariantCulture, "{0} ", CurrentClause.Sql);
                }

                if (CurrentClause.Pre is not null)
                {
                    Sql.AppendFormat(CultureInfo.InvariantCulture, "{0} ", CurrentClause.Pre);
                }
            }
            else
            {
                Sql.AppendFormat(CultureInfo.InvariantCulture, "{0}(", CurrentFunction.Sql);
            }
        }

        protected override void PostAppendClause()
        {
            if (CurrentFunction is null)
            {
                if (CurrentClause.Post is not null)
                {
                    Sql.AppendFormat(CultureInfo.InvariantCulture, " {0}", CurrentClause.Post);
                }

                if (CurrentClause.Type == ClauseType.FromQuery)
                {
                    Sql.AppendFormat(CultureInfo.InvariantCulture, " AS [{0}]", QueryAlias);
                }
            }
            else
            {
                Sql.Append(')');
            }
        }
    }
}
