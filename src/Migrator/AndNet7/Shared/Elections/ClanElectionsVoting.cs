using System.Text.Json.Serialization;
using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared.Elections;

public class ClanElectionsVoting
{
    [JsonIgnore]
    public int ElectionsId { get; set; }

    [JsonIgnore]
    public virtual ClanElections Elections { get; set; } = null!;

    public virtual ClanDepartmentEnum Department { get; set; }

    public int AgainstAll { get; set; } = 0;
    public virtual IList<ClanElectionsMember> Results { get; set; } = null!;

    [JsonIgnore]
    public int VotesCount => Results.Count(x => x.Votes is not null);
}