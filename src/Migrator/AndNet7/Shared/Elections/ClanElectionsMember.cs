using System.Text.Json.Serialization;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared.Elections;

public class ClanElectionsMember : IEquatable<ClanElectionsMember>, IComparable<ClanElectionsMember>
{
    [JsonIgnore]
    public int ElectionsId { get; set; }

    [JsonIgnore]
    public ClanDepartmentEnum Department { get; set; }

    [JsonIgnore]
    public virtual ClanElectionsVoting Voting { get; set; } = null!;

    [JsonIgnore]
    public int MemberId { get; set; }

    public virtual ClanMember Member { get; set; } = null!;
    public int? Votes { get; set; }
    public Guid VoterId { get; set; }
    public bool Voted { get; set; }

    public int CompareTo(ClanElectionsMember? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Nullable.Compare(Votes, other.Votes);
    }

    public bool Equals(ClanElectionsMember? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ElectionsId == other.ElectionsId && Department == other.Department && MemberId == other.MemberId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ClanElectionsMember)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ElectionsId, (int)Department, MemberId);
    }
}