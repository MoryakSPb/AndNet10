using AndNet.Migrator.AndNet7.AndNet7.Shared.Enums;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared.DTO;

public class ClanElectionCode
{
    public int MemberId { get; set; }
    public Guid Code { get; set; }
    public ClanDepartmentEnum Department { get; set; }
}