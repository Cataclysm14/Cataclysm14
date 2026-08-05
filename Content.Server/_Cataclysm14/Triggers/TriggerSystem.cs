using Content.Shared.FixedPoint;
using Content.Shared.Mobs;

namespace Content.Server._Cataclysm14.Triggers;

public sealed class TriggerSystem : EntitySystem
{
    [Dependency] private Explosion.EntitySystems.TriggerSystem _triggerSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RepeatTimerTriggerComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, RepeatTimerTriggerComponent component, ref MobStateChangedEvent args)
    {
        component.IsAlive = args.NewMobState == MobState.Alive;
    }

    public override void Update(float frameTime)
    {
        var time = FixedPoint2.New(frameTime);

        var timers = EntityQueryEnumerator<RepeatTimerTriggerComponent>();
        while (timers.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsAlive)
                continue;

            comp.CurrentTime += time;
            if (comp.CurrentTime >= comp.Interval)
            {
                comp.CurrentTime -= comp.Interval;
                _triggerSystem.Trigger(uid);
            }
        }
    }
}
