using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Popups;
using Robust.Shared.Timing;
using Content.Shared.Foldable;
using Robust.Shared.Utility;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class HoodedClothingSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AttachedClothingComponent, MapInitEvent>(OnAttachedMapInit);
        SubscribeLocalEvent<AttachedClothingComponent, ClothingGotEquippedEvent>(OnAttachedEquipped);
        SubscribeLocalEvent<AttachedClothingComponent, ClothingGotUnequippedEvent>(OnAttachedUnequipped);
        SubscribeLocalEvent<ToggleableClothingComponent, ToggleClothingAttemptEvent>(OnToggleAttempt);
        SubscribeLocalEvent<HoodedClothingComponent, FoldedEvent>(OnFolded);
    }

    private void OnToggleAttempt(Entity<ToggleableClothingComponent> ent, ref ToggleClothingAttemptEvent args)
    {
        var comp = ent.Comp;

        if (comp.Container == null)
            return;

        var wearer = Transform(ent.Owner).ParentUid;
        if (!wearer.IsValid())
            return;

        foreach (var (clothingUid, slot) in comp.ClothingUids)
        {
            if (!comp.Container.Contains(clothingUid))
                continue;

            if (_inventorySystem.TryGetSlotEntity(wearer, slot, out var existing) && existing != clothingUid)
            {
                _popupSystem.PopupClient(Loc.GetString("toggleable-clothing-remove-first", ("entity", existing)), args.User, args.User);
                args.Cancel();
                return;
            }
        }
    }
    private void OnFolded(Entity<HoodedClothingComponent> ent, ref FoldedEvent args)
    {
        if (!TryComp<ClothingComponent>(ent.Owner, out var clothing)
            || !clothing.ClothingVisuals.TryGetValue(ent.Comp.VisualLayerKey, out var layers) || layers.Count == 0)
            return;

        var newState = args.IsFolded ? $"{ent.Comp.EquippedPrefix}-{ent.Comp.BaseState}" : ent.Comp.BaseState;

        var layer = layers[0];
        layer.State = newState;
        var updated = new List<PrototypeLayerData>(layers) { [0] = layer };
        clothing.ClothingVisuals[ent.Comp.VisualLayerKey] = updated;
        Dirty(ent.Owner, clothing);

        RaiseLocalEvent(Transform(ent.Owner).ParentUid, new VisualsChangedEvent(GetNetEntity(ent.Owner), clothing.InSlot ?? ent.Comp.VisualLayerKey));
    }
    private void OnAttachedMapInit(Entity<AttachedClothingComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<TransformComponent>(ent.Comp.AttachedUid, out var xform) || !xform.ParentUid.IsValid())
            return;

        UpdateVisual(ent.Owner, ent.Comp.AttachedUid, xform.ParentUid);
    }

    private void OnAttachedEquipped(Entity<AttachedClothingComponent> ent, ref ClothingGotEquippedEvent args)
    {
        UpdateVisual(ent.Owner, ent.Comp.AttachedUid, args.Wearer);
    }

    private void OnAttachedUnequipped(Entity<AttachedClothingComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        UpdateVisual(ent.Owner, ent.Comp.AttachedUid, args.Wearer);
    }

    private void UpdateVisual(EntityUid attachedUid, EntityUid hoodieUid, EntityUid wearer)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (!TryComp<HoodedClothingComponent>(hoodieUid, out var hooded)
            || !TryComp<ClothingComponent>(hoodieUid, out var clothing)
            || !TryComp<ToggleableClothingComponent>(hoodieUid, out var toggleable)
            || !toggleable.ClothingUids.TryGetValue(attachedUid, out var slot))
            return;

        var raised = _inventorySystem.TryGetSlotEntity(wearer, slot, out var slotEnt) && slotEnt == attachedUid;

        clothing.EquippedPrefix = raised ? hooded.EquippedPrefix : null;
        Dirty(hoodieUid, clothing);

        if (clothing.ClothingVisuals.TryGetValue(hooded.VisualLayerKey, out var layers) && layers.Count > 0)
        {
            var newState = raised ? $"{hooded.EquippedPrefix}-{hooded.BaseState}" : hooded.BaseState;
            var layer = layers[0];
            layer.State = newState;
            var updated = new List<PrototypeLayerData>(layers) { [0] = layer };
            clothing.ClothingVisuals[hooded.VisualLayerKey] = updated;
            Dirty(hoodieUid, clothing);
        }

        RaiseLocalEvent(wearer, new VisualsChangedEvent(GetNetEntity(hoodieUid), slot));
    }
}
