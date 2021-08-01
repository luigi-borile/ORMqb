using System;
using System.Collections.Generic;

namespace ORMqb.Schema
{
    public sealed class DbObjectDefinition
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal DbObjectDefinition(Type type)
#pragma warning restore CS8618 // DbObjectBuilder checks that source and columns are not empty
        {
            Type = type;
        }

        public string Source { get; internal set; }

        public IReadOnlyDictionary<string, string> Columns { get; internal set; }

        public Type Type { get; }
    }
}
