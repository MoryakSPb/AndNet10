using AndNet.Manager.Database;
using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;
using AndNet.Manager.DocumentExecutor;
using AndNet.Manager.Shared.Enums;
using AndNet.Manager.Shared.Models.Documentation.Info.Decision.Player;
using AndNet.Migrator.AndNet7.AndNet7;
using AndNet.Migrator.AndNet7.AndNet7.Shared;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Hosting;

namespace AndNet.Migrator.AndNet7;

public class Migrator : BackgroundService
{
    private readonly ClanContext _clanContext;
    private readonly DatabaseContext _databaseContext;
    private readonly DocumentService _documentService;

    public Migrator(DatabaseContext databaseContext, ClanContext clanContext, DocumentService documentService)
    {
        _databaseContext = databaseContext;
        _clanContext = clanContext;
        _documentService = documentService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DbClanPlayer firstAdvisor = null!;

        foreach (ClanMember clanContextMember in _clanContext.Members
                     .Include(x => x.Awards)
                     .OrderByDescending(x => x.Rank)
                     .ThenByDescending(x => x.Awards.Count))
        {
            clanContextMember.JoinDate = clanContextMember.JoinDate;
            DbPlayer player;
            if (clanContextMember.Rank >= ClanMemberRankEnum.Neophyte)
            {
                player = new DbClanPlayer
                {
                    Status = PlayerStatus.Former,
                    JoinDate = clanContextMember.JoinDate,
                    OnReserve = clanContextMember.Department == ClanDepartmentEnum.Reserve,
                    Rank = clanContextMember.Rank switch
                    {
                        ClanMemberRankEnum.FirstAdvisor => PlayerRank.FirstAdvisor,
                        ClanMemberRankEnum.Advisor => PlayerRank.Advisor,
                        _ => PlayerRank.Neophyte
                    }
                };
                player.Status = PlayerStatus.Member;
            }
            else
            {
                player = new DbFormerClanPlayer
                {
                    Status = PlayerStatus.Former,
                    JoinDate = clanContextMember.JoinDate,
                    LeaveDate = DateTime.UtcNow,
                    Relationship = PlayerRelationship.Unknown,
                    RestorationAvailable = false,
                    LeaveReason = PlayerLeaveReason.Unknown
                };
            }

            player.DiscordId = clanContextMember.DiscordId;
            player.DetectionDate = clanContextMember.JoinDate;
            player.SteamId = clanContextMember.SteamId;
            player.Nickname = clanContextMember.Nickname;
            player.RealName = clanContextMember.RealName;
            if (clanContextMember.VkId is not null)
            {
                player.Contacts ??= new List<DbPlayerContact>();
                player.Contacts.Add(new()
                {
                    Type = "VK",
                    Value = clanContextMember.VkId.Value.ToString()
                });
            }

            if (player is DbClanPlayer { Rank: PlayerRank.FirstAdvisor } clanPlayer) firstAdvisor = clanPlayer;

            player.Awards ??= new List<DbAward>();
            await _databaseContext.Players.AddAsync(player, stoppingToken).ConfigureAwait(false);
            await _databaseContext.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
            foreach (ClanAward award in clanContextMember.Awards)
            {
                award.Date = award.Date;
                EntityEntry<DbDoc> document = await _databaseContext.Documents.AddAsync(
                    new()
                    {
                        Info = new DecisionCouncilPlayerAwardSheet
                        {
                            PlayerId = player.Id,
                            Action = DecisionCouncilPlayer.PlayerAction.Generic,
                            AwardType = award.Type switch
                            {
                                ClanAwardTypeEnum.None => AwardType.Copper,
                                ClanAwardTypeEnum.Bronze => AwardType.Bronze,
                                ClanAwardTypeEnum.Silver => AwardType.Silver,
                                ClanAwardTypeEnum.Gold => AwardType.Gold,
                                ClanAwardTypeEnum.Hero => AwardType.Hero,
                                _ => throw new ArgumentOutOfRangeException()
                            },
                            Description = award.Description ?? string.Empty,
                            PredeterminedIssueDate = award.Date < firstAdvisor.JoinDate ? new(2015, 1, 1) : award.Date,
                            MinYesVotesPercent = 0d
                        },
                        CreationDate = DateTime.UtcNow,
                        AuthorId = 1,
                        Author = firstAdvisor,
                        Body = new()
                        {
                            Body = @$"# Наградной лист

В процессе ввода в эксплуатацию системы управления привести награду, выданную или зарегистрированную в соответствии с более ранним уставом, к современному виду.
Процесс перевода проводится автоматически, отдельное голосование по наградному листу не проводилось.

Изначальные данные:

- Номер: `{award.Id}`;
- Тип: `{award.Type:D}`;
- Дата выдачи: {award.Date.ToShortDateString()};
- Описание: *{award.Description}*.
"
                        }
                    }, stoppingToken).ConfigureAwait(false);
                document.Entity.GenerateTitleFromBody();
                await _databaseContext.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
                await _documentService.AgreeExecuteAsync(document.Entity, firstAdvisor).ConfigureAwait(false);
                document.State = EntityState.Modified;
            }

            if (player is DbClanPlayer clan) clan.CalcPlayer();
            await _databaseContext.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
        }

        await _databaseContext.SaveChangesAsync(stoppingToken).ConfigureAwait(false);
        Console.WriteLine("Migration done!");
        Environment.Exit(0);
    }
}