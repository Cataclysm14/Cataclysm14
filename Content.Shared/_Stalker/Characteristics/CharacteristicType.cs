using Robust.Shared.Serialization;

namespace Content.Shared._Stalker.Characteristics;

[Serializable, NetSerializable]
public enum CharacteristicType
{
    Strength,
    Dexterity,
    Endurance,
    Knowledge,
    Attention,

    // Hidden
    Psionics,
    Karma,
}
