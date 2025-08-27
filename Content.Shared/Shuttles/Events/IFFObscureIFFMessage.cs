// SPDX-FileCopyrightText: 2025 Onezero0
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

[Serializable, NetSerializable]
// Mono -  obscure iff class
public sealed class IFFObscureIFFMessage : BoundUserInterfaceMessage
{
    public bool Show;
}
