using System;

namespace ORMqb.Schema
{

    [Serializable]
    public class DbObjectDefinitionException : Exception
    {
        public DbObjectDefinitionException() { }
        public DbObjectDefinitionException(string message) : base(message) { }
        public DbObjectDefinitionException(string message, Exception inner) : base(message, inner) { }
        protected DbObjectDefinitionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
