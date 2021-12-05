using System;
using System.Collections.Generic;

namespace ORMqb.Schema
{
    public sealed class DbSchemaBuilder : ISchemaProvider
    {
        private readonly List<DbObjectDefinition> _dbObjects;
        private readonly List<StoredProcedureDefinition> _storedProcedures;
        private readonly Lazy<DbSchemaDefinition> _schemaDefinition;

        public DbSchemaBuilder()
        {
            _dbObjects = new List<DbObjectDefinition>();
            _storedProcedures = new List<StoredProcedureDefinition>();

            _schemaDefinition = new Lazy<DbSchemaDefinition>(() =>
                new DbSchemaDefinition(_dbObjects, _storedProcedures));
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
