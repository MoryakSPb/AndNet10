using AndNet.Manager.Shared.Enums;
using Microsoft.AspNetCore.Components;

namespace AndNet.Manager.Client.Shared.Player;

public partial class AwardTypeCount : ComponentBase
{
    [Parameter]
    public AwardType Type { get; set; }

    [Parameter]
    public int Count { get; set; }
}