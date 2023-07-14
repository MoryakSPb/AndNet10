using System.Collections.Immutable;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.DocumentExecutor.Strategy;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Directive;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Expedition;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using Microsoft.Extensions.DependencyInjection;

namespace AndNet.Manager.DocumentExecutor;

public class DocumentService
{
    private static readonly ImmutableDictionary<Type, Type> _strategies = ImmutableDictionary<Type, Type>.Empty
        .Add(typeof(DecisionCouncilPlayerAwardSheet), typeof(DecisionCouncilPlayerAwardSheetStrategy))
        .Add(typeof(DecisionCouncilDirective), typeof(DecisionCouncilDirectiveStrategy))
        .Add(typeof(DecisionCouncilDirectiveChange), typeof(DecisionCouncilDirectiveStrategy))
        .Add(typeof(DecisionCouncilPlayerAcceptApplication), typeof(DecisionCouncilPlayerAcceptApplicationStrategy))
        .Add(typeof(DecisionCouncilExpedition), typeof(DecisionCouncilExpeditionStrategy))
        .Add(typeof(DecisionCouncilExpeditionClose), typeof(DecisionCouncilExpeditionStrategy))
        .Add(typeof(DecisionCouncilExpeditionCreate), typeof(DecisionCouncilExpeditionStrategy))
        .Add(typeof(DecisionCouncilExpeditionPlayer), typeof(DecisionCouncilExpeditionStrategy))
        .Add(typeof(DecisionCouncilExpeditionProlongation), typeof(DecisionCouncilExpeditionStrategy))
        .Add(typeof(DecisionCouncilPlayer), typeof(DecisionCouncilPlayerStrategy))
        .Add(typeof(DecisionCouncilPlayerChange), typeof(DecisionCouncilPlayerStrategy))
        .Add(typeof(DecisionCouncilPlayerKick), typeof(DecisionCouncilPlayerStrategy));

    private readonly IServiceScopeFactory _scopeFactory;

    public DocumentService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public static bool ExecuteSupported(Type infoType)
    {
        return _strategies.Keys.Any(x => x == infoType);
    }

    public async Task AgreeExecuteAsync(DbDoc doc, DbClanPlayer signer)
    {
        if (doc.Info is not Decision info) return;
        if (signer.Rank < PlayerRank.Advisor) throw new ArgumentOutOfRangeException(nameof(signer));
        if (!info.IsAgreeAvailable) throw new InvalidOperationException();

        info.ExecuteDate = DateTime.UtcNow;
        info.ExecutorId = signer.Id;

        Type? strategyType = _strategies.GetValueOrDefault(info.GetType());
        if (strategyType is not null)
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            DocStrategy strategy = (DocStrategy)scope.ServiceProvider.GetRequiredService(strategyType);
            await strategy.Execute(doc, signer).ConfigureAwait(false);
        }

        info.IsExecuted = true;
        doc.Info = info;
    }

    public void DeclineExecuteAsync(DbDoc doc, DbClanPlayer signer)
    {
        if (doc.Info is not Decision info) return;
        if (signer.Rank < PlayerRank.Advisor) throw new ArgumentOutOfRangeException(nameof(signer));
        if (info.IsExecuted.HasValue) throw new InvalidOperationException();
        if (!info.IsDeclineAvailable) throw new InvalidOperationException();
        info.ExecuteDate = DateTime.UtcNow;
        info.ExecutorId = signer.Id;
        info.IsExecuted = false;
    }
}