using Interstellar.Schema;

namespace Interstellar
{
    public interface ISchemaProvider
    {
        DbSchemaDefinition DbSchema { get; }
    }
}