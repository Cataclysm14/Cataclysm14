using Robust.Shared.Serialization;
using Content.Shared.Storage;

namespace Content.Shared._Stalker.Storage;

public sealed class StorageAfterRemoveItemEvent : EntityEventArgs
{
    public readonly EntityUid StorageEnt;
    public readonly EntityUid ItemEnt;
    public readonly EntityUid User;
    public StorageAfterRemoveItemEvent(EntityUid itemEnt, EntityUid storageEnt, EntityUid user)
    {
        ItemEnt = itemEnt;
        StorageEnt = storageEnt;
        User = user;
    }
}

[Serializable, NetSerializable]
public sealed class StorageInsertItemIntoLocationEvent : EntityEventArgs
{
    public readonly NetEntity ItemEnt;

    public readonly NetEntity StorageEnt;

    public readonly ItemStorageLocation Location;

    public StorageInsertItemIntoLocationEvent(NetEntity itemEnt, NetEntity storageEnt, ItemStorageLocation location)
    {
        ItemEnt = itemEnt;
        StorageEnt = storageEnt;
        Location = location;
    }
}

public sealed class StorageAfterInsertItemIntoLocationEvent : EntityEventArgs
{
    public readonly EntityUid ItemEnt;

    public readonly EntityUid StorageEnt;

    public readonly EntityUid User;

    public StorageAfterInsertItemIntoLocationEvent(EntityUid itemEnt, EntityUid storageEnt, EntityUid user)
    {
        ItemEnt = itemEnt;
        StorageEnt = storageEnt;
        User = user;
    }
}
