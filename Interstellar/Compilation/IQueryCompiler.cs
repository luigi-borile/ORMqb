using System.Linq.Expressions;

namespace Interstellar.Compilation
{
    public interface IQueryCompiler
    {
        CompileResult Compile(Expression query);
    }
}
