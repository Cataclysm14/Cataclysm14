using Content.Shared.Bed.Sleep;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SleepingComponent, ComponentStartup>(OnSleepingStartup);
        SubscribeLocalEvent<SleepingComponent, ComponentShutdown>(OnSleepingShutdown);
    }

    protected override void UpdateCanMove(
        EntityUid uid,
        StunnedComponent component,
        EntityEventArgs args)
    {
        // keep the normal stun behavior
        base.UpdateCanMove(uid, component, args);

        switch (args)
        {
            case ComponentStartup:
                UpdateStunVisual(uid, stunnedOverride: true);
                break;

            case ComponentShutdown:
                UpdateStunVisual(uid, stunnedOverride: false);
                break;
        }
    }

    private void OnSleepingStartup(
        EntityUid uid,
        SleepingComponent component,
        ComponentStartup args)
    {
        UpdateStunVisual(uid, sleepingOverride: true);
    }

    private void OnSleepingShutdown(
        EntityUid uid,
        SleepingComponent component,
        ComponentShutdown args)
    {
        UpdateStunVisual(uid, sleepingOverride: false);
    }

    private void UpdateStunVisual(
        EntityUid uid,
        bool? stunnedOverride = null,
        bool? sleepingOverride = null)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var stunned = stunnedOverride ?? HasComp<StunnedComponent>(uid);
        var sleeping = sleepingOverride ?? HasComp<SleepingComponent>(uid);
        var incapacitated = TryComp<MobStateComponent>(uid, out var mobState) &&
                            mobState.CurrentState != MobState.Alive;

        if (stunned && !sleeping && !incapacitated)
        {
            ShowStunVisual(uid, sprite);
            return;
        }

        HideStunVisual(uid, sprite);
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
