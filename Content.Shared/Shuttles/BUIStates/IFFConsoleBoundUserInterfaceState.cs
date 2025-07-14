// SPDX-FileCopyrightText: 2022 metalgearsloth
// SPDX-FileCopyrightText: 2025 Redrover1760
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Shuttles.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class IFFConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public IFFFlags AllowedFlags;
    public IFFFlags Flags;

    /// <summary>
    /// the current cloak heat
    /// </summary>
    public float CloakHeat;

    public IFFConsoleBoundUserInterfaceState(float cloakHeat = 0)
    {
        CloakHeat = cloakHeat;
    }
}

[Serializable, NetSerializable]
public enum IFFConsoleUiKey : byte
{
    Key,
}
