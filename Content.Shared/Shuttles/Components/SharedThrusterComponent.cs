// SPDX-FileCopyrightText: 2021 metalgearsloth
// SPDX-FileCopyrightText: 2022 wrexbe
// SPDX-FileCopyrightText: 2023 Dvir
// SPDX-FileCopyrightText: 2025 Onezero0
// SPDX-FileCopyrightText: 2025 gus
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Components
{
    [Serializable, NetSerializable]
    public enum ThrusterVisualState : byte
    {
        State,
        Thrusting,
    }
}
