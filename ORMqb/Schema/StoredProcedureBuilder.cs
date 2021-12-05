using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace ORMqb.Schema
{
    public sealed class StoredProcedureBuilder<T>
    {
        private readonly StoredProcedureDefinition _definition;
        private readonly Dictionary<string, StoredProcedureParameter> _parameters;

        public StoredProcedureBuilder()
        {
            _definition = new StoredProcedureDefinition(typeof(T));
            _parameters = new Dictionary<string, StoredProcedureParameter>();
        }

        public StoredProcedureDefinition Build()
        {
            if (_definition.Source is null)
            {
                throw new DbObjectDefinitionException($"No name defined for stored procedure {typeof(T).Name}");
            }
            if (_parameters.Count == 0)
            {
                throw new DbObjectDefinitionException($"No parameters defined for stored procedure {typeof(T).Name}");
            }

            _definition.Parameters = _parameters;
            return _definition;
        }

        public StoredProcedureBuilder<T> Source(string name)
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

        public StoredProcedureBuilder<T> Parameter(Expression<Func<T, object>> property, string name)
        {
            AddParameter(property, name, ParameterDirection.Input);

            return this;
        }

        public StoredProcedureBuilder<T> OutputParameter(Expression<Func<T, object>> property, string name)
        {
            AddParameter(property, name, ParameterDirection.Output);

            return this;
        }

        private void AddParameter(Expression<Func<T, object>> property, string name, in ParameterDirection direction)
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            MemberInfo? member = property.GetMemberInfo();
            if (member is null)
            {
                throw new ArgumentException("Unexpected property expression format", nameof(property));
            }

            _parameters.Add(member.Name, new StoredProcedureParameter(name, direction));
        }
    }
}
