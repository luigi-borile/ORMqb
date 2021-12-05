using System.Linq.Expressions;

namespace ORMqb.Compilation
{
    public interface IQueryCompiler
    {
        CompileResult Compile(Expression query);
        CompileResult CompileSp<T>(T storedProcedure);
    }
}
