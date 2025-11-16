// SPDX-FileCopyrightText: 2025 Ilya246
//
// SPDX-License-Identifier: MPL-2.0

using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Map;

namespace Content.Server._Mono.Cleanup;

/// <summary>
///     System with helper methods for entity cleanup.
/// </summary>
public sealed class CleanupHelperSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private EntityQuery<GhostComponent> _ghostQuery;
    private EntityQuery<MindComponent> _mindQuery;

    public override void Initialize()
    {
        base.Initialize();

        _ghostQuery = GetEntityQuery<GhostComponent>();
        _mindQuery = GetEntityQuery<MindComponent>();
    }

    /// <summary>
    ///     Whether there is an entity with a player bound to it in radius. Counts dead people and brains but not ghosts.
    /// </summary>
    public bool HasNearbyPlayers(EntityCoordinates coord, float radius) {
        var minds = _lookup.GetEntitiesInRange<MindContainerComponent>(coord, radius);

        foreach (var (uid, comp) in minds)
        {
            if (!comp.HasMind
                || _ghostQuery.HasComp(uid)
                || _mindQuery.CompOrNull(comp.Mind.Value)?.OwnedEntity == null
            )
                continue;

            var entCoord = Transform(uid).Coordinates;

            if (coord.TryDistance(EntityManager, entCoord, out var distance)
                && distance <= radius
            )
                return true;
        }
        return false;
    }
}
