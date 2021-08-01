using System.Linq.Expressions;

namespace ORMqb.Compilation
{
    public interface IQueryCompiler
    {
        CompileResult Compile(Expression query);
        CompileResult Compile<T>(T storedProcedure);
    }
}
