namespace ORMqb.Compilation
{
    public sealed record SqlFunction
    {
        public SqlFunction(string sql, bool isSubQuery)
        {
            (Sql, IsSubQuery) = (sql, isSubQuery);
        }

        public string Sql { get; }

        public bool IsSubQuery { get; }
    }
}
