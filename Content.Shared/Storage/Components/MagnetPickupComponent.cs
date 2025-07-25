// SPDX-FileCopyrightText: 2023 Stray-Pyramid
// SPDX-FileCopyrightText: 2023 crystalHex
// SPDX-FileCopyrightText: 2023 metalgearsloth
// SPDX-FileCopyrightText: 2024 Mnemotechnican
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers
// SPDX-FileCopyrightText: 2025 Dvir
// SPDX-FileCopyrightText: 2025 Redrover1760
// SPDX-FileCopyrightText: 2025 cheetah1984
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Robust.Shared.GameStates; // Frontier

namespace Content.Server.Storage.Components;

/// <summary>
/// Applies an ongoing pickup area around the attached entity.
/// </summary>
[NetworkedComponent]
[RegisterComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class MagnetPickupComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("nextScan")]
    [AutoPausedField]
    public TimeSpan NextScan = TimeSpan.Zero;

    /// <summary>
    /// If true, ignores SlotFlags and can magnet pickup on hands/ground.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    [AutoNetworkedField]
    public bool ForcePickup = true;

    [ViewVariables(VVAccess.ReadWrite), DataField("range")]
    public float Range = 1f;
}
