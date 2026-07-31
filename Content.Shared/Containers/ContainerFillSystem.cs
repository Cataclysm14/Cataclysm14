using System.Numerics;
using Content.Shared.EntityTable;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;

using Content.Shared._Cataclysm14.Containers; // Cataclysm14
using Content.Shared._Cataclysm14.Storage; // Cataclysm14
using Content.Shared.Item; // Cataclysm14
using Content.Shared.Storage; // Cataclysm14

namespace Content.Shared.Containers;

public sealed partial class ContainerFillSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedItemSystem _item = default!; // Cataclysm14

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ContainerFillComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EntityTableContainerFillComponent, MapInitEvent>(OnTableMapInit);
        SubscribeLocalEvent<EntityTableOutfitContainerFillComponent, MapInitEvent>(OnOutfitMapInit); // Cataclysm14
    }

    private void OnMapInit(EntityUid uid, ContainerFillComponent component, MapInitEvent args)
    {
        if (!TryComp(uid, out ContainerManagerComponent? containerComp))
            return;

        var xform = Transform(uid);
        var coords = new EntityCoordinates(uid, Vector2.Zero);

        foreach (var (contaienrId, prototypes) in component.Containers)
        {
            if (!_containerSystem.TryGetContainer(uid, contaienrId, out var container, containerComp))
            {
                Log.Error($"Entity {ToPrettyString(uid)} with a {nameof(ContainerFillComponent)} is missing a container ({contaienrId}).");
                continue;
            }

            foreach (var proto in prototypes)
            {
                var ent = Spawn(proto, coords);
                if (!_containerSystem.Insert(ent, container, containerXform: xform))
                {
                    Log.Error($"Entity {ToPrettyString(uid)} with a {nameof(ContainerFillComponent)} failed to insert an entity: {ToPrettyString(ent)}.");
                    _transform.AttachToGridOrMap(ent);
                    break;
                }
            }
        }
    }

    // Cataclysm14 Begin
    private void OnTableMapInit(Entity<EntityTableContainerFillComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp(ent, out ContainerManagerComponent? containerComp))
            return;

        if (TerminatingOrDeleted(ent) || !Exists(ent))
            return;

        var xform = Transform(ent);
        var coords = new EntityCoordinates(ent, Vector2.Zero);

        foreach (var (containerId, table) in ent.Comp.Containers)
        {
            if (!_containerSystem.TryGetContainer(ent, containerId, out var container, containerComp))
            {
                Log.Error($"Entity {ToPrettyString(ent)} with a {nameof(EntityTableContainerFillComponent)} is missing a container ({containerId}).");
                continue;
            }

            var spawns = _entityTable.GetSpawns(table);
            foreach (var proto in spawns)
            {
                var spawn = Spawn(proto, coords);
                if (!_containerSystem.Insert(spawn, container, containerXform: xform))
                {
                    if (HasComp<ResizableStorageOnFillComponent>(ent.Owner)
                        && TryComp(ent.Owner, out StorageComponent? storageComponent)
                        && TryComp(spawn, out ItemComponent? itemComponent))
                    {
                        storageComponent.Grid[0] = _item.GetAdjustedItemShape((spawn, itemComponent), Angle.Zero, Vector2i.Zero)[0];
                        Dirty(ent, storageComponent);
                        if (!_containerSystem.Insert(spawn, container, containerXform: xform))
                            Log.Error($"Entity {ToPrettyString(ent)} with a {nameof(EntityTableContainerFillComponent)} and {nameof(ResizableStorageOnFillComponent)} failed to insert an entity: {ToPrettyString(spawn)}.");
                        break;
                    }

                    Log.Warning($"Entity {ToPrettyString(ent)} with a {nameof(EntityTableContainerFillComponent)} failed to insert an entity: {ToPrettyString(spawn)}.");
                    _transform.AttachToGridOrMap(spawn);
                    break;
                }
            }
        }
    }
    // Cataclysm14 End
}
