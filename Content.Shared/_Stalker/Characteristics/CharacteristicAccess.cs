using Robust.Shared.Serialization;

namespace Content.Shared._Stalker.Characteristics;

[Serializable, NetSerializable]
public enum CharacteristicAccess : short
{
    Replicated,
    ServerOnly,
}
