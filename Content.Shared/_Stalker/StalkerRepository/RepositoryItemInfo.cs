using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Stalker.StalkerRepository;

[Serializable, NetSerializable]
public sealed class RepositoryItemInfo
{
    public int Count;

    // Not null if it was put in this round
    // Used to speed up putting items inside repository
    public List<NetEntity>? Entities;

    public string ProductEntity = string.Empty;

    public SpriteSpecifier? Icon;

    public string Name = string.Empty;

    public string Desc = string.Empty;

    public float Weight = 0f;

    // Weight with sums of all contained items to display in user category
    public float SumWeight = 0f;

    public string Category = string.Empty;

    public bool UserItem;

    public object? SStorageData;

    public string Identifier = string.Empty;
}

