using Content.Shared.FixedPoint;

namespace Content.Server._Cataclysm14.Triggers;

public sealed class TriggerSystem : EntitySystem
{
    [Dependency] private Explosion.EntitySystems.TriggerSystem _triggerSystem = default!;

    public override void Update(float frameTime)
    {
        var time = FixedPoint2.New(frameTime);

        var timers = EntityQueryEnumerator<RepeatTimerTriggerComponent>();
        while (timers.MoveNext(out var uid, out var comp))
        {
            comp.CurrentTime += time;
            if (comp.CurrentTime >= comp.Interval)
            {
                comp.CurrentTime -= comp.Interval;
                _triggerSystem.Trigger(uid);
            }
        }
    }
}
