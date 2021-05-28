using System;

namespace Interstellar.Schema
{

    [Serializable]
    public class DbSchemaDefinitionException : Exception
    {
        public DbSchemaDefinitionException() { }
        public DbSchemaDefinitionException(string message) : base(message) { }
        public DbSchemaDefinitionException(string message, Exception inner) : base(message, inner) { }
        protected DbSchemaDefinitionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
