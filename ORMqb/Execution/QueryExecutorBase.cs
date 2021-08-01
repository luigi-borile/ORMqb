using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace ORMqb.Execution
{
    public abstract class QueryExecutorBase : IQueryExecutor
    {
        private static readonly Dictionary<Type, Delegate> _cachedMappers = new();

        public abstract void BeginTransaction();
        public abstract Task BeginTransactionAsync();
        public abstract void CommitTransaction();
        public abstract void RollbackTransaction();

        public abstract Task<TResult> GetAsync<TResult>(CompileResult compileResult);
        public abstract Task<IEnumerable<TResult>> GetManyAsync<TResult>(CompileResult compileResult);
        public abstract Task<int> ExecAsync(CompileResult compileResult);

        protected delegate T MapEntity<T>(DbDataReader dataReader);

        protected static MapEntity<T> GetMapFunc<T>(DbDataReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            Type entityType = typeof(T);
            if (_cachedMappers.TryGetValue(entityType, out Delegate mapFunction))
            {
                return (MapEntity<T>)mapFunction;
            }
            else
            {
                MapEntity<T> newMapFunction = CreateMap<T>(reader);
                _cachedMappers.Add(entityType, newMapFunction);
                return newMapFunction;
            }
        }

        private static MapEntity<T> CreateMap<T>(DbDataReader reader)
        {
            Type entityType = typeof(T);
            Type objectType = typeof(object);
            Type readerType = typeof(DbDataReader);
            var dm = new DynamicMethod("CreateMap" + Guid.NewGuid().ToString(), entityType, new Type[] { readerType }, true);

            ILGenerator il = dm.GetILGenerator();

            string[] readerColumns = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                readerColumns[i] = reader.GetName(i);
            }

            var properties = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && readerColumns.Contains(p.Name))
                .ToList();

            LocalBuilder post = il.DeclareLocal(entityType);
            il.Emit(OpCodes.Newobj, entityType.GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc, post);

            LocalBuilder dbnull = il.DeclareLocal(typeof(DBNull));

            FieldInfo fieldInfo = typeof(DBNull).GetField("Value", BindingFlags.Static | BindingFlags.Public);
            il.Emit(OpCodes.Ldsfld, fieldInfo);
            il.Emit(OpCodes.Stloc, dbnull);

            MethodInfo getItem = typeof(DbDataReader).GetMethod("get_Item", new Type[] { typeof(int) });

            int index = 0;

            Label isNullLabel;
            Label finishLabel;

            foreach (PropertyInfo item in properties)
            {
                isNullLabel = il.DefineLabel();
                finishLabel = il.DefineLabel();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, index++);
                il.Emit(OpCodes.Callvirt, getItem);

                // we need this on the stack 
                LocalBuilder tmp = il.DeclareLocal(objectType);
                il.Emit(OpCodes.Stloc, tmp);
                il.Emit(OpCodes.Ldloc, tmp);

                il.Emit(OpCodes.Ldloc, dbnull);
                il.Emit(OpCodes.Beq, isNullLabel);

                il.Emit(OpCodes.Ldloc, post);
                il.Emit(OpCodes.Ldloc, tmp);
                il.Emit(OpCodes.Unbox_Any, item.PropertyType);
                il.Emit(OpCodes.Callvirt, item.GetSetMethod());

                il.Emit(OpCodes.Br, finishLabel);
                il.MarkLabel(isNullLabel);
                il.MarkLabel(finishLabel);
            }

            il.Emit(OpCodes.Ldloc, post);
            il.Emit(OpCodes.Ret);

            return (MapEntity<T>)dm.CreateDelegate(typeof(MapEntity<T>));
        }
    }
}
