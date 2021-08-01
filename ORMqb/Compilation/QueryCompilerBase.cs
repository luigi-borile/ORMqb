using ORMqb.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ORMqb.Compilation
{
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "All visit methods have not null parameter")]
    public abstract class QueryCompilerBase : ExpressionVisitor, IQueryCompiler
    {
        private CompileContext? _context;
        private byte _deepLevel;

        protected QueryCompilerBase(ISchemaProvider schemaProvider)
        {
            SchemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
        }

        protected ISchemaProvider SchemaProvider { get; }

        protected Type? ResultType { get; private set; }

        protected string? QueryAlias { get; private set; }

        protected SqlClause CurrentClause
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(CurrentClause)} before calling {nameof(Compile)}");
                }

                return _context.Clause ?? throw new InvalidOperationException($"Cannot use {nameof(CurrentClause)} before setting it");
            }
            set
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(CurrentClause)} before calling {nameof(Compile)}");
                }

                _context.Clause = value;
            }
        }

        protected SqlFunction? CurrentFunction
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(CurrentFunction)} before calling {nameof(Compile)}");
                }

                return _context.Function;
            }
            set
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(CurrentFunction)} before calling {nameof(Compile)}");
                }

                _context.Function = value;
            }
        }

        protected Type? CurrentParameterType { get; private set; }
        protected string? CurrentParameterName { get; private set; }

        protected StringBuilder Sql
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(Sql)} before calling {nameof(Compile)}");
                }

                return _context.Sql;
            }
        }

        protected ICollection<QueryParameter> Parameters
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(Parameters)} before calling {nameof(Compile)}");
                }

                return _context.Parameters;
            }
        }

        protected abstract IDictionary<string, SqlClause> ClauseMappings { get; }
        protected abstract IDictionary<string, SqlFunction> FunctionMappings { get; }

        protected abstract void PreAppendClause(bool firstAppend);
        protected abstract void PostAppendClause();

        public CompileResult Compile(Expression query)
        {
            if (query is not LambdaExpression lambda)
            {
                throw new QueryCompilerException("Unexpected expression format. Lambda expression is required");
            }
            if (lambda.Parameters.Count != 1 ||
                lambda.Parameters[0].Type.GetGenericTypeDefinition() != typeof(Query<>))
            {
                throw new QueryCompilerException("Unexpected expression format. Query type is required");
            }

            ParameterExpression queryParameter = lambda.Parameters.FirstOrDefault(
                p => p.Type.IsGenericType && p.Type.GetGenericTypeDefinition() == typeof(Query<>));
            if (queryParameter is null)
            {
                throw new QueryCompilerException("Unexpected lambda format. Query parameter type is required");
            }

            QueryAlias = queryParameter.Name;
            ResultType = queryParameter.Type.GenericTypeArguments[0];

            CompileContext? prevContext = null;

            if (_context is null)
            {
                _context = new CompileContext();
            }
            else
            {
                prevContext = _context;
                _context = prevContext.NewWithParameters();
                _deepLevel++;
            }

            Visit(lambda.Body);
            CompileResult result = _context.GetResult();

            if (_deepLevel > 0)
            {
                prevContext!.Restore(_context);
                _context = prevContext;
                _deepLevel--;
            }

            return result;
        }

        public virtual CompileResult Compile<T>(T storedProcedure)
        {
            Type type = typeof(T);
            if (!SchemaProvider.DbSchema.StoredProcedures.TryGetValue(type, out StoredProcedureDefinition spSchema))
            {
                throw new QueryCompilerException($"No schema defined for type {type.Name}");
            }

            var parameters = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p =>
                {
                    if (!spSchema.Parameters.TryGetValue(p.Name, out StoredProcedureParameter spParam))
                    {
                        throw new QueryCompilerException($"No parameter defined for property {p.Name} of type {type.Name}");
                    }

                    return new QueryParameter(spParam.Name, p.GetValue(storedProcedure), null, spParam.Direction)
                    {
                        SetOutputValue = value => p.SetValue(storedProcedure, value)
                    };
                })
                .ToList();

            return new CompileResult(spSchema.Source, parameters);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object != null &&
                node.Object.NodeType != ExpressionType.Parameter)
            {
                Visit(node.Object);
            }

            SetClause(node);
            PreAppendClause(_context!.FirstAppend);

            ParameterInfo[]? methodParameters = node.Method.GetParameters();

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                CurrentParameterType = methodParameters[i].ParameterType;
                CurrentParameterName = methodParameters[i].Name;

                Expression? arg = node.Arguments[i];

                if (CurrentClause.IsSubQuery ||
                    CurrentFunction?.IsSubQuery == true)
                {
                    CompileResult result = Compile(arg);
                    Sql.Append(result.Sql);
                }
                else
                {
                    Visit(arg);
                }
            }

            PostAppendClause();

            CurrentParameterType = null;
            CurrentParameterName = null;
            CurrentFunction = null;

            return node;
        }

        private void SetClause(MethodCallExpression node)
        {
            if (ClauseMappings.TryGetValue(node.Method.Name, out SqlClause clause))
            {
                CurrentClause = clause;
            }
            else if (FunctionMappings.TryGetValue(node.Method.Name, out SqlFunction function))
            {
                CurrentFunction = function;
            }
            else
            {
                throw new QueryCompilerException($"Method {node.Method.Name} not supported");
            }
        }
    }
}
