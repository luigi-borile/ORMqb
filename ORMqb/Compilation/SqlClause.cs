namespace ORMqb.Compilation
{
    public sealed record SqlClause
    {
        public SqlClause(
            ClauseType type,
            string sql,
            bool allowMultiple,
            bool isSubQuery,
            string? separator = null,
            string? pre = null,
            string? post = null)
        {
            (Type, Sql, AllowMultiple, IsSubQuery, Separator, Pre, Post) =
            (type, sql, allowMultiple, isSubQuery, separator, pre, post);
        }

        public ClauseType Type { get; }

        public string Sql { get; }

        public bool AllowMultiple { get; }

        public bool IsSubQuery { get; }

        public string? Separator { get; }

        public string? Pre { get; }
        public string? Post { get; }
    }
}
