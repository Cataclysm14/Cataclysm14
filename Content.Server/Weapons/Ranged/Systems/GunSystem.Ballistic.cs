using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    /// <summary>
    /// Adds ammo to a ballistic ammo provider by incrementing UnspawnedCount.
    /// </summary>
    public void AddBallisticAmmo(EntityUid uid, BallisticAmmoProviderComponent component, int amount = 1)
    {
        component.UnspawnedCount += amount;

        DirtyField(uid, component, nameof(BallisticAmmoProviderComponent.UnspawnedCount));
    }

    protected override void Cycle(EntityUid uid, BallisticAmmoProviderComponent component, MapCoordinates coordinates)
    {
        EntityUid? ent = null;

        // TODO: Combine with TakeAmmo
        if (component.Entities.Count > 0)
        {
            var existing = component.Entities[^1];
            component.Entities.RemoveAt(component.Entities.Count - 1);
            component.EntProtos.RemoveAt(component.EntProtos.Count - 1); // stalker-changes

            Containers.Remove(existing, component.Container);
			ent = existing; //Mono: Sound bugfix
            EnsureShootable(existing);
        }
        else if (component.UnspawnedCount > 0)
        {
            var copy = component.EntProtos; // stalker-changes-start
            copy.Reverse();
            var proto = copy.FirstOrNull();
            if (proto != null)
            {
                ent = Spawn(proto.Value, coordinates);
                EnsureShootable(ent.Value);
                component.EntProtos.RemoveAt(component.EntProtos.Count - 1);
                component.UnspawnedCount--;
            }
            else
            {
                component.UnspawnedCount--;
                ent = Spawn(component.Proto, coordinates);
                EnsureShootable(ent.Value);
            } // stalker-changes-end
        }

        if (ent != null)
            EjectCartridge(ent.Value);

        var cycledEvent = new GunCycledEvent();
        RaiseLocalEvent(uid, ref cycledEvent);
    }
}
