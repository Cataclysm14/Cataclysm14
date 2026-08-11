using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Cataclysm14.Forage;

public abstract partial class SharedForageableSystem : EntitySystem
{
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ForageableComponent, GetVerbsEvent<AlternativeVerb>>(AddForageVerb);
        SubscribeLocalEvent<ForageableComponent, InteractHandEvent>(OnInteractHand);
    }

    private void AddForageVerb(EntityUid uid, ForageableComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !component.ReadyToForage)
            return;

        args.Verbs.Add(new()
        {
            Act = () => Forage(uid, component, args.User),
            Text = "Forage",
            Priority = 666,
        });
    }

    private void OnInteractHand(EntityUid uid, ForageableComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Forage(uid, component, args.User);
    }

    private void Forage(EntityUid uid, ForageableComponent component, EntityUid user)
    {
        if (!component.ReadyToForage)
            return;

        component.ReadyToForage = false;
        DirtyField(uid, component, nameof(ForageableComponent.ReadyToForage));

        Appearance.SetData(uid, ForageableVisuals.State, component.ReadyToForage ? component.ForageableState : component.HarvestedState);

        var position = Transform(uid).Coordinates;
        for (var i = 0; i < component.SpawnAmount; i++)
        {
            PredictedSpawnAtPosition(component.FoodProto, position);
        }

        Audio.PlayPredicted(component.ForageSound, uid, user);

        QueryRespawn(uid, component);
    }

    protected virtual void QueryRespawn(EntityUid uid, ForageableComponent component)
    {
        // only for server
    }
}
