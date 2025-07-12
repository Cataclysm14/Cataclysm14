// SPDX-FileCopyrightText: 2024 LordCarve
// SPDX-FileCopyrightText: 2024 Nemanja
// SPDX-FileCopyrightText: 2025 Schepka
// SPDX-FileCopyrightText: 2025 starch
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Ame.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AmeFuelContainerComponent : Component
{
    /// <summary>
    /// The amount of fuel in the container.
    /// </summary>
    [DataField("fuelamount", required: true), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int FuelAmount; // mono

    /// <summary>
    /// The maximum fuel capacity of the container.
    /// </summary>
    [DataField("fuelcapacity", required: true), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int FuelCapacity; // mono
}
