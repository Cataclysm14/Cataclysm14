// SPDX-FileCopyrightText: 2022 Leon Friedrich
// SPDX-FileCopyrightText: 2023 TemporalOroboros
// SPDX-FileCopyrightText: 2024 Nemanja
// SPDX-FileCopyrightText: 2025 Kevin Zheng
// SPDX-FileCopyrightText: 2025 Redrover1760
// SPDX-FileCopyrightText: 2025 metalgearsloth
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Shuttles.UI;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Events;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class IFFConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private IFFConsoleWindow? _window;

    public IFFConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindowCenteredLeft<IFFConsoleWindow>();
        _window.ShowIFF += SendIFFMessage;
        _window.ShowVessel += SendVesselMessage;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not IFFConsoleBoundUserInterfaceState bState)
            return;

        _window?.UpdateState(bState);
        _window?.SetVesselHeat(bState.CloakHeat);
    }

    private void SendIFFMessage(bool obj)
    {
        SendMessage(new IFFShowIFFMessage()
        {
            Show = obj,
        });
    }

    private void SendVesselMessage(bool obj)
    {
        SendMessage(new IFFShowVesselMessage()
        {
            Show = obj,
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _window?.Close();
            _window = null;
        }
    }
}
