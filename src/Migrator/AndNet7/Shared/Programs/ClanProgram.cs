namespace AndNet.Migrator.AndNet7.AndNet7.Shared.Programs;

public class ClanProgram
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual IList<ClanProgramTask> Tasks { get; set; } = null!;
    public virtual ClanMember Initiator { get; set; } = null!;
    public virtual IList<ClanMember> Members { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public DateTime DisbandDate { get; set; }
}