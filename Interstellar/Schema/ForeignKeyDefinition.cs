using System;
using System.Collections.Generic;

namespace Interstellar.Schema
{
    public sealed record ForeignKeyDefinition
    {
        public ForeignKeyDefinition(Type primaryTable, Type foreignTable, IReadOnlyDictionary<string, string> fields)
        {
            (PrimaryTable, ForeingTable, Fields) = (primaryTable, foreignTable, fields);
        }

        public Type PrimaryTable { get; }

        public Type ForeingTable { get; }

        public IReadOnlyDictionary<string, string> Fields { get; }
    }
}
