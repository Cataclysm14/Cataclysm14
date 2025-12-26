namespace Content.Server._Stalker.Storage;

[RegisterComponent]
public sealed partial class StalkerStorageComponent : Component
{
    [DataField("Owner")]
    public string StorageOwner = "";

    [DataField("LoadedDBJson")]
    public string LoadedDbJson = "";
}
