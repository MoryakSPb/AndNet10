using Microsoft.EntityFrameworkCore.Infrastructure;

using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// RUM index specific extension methods for <see cref="NpgsqlDbContextOptionsBuilder"/>.
    /// </summary>
    public static class NpgsqlRumDbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Enable RUM index methods and operators.
        /// </summary>
        /// <param name="optionsBuilder">The build being used to configure Postgres.</param>
        /// <returns>The options builder so that further configuration can be chained.</returns>
        public static DbContextOptionsBuilder UseRum(
            this DbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsBuilder = ((IDbContextOptionsBuilderInfrastructure)optionsBuilder);
            coreOptionsBuilder.AddOrUpdateExtension(new NpgsqlRumOptionsExtension());
            return optionsBuilder;
        }
    }
}
