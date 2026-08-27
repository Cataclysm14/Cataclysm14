using Content.Shared.Stunnable;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Client.Stunnable;

public sealed class StunSystem : SharedStunSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private static readonly ResPath StunnedRsi =
        new("Mobs/Effects/stunned.rsi");

    private static readonly RSI.StateId StunnedState =
        new("stunned");

    private enum StunVisualLayers : byte
    {
        Stunned,
    }

    protected override void UpdateCanMove(
        EntityUid uid,
        StunnedComponent component,
        EntityEventArgs args)
    {
        // keep the normal stun behavior
        base.UpdateCanMove(uid, component, args);

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        switch (args)
        {
            case ComponentStartup:
                ShowStunVisual(uid, sprite);
                break;

            case ComponentShutdown:
                HideStunVisual(uid, sprite);
                break;
        }
    }

    private void ShowStunVisual(
        EntityUid uid,
        SpriteComponent sprite)
    {
        _sprite.LayerMapReserve(
            (uid, sprite),
            StunVisualLayers.Stunned);

        _sprite.LayerSetRsi(
            (uid, sprite),
            StunVisualLayers.Stunned,
            StunnedRsi,
            StunnedState);

        _sprite.LayerSetVisible(
            (uid, sprite),
            StunVisualLayers.Stunned,
            true);

        _sprite.LayerSetOffset(
            (uid, sprite),
            StunVisualLayers.Stunned,
            new Vector2(0f, 0.35f));
    }

    private void HideStunVisual(
        EntityUid uid,
        SpriteComponent sprite)
    {
        _sprite.RemoveLayer(
            (uid, sprite),
            StunVisualLayers.Stunned,
            false);
    }
}
