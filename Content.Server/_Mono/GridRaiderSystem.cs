using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Mono;
using Content.Shared._Mono.NoHack;
using Content.Shared._Mono.NoDeconstruct;
using Content.Shared.Doors.Components;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;

namespace Content.Server._Mono;

/// <summary>
/// System that handles the GridRaiderComponent, which applies NoHack and NoDeconstruct to all entities with Door components on a grid.
/// </summary>
public sealed class GridRaiderSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

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

    private void OnGridRaiderStartup(EntityUid uid, GridRaiderComponent component, ComponentStartup args)
    {
        // Verify this is applied to a grid
        if (!HasComp<MapGridComponent>(uid))
        {
            Log.Warning($"GridRaiderComponent applied to non-grid entity {ToPrettyString(uid)}");
            return;
        }

        // Find all entities on the grid and apply NoHack/NoDeconstruct to doors
        var allEntitiesOnGrid = _lookup.GetEntitiesIntersecting(uid).ToHashSet();

        foreach (var entity in allEntitiesOnGrid)
        {
            // Skip the grid itself and entities inside containers (they'll be handled by container logic)
            if (entity == uid || _container.IsEntityInContainer(entity))
                continue;

            ProcessEntityOnGrid(uid, entity, component);
        }
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
    /// Process an entity on a grid and apply NoHack/NoDeconstruct if it's a door
    /// </summary>
    private void ProcessEntityOnGrid(EntityUid gridUid, EntityUid entityUid, GridRaiderComponent component)
    {
        // Only apply protection to entities with Door components
        if (!HasComp<DoorComponent>(entityUid))
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
}
