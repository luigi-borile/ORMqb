using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Interstellar.Execution
{
    public abstract class QueryExecutorBase : IQueryExecutor
    {
        public abstract void BeginTransaction();
        public abstract Task BeginTransactionAsync();
        public abstract void CommitTransaction();
        public abstract void RollbackTransaction();

        public abstract Task<TResult> GetAsync<TResult>(CompileResult compileResult);
        public abstract Task<IEnumerable<TResult>> GetManyAsync<TResult>(CompileResult compileResult);

        protected static Func<IDataReader, TResult> GetMapFunc<TResult>(IDataReader dataReader)
        {
            var exps = new List<Expression>();

            ParameterExpression paramExp = Expression.Parameter(typeof(IDataRecord), "o7thDR");

            ParameterExpression targetExp = Expression.Variable(typeof(TResult));
            exps.Add(Expression.Assign(targetExp, Expression.New(targetExp.Type)));

            //does int based lookup
            PropertyInfo indexerInfo = typeof(IDataRecord).GetProperty("Item", new[] { typeof(int) });

            var columnNames = Enumerable.Range(0, dataReader.FieldCount)
                .Select(i => new { i, name = dataReader.GetName(i) })
                .ToArray();

            foreach (var column in columnNames)
            {
                PropertyInfo property = targetExp.Type.GetProperty(
                    column.name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null)
                {
                    continue;
                }

                ConstantExpression columnNameExp = Expression.Constant(column.i);
                IndexExpression propertyExp = Expression.MakeIndex(
                    paramExp, indexerInfo, new[] { columnNameExp });
                UnaryExpression convertExp = Expression.Convert(propertyExp, property.PropertyType);
                BinaryExpression bindExp = Expression.Assign(
                    Expression.Property(targetExp, property), convertExp);
                exps.Add(bindExp);
            }

            exps.Add(targetExp);
            return Expression.Lambda<Func<IDataReader, TResult>>(
                Expression.Block(new[] { targetExp }, exps), paramExp).Compile();
        }
    }
}
