using System.Text.Json.Serialization;

namespace AndNet.Migrator.AndNet7.AndNet7.Shared.Programs;

public class ClanProgramTask
{
    [JsonIgnore]
    public int ProgramId { get; set; }

    [JsonIgnore]
    public virtual ClanProgram Program { get; set; } = null!;

    public int TaskNumber { get; set; }
    public bool? Status { get; set; }
    public string TaskDescription { get; set; } = string.Empty;
    public string? FinalDescription { get; set; }
}