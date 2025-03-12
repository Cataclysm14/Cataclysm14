using Content.Server.Power.Components;
using Content.Shared._Mono.Ships;
using Content.Shared.Power;

namespace Content.Server._Mono.Ships;

public sealed class FTLDriveSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FTLDriveComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FTLDriveComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnStartup(EntityUid uid, FTLDriveComponent component, ComponentStartup args)
    {
        // Set initial power state
        if (TryComp<ApcPowerReceiverComponent>(uid, out var powerReceiver))
        {
            component.Powered = powerReceiver.Powered;
            Dirty(uid, component);
        }
    }

    private void OnPowerChanged(EntityUid uid, FTLDriveComponent component, ref PowerChangedEvent args)
    {
        component.Powered = args.Powered;
        Dirty(uid, component);
    }
}
