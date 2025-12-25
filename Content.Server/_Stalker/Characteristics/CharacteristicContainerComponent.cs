using Content.Shared._Stalker.Characteristics;

namespace Content.Server._Stalker.Characteristics;

// TODO: Move this component on another ent with RespawnContainerSystem(that one should be updated for it)
[RegisterComponent, Access(typeof(CharacteristicContainerSystem))]
public sealed partial class CharacteristicContainerComponent : Component
{
    [DataField]
    public Dictionary<CharacteristicType, Characteristic> Characteristics = new();
}
