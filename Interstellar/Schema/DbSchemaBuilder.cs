using System;
using System.Collections.Generic;

namespace Interstellar.Schema
{
    public sealed class DbSchemaBuilder : ISchemaProvider
    {
        private readonly List<DbObjectDefinition> _dbObjects;
        private readonly List<ForeignKeyDefinition> _foreignKeys;
        private readonly Lazy<DbSchemaDefinition> _schemaDefinition;

        public DbSchemaBuilder()
        {
            _dbObjects = new List<DbObjectDefinition>();
            _foreignKeys = new List<ForeignKeyDefinition>();
            _schemaDefinition = new Lazy<DbSchemaDefinition>(() =>
                new DbSchemaDefinition(_dbObjects, _foreignKeys));
        }

        public DbSchemaDefinition DbSchema =>
            _schemaDefinition.Value;

        public DbSchemaBuilder SchemaFor<T>() =>
            SchemaFor<T>(buildSchema: null);

        public DbSchemaBuilder SchemaFor<T>(Action<DbObjectBuilder<T>> buildSchema)
        {
            var builder = new DbObjectBuilder<T>();
            buildSchema?.Invoke(builder);

            _dbObjects.Add(builder.Build());

            return this;
        }

        public DbSchemaBuilder ForeignKey<TPrimaryTable, TForeignTable>(Action<ForeignKeyBuilder<TPrimaryTable, TForeignTable>> buildForeignKey)
        {
            var builder = new ForeignKeyBuilder<TPrimaryTable, TForeignTable>();
            buildForeignKey?.Invoke(builder);

            _foreignKeys.Add(builder.Build());

            return this;
        }
    }
}
