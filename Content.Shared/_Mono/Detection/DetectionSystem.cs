using Robust.Shared.Map.Components;
using System;

namespace Content.Shared._Mono.Detection;

/// <summary>
///     Handles the logic for grid and entity detection.
/// </summary>
public sealed class DetectionSystem : EntitySystem
{
    public DetectionLevel IsGridDetected(Entity<MapGridComponent?> grid, EntityUid byUid)
    {
        if (!Resolve(grid, ref grid.Comp))
            return DetectionLevel.Undetected;

        var comp = EnsureComp<DetectionRangeMultiplierComponent>(byUid);

        var gridAABB = grid.Comp.LocalAABB;
        var gridDiagonal = MathF.Sqrt(gridAABB.Width * gridAABB.Width + gridAABB.Height * gridAABB.Height);
        var visualSig = gridDiagonal;
        var visualRadius = visualSig * comp.VisualMultiplier;

        var thermalSig = TryComp<ThermalSignatureComponent>(grid, out var sigComp) ? MathF.Max(sigComp.TotalHeat, 0f) : 0f;
        var thermalRadius = MathF.Sqrt(thermalSig) * comp.InfraredMultiplier;
        var outlineRadius = thermalRadius * comp.InfraredOutlinePortion;

        outlineRadius = MathF.Max(outlineRadius, visualRadius);

        var level = DetectionLevel.Undetected;

        var xform = Transform(grid);
        var byXform = Transform(byUid);
        if (xform.Coordinates.TryDistance(EntityManager, byXform.Coordinates, out var distance))
        {
            if (distance <= outlineRadius)
                level = DetectionLevel.Detected;
            else if (distance < thermalRadius)
                level = DetectionLevel.PartialDetected;
        }

        return level;
    }
}

public enum DetectionLevel
{
    Detected,
    PartialDetected,
    Undetected
}
