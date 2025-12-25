using Content.Shared._Stalker.Characteristics;

namespace Content.Server._Stalker.Characteristics.Modifiers.Weight;

[RegisterComponent]
public sealed partial class CharacteristicModifierWeightMaximumComponent : BaseCharacteristicFloatModifierComponent
{
    [DataField]
    public override CharacteristicType AllowedType { get; set; } = CharacteristicType.Strength;

    [DataField]
    public override float MinModifier { get; set; } = 0.1f;

    [DataField]
    public override float MaxModifier { get; set; } = 3f;

    [DataField]
    public override float Modifier { get; set; } = 0.05f;
}
