using System;
using System.Collections.Generic;

namespace ORMqb.Schema
{
    public sealed class DbSchemaBuilder : ISchemaProvider
    {
        private readonly List<DbObjectDefinition> _dbObjects;
        private readonly List<ForeignKeyDefinition> _foreignKeys;
        private readonly List<StoredProcedureDefinition> _storedProcedures;
        private readonly Lazy<DbSchemaDefinition> _schemaDefinition;

        public DbSchemaBuilder()
        {
            _dbObjects = new List<DbObjectDefinition>();
            _foreignKeys = new List<ForeignKeyDefinition>();
            _storedProcedures = new List<StoredProcedureDefinition>();

            _schemaDefinition = new Lazy<DbSchemaDefinition>(() =>
                new DbSchemaDefinition(_dbObjects, _foreignKeys, _storedProcedures));
        }

        public DbSchemaDefinition DbSchema =>
            _schemaDefinition.Value;

        public DbSchemaBuilder SchemaFor<T>(Action<DbObjectBuilder<T>> buildSchema)
        {
            if (buildSchema is null)
            {
                throw new ArgumentNullException(nameof(buildSchema));
            }

            var builder = new DbObjectBuilder<T>();
            buildSchema(builder);

            _dbObjects.Add(builder.Build());

            return this;
        }

        public DbSchemaBuilder ForeignKey<TPrimaryTable, TForeignTable>(Action<ForeignKeyBuilder<TPrimaryTable, TForeignTable>> buildForeignKey)
        {
            if (buildForeignKey is null)
            {
                throw new ArgumentNullException(nameof(buildForeignKey));
            }

            var builder = new ForeignKeyBuilder<TPrimaryTable, TForeignTable>();
            buildForeignKey(builder);

            _foreignKeys.Add(builder.Build());

            return this;
        }

        public DbSchemaBuilder StoredProcedure<T>(Action<StoredProcedureBuilder<T>> buildSchema)
        {
            if (buildSchema is null)
            {
                throw new ArgumentNullException(nameof(buildSchema));
            }

            var builder = new StoredProcedureBuilder<T>();
            buildSchema(builder);

            _storedProcedures.Add(builder.Build());

            return this;
        }
    }
}
