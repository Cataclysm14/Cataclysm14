namespace Content.Server._Stalker.Characteristics.Modifiers.MovementSpeed;

[RegisterComponent]
public sealed partial class CharacteristicModifierMovementSpeedComponent : Component
{
    [DataField]
    public float MaxBonus = 2f;

    [DataField]
    public float MinBonus = 0.1f;

    [DataField]
    public float PositiveModifier = 0.02f;

    [DataField]
    public float NegativeModifier = -0.02f;
}
