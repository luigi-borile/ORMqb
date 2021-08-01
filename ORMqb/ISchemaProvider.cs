using ORMqb.Schema;

namespace ORMqb
{
    public interface ISchemaProvider
    {
        DbSchemaDefinition DbSchema { get; }
    }
}