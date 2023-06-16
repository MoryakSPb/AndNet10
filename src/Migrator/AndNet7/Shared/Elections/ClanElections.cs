using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared.Elections;

public class ClanElections
{
    public int Id { get; set; }
    public DateTime AdvisorsStartDate { get; set; }
    public ClanElectionsStageEnum Stage { get; set; }

    public virtual IList<ClanElectionsVoting> Voting { get; set; } = null!;
}