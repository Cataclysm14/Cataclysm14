// SPDX-FileCopyrightText: 2025 Ilya246
//
// SPDX-License-Identifier: MPL-2.0

using Content.Shared.Ghost;
using Content.Shared.Mobs.Systems;
using Robust.Server.Player;
using Robust.Shared.Map;

namespace Content.Server._Mono.Cleanup;

/// <summary>
///     System with helper methods for entity cleanup.
/// </summary>
public sealed class CleanupHelperSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private EntityQuery<GhostComponent> _ghostQuery;

    public override void Initialize()
    {
        base.Initialize();

        _ghostQuery = GetEntityQuery<GhostComponent>();
    }

    public bool HasNearbyPlayers(EntityCoordinates coord, float radius) {
        var allPlayerData = _player.GetAllPlayerData();
        foreach (var playerData in allPlayerData)
        {
            if (!_player.TryGetSessionById(playerData.UserId, out var session)
                || session.AttachedEntity is not { Valid: true } playerEnt
                || _ghostQuery.HasComp(playerEnt)
                || _mobState.IsDead(playerEnt)
            )
                continue;

            var playerCoords = Transform(playerEnt).Coordinates;

            if (coord.TryDistance(EntityManager, playerCoords, out var distance)
                && distance <= radius
            )
                return true;
        }
        return false;
    }
}
