using Content.Server.Administration;
using Content.Server.NPC.Pathfinding;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Cataclysm14.DebugUtils;

[AdminCommand(AdminFlags.Debug)]
public sealed class RegenerateChunk : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override string Command => "regenchunk";

    public override string Description => "regenerates pathfinding chunk under the given entity. (if an entity is not specified, then under the one who entered the)";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        EntityUid ent;
        if (args.Length != 1)
        {
            ent = shell.Player!.AttachedEntity!.Value;
        }
        else
        {
            if (!int.TryParse(args[0], out var entityId))
            {
                shell.WriteError($"{args[0]} is not a valid integer.");
                return;
            }

            ent = new EntityUid(entityId);
        }

        if (!_entityManager.TryGetComponent<TransformComponent>(ent, out var transform))
        {
            shell.WriteError($"{args[0]} is not a valid target.");
            return;
        }

        var grid = transform.ParentUid;
        if (!_entityManager.TryGetComponent<GridPathfindingComponent>(grid, out var gridPathfinding))
        {
            shell.WriteError($"{args[0]} does not stand on a chunk");
            return;
        }

        _entityManager.System<PathfindingSystem>().DirtyChunk(grid, transform.Coordinates);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
            return CompletionResult.FromHint("Player EntityUid");

        return CompletionResult.Empty;
    }
}
