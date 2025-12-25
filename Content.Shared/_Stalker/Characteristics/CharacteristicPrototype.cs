using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Stalker.Characteristics;

[Prototype("characteristic"), Serializable, NetSerializable]
public sealed partial class CharacteristicPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField]
    public string Name = string.Empty;

    [DataField]
    public string Description = string.Empty;

    [DataField("group", required: true)]
    public CharacteristicType Type;

    [DataField]
    public int MinLevel = 0;

    [DataField]
    public int MaxLevel = 20;

    [DataField]
    public int BaseLevel = 10;

    [DataField]
    public int DefaultLevel = 10;

    [DataField]
    public CharacteristicAccess Access = CharacteristicAccess.Replicated;
}
