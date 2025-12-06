// SPDX-FileCopyrightText: 2025 NazrinNya
//
// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Reactions;
using Content.Shared.GameTicking;
using Robust.Shared.Random;

namespace Content.Server._Mono.Atmos.EntitySystems;

/// <summary>
/// This handles...
/// </summary>
public sealed class GasReactionAmplitudeSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    private readonly Random _random = Random.Shared;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartedEvent ev)
    {
        foreach (var reaction in _atmosphereSystem.GasReactions)
        {
            var minTAmplitude = reaction.MinimumTemperatureAmplitude;
            var maxTAmplitude = reaction.MaximumTemperatureAmplitude;

            reaction.CurrentMinimumTemperatureRequirement = reaction.MinimumTemperatureRequirement + _random.NextFloat(-minTAmplitude, minTAmplitude);
            reaction.CurrentMaximumTemperatureRequirement = reaction.MaximumTemperatureRequirement + _random.NextFloat(-maxTAmplitude, maxTAmplitude);
        }
    }
}
