using System;
using System.Numerics;
using Content.Shared._Cataclysm14.Zombies.Smoker;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;
using static Robust.Client.UserInterface.Control;

namespace Content.Client._Cataclysm14.Zombies.Smoker;

/// <summary>
/// Displays the victim's tongue struggle bar and clickable escape button
/// Progress is still server-authoritative and decays while the victim is held
/// </summary>
public sealed class SmokerStruggleSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    private const float HudVerticalOffset = 86f;

    private BoxContainer? _hud;
    private Label? _label;
    private ProgressBar? _bar;
    private Button? _button;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalEntity is not { } player ||
            !TryComp<SmokerTonguedComponent>(player, out var caught))
        {
            HideHud();
            return;
        }

        EnsureHud();
        _hud!.Visible = true;
        _bar!.MinValue = 0f;
        _bar.MaxValue = MathF.Max(1f, caught.RequiredProgress);
        _bar.Value = caught.EscapeProgress;
    }

    public override void Shutdown()
    {
        _hud?.Orphan();
        _hud = null;
        _label = null;
        _bar = null;
        _button = null;
        base.Shutdown();
    }

    private void EnsureHud()
    {
        if (_hud != null)
            return;

        _label = new Label
        {
            Text = Loc.GetString("smoker-tongue-struggle-title"),
            Align = Label.AlignMode.Center,
            MinSize = new Vector2(420, 24),
            MouseFilter = MouseFilterMode.Ignore,
        };

        _bar = new ProgressBar
        {
            MinValue = 0f,
            MaxValue = 100f,
            MinSize = new Vector2(420, 22),
            MouseFilter = MouseFilterMode.Ignore,
        };

        _button = new Button
        {
            Text = Loc.GetString("smoker-tongue-struggle-button"),
            MinSize = new Vector2(420, 38),
            HorizontalAlignment = HAlignment.Stretch,
        };
        _button.OnPressed += _ => RequestStruggle();

        _hud = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            MouseFilter = MouseFilterMode.Pass,
        };
        _hud.AddChild(_label);
        _hud.AddChild(_bar);
        _hud.AddChild(_button);
        _ui.PopupRoot.AddChild(_hud);

        // Anchors this widget roughly to the
        // viewport center and offset it downward so it sits just below the player
        _hud.Measure(Vector2Helpers.Infinity);
        var size = _hud.DesiredSize;
        LayoutContainer.SetAnchorLeft(_hud, 0.5f);
        LayoutContainer.SetAnchorRight(_hud, 0.5f);
        LayoutContainer.SetAnchorTop(_hud, 0.5f);
        LayoutContainer.SetAnchorBottom(_hud, 0.5f);
        LayoutContainer.SetMarginLeft(_hud, -size.X / 2f);
        LayoutContainer.SetMarginRight(_hud, size.X / 2f);
        LayoutContainer.SetMarginTop(_hud, HudVerticalOffset);
        LayoutContainer.SetMarginBottom(_hud, HudVerticalOffset + size.Y);
    }

    private void RequestStruggle()
    {
        if (_player.LocalEntity is not { } player ||
            !HasComp<SmokerTonguedComponent>(player))
        {
            return;
        }

        RaiseNetworkEvent(new SmokerStruggleRequestEvent());
    }

    private void HideHud()
    {
        if (_hud != null)
            _hud.Visible = false;
    }
}
