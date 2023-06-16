using System.Text.Json.Serialization;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Programs;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared;

public class DiscordChannelMetadata : IComparable<DiscordChannelMetadata>, IComparable
{
    private ulong _discordId;

    public ulong DiscordId
    {
        get => _discordId;
        set
        {
            _discordId = value;
            if (DepartmentsPermissions is null) return;
            foreach (DiscordDepartmentPermissions departmentsPermission in DepartmentsPermissions)
                departmentsPermission.ChannelId = value;
        }
    }

    public string Name { get; set; }
    public int? CategoryPosition { get; set; }
    public int ChannelPosition { get; set; }

    [JsonIgnore]
    public virtual DiscordChannelCategory Category { get; set; }

    public DiscordChannelTypeEnum Type { get; set; }
    public DiscordPermissionsFlags EveryonePermissions { get; set; }
    public DiscordPermissionsFlags MemberPermissions { get; set; }
    public DiscordPermissionsFlags AdvisorPermissions { get; set; }

    public virtual IList<DiscordDepartmentPermissions> DepartmentsPermissions { get; set; }

    public int? DruzhinaId { get; set; }

    [JsonIgnore]
    public virtual ClanDruzhina Druzhina { get; set; }

    public int? ProgramId { get; set; }

    [JsonIgnore]
    public virtual ClanProgram Program { get; set; }

    public int CompareTo(object obj)
    {
        if (obj is null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is DiscordChannelMetadata other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(DiscordChannelMetadata)}");
    }

    public int CompareTo(DiscordChannelMetadata other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        if (CategoryPosition is null) return other.CategoryPosition is null ? 0 : -1;
        if (other.CategoryPosition is null) return 1;
        int categoryCompare = CategoryPosition.Value.CompareTo(other.CategoryPosition.Value);
        return categoryCompare == 0 ? ChannelPosition.CompareTo(other.ChannelPosition) : categoryCompare;
    }

    /*public IEnumerable<Overwrite> ToOverwrites(DiscordRoleManager roleManager, IReadOnlyDictionary<ClanDepartmentEnum, ClanMember> advisors)
    {
        yield return new Overwrite(roleManager.EveryoneRole.Id, PermissionTarget.Role, EveryonePermissions.ToOverwritePermissions());
        yield return new Overwrite(roleManager.DefaultRole.Id, PermissionTarget.Role, MemberPermissions.ToOverwritePermissions());
        yield return new Overwrite(roleManager.AdvisorRole.Id, PermissionTarget.Role, AdvisorPermissions.ToOverwritePermissions());
        foreach (DiscordDepartmentPermissions departmentPermission in DepartmentsPermissions) yield return new Overwrite(roleManager.DepartmentRoles[departmentPermission.Department].Id, PermissionTarget.Role, departmentPermission.Permissions.ToOverwritePermissions());

        if (DruzhinaId is null && ProgramId is null) yield break;
        if (DruzhinaId is not null)
        {
            if (ProgramId is not null) throw new ArgumentException();
            foreach (ClanDruzhinaMember druzhinaMember in Druzhina.ActiveMembers)
            {
                DiscordPermissionsFlags permissions = druzhinaMember.Position switch
                {
                    ClanDruzhinaPositionEnum.None => DiscordPermissionsFlags.None,
                    ClanDruzhinaPositionEnum.Troop => DiscordPermissionsFlags.Write,
                    ClanDruzhinaPositionEnum.Lieutenant => DiscordPermissionsFlags.Priority,
                    ClanDruzhinaPositionEnum.Captain => DiscordPermissionsFlags.Moderator,
                    _ => throw new ArgumentOutOfRangeException(),
                };
                yield return new Overwrite(druzhinaMember.Member.DiscordId, PermissionTarget.User, permissions.ToOverwritePermissions());
            }

            ClanMember advisor = advisors?[Druzhina.Department];
            if (advisor is not null) yield return new Overwrite(advisor.DiscordId, PermissionTarget.User, DiscordPermissionsFlags.Moderator.ToOverwritePermissions());
        }
        else
        {
            if (Program is null) throw new ArgumentNullException();
            foreach (ClanMember member in Program.Members)
            {
                if (member.Druzhina is not null && member.Rank == ClanMemberRankEnum.Captain)
                {
                    foreach (ClanDruzhinaMember activeMember in member.Druzhina.ActiveMembers.Where(x => x.Member.Programs.All(y => y.Id != ProgramId)))
                    {
                        yield return new Overwrite(activeMember.Member.DiscordId, PermissionTarget.User, DiscordPermissionsFlags.Write.ToOverwritePermissions());
                    }
                }
                DiscordPermissionsFlags permissions = (Program.Initiator is not null && member.Id == Program.Initiator.Id) ? DiscordPermissionsFlags.Moderator : DiscordPermissionsFlags.Write;
                yield return new Overwrite(member.DiscordId, PermissionTarget.User, permissions.ToOverwritePermissions());
            }
        }
    }*/

    public static bool operator <(DiscordChannelMetadata left, DiscordChannelMetadata right)
    {
        return Comparer<DiscordChannelMetadata>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(DiscordChannelMetadata left, DiscordChannelMetadata right)
    {
        return Comparer<DiscordChannelMetadata>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(DiscordChannelMetadata left, DiscordChannelMetadata right)
    {
        return Comparer<DiscordChannelMetadata>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(DiscordChannelMetadata left, DiscordChannelMetadata right)
    {
        return Comparer<DiscordChannelMetadata>.Default.Compare(left, right) >= 0;
    }
}