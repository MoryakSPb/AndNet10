using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// RUM index extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class NpgsqlRumServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for RUM index support in the Npgsql provider for Entity Framework.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddEntityFrameworkNpgsqlRum(
            this IServiceCollection serviceCollection)
        {
            /*new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAddProviderSpecificServices(
                    x => x.TryAddSingletonEnumerable<IMethodCallTranslatorPlugin, NpgsqlRumMethodCallTranslatorPlugin>());*/

            return serviceCollection;
        }
    }
}
