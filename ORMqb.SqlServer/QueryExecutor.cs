using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ORMqb.Execution.SqlServer
{
    public class QueryExecutor : QueryExecutorBase, IDisposable
    {
        private readonly SqlConnection _connection;
        private SqlTransaction? _transaction;
        private byte _tranCount;
        private bool _disposedValue;

        public QueryExecutor(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connection = new SqlConnection(connectionString);
        }

        public override void BeginTransaction() =>
            BeginTransactionAsync().RunSynchronously();

        public override async Task BeginTransactionAsync()
        {
            await EnsureOpenedAsync().ConfigureAwait(false);

            if (_tranCount == 0)
            {
                _transaction = _connection.BeginTransaction();
            }
            _tranCount++;
        }

        public override void CommitTransaction()
        {
            if (_tranCount == 0)
            {
                throw new InvalidOperationException("No opened connection");
            }

            if (_tranCount == 1)
            {
                _transaction!.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
            _tranCount--;
        }

        public override void RollbackTransaction()
        {
            if (_tranCount == 0)
            {
                throw new InvalidOperationException("No opened connection");
            }

            if (_tranCount == 1)
            {
                _transaction!.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
            _tranCount--;
        }

        public override async Task<T> GetAsync<T>(CompileResult compileResult)
        {
            if (compileResult is null)
            {
                throw new ArgumentNullException(nameof(compileResult));
            }

            return await ExecConnectedAsync(async () =>
            {
                using SqlCommand command = GetCommand(compileResult);

                using SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false);

                MapEntity<T> mapFunc = GetMapFunc<T>(reader);

                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    return mapFunc(reader);
                }
                else
                {
                    return default;
                }
            }).ConfigureAwait(false);
        }

        public override async Task<IEnumerable<T>> GetManyAsync<T>(CompileResult compileResult)
        {
            if (compileResult is null)
            {
                throw new ArgumentNullException(nameof(compileResult));
            }

            return await ExecConnectedAsync(async () =>
            {
                using SqlCommand command = GetCommand(compileResult);

                using SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

                MapEntity<T> mapFunc = GetMapFunc<T>(reader);
                var result = new List<T>();

                await Task.Factory.StartNew(() =>
                {
                    while (reader.Read())
                    {
                        result.Add(mapFunc(reader));
                    }
                }, default, TaskCreationOptions.None, TaskScheduler.Current).ConfigureAwait(false);

                return result;
            }).ConfigureAwait(false);
        }

        public override async Task<int> ExecAsync(CompileResult compileResult)
        {
            if (compileResult is null)
            {
                throw new ArgumentNullException(nameof(compileResult));
            }

            return await ExecConnectedAsync(async () =>
            {
                using SqlCommand command = GetCommand(compileResult);
                command.CommandType = CommandType.StoredProcedure;

                int result = await command.ExecuteNonQueryAsync().ConfigureAwait(false);

                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    SqlParameter parameter = command.Parameters[i];
                    if (parameter.Direction == ParameterDirection.Output)
                    {
                        object? value = parameter.Value == DBNull.Value ? null : parameter.Value;
                        compileResult.Parameters![i].SetOutputValue?.Invoke(value);
                    }
                }

                return result;
            }).ConfigureAwait(false);
        }

        protected async Task<bool> EnsureOpenedAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync().ConfigureAwait(false);
                return true;
            }

            return false;
        }

        protected void EnsuredClosed()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        protected async Task<T> ExecConnectedAsync<T>(Func<Task<T>> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            bool opened = await EnsureOpenedAsync().ConfigureAwait(false);

            try
            {
                return await action().ConfigureAwait(false);
            }
            finally
            {
                if (opened)
                {
                    EnsuredClosed();
                }
            }
        }

        protected virtual SqlParameter MapParameter(QueryParameter parameter)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var sqlParameter = new SqlParameter()
            {
                ParameterName = parameter.Name,
                Direction = parameter.Direction
            };

            if (parameter.Direction == ParameterDirection.Input)
            {
                sqlParameter.Value = parameter.Value ?? DBNull.Value;
            }
            else
            {
                sqlParameter.SqlDbType = SqlDbType.VarChar;
                sqlParameter.Size = 255;
            }

            if (parameter.Size.HasValue)
            {
                sqlParameter.Size = parameter.Size.Value;
            }

            return sqlParameter;
        }

        [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Command text is created throught IQueryCompiler")]
        private SqlCommand GetCommand(CompileResult compileResult)
        {
            SqlCommand command = _connection.CreateCommand();
            command.CommandText = compileResult.Sql;

            if (compileResult.HasParameters)
            {
                foreach (QueryParameter parameter in compileResult.Parameters!)
                {
                    command.Parameters.Add(MapParameter(parameter));
                }
            }

            return command;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
