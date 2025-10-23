// SPDX-FileCopyrightText: 2025 ark1368
//
// SPDX-License-Identifier: MPL-2.0

using Robust.Shared.GameStates;

namespace Content.Shared._Mono.ArmorPlate;

/// <summary>
/// Component that handles movement speed modifiers for armor plates.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ArmorPlateSpeedModifierComponent : Component;
