namespace AndNet.Manager.Database.Models.Documentation.Decisions;

public record DbDocumentDecisionGeneralMeeting : DbDocumentDecision
{
    public override string Prefix { get; set; } = "РО";
}