using AndNet.Manager.Database.Models.Player;
using Microsoft.AspNetCore.Identity;

namespace AndNet.Manager.Database.Models.Auth;

public class DbUser : IdentityUser<int>
{
    public DbPlayer Player { get; set; } = null!;
}