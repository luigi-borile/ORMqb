using System;

namespace Interstellar.Compilation
{

    [Serializable]
    public class QueryCompilerException : Exception
    {
        public QueryCompilerException() { }
        public QueryCompilerException(string message) : base(message) { }
        public QueryCompilerException(string message, Exception inner) : base(message, inner) { }
        protected QueryCompilerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
