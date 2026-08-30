using System;
using System.Collections.Generic;
using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.Cuffs;
using Content.Shared._Cataclysm14.Zombies.Smoker;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Cuffs.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Cataclysm14.Zombies.Smoker;

/// <summary>
/// Server-authoritative Smoker tongue, voice cues, struggle, and delayed smoke shutdown
/// </summary>
public sealed class SmokerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly CuffableSystem _cuffable = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmokerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SmokerComponent, SmokerTongueActionEvent>(OnTongueAction);
        SubscribeLocalEvent<SmokerComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<SmokerComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);

        SubscribeNetworkEvent<SmokerStruggleRequestEvent>(OnStruggleRequest);
        SubscribeAllEvent<SmokerTongueArmRequestEvent>(OnTongueArmRequest);
        SubscribeAllEvent<SmokerTongueDisarmRequestEvent>(OnTongueDisarmRequest);
        SubscribeAllEvent<SmokerTongueCancelRequestEvent>(OnTongueCancelRequest);
    }

    private void OnMapInit(Entity<SmokerComponent> ent, ref MapInitEvent args)
    {
        SpawnSmokeEmitter(ent);
    }

    private void SpawnSmokeEmitter(Entity<SmokerComponent> ent)
    {
        if (ent.Comp.SmokeEmitter is { } oldEmitter && !Deleted(oldEmitter))
            return;

        ent.Comp.SmokeEmitter = Spawn(ent.Comp.SmokeEmitterPrototype,
            new EntityCoordinates(ent.Owner, Vector2.Zero));
    }

    private void OnEmote(Entity<SmokerComponent> ent, ref EmoteEvent args)
    {
        if (args.Emote.ID != "Cough")
            return;

        _audio.PlayPvs(ent.Comp.CoughSound, ent.Owner);

        // prevent another emote handler from layering a human cough over the Smoker voice line
        args.Handled = true;
    }

    private void OnTongueArmRequest(SmokerTongueArmRequestEvent ev, EntitySessionEventArgs args)
    {
        if (!TryValidateArmRequest(ev.Action, args, out var smoker, out var smokerComp, out var armComp))
            return;

        if (smokerComp.TongueArmed || smokerComp.TongueTarget != null || !IsAlive(smoker))
            return;

        smokerComp.TongueArmed = true;
        smokerComp.TongueReadyAt = _timing.CurTime + armComp.ArmDelay;

        // SoundCollectionSpecifier chooses one alert_01 --> alert_06 entry at random
        // this is only played on the transition into the armed state
        _audio.PlayPvs(smokerComp.AlertSound, smoker);

        // emote when the tongue is armed
        _chat.TrySendInGameICMessage(
            smoker,
            Loc.GetString("smoker-tongue-arm-emote"),
            InGameICChatType.Emote,
            false);
    }

    private void OnTongueDisarmRequest(SmokerTongueDisarmRequestEvent ev, EntitySessionEventArgs args)
    {
        if (!TryValidateArmRequest(ev.Action, args, out _, out var smokerComp, out _))
            return;

        if (smokerComp.TongueArmed && _timing.CurTime < smokerComp.TongueReadyAt)
            return;

        smokerComp.TongueArmed = false;
        smokerComp.TongueReadyAt = TimeSpan.Zero;
    }

    private bool TryValidateArmRequest(
        NetEntity netAction,
        EntitySessionEventArgs args,
        out EntityUid smoker,
        out SmokerComponent smokerComp,
        out SmokerTongueActionComponent armComp)
    {
        smoker = default;
        smokerComp = default!;
        armComp = default!;

        if (args.SenderSession.AttachedEntity is not { } user ||
            !TryComp(user, out SmokerComponent? foundSmokerComp) ||
            foundSmokerComp == null)
        {
            return false;
        }

        var action = GetEntity(netAction);
        if (!TryComp(action, out EntityTargetActionComponent? targetAction) ||
            !TryComp(action, out SmokerTongueActionComponent? foundArmComp) ||
            targetAction == null ||
            foundArmComp == null ||
            targetAction.AttachedEntity != user ||
            !targetAction.Enabled ||
            targetAction is { Charges: 0, RenewCharges: false } ||
            targetAction.Cooldown is { } cooldown && cooldown.End > _timing.CurTime)
        {
            return false;
        }

        // AttachedEntity is authoritative here. SharedActionsSystem clears it when an action is removed,
        // so do not directly touch ActionsComponent.Actions (which is restricted to SharedActionsSystem)
        smoker = user;
        smokerComp = foundSmokerComp;
        armComp = foundArmComp;
        return true;
    }

    private void OnRefreshMovementSpeed(Entity<SmokerComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.TongueTarget != null)
            args.ModifySpeed(0f);
    }

    private void OnTongueCancelRequest(SmokerTongueCancelRequestEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } smoker ||
            !TryComp<SmokerComponent>(smoker, out var smokerComp) ||
            smokerComp.TongueTarget == null)
        {
            return;
        }

        var action = GetEntity(ev.Action);
        if (!TryComp<EntityTargetActionComponent>(action, out var targetAction) ||
            !HasComp<SmokerTongueActionComponent>(action) ||
            targetAction.AttachedEntity != smoker)
        {
            return;
        }

        // Voluntary release is allowed during the normal action cooldown
        // it is silent and does not reset or shorten that cooldown
        BreakTongue(smoker, smokerComp);
    }

    private void OnTongueAction(Entity<SmokerComponent> ent, ref SmokerTongueActionEvent args)
    {
        if (args.Handled)
            return;

        var smoker = ent.Owner;
        var target = args.Target;

        if (!ent.Comp.TongueArmed || _timing.CurTime < ent.Comp.TongueReadyAt)
        {
            _popup.PopupEntity(Loc.GetString("smoker-tongue-arming"), smoker, smoker);
            return;
        }

        ent.Comp.TongueArmed = false;
        ent.Comp.TongueReadyAt = TimeSpan.Zero;

        if (ent.Comp.TongueTarget != null)
        {
            _popup.PopupEntity(Loc.GetString("smoker-tongue-busy"), smoker, smoker);
            return;
        }

        if (HasComp<SmokerTonguedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("smoker-tongue-target-busy"), smoker, smoker);
            return;
        }

        if (!IsAlive(smoker) || !IsAlive(target))
        {
            _popup.PopupEntity(Loc.GetString("smoker-tongue-invalid-target"), smoker, smoker);
            return;
        }

        var smokerPos = _transform.GetMapCoordinates(smoker);
        var targetPos = _transform.GetMapCoordinates(target);
        if (smokerPos.MapId != targetPos.MapId)
            return;

        var distance = (targetPos.Position - smokerPos.Position).Length();
        if (distance < ent.Comp.TongueMinRange)
        {
            _popup.PopupEntity(Loc.GetString("smoker-tongue-too-close"), smoker, smoker);
            return;
        }

        if (distance > ent.Comp.TongueMaxRange ||
            !_interaction.InRangeUnobstructed(smoker, target, ent.Comp.TongueMaxRange, popup: false))
        {
            _popup.PopupEntity(Loc.GetString("smoker-tongue-no-line"), smoker, smoker);
            return;
        }

        if (!TryComp<PhysicsComponent>(smoker, out _) || !TryComp<PhysicsComponent>(target, out _))
            return;

        // play only when a valid tongue shot actually launches, never on arming/cancelling
        _audio.PlayPvs(ent.Comp.LaunchTongueSound, smoker);
        StartTongue(smoker, target, ent.Comp);

        // SharedActionsSystem only applies useDelay after the event is handled
        // Therefore invalid/blocked shots do not consume the cooldown
        args.Handled = true;
    }

    private void StartTongue(EntityUid smoker, EntityUid target, SmokerComponent smokerComp)
    {
        smokerComp.TongueTarget = target;
        // The visual is rendered client-side as one continuous textured quad
        var activeTongue = EnsureComp<SmokerTongueActiveComponent>(smoker);
        activeTongue.Target = target;
        Dirty(smoker, activeTongue);

        // freeze the Smoker for the duration of the grapple
        _movement.RefreshMovementSpeedModifiers(smoker);
        if (TryComp<PhysicsComponent>(smoker, out var smokerPhysics))
            _physics.SetLinearVelocity(smoker, Vector2.Zero, body: smokerPhysics);

        var victim = EnsureComp<SmokerTonguedComponent>(target);
        victim.Smoker = smoker;
        victim.EscapeProgress = 0f;
        victim.RequiredProgress = smokerComp.StruggleRequiredProgress;
        victim.ProgressPerPress = smokerComp.StruggleProgressPerPress;
        victim.NextAcceptedPress = TimeSpan.Zero;
        victim.NextProgressNetworkUpdate = TimeSpan.Zero;
        victim.TongueCuffs = null;

        _chat.TryEmoteWithChat(target, "Scream", ChatTransmitRange.Normal);

        ForceDropHands(target);
        ApplyTongueCuffs(smoker, target, smokerComp, victim);
        Dirty(target, victim);

        _popup.PopupEntity(Loc.GetString("smoker-tongue-caught"), target, target);
    }

    private void ForceDropHands(EntityUid target)
    {
        if (!TryComp<HandsComponent>(target, out var hands))
            return;

        foreach (var hand in _hands.EnumerateHands(target, hands))
        {
            if (hand.HeldEntity is not { } held || HasComp<UnremoveableComponent>(held))
                continue;

            _hands.DoDrop(target, hand, true, hands);
        }
    }

    private void ApplyTongueCuffs(
        EntityUid smoker,
        EntityUid target,
        SmokerComponent smokerComp,
        SmokerTonguedComponent victim)
    {
        if (!TryComp<CuffableComponent>(target, out var cuffable))
            return;

        var tongueCuffs = Spawn(smokerComp.TongueCuffPrototype, new EntityCoordinates(target, Vector2.Zero));
        if (!_cuffable.TryAddNewCuffs(target, smoker, tongueCuffs, cuffable))
        {
            QueueDel(tongueCuffs);
            return;
        }

        victim.TongueCuffs = tongueCuffs;
    }

    private void RemoveTongueCuffs(SmokerTonguedComponent victim)
    {
        if (victim.TongueCuffs is not { } tongueCuffs || Deleted(tongueCuffs))
            return;

        QueueDel(tongueCuffs);
        victim.TongueCuffs = null;
    }

    private void OnStruggleRequest(SmokerStruggleRequestEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } victim ||
            !TryComp<SmokerTonguedComponent>(victim, out var victimComp))
        {
            return;
        }

        var now = _timing.CurTime;
        if (now < victimComp.NextAcceptedPress)
            return;

        if (victimComp.Smoker is not { } smoker ||
            Deleted(smoker) ||
            !TryComp<SmokerComponent>(smoker, out var smokerComp) ||
            smokerComp.TongueTarget != victim)
        {
            RemoveTongueCuffs(victimComp);
            RemComp<SmokerTonguedComponent>(victim);
            return;
        }

        victimComp.NextAcceptedPress = now + TimeSpan.FromSeconds(smokerComp.StrugglePressInterval);
        victimComp.EscapeProgress = MathF.Min(
            victimComp.RequiredProgress,
            victimComp.EscapeProgress + victimComp.ProgressPerPress);
        Dirty(victim, victimComp);

        if (victimComp.EscapeProgress >= victimComp.RequiredProgress)
            BreakTongue(smoker, smokerComp, escaped: true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        CleanupOrphanedVictims();

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<SmokerComponent>();
        while (query.MoveNext(out var smoker, out var comp))
        {
            if (!comp.DeathHandled && TryComp<MobStateComponent>(smoker, out var mob) && mob.CurrentState == MobState.Dead)
            {
                comp.DeathHandled = true;
                comp.StopSmokeAt = now + TimeSpan.FromSeconds(comp.SmokeDeathDelay);
                comp.TongueArmed = false;
                comp.TongueReadyAt = TimeSpan.Zero;
                BreakTongue(smoker, comp);
            }

            if (comp.StopSmokeAt is { } stopAt && now >= stopAt)
            {
                if (comp.SmokeEmitter is { } emitter && !Deleted(emitter))
                    QueueDel(emitter);

                comp.SmokeEmitter = null;
                comp.StopSmokeAt = null;
            }

            if (comp.TongueTarget is not { } target)
                continue;

            // Keep the Smoker planted while the tongue is attached, including cancelling any
            // input that may have been present when the hook landed
            if (TryComp<PhysicsComponent>(smoker, out var smokerPhysics))
                _physics.SetLinearVelocity(smoker, Vector2.Zero, body: smokerPhysics);

            if (Deleted(target) || !IsAlive(smoker) || !IsAlive(target))
            {
                BreakTongue(smoker, comp);
                continue;
            }

            var smokerPos = _transform.GetMapCoordinates(smoker);
            var targetPos = _transform.GetMapCoordinates(target);
            if (smokerPos.MapId != targetPos.MapId)
            {
                BreakTongue(smoker, comp);
                continue;
            }

            var distance = (targetPos.Position - smokerPos.Position).Length();
            if (distance > comp.TongueBreakRange ||
                !_interaction.InRangeUnobstructed(smoker, target, comp.TongueBreakRange, popup: false))
            {
                BreakTongue(smoker, comp);
                continue;
            }

            DecayStruggle(target, comp, frameTime, now);
            PullTarget(smoker, target, comp, smokerPos.Position, targetPos.Position, distance);
            if (comp.TongueTarget == null)
                continue;
        }
    }

    private void DecayStruggle(EntityUid target, SmokerComponent smokerComp, float frameTime, TimeSpan now)
    {
        if (!TryComp<SmokerTonguedComponent>(target, out var victim) || victim.EscapeProgress <= 0f)
            return;

        victim.EscapeProgress = MathF.Max(
            0f,
            victim.EscapeProgress - smokerComp.StruggleDecayPerSecond * frameTime);

        if (now < victim.NextProgressNetworkUpdate)
            return;

        victim.NextProgressNetworkUpdate = now + TimeSpan.FromSeconds(smokerComp.StruggleNetworkUpdateInterval);
        Dirty(target, victim);
    }

    private void PullTarget(
        EntityUid smoker,
        EntityUid target,
        SmokerComponent comp,
        Vector2 smokerPosition,
        Vector2 targetPosition,
        float distance)
    {
        if (!TryComp<PhysicsComponent>(target, out var targetPhysics))
        {
            BreakTongue(smoker, comp);
            return;
        }

        if (distance <= comp.TongueStopDistance)
        {
            _physics.SetLinearVelocity(target, Vector2.Zero, body: targetPhysics);
            return;
        }

        var delta = smokerPosition - targetPosition;
        if (delta.LengthSquared() <= 0.0001f)
            return;

        var velocity = Vector2.Normalize(delta) * comp.TonguePullSpeed;
        _physics.SetLinearVelocity(target, velocity, body: targetPhysics);
    }

    private void BreakTongue(EntityUid smoker, SmokerComponent comp, bool escaped = false)
    {
        var target = comp.TongueTarget;
        if (target is { } victim)
        {
            if (!Deleted(victim) && TryComp<PhysicsComponent>(victim, out var victimPhysics))
                _physics.SetLinearVelocity(victim, Vector2.Zero, body: victimPhysics);

            if (!Deleted(victim) && TryComp<SmokerTonguedComponent>(victim, out var victimComp) && victimComp.Smoker == smoker)
            {
                RemoveTongueCuffs(victimComp);
                RemComp<SmokerTonguedComponent>(victim);
                if (escaped)
                    _popup.PopupEntity(Loc.GetString("smoker-tongue-escaped"), victim, victim);
            }
        }

        comp.TongueTarget = null;

        if (!Deleted(smoker))
        {
            RemComp<SmokerTongueActiveComponent>(smoker);
            _movement.RefreshMovementSpeedModifiers(smoker);
        }
    }

    private bool IsAlive(EntityUid uid)
    {
        return TryComp<MobStateComponent>(uid, out var mob) && mob.CurrentState == MobState.Alive;
    }

    private void CleanupOrphanedVictims()
    {
        List<EntityUid>? orphaned = null;
        var query = EntityQueryEnumerator<SmokerTonguedComponent>();
        while (query.MoveNext(out var victim, out var caught))
        {
            if (caught.Smoker is { } smokerUid &&
                !Deleted(smokerUid) &&
                TryComp<SmokerComponent>(smokerUid, out var smoker) &&
                smoker.TongueTarget == victim)
            {
                continue;
            }

            orphaned ??= new List<EntityUid>();
            orphaned.Add(victim);
        }

        if (orphaned == null)
            return;

        foreach (var victim in orphaned)
        {
            if (Deleted(victim) || !TryComp<SmokerTonguedComponent>(victim, out var victimComp))
                continue;

            RemoveTongueCuffs(victimComp);
            RemComp<SmokerTonguedComponent>(victim);
        }
    }
}
