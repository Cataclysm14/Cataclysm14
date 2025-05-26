using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Mono;
using Content.Shared._Mono.NoHack;
using Content.Shared._Mono.NoDeconstruct;
using Content.Shared.Doors.Components;
using Content.Shared.VendingMachines;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._Mono;

/// <summary>
/// System that handles the GridRaiderComponent, which applies NoHack and NoDeconstruct to entities with Door and/or VendingMachine components on a grid.
/// </summary>
public sealed class GridRaiderSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GridRaiderComponent, ComponentStartup>(OnGridRaiderStartup);
        SubscribeLocalEvent<GridRaiderComponent, ComponentShutdown>(OnGridRaiderShutdown);
        SubscribeLocalEvent<MoveEvent>(OnEntityMoved);
        SubscribeLocalEvent<EntParentChangedMessage>(OnEntityParentChanged);
        SubscribeLocalEvent<EntInsertedIntoContainerMessage>(OnEntityInsertedInContainer);
        SubscribeLocalEvent<EntRemovedFromContainerMessage>(OnEntityRemovedFromContainer);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<GridRaiderComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            // Skip if it's not time for the next settings check yet
            if (component.NextSettingsCheck > curTime)
                continue;

            // Update the next check time
            component.NextSettingsCheck = curTime + component.SettingsCheckInterval;

            // Reprocess all entities on the grid to apply/remove protection based on current settings
            RefreshGridProtection(uid, component);
        }
    }

    private void OnGridRaiderStartup(EntityUid uid, GridRaiderComponent component, ComponentStartup args)
    {
        // Verify this is applied to a grid
        if (!HasComp<MapGridComponent>(uid))
        {
            Log.Warning($"GridRaiderComponent applied to non-grid entity {ToPrettyString(uid)}");
            return;
        }

        // Set the initial settings check time
        component.NextSettingsCheck = _timing.CurTime + component.SettingsCheckInterval;

        // Find all entities on the grid and apply NoHack/NoDeconstruct to doors and vending machines
        RefreshGridProtection(uid, component);
    }

    private void OnGridRaiderShutdown(EntityUid uid, GridRaiderComponent component, ComponentShutdown args)
    {
        // When the component is removed, remove NoHack/NoDeconstruct from all protected entities
        foreach (var entity in component.ProtectedEntities.ToList())
        {
            if (EntityManager.EntityExists(entity))
            {
                RemoveProtection(entity);
            }
        }

        component.ProtectedEntities.Clear();
    }

    private void OnEntityMoved(ref MoveEvent args)
    {
        // Check if the entity moved to or from a grid with GridRaiderComponent
        var entity = args.Entity;

        // Skip entities in containers as they're handled by container events
        if (_container.IsEntityInContainer(entity.Owner))
            return;

        // If the entity is already protected by a GridRaiderComponent, check if it left the grid
        if (TryGetGridRaiderComponent(args.OldPosition.EntityId, out var oldGridComp) &&
            oldGridComp != null && oldGridComp.ProtectedEntities.Contains(entity.Owner) &&
            args.NewPosition.EntityId != args.OldPosition.EntityId)
        {
            RemoveProtection(entity.Owner);
            oldGridComp.ProtectedEntities.Remove(entity.Owner);
        }

        // If the entity moved to a grid with GridRaiderComponent, check if it should get protection
        if (args.NewPosition.EntityId.IsValid() && // Ensure NewPosition.EntityId is valid
            TryGetGridRaiderComponent(args.NewPosition.EntityId, out var newGridComp) &&
            newGridComp != null && !newGridComp.ProtectedEntities.Contains(entity.Owner))
        {
            ProcessEntityOnGrid(args.NewPosition.EntityId, entity.Owner, newGridComp);
        }
    }

    private void OnEntityParentChanged(ref EntParentChangedMessage args)
    {
        var entity = args.Entity;

        // Skip entities in containers as they're handled by container events
        if (_container.IsEntityInContainer(entity))
            return;

        // If the entity was on a protected grid and left
        if (args.OldParent.HasValue && args.OldParent.Value.IsValid() && // Ensure OldParent is valid
            TryGetGridRaiderComponent(args.OldParent.Value, out var oldGridComp) &&
            oldGridComp != null && oldGridComp.ProtectedEntities.Contains(entity))
        {
            // Entity moved away from a protected grid - remove protection
            RemoveProtection(entity);
            oldGridComp.ProtectedEntities.Remove(entity);
        }

        // If the entity moved to a protected grid
        if (args.Transform.ParentUid.IsValid() && // Ensure ParentUid is valid before using it
            TryGetGridRaiderComponent(args.Transform.ParentUid, out var newGridComp) &&
            newGridComp != null && !newGridComp.ProtectedEntities.Contains(entity))
        {
            ProcessEntityOnGrid(args.Transform.ParentUid, entity, newGridComp);
        }
    }

    // New handler for entities inserted into containers
    private void OnEntityInsertedInContainer(EntInsertedIntoContainerMessage args)
    {
        var entity = args.Entity;
        // Entity was protected but is now in a container - remove protection
        // Iterate over all grids that might be protecting this entity.
        var query = EntityQueryEnumerator<GridRaiderComponent, TransformComponent>();
        while (query.MoveNext(out var gridUid, out var gridComp, out _)) // Querying for the component directly on grids
        {
            if (gridComp.ProtectedEntities.Contains(entity))
            {
                RemoveProtection(entity);
                gridComp.ProtectedEntities.Remove(entity);
                // It's unlikely to be protected by multiple grids, but break if you're certain.
            }
        }
    }

    // New handler for entities removed from containers
    private void OnEntityRemovedFromContainer(EntRemovedFromContainerMessage args)
    {
        var entity = args.Entity;
        // If the entity is now directly on a protected grid
        if (TryComp<TransformComponent>(entity, out var xform) &&
            xform.GridUid.HasValue && // Ensure GridUid is not null
            TryGetGridRaiderComponent(xform.GridUid.Value, out var gridComp) &&
            gridComp != null && // Ensure component is found
            !gridComp.ProtectedEntities.Contains(entity))
        {
            ProcessEntityOnGrid(xform.GridUid.Value, entity, gridComp);
        }
    }

    /// <summary>
    /// Process an entity on a grid and apply NoHack/NoDeconstruct if it's a door or vending machine
    /// </summary>
    private void ProcessEntityOnGrid(EntityUid gridUid, EntityUid entityUid, GridRaiderComponent component)
    {
        // Check if this entity should be protected based on component settings
        var shouldProtect = false;

        if (component.ProtectDoors && HasComp<DoorComponent>(entityUid))
            shouldProtect = true;

        if (component.ProtectVendingMachines && HasComp<VendingMachineComponent>(entityUid))
            shouldProtect = true;

        if (!shouldProtect)
            return;

        ApplyProtection(gridUid, entityUid, component);
    }

    /// <summary>
    /// Applies NoHack and NoDeconstruct to an entity and adds it to the protected entities list
    /// </summary>
    private void ApplyProtection(EntityUid gridUid, EntityUid entityUid, GridRaiderComponent component)
    {
        // Skip if the entity is already protected
        if (component.ProtectedEntities.Contains(entityUid))
            return;

        // Apply NoHack and NoDeconstruct components
        EnsureComp<NoHackComponent>(entityUid);
        EnsureComp<NoDeconstructComponent>(entityUid);
        component.ProtectedEntities.Add(entityUid);
    }

    /// <summary>
    /// Removes NoHack and NoDeconstruct from an entity
    /// </summary>
    private void RemoveProtection(EntityUid entityUid)
    {
        if (HasComp<NoHackComponent>(entityUid))
        {
            RemComp<NoHackComponent>(entityUid);
        }

        if (HasComp<NoDeconstructComponent>(entityUid))
        {
            RemComp<NoDeconstructComponent>(entityUid);
        }
    }

    /// <summary>
    /// Helper method to get the GridRaiderComponent from a grid entity
    /// </summary>
    private bool TryGetGridRaiderComponent(EntityUid? gridUid, [NotNullWhen(true)] out GridRaiderComponent? component)
    {
        component = null;

        if (gridUid == null || !gridUid.Value.IsValid() || !EntityManager.EntityExists(gridUid.Value))
            return false;

        return TryComp(gridUid.Value, out component);
    }

    /// <summary>
    /// Refreshes protection for all entities on a grid based on current component settings
    /// </summary>
    private void RefreshGridProtection(EntityUid gridUid, GridRaiderComponent component)
    {
        // Get all entities currently on the grid
        var allEntitiesOnGrid = _lookup.GetEntitiesIntersecting(gridUid).ToHashSet();
        var entitiesThatShouldBeProtected = new HashSet<EntityUid>();

        // Find entities that should be protected based on current settings
        foreach (var entity in allEntitiesOnGrid)
        {
            // Skip the grid itself and entities inside containers
            if (entity == gridUid || _container.IsEntityInContainer(entity))
                continue;

            // Check if this entity should be protected based on current settings
            var shouldProtect = false;

            if (component.ProtectDoors && HasComp<DoorComponent>(entity))
                shouldProtect = true;

            if (component.ProtectVendingMachines && HasComp<VendingMachineComponent>(entity))
                shouldProtect = true;

            if (shouldProtect)
                entitiesThatShouldBeProtected.Add(entity);
        }

        // Remove protection from entities that should no longer be protected
        var entitiesToUnprotect = component.ProtectedEntities.Except(entitiesThatShouldBeProtected).ToList();
        foreach (var entity in entitiesToUnprotect)
        {
            if (EntityManager.EntityExists(entity))
            {
                RemoveProtection(entity);
            }
            component.ProtectedEntities.Remove(entity);
        }

        // Add protection to entities that should be protected but aren't yet
        var entitiesToProtect = entitiesThatShouldBeProtected.Except(component.ProtectedEntities).ToList();
        foreach (var entity in entitiesToProtect)
        {
            ApplyProtection(gridUid, entity, component);
        }
    }
}
