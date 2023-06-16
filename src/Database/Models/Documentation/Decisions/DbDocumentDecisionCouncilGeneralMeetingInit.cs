namespace AndNet.Manager.Database.Models.Documentation.Decisions;

public record DbDocumentDecisionCouncilGeneralMeetingInit : DbDocumentDecisionCouncil
{
    public override string Prefix { get; set; } = "РСОС";
    public DateTime Date { get; set; }
}