using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Interstellar.Compilation
{
    public abstract class QueryCompilerBase : ExpressionVisitor, IQueryCompiler
    {
        private CompileContext? _context;
        private byte _deepLevel;
        protected static readonly IEnumerable<string> _clauses = Enum.GetNames(typeof(Clause));

        public QueryCompilerBase(ISchemaProvider schemaProvider)
        {
            SchemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
        }

        protected ISchemaProvider SchemaProvider { get; }

        protected Type? ResultType { get; private set; }

        protected string? QueryAlias { get; private set; }

        protected Clause CurrentClause
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(CurrentClause)} before calling {nameof(Compile)}");
                }

                return _context.Clause;
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

        protected Function? CurrentFunction
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

        protected List<QueryParameter> Parameters
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

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            SetClause(node);
            PreAppendClause(_context!.FirstAppend);

            ParameterInfo[]? methodParameters = node.Method.GetParameters();

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                CurrentParameterType = methodParameters[i].ParameterType;
                CurrentParameterName = methodParameters[i].Name;

                Expression? arg = node.Arguments[i];

                if (CurrentClause == Clause.FromQuery ||
                    CurrentFunction == Function.Exists)
                {
                    CompileResult result = Compile(arg);
                    Sql.Append(result.Sql);
                }
                else
                {
                    Visit(arg);
                }
            }

            SetClause(node);
            PostAppendClause();

            CurrentParameterType = null;
            CurrentParameterName = null;

            if (node.Object != null &&
                node.Object.NodeType != ExpressionType.Parameter)
            {
                Visit(node.Object);
            }

            return node;
        }

        private void SetClause(MethodCallExpression node)
        {
            if (typeof(SqlFunctions).IsAssignableFrom(node.Method.DeclaringType))
            {
                if (!Enum.TryParse(node.Method.Name, out Function function))
                {
                    throw new QueryCompilerException($"Function {node.Method.Name} not supported");
                }

                CurrentFunction = function;
            }
            else
            {
                if (!Enum.TryParse(node.Method.Name, out Clause clause))
                {
                    throw new QueryCompilerException($"Clause {node.Method.Name} not supported");
                }

                CurrentClause = clause;
                CurrentFunction = null;
            }
        }
    }
}
