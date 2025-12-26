using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._Stalker.StalkerRepository;

[RegisterComponent]
public sealed partial class StalkerRepositoryComponent : Component
{
    [DataField("Owner")]
    public string StorageOwner = "";

    [DataField("LoadedDBJson")]
    public string LoadedDbJson = "";

    [ViewVariables]
    public List<RepositoryItemInfo> ContainedItems = new();

    [DataField("whitelist")] public EntityWhitelist? Whitelist;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float MaxWeight = 75f; // kilos

    public float CurrentWeight = 0f;
}
/// <summary>
/// Raised on user entity
/// </summary>
public sealed class RepositoryItemInjectedEvent : HandledEntityEventArgs
{
    public EntityUid RepositoryEnt;
    public RepositoryItemInfo Item;

    public RepositoryItemInjectedEvent(EntityUid repositoryEnt, RepositoryItemInfo item)
    {
        RepositoryEnt = repositoryEnt;
        Item = item;
    }
}
