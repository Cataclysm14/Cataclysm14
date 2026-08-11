using Content.Shared.FixedPoint;

namespace Content.Server._Cataclysm14.Triggers;

[RegisterComponent]
public sealed partial class RepeatTimerTriggerComponent : Component
{
    [DataField]
    public bool IsAlive = true;

    [DataField]
    public FixedPoint2 Interval = 1f;

    public FixedPoint2 CurrentTime;
}
