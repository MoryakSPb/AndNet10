using NpgsqlTypes;

namespace AndNet.Manager.Database;

public static class Extensions
{
    public static float Distance(this NpgsqlTsVector vector, NpgsqlTsQuery query)
    {
        throw new InvalidOperationException();
    }
}