﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ORMqb.Schema
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
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (foreignKey is null)
            {
                throw new ArgumentNullException(nameof(foreignKey));
            }

            MemberInfo? propMember = property.GetMemberInfo();
            if (propMember is null)
            {
                throw new ArgumentException("Unexpected property expression format", nameof(property));
            }

            MemberInfo? fkMember = foreignKey.GetMemberInfo();
            if (fkMember is null)
            {
                throw new ArgumentException("Unexpected foreing key expression format", nameof(property));
            }

            string pField = propMember.Name;
            string fField = fkMember.Name;

            if (_fields.ContainsKey(pField))
            {
                throw new DbSchemaDefinitionException("Foreing key column already defined");
            }

            _fields.Add(pField, fField);

            return this;
        }
    }
}