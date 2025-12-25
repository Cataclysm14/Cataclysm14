using Content.Shared._Stalker.Characteristics;

namespace Content.Server._Stalker.Characteristics.Modifiers;

public abstract partial class BaseCharacteristicFloatModifierComponent : BaseCharacteristicModifierComponent
{
    [ViewVariables]
    public virtual float Value { get; set; } = 0f;

    [DataField]
    public virtual CharacteristicType AllowedType { get; set; } = CharacteristicType.Dexterity;

    [DataField]
    public virtual float MinModifier { get; set; } = 0.1f;

    [DataField]
    public virtual float MaxModifier { get; set; } = 1.7f;

    [DataField]
    public virtual float Modifier { get; set; } = 0.05f;
}
