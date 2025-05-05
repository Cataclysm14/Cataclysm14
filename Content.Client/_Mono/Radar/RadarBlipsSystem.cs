using System.Numerics;
using Content.Shared._Mono.Radar;
using Robust.Shared.Timing;

namespace Content.Client._Mono.Radar;

public sealed partial class RadarBlipsSystem : EntitySystem
{
    private const double BlipStaleSeconds = 3.0;
    private static readonly List<(Vector2, float, Color, RadarBlipShape)> EmptyBlipList = new();
    private static readonly List<(NetEntity? Grid, Vector2 Position, float Scale, Color Color, RadarBlipShape Shape)> EmptyRawBlipList = new();
    private TimeSpan _lastRequestTime = TimeSpan.Zero;
    private static readonly TimeSpan RequestThrottle = TimeSpan.FromMilliseconds(250);

    // Maximum distance for blips to be considered visible
    private const float MaxBlipRenderDistance = 1000f;

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private TimeSpan _lastUpdatedTime;
    private List<(NetEntity? Grid, Vector2 Position, float Scale, Color Color, RadarBlipShape Shape)> _blips = new();
    private Vector2 _radarWorldPosition;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GiveBlipsEvent>(HandleReceiveBlips);
    }

    private void HandleReceiveBlips(GiveBlipsEvent ev, EntitySessionEventArgs args)
    {
        if (ev?.Blips == null)
        {
            _blips = EmptyRawBlipList;
            return;
        }
        _blips = ev.Blips;
        _lastUpdatedTime = _timing.CurTime;
    }

    public void RequestBlips(EntityUid console)
    {
        // Only request if we have a valid console
        if (!Exists(console))
            return;

        // Add request throttling to avoid network spam
        if (_timing.CurTime - _lastRequestTime < RequestThrottle)
            return;

        _lastRequestTime = _timing.CurTime;

        // Cache the radar position for distance culling
        if (TryComp<TransformComponent>(console, out var xform))
        {
            _radarWorldPosition = _xform.GetWorldPosition(console);
        }

        var netConsole = GetNetEntity(console);
        var ev = new RequestBlipsEvent(netConsole);
        RaiseNetworkEvent(ev);
    }

    /// <summary>
    /// Gets the current blips as world positions with their scale, color and shape.
    /// This is needed for the legacy radar display that expects world coordinates.
    /// </summary>
    public List<(Vector2, float, Color, RadarBlipShape)> GetCurrentBlips()
    {
        // If it's been more than the stale threshold since our last update,
        // the data is considered stale - return an empty list
        if (_timing.CurTime.TotalSeconds - _lastUpdatedTime.TotalSeconds > BlipStaleSeconds)
            return EmptyBlipList;

        var result = new List<(Vector2, float, Color, RadarBlipShape)>(_blips.Count);

        foreach (var blip in _blips)
        {
            Vector2 worldPosition;

            // If no grid, position is already in world coordinates
            if (blip.Grid == null)
            {
                worldPosition = blip.Position;

                // Distance culling for world position blips
                if (Vector2.DistanceSquared(worldPosition, _radarWorldPosition) > MaxBlipRenderDistance * MaxBlipRenderDistance)
                    continue;

                result.Add((worldPosition, blip.Scale, blip.Color, blip.Shape));
                continue;
            }

            // If grid exists, transform from grid-local to world coordinates
            if (TryGetEntity(blip.Grid, out var gridEntity))
            {
                // Transform the grid-local position to world position
                var worldPos = _xform.GetWorldPosition(gridEntity.Value);
                var gridRot = _xform.GetWorldRotation(gridEntity.Value);

                // Rotate the local position by grid rotation and add grid position
                var rotatedLocalPos = gridRot.RotateVec(blip.Position);
                worldPosition = worldPos + rotatedLocalPos;

                // Distance culling for grid position blips
                if (Vector2.DistanceSquared(worldPosition, _radarWorldPosition) > MaxBlipRenderDistance * MaxBlipRenderDistance)
                    continue;

                result.Add((worldPosition, blip.Scale, blip.Color, blip.Shape));
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the raw blips data which includes grid information for more accurate rendering.
    /// </summary>
    public List<(NetEntity? Grid, Vector2 Position, float Scale, Color Color, RadarBlipShape Shape)> GetRawBlips()
    {
        if (_timing.CurTime.TotalSeconds - _lastUpdatedTime.TotalSeconds > BlipStaleSeconds)
            return EmptyRawBlipList;

        // Implement distance culling for raw blips as well
        if (_blips.Count == 0)
            return _blips;

        var filteredBlips = new List<(NetEntity? Grid, Vector2 Position, float Scale, Color Color, RadarBlipShape Shape)>(_blips.Count);

        foreach (var blip in _blips)
        {
            // For non-grid blips, do direct distance check
            if (blip.Grid == null)
            {
                if (Vector2.DistanceSquared(blip.Position, _radarWorldPosition) <= MaxBlipRenderDistance * MaxBlipRenderDistance)
                {
                    filteredBlips.Add(blip);
                }
                continue;
            }

            // For grid blips, transform to world space for distance check
            if (TryGetEntity(blip.Grid, out var gridEntity))
            {
                var worldPos = _xform.GetWorldPosition(gridEntity.Value);
                var gridRot = _xform.GetWorldRotation(gridEntity.Value);
                var rotatedLocalPos = gridRot.RotateVec(blip.Position);
                var worldPosition = worldPos + rotatedLocalPos;

                if (Vector2.DistanceSquared(worldPosition, _radarWorldPosition) <= MaxBlipRenderDistance * MaxBlipRenderDistance)
                {
                    filteredBlips.Add(blip);
                }
            }
        }

        return filteredBlips;
    }
}
