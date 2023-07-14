using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Shared.Player;

public partial class PlayerSelect : ComponentBase
{
    [Inject]
    public PlayerNicknamesService PlayerNicknamesService { get; set; } = null!;

    [Parameter]
    public int[] LockedPlayers { get; set; } = Array.Empty<int>();

    [Parameter]
    public int MaxSelectedPlayers { get; set; } = 4;

    [Parameter]
    public List<int> SelectedPlayers { get; set; } = null!;

    [Parameter]
    public List<int> AllPlayers { get; set; } = null!;

    [Parameter]
    public Action? Update { get; set; }

    public int SelectedCount { get; set; }

    public bool IsLocked(int id)
    {
        return LockedPlayers.Contains(id);
    }

    protected override Task OnInitializedAsync()
    {
        SelectedCount = SelectedPlayers.Count;
        return base.OnInitializedAsync();
    }

    public void AddPlayer(int id)
    {
        if (IsLocked(id) || SelectedPlayers.Count >= MaxSelectedPlayers) return;
        SelectedPlayers.Add(id);
        AllPlayers.Remove(id);
        SelectedCount = SelectedPlayers.Count;
        StateHasChanged();
        Update?.Invoke();
    }

    public void RemovePlayer(int id)
    {
        if (IsLocked(id)) return;
        SelectedPlayers.Remove(id);
        AllPlayers.Add(id);
        SelectedCount = SelectedPlayers.Count;
        StateHasChanged();
        Update?.Invoke();
    }
}