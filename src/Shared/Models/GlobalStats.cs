namespace AndNet.Manager.Shared.Models;

public record GlobalStats
{
    public int MaxOnline { get; set; }
    public int MaxInGame { get; set; }
    public int HoursPlayed { get; set; }
    public int Battles { get; set; }
    public int NewPlayers { get; set; }
    public int AwardsIssued { get; set; }
}