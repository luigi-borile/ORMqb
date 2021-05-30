using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        protected Type? QueryType { get; private set; }

        protected Clause CurrentClause
        {
            get
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(CurrentClause)} before calling {nameof(Compile)}");
                }

                return _context.CurrentClause;
            }
            set
            {
                if (_context is null)
                {
                    throw new InvalidOperationException($"Cannot use {nameof(CurrentClause)} before calling {nameof(Compile)}");
                }

                _context.CurrentClause = value;
            }
        }

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

            QueryType = queryParameter.Type.GenericTypeArguments[0];

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
    }
}
