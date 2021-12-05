using System;
using System.Collections.Generic;
using System.Reflection;

namespace ORMqb.Schema
{
    public class DbSchemaDefinition
    {
        internal DbSchemaDefinition(
            IReadOnlyList<DbObjectDefinition> dbObjects,
            IReadOnlyList<StoredProcedureDefinition> storedProcedures)
        {
            var objects = new Dictionary<Type, DbObjectDefinition>();
            var procedures = new Dictionary<Type, StoredProcedureDefinition>();

            if (dbObjects != null)
            {
                foreach (DbObjectDefinition obj in dbObjects)
                {
                    objects.Add(obj.Type, obj);
                }
            }

            if (storedProcedures != null)
            {
                foreach (StoredProcedureDefinition? obj in storedProcedures)
                {
                    procedures.Add(obj.Type, obj);
                }
            }

            DbObjects = objects;
            StoredProcedures = procedures;
        }

        public IReadOnlyDictionary<Type, DbObjectDefinition> DbObjects { get; }

        public IReadOnlyDictionary<Type, StoredProcedureDefinition> StoredProcedures { get; }

        public string GetTableSource(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (DbObjects.TryGetValue(type, out DbObjectDefinition definition))
            {
                return definition.Source;
            }
            return type.Name;
        }

        public string GetColumnName(MemberInfo member)
        {
            if (member is null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            if (DbObjects.TryGetValue(member.DeclaringType, out DbObjectDefinition definition) &&
                definition.Columns.TryGetValue(member.Name, out string column))
            {
                return column;
            }
            return member.Name;
        }
    }
}
