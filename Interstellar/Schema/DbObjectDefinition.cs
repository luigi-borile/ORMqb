using System;
using System.Collections.Generic;

namespace Interstellar.Schema
{
    public sealed class DbObjectDefinition
    {
        internal DbObjectDefinition(Type type)
        {
            Type = type;
        }

        public string Source { get; internal set; }

        public IReadOnlyDictionary<string, string> Columns { get; internal set; }

        public Type Type { get; }
    }
}
