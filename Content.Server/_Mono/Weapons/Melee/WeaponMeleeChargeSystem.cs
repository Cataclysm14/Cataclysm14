// SPDX-FileCopyrightText: 2025 NazrinNya
//
// SPDX-License-Identifier: MPL-2.0

using Content.Shared.Examine;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Mono.Weapons.Melee;

public sealed class MeleeChargeSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private TimeSpan _acculumator = TimeSpan.Zero;
    public override void Initialize()
    {
        SubscribeLocalEvent<WeaponMeleeChargeComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<WeaponMeleeChargeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<WeaponMeleeChargeComponent, ItemToggledEvent>(OnToggle);
        SubscribeLocalEvent<WeaponMeleeChargeComponent, ItemToggleActivateAttemptEvent>(OnToggleAttempt);
    }

    private void OnExamined(Entity<WeaponMeleeChargeComponent> ent, ref ExaminedEvent args)
    {
        if (InCooldown(ent))
            args.PushMarkup(Loc.GetString("melee-charge-weakened", ("cooldown", CooldownToSeconds(ent))));
    }

    private void OnMeleeHit(Entity<WeaponMeleeChargeComponent> ent, ref MeleeHitEvent args)
    {
        if (InCooldown(ent))
        {
            args.BonusDamage += ent.Comp.CooldownDamagePenalty;
            return;
        }

        if (!IsActive(ent))
            return;

        TryDeactivate(ent, ent.Comp);
    }

    private void OnToggleAttempt(Entity<WeaponMeleeChargeComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (!InCooldown(ent))
            return;

        _popup.PopupEntity(Loc.GetString("melee-charge-remaining-cooldown", ("remainingCooldown", CooldownToSeconds(ent))),
            args.User ?? ent);

        args.Cancelled = true;
    }

    private void OnToggle(Entity<WeaponMeleeChargeComponent> ent, ref ItemToggledEvent args)
    {
        if (args.Activated)
            Activate(ent, ent.Comp);
        else
            TryDeactivate(ent, ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveWeaponMeleeChargeComponent, WeaponMeleeChargeComponent>();

        while (query.MoveNext(out var uid, out _, out var charge))
        {
            if (!ActiveTimePassed(charge))
                continue;

            TryDeactivate(uid, charge);
        }

        _acculumator += TimeSpan.FromSeconds(frameTime);
    }

    private void TryDeactivate(EntityUid uid, WeaponMeleeChargeComponent charge)
    {
        if(!_toggle.TryDeactivate(uid))
            return;

        if (HasComp<ActiveWeaponMeleeChargeComponent>(uid))
            RemComp<ActiveWeaponMeleeChargeComponent>(uid);

        charge.CurrentCooldown = TimeSpan.FromSeconds(charge.Cooldown) + _acculumator;
    }

    private void Activate(EntityUid uid, WeaponMeleeChargeComponent charge)
    {
        AddComp<ActiveWeaponMeleeChargeComponent>(uid);
        charge.CurrentActiveTime = TimeSpan.FromSeconds(charge.ActiveTime) + _acculumator;
    }

    private bool InCooldown(WeaponMeleeChargeComponent charge)
    {
        return charge.CurrentCooldown > _acculumator;
    }

    private bool IsActive(EntityUid uid)
    {
        return HasComp<ActiveWeaponMeleeChargeComponent>(uid);
    }

    private bool ActiveTimePassed(WeaponMeleeChargeComponent charge)
    {
        return charge.CurrentActiveTime < _acculumator;
    }

    private int CooldownToSeconds(Entity<WeaponMeleeChargeComponent> ent)
    {
        // The thing about adding 1 here is that results of TimeSpan substraction is floored (i think?)
        // This means that even 0.99 turns into 0, and showing that 0 seconds remain until cooldown ends is not good.
        return (ent.Comp.CurrentCooldown - _acculumator).Seconds + 1;
    }
}
