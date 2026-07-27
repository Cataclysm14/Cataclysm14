using System.Numerics;
using Content.Shared._Cataclysm.Projectiles;
using Content.Shared.Projectiles;

namespace Content.Server._Cataclysm.Projectiles;

public sealed class ProjectileDamageFalloffSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProjectileDamageFalloffComponent, MapInitEvent>(
            OnMapInit);

        SubscribeLocalEvent<ProjectileDamageFalloffComponent, ProjectileHitEvent>(
            OnProjectileHit);
    }

    private void OnMapInit(
        EntityUid uid,
        ProjectileDamageFalloffComponent component,
        MapInitEvent args)
    {
        component.Origin = _transform.GetMapCoordinates(uid);
        component.OriginInitialized = true;
    }

    private void OnProjectileHit(
        EntityUid uid,
        ProjectileDamageFalloffComponent component,
        ref ProjectileHitEvent args)
    {
        if (!component.OriginInitialized)
            return;

        var current = _transform.GetMapCoordinates(uid);

        if (current.MapId != component.Origin.MapId)
            return;

        // Map coordinates use world units
        // One world unit to one tile
        var distance = Vector2.Distance(
            component.Origin.Position,
            current.Position);

        var falloffStart = MathF.Max(0f, component.FalloffStart);
        var distanceAfterFalloff = MathF.Max(0f, distance - falloffStart);

        if (distanceAfterFalloff <= 0f)
            return;

        float falloffUnits;

        if (component.StepByTile)
        {
            // Falloff only increases after completing another full tile
            falloffUnits = MathF.Floor(distanceAfterFalloff + 0.001f);
        }
        else
        {
            // Continuous falloff, including fractional tile distances
            falloffUnits = distanceAfterFalloff;
        }

        if (falloffUnits <= 0f)
            return;

        var falloffPerTile = MathF.Max(0f, component.FalloffPerTile);
        var minimumMultiplier = Math.Clamp(
            component.MinimumMultiplier,
            0f,
            1f);

        var multiplier = 1f - falloffUnits * falloffPerTile;
        multiplier = Math.Clamp(multiplier, minimumMultiplier, 1f);

        args.Damage *= multiplier;
    }
}