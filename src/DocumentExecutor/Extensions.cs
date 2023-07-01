using AndNet.Manager.DocumentExecutor.Strategy;
using Microsoft.Extensions.DependencyInjection;

namespace AndNet.Manager.DocumentExecutor;

public static class Extensions
{
    public static IServiceCollection AddDocumentService(this IServiceCollection services)
    {
        services.AddTransient<DecisionCouncilDirectiveStrategy>();
        services.AddTransient<DecisionCouncilExpeditionStrategy>();
        services.AddTransient<DecisionCouncilPlayerAcceptApplicationStrategy>();
        services.AddTransient<DecisionCouncilPlayerAwardSheetStrategy>();
        services.AddTransient<DecisionCouncilPlayerStrategy>();

        services.AddSingleton<DocumentService>();

        return services;
    }
}