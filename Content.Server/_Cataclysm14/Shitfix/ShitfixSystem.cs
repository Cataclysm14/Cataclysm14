using Content.Server.NPC.Pathfinding;
namespace Content.Server._Cataclysm14.Shitfix;

public sealed class ShitfixSystem : EntitySystem
{
    private PathfindingSystem _pathfinding = default!;

    private ISawmill _logger = null!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        _pathfinding = EntityManager.System<PathfindingSystem>();
        _logger = Logger.GetSawmill("ShitfixSystem");

        SubscribeLocalEvent<MakeChunkDirtyOnSpawnComponent, MapInitEvent>(MakeChunkDirtyOnMapInit);
    }

    private void MakeChunkDirtyOnMapInit(EntityUid uid, MakeChunkDirtyOnSpawnComponent component, MapInitEvent args)
    {
        var xform = Transform(uid);
        var grid = xform.ParentUid;
        var center = _pathfinding.GetOrigin(xform.Coordinates, grid);
        for (var x = component.Left; x <= component.Right; x++)
        {
            for (var y = component.Bottom; y <= component.Top; y++)
            {
                _pathfinding.DirtyChunk(grid, new Vector2i(center.X + x, center.Y + y));
            }
        }
    }
}
