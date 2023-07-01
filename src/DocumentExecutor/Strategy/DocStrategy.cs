using AndNet.Manager.Database.Models;
using AndNet.Manager.Database.Models.Player;

namespace AndNet.Manager.DocumentExecutor.Strategy;

public abstract class DocStrategy
{
    public abstract Task Execute(DbDoc doc, DbClanPlayer executor);
}