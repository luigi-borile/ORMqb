using System;
using System.Collections.Generic;

namespace Interstellar.Schema
{
    public class DbSchemaDefinition
    {
        internal DbSchemaDefinition(IReadOnlyList<DbObjectDefinition> dbObjects, IReadOnlyList<ForeignKeyDefinition> foreignKeys)
        {
            var objects = new Dictionary<Type, DbObjectDefinition>();
            foreach (DbObjectDefinition obj in dbObjects)
            {
                objects.Add(obj.Type, obj);
            }

            DbObjects = objects;
            ForeignKeys = foreignKeys;
        }

        public IReadOnlyDictionary<Type, DbObjectDefinition> DbObjects { get; }

        public IReadOnlyList<ForeignKeyDefinition> ForeignKeys { get; }

        public DbObjectDefinition GetDefinition(Type type)
        {
            if (!TryGetDefinition(type, out DbObjectDefinition definition))
            {
                throw new InvalidOperationException($"No definition found for type {type}");
            }

            return definition;
        }

        public bool TryGetDefinition(Type type, out DbObjectDefinition definition) =>
            DbObjects.TryGetValue(type, out definition);
    }
}
