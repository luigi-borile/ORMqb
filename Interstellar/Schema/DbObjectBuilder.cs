using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Interstellar.Schema
{
    public sealed class DbObjectBuilder<T>
    {
        private readonly DbObjectDefinition _definition;
        private readonly Dictionary<string, string> _columns;

        public DbObjectBuilder()
        {
            _definition = new DbObjectDefinition(typeof(T));
            _columns = new Dictionary<string, string>();
        }

        public DbObjectDefinition Build()
        {
            if (_definition.Source is null)
            {
                throw new DbObjectDefinitionException($"No source defined for type {typeof(T).Name}");
            }
            if (_columns.Count == 0)
            {
                throw new DbObjectDefinitionException($"No columns defined for type {typeof(T).Name}");
            }

            _definition.Columns = _columns;
            return _definition;
        }

        public DbObjectBuilder<T> Source(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!string.IsNullOrWhiteSpace(_definition.Source))
            {
                throw new DbObjectDefinitionException("Source already defined");
            }

            _definition.Source = name;
            return this;
        }

        public DbObjectBuilder<T> Column(Expression<Func<T, object>> property, string name)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            MemberInfo? member = property.GetMember();
            if (member is null)
            {
                throw new ArgumentException("Unexpected property expression format", nameof(property));
            }

            _columns.Add(member.Name, name);

            return this;
        }
    }
}
