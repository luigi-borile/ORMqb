using Interstellar.Compilation;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Interstellar.SqlServer
{
    public class QueryCompiler : QueryCompilerBase
    {
        public QueryCompiler(ISchemaProvider schemaProvider)
            : base(schemaProvider)
        { }

        protected override string GetSql()
        {
            var sb = new StringBuilder();

            if (_selectSql.Length > 0)
            {
                // Removes last comma and space
                _selectSql.Remove(_selectSql.Length - 2, 2);
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

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            string clause = _clauses.FirstOrDefault(c => node.Method.Name.Contains(c));

            if (clause is null)
            {
                throw new QueryCompilerException($"Method {node.Method.Name} not supported");
            }

            CurrentClause = (Clause)Enum.Parse(typeof(Clause), clause);

            AppendClause();

            foreach (Expression arg in node.Arguments)
            {
                Visit(arg);
            }

            if (CurrentClause == Clause.Select)
            {
                Sql.Append(", ");
            }

            if (node.Object.NodeType != ExpressionType.Parameter)
            {
                Visit(node.Object);
            }

            return node;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            ParameterExpression queryParameter = node.Parameters.FirstOrDefault(
                p => p.Type.IsGenericType && p.Type.GetGenericTypeDefinition() == typeof(Query<>));
            if (queryParameter != null)
            {
                _queryType = queryParameter.Type.GenericTypeArguments[0];
            }
            else
            {
                if (CurrentClause == Clause.Join)
                {
                    string tableSource = SchemaProvider.DbSchema.GetTableSource(node.Parameters[1].Type);
                    Sql.AppendFormat("{0} AS {1} ON ", tableSource, node.Parameters[1].Name);
                }
            }

            return Visit(node.Body);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(Query<>))
            {
                return node;
            }

            Sql.AppendFormat("{0}.", node.Name);

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Visit(node.Expression);

            string columnName = SchemaProvider.DbSchema.GetColumnName(node.Member);
            Sql.Append(columnName);

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

            return node;
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

        protected void AppendClause()
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
                    if (Sql.Length > 0)
                    {
                        throw new QueryCompilerException("FROM clause specied more then once");
                    }
                    break;
            }

            if (append)
            {
                Sql.AppendFormat("{0} ", sqlClause);
            }
        }

        //protected CompileResult Compile(CompileContext context) =>
        //CompileFrom(context);
        //CompileWhere(context);
        //CompileSelect(context);

        //new CompileResult(context.Sql.ToString(), context.Parameters);


        /*
        protected virtual void CompileSelect(CompileContext context)
        {
            SelectClause[] selectClauses = context.Clauses.Where(c => c.Type == ClauseType.Select).Cast<SelectClause>().ToArray();
            var selectSql = new StringBuilder("SELECT");

            if (selectClauses.Length == 0)
            {
                foreach (FromClause clause in context.Tables)
                {
                    DbObjectDefinition definition = SchemaProvider.DbSchema.GetDefinition(clause.Table);
                    foreach (KeyValuePair<string, string> field in definition.Columns)
                    {
                        selectSql.AppendFormat(" {0}.{1} AS [{2}],", clause.Alias, field.Value, field.Key);
                    }
                }
            }
            else
            {
                foreach (SelectClause clause in selectClauses)
                {
                    selectSql.Append(' ');

                    if (clause.Field is null)
                    {
                        if (clause.IsNumericValue)
                        {
                            selectSql.Append(clause.Value);
                        }
                        else
                        {
                            selectSql.AppendFormat("= '{0}'", clause.Value);
                        }

                        if (string.IsNullOrWhiteSpace(clause.ValueAlias))
                        {
                            selectSql.Append(',');
                        }
                        else
                        {
                            selectSql.AppendFormat(" AS [{0}],", clause.ValueAlias);
                        }
                    }
                    else
                    {
                        DbObjectDefinition dbObj = SchemaProvider.DbSchema.GetDefinition(clause.Field.Table);
                        selectSql.AppendFormat("{0} AS [{1}],", dbObj.Columns[clause.Field.Name], clause.Field.Name);
                    }
                }
            }

            selectSql.Length--;
            context.Sql.Insert(0, selectSql.ToString());
        }

        protected virtual void CompileFrom(CompileContext context)
        {
            if (context.Clauses.FirstOrDefault(c => c.Type == ClauseType.From) is not FromClause fromClause)
            {
                throw new QueryCompilerException("Missing from clause");
            }
            DbObjectDefinition definition = SchemaProvider.DbSchema.GetDefinition(fromClause.Table);

            context.Sql.AppendFormat(" FROM {0}", definition.Source);

            if (fromClause.HasAlias)
            {
                context.Sql.AppendFormat(" AS [{0}]", fromClause.Alias);
            }

            context.Tables.Add(new FromClause(definition.Type, fromClause.HasAlias ? fromClause.Alias : definition.Source));
        }

        protected virtual void CompileWhere(CompileContext context)
        {
            WhereClause[] whereClauses = context.Clauses.Where(c => c.Type == ClauseType.Where).Cast<WhereClause>().ToArray();

            if (whereClauses.Length == 0)
            {
                return;
            }

            context.Sql.Append(" WHERE");

            foreach (WhereClause clause in whereClauses)
            {
                context.Sql.Append(' ');

                if (clause.IsNot)
                {
                    context.Sql.Append("NOT(");
                }

                if (clause.SubQuery is null)
                {
                    CompileInlineWhere(context, clause);
                }
                else
                {
                    CompileSubWhere(context, clause);
                }

                if (clause.IsNot)
                {
                    context.Sql.Append(')');
                }

                context.Sql.Append(clause.OrCondition ? " OR" : " AND");
            }

            context.Sql.Length -= whereClauses[whereClauses.Length - 1].OrCondition ? 3 : 4;
        }

        private void CompileSubWhere(CompileContext context, WhereClause clause)
        {
            context.Sql.Append("EXISTS (");

            var subQuery = new Query();
            clause.SubQuery.Invoke(subQuery);

            if (!subQuery.Clauses.Any(c => c.Type == ClauseType.Select))
            {
                subQuery.SelectValue(1);
            }

            CompileResult result = Compile(new CompileContext(subQuery.Clauses, context.Parameters, context.Tables));

            context.Sql.Append(result.Sql);
            context.Sql.Append(')');
        }

        private void CompileInlineWhere(CompileContext context, WhereClause clause)
        {
            DbObjectDefinition obj1 = SchemaProvider.DbSchema.GetDefinition(clause.Field1.Table);

            string table1;
            IEnumerable<FromClause> tables = context.Tables.Where(c => c.Table == obj1.Type);
            if (!tables.Any())
            {
                throw new QueryCompilerException("Where clause in an object not included in the query");
            }
            else if (tables.Count() == 1)
            {
                table1 = tables.First().Alias;
            }
            else
            {
                table1 = clause.Field1.TableAlias;
            }

            string column1 = obj1.Columns[clause.Field1.Name];

            context.Sql.AppendFormat("{0}.[{1}] ", table1, column1);

            if (clause.Field2 is null)
            {
                if (clause.Value is null)
                {
                    context.Sql.Append("IS NULL");
                }
                else
                {
                    string parameterName = $"@p{context.Parameters.Count}";

                    context.Sql.AppendFormat("{0} {1}", clause.SqlOperator, parameterName);
                    context.Parameters.Add(new QueryParameter(parameterName, clause.Value));
                }
            }
            else
            {
                DbObjectDefinition obj2 = SchemaProvider.DbSchema.GetDefinition(clause.Field2.Table);

                string table2;
                tables = context.Tables.Where(c => c.Table == obj2.Type);
                if (!tables.Any())
                {
                    throw new QueryCompilerException("Where clause in an object not included in the query");
                }
                else if (tables.Count() == 1)
                {
                    table2 = tables.First().Alias;
                }
                else
                {
                    table2 = clause.Field2.TableAlias;
                }

                string column2 = obj2.Columns[clause.Field2.Name];

                context.Sql.AppendFormat("= {0}.[{1}]", table2, column2);
            }
        }*/
    }
}
