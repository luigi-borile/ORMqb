using System.Data;

namespace ORMqb.Schema
{
    public sealed class StoredProcedureParameter
    {
        public StoredProcedureParameter(string name, ParameterDirection direction)
        {
            Name = name;
            Direction = direction;
        }

        public string Name { get; }

        public ParameterDirection Direction { get; }
    }
}
