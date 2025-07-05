// SPDX-FileCopyrightText: 2025 Ark
// SPDX-FileCopyrightText: 2025 core-mene
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Theta.ShipEvent.Components;

/// <summary>
/// Makes circular shields appear on radar displays.
/// </summary>
[RegisterComponent]
public sealed partial class CircularShieldRadarComponent : Component
{
    /// <summary>
    /// When true, this shield blip will be visible on radar regardless of which grid the radar is on.
    /// </summary>
    [DataField]
    public bool VisibleFromOtherGrids = true;
}
