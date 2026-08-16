using Content.Shared.Item;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Cataclysm14.Containers;

public sealed class SharedContainerForPrototypesSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly SharedStorageSystem _storageSystem = default!;

    private EntityQuery<ItemComponent> _itemQuery;

    public override void Initialize()
    {
        _itemQuery = GetEntityQuery<ItemComponent>();
    }

    public bool CanInsertProto(EntProtoId proto, Entity<StorageComponent> container)
    {
        if (!_proto.TryIndex(proto, out var entityPrototype))
            return false; // YAMLLinter already knows about it

        if (!entityPrototype.Components.TryGetComponent(_componentFactory, out ItemComponent? itemComponent))
            return false;

        // from SharedStorage.CanInsert
        var maxSize = _storageSystem.GetMaxItemSize(container.AsNullable());
        if (_item.GetSizePrototype(itemComponent.Size) > maxSize)
            return false;

        // we cant check storagecomp from proto, because it requires spawn entity... so skip it
        return TryGetAvailableGridSpaceForPrototype(container, itemComponent, entityPrototype.Name);
    }

    // from SharedStorageSystem
    public bool TryGetAvailableGridSpaceForPrototype(
        Entity<StorageComponent> storageEnt,
        ItemComponent itemEnt,
        string name)
    {
        // if the item has an available saved location, use that
        if (FindSavedLocationForPrototype(storageEnt, itemEnt, name))
            return true;

        var storageBounding = storageEnt.Comp.Grid.GetBoundingBox();

        Angle startAngle;
        if (storageEnt.Comp.DefaultStorageOrientation == null)
        {
            startAngle = Angle.FromDegrees(-itemEnt.StoredRotation);
        }
        else
        {
            if (storageBounding.Width < storageBounding.Height)
            {
                startAngle = storageEnt.Comp.DefaultStorageOrientation == StorageDefaultOrientation.Horizontal
                    ? Angle.Zero
                    : Angle.FromDegrees(90);
            }
            else
            {
                startAngle = storageEnt.Comp.DefaultStorageOrientation == StorageDefaultOrientation.Vertical
                    ? Angle.Zero
                    : Angle.FromDegrees(90);
            }
        }

        for (var y = storageBounding.Bottom; y <= storageBounding.Top; y++)
        {
            for (var x = storageBounding.Left; x <= storageBounding.Right; x++)
            {
                for (var angle = startAngle; angle <= Angle.FromDegrees(360 - startAngle); angle += Math.PI / 2f)
                {
                    var location = new ItemStorageLocation(angle, (x, y));
                    if (ItemFitsInGridLocationForPrototype(itemEnt, storageEnt, location))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool FindSavedLocationForPrototype(
        Entity<StorageComponent> ent,
        ItemComponent item,
        string name)
    {
        if (!ent.Comp.SavedLocations.TryGetValue(name, out var list))
            return false;

        foreach (var location in list)
        {
            if (ItemFitsInGridLocationForPrototype(item, ent, location))
            {
                return true;
            }
        }

        return false;
    }

    public bool ItemFitsInGridLocationForPrototype(
        ItemComponent itemEnt,
        Entity<StorageComponent> storageEnt,
        ItemStorageLocation location)
    {
        return ItemFitsInGridLocationForPrototype(itemEnt, storageEnt, location.Position, location.Rotation);
    }

    public bool ItemFitsInGridLocationForPrototype(
        ItemComponent itemEnt,
        Entity<StorageComponent> storageEnt,
        Vector2i position,
        Angle rotation)
    {
        var gridBounds = storageEnt.Comp.Grid.GetBoundingBox();
        if (!gridBounds.Contains(position))
            return false;

        var itemShape = GetAdjustedItemShapeForPrototype(itemEnt, rotation, position);

        foreach (var box in itemShape)
        {
            for (var offsetY = box.Bottom; offsetY <= box.Top; offsetY++)
            {
                for (var offsetX = box.Left; offsetX <= box.Right; offsetX++)
                {
                    var pos = (offsetX, offsetY);

                    if (!IsGridSpaceEmptyForPrototype(itemEnt, storageEnt, pos))
                        return false;
                }
            }
        }

        return true;
    }

    public IReadOnlyList<Box2i> GetAdjustedItemShapeForPrototype(ItemComponent entity, ItemStorageLocation location)
    {
        return GetAdjustedItemShapeForPrototype(entity, location.Rotation, location.Position);
    }

    public IReadOnlyList<Box2i> GetAdjustedItemShapeForPrototype(ItemComponent entity, Angle rotation, Vector2i position)
    {
        var shapes = GetItemShape(entity);
        var boundingShape = shapes.GetBoundingBox();
        var boundingCenter = ((Box2) boundingShape).Center;
        var matty = Matrix3Helpers.CreateTransform(boundingCenter, rotation);
        var drift = boundingShape.BottomLeft - matty.TransformBox(boundingShape).BottomLeft;

        var adjustedShapes = new List<Box2i>();
        foreach (var shape in shapes)
        {
            var transformed = matty.TransformBox(shape).Translated(drift);
            var floored = new Box2i(transformed.BottomLeft.Floored(), transformed.TopRight.Floored());
            var translated = floored.Translated(position);

            adjustedShapes.Add(translated);
        }

        return adjustedShapes;
    }

    public IReadOnlyList<Box2i> GetItemShape(ItemComponent item)
    {
        return item.Shape ?? _item.GetSizePrototype(item.Size).DefaultShape;
    }

    public bool IsGridSpaceEmptyForPrototype(ItemComponent itemEnt, Entity<StorageComponent> storageEnt, Vector2i location)
    {
        var validGrid = false;
        foreach (var grid in storageEnt.Comp.Grid)
        {
            if (grid.Contains(location))
            {
                validGrid = true;
                break;
            }
        }

        if (!validGrid)
            return false;

        foreach (var (ent, storedItem) in storageEnt.Comp.StoredItems)
        {
            if (ent == itemEnt.Owner)
                continue;

            if (!_itemQuery.TryGetComponent(ent, out var itemComp))
                continue;

            var adjustedShape = _item.GetAdjustedItemShape((ent, itemComp), storedItem);
            foreach (var box in adjustedShape)
            {
                if (box.Contains(location))
                    return false;
            }
        }

        return true;
    }
}
