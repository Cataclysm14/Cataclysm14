using Robust.Shared.Timing;

namespace Content.Shared._Stalker.Modifier;

public abstract class BaseFloatModifierSystem<TComponent> : EntitySystem where TComponent : BaseFloatModifierComponent
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public void RefreshModifiers(EntityUid uid)
    {
        if (!TryComp<TComponent>(uid, out var component))
            return;

        RefreshModifiers((uid, component));
    }

    public void RefreshModifiers(Entity<TComponent?> modifier)
    {
        if (_timing.ApplyingState)
            return;

        if (!Resolve(modifier, ref modifier.Comp, false))
            return;

        var refreshEvent = new FloatModifierRefreshEvent<TComponent>();
        RaiseLocalEvent(modifier, refreshEvent);

        if (MathHelper.CloseTo(refreshEvent.Modifier, modifier.Comp.Modifier))
            return;

        modifier.Comp.Modifier = refreshEvent.Modifier;

        var updatedEvent = new UpdatedFloatModifierEvent<TComponent>(modifier.Comp.Modifier);
        RaiseLocalEvent(modifier, updatedEvent);
    }
}
