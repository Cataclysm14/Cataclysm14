// SPDX-FileCopyrightText: 2025 Ilya246
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Server.Worldgen.Components.Debris;
using Content.Shared.Verbs;

namespace Content.Server._Mono.GridAntiCollision;

public sealed class GridAntiCollisionSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;



}
