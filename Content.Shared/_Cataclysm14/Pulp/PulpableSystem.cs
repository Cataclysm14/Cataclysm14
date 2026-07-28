using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Gibbing.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Verbs;

namespace Content.Shared._Cataclysm14.Pulp;

public sealed class PulpableSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    private EntityQuery<MobThresholdsComponent> _mobThresholds = default!;
    private EntityQuery<GhostComponent> _ghost = default!;
    private EntityQuery<OrganComponent> _organs = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PulpableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PulpableComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        _mobThresholds = EntityManager.GetEntityQuery<MobThresholdsComponent>();
        _ghost = EntityManager.GetEntityQuery<GhostComponent>();
        _organs = EntityManager.GetEntityQuery<OrganComponent>();
    }

    private bool IsDead(EntityUid uid)
    {
        return _mobThresholds.TryComp(uid, out var thresholds) && thresholds.CurrentThresholdState == MobState.Dead;
    }

    private void OnExamined(EntityUid uid, PulpableComponent component, ExaminedEvent args)
    {
        if (!IsDead(uid))
            return;

        args.PushText(component.IsPulped ? "Pulped" : "You can pulp this corpse");
    }

    private void OnGetVerbs(EntityUid uid, PulpableComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!IsDead(uid) || component.IsPulped || !args.CanInteract)
            return;

        args.Verbs.Add(new()
        {
            Text = "Pulp",
            Act = () => Pulp(uid, component),
        });
    }

    public void Pulp(EntityUid uid, PulpableComponent component)
    {
        component.IsPulped = true;
        Dirty(uid, component);

        if (HasComp<PassiveDamageComponent>(uid))
            RemComp<PassiveDamageComponent>(uid);

        var organsAndLoot = _bodySystem.GibBody(uid, gibOrgans: true, gib: GibType.Gib);
        foreach (var entityUid in organsAndLoot)
        {
            if (_organs.HasComp(entityUid))
                QueueDel(entityUid);
        }
    }
}
