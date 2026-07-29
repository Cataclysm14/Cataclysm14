using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Gibbing.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Shared._Cataclysm14.Pulp;

public sealed class PulpableSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private EntityQuery<MobThresholdsComponent> _mobThresholds = default!;
    private DamageTypePrototype _bluntDamage = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MobThresholdsComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MobThresholdsComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        _mobThresholds = EntityManager.GetEntityQuery<MobThresholdsComponent>();
        _bluntDamage = _prototypeManager.Index<DamageTypePrototype>("Blunt");
    }

    private bool IsDead(EntityUid uid)
    {
        return _mobThresholds.TryComp(uid, out var thresholds) && thresholds.CurrentThresholdState == MobState.Dead;
    }

    private void OnExamined(EntityUid uid, MobThresholdsComponent component, ExaminedEvent args)
    {
        if (!IsDead(uid))
            return;

        args.PushText("You can pulp this corpse");
    }

    private void OnGetVerbs(EntityUid uid, MobThresholdsComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !IsDead(uid) || !TryComp(uid, out DamageableComponent? damageableComponent))
            return;

        args.Verbs.Add(new()
        {
            Text = "Pulp",
            Act = () => Pulp(uid, damageableComponent),
        });
    }

    public void Pulp(EntityUid uid, DamageableComponent damageableComponent)
    {
        if (HasComp<PassiveDamageComponent>(uid))
            RemComp<PassiveDamageComponent>(uid);

        _damageableSystem.SetDamage(uid, damageableComponent, new(_bluntDamage, 99999));
    }
}
