using System.Text.Json.Serialization;
using AndNet.Manager.Shared.Enums;

namespace AndNet.Manager.Shared.Models;

public record Award(int Id, bool IsMarkedForDelete, uint Version, AwardType AwardType, DateTime IssueDate, int PlayerId,
    int? IssuerId, string Description, int DecisionId, double CurrentScore);

public record Expedition(int Id, bool IsMarkedForDelete, uint Version,
    DateTime StartDate, DateTime EndDate, ulong? DiscordRoleId, int CommanderId, bool IsActive,
    int[]? Players = null);

public record PlayerContact
    (bool IsMarkedForDelete, uint Version, int PlayerId, string Type, string Value);

[JsonPolymorphic(TypeDiscriminatorPropertyName = null,
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
[JsonDerivedType(typeof(ExternalPlayer), (int)PlayerStatus.External)]
[JsonDerivedType(typeof(FormerClanPlayer), (int)PlayerStatus.Former)]
[JsonDerivedType(typeof(ClanPlayer), (int)PlayerStatus.Member)]
public record Player(int Id, uint Version, PlayerStatus Status, string Nickname, string FullNickname,
    string? RealName, ulong? DiscordId, ulong? SteamId, DateTime DetectionDate, string? TimeZone)
{
    public override string ToString()
    {
        string result = Nickname;
        if (RealName is not null) result += $" ({RealName})";
        return result;
    }
}

[JsonDerivedType(typeof(ExternalPlayer), (int)PlayerStatus.External)]
public record ExternalPlayer(int Id, uint Version, PlayerStatus Status, string Nickname, string FullNickname,
    string? RealName, ulong? DiscordId, ulong? SteamId, DateTime DetectionDate, string? TimeZone,
    PlayerRelationship Relationship) :
    Player(Id, Version, Status, Nickname, FullNickname, RealName, DiscordId, SteamId, DetectionDate, TimeZone)
{
    public override string ToString()
    {
        return base.ToString();
    }
}

[JsonDerivedType(typeof(FormerClanPlayer), (int)PlayerStatus.Former)]
public record FormerClanPlayer(int Id, uint Version, PlayerStatus Status, string Nickname, string FullNickname,
    string? RealName, ulong? DiscordId, ulong? SteamId, DateTime DetectionDate, string? TimeZone,
    PlayerRelationship Relationship,
    DateTime JoinDate, DateTime LeaveDate, PlayerLeaveReason LeaveReason) : ExternalPlayer(Id,
    Version, Status, Nickname, FullNickname, RealName, DiscordId, SteamId, DetectionDate, TimeZone, Relationship)
{
    public override string ToString()
    {
        return base.ToString();
    }
}

[JsonDerivedType(typeof(ClanPlayer), (int)PlayerStatus.Member)]
public record ClanPlayer(int Id, uint Version, PlayerStatus Status, string Nickname, string FullNickname,
    string? RealName, ulong? DiscordId, ulong? SteamId, DateTime DetectionDate, string? TimeZone, DateTime JoinDate,
    PlayerRank Rank,
    double Score, bool OnReserve) : Player(Id, Version, Status, Nickname, FullNickname, RealName, DiscordId,
    SteamId, DetectionDate.Date, TimeZone)
{
    public override string ToString()
    {
        return $"[{RankRules.Icons[Rank]}] {base.ToString()}";
    }
}