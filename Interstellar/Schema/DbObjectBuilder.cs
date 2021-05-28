using System;
using System.Collections.Generic;
using System.Linq;
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
            IEnumerable<string> missingFields = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                .Where(p => !_columns.ContainsKey(p.Name))
                .Select(p => p.Name);

            foreach (string field in missingFields)
            {
                _columns.Add(field, field);
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
            _columns.Add(property.GetMember().Name, name);

            return this;
        }
    }
}
