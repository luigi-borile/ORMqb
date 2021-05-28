using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Interstellar.Schema
{
    public sealed class ForeignKeyBuilder<TPrimary, TForeign>
    {
        private readonly Dictionary<string, string> _fields;

        public ForeignKeyBuilder()
        {
            _fields = new Dictionary<string, string>();
        }

        public ForeignKeyDefinition Build() =>
            new ForeignKeyDefinition(typeof(TPrimary), typeof(TForeign), _fields);

        public ForeignKeyBuilder<TPrimary, TForeign> Column(Expression<Func<TPrimary, object>> property, Expression<Func<TForeign, object>> foreignKey)
        {
            string pField = property.GetMember().Name;
            string fField = foreignKey.GetMember().Name;

            if (_fields.ContainsKey(pField))
            {
                throw new DbSchemaDefinitionException("Foreing key column already defined");
            }

            _fields.Add(pField, fField);

            return this;
        }
    }
}
