namespace Content.Shared._Cataclysm14.Body.Components;

[RegisterComponent]
public sealed partial class UnshitmedMovementModifierComponent : Component
{
    [DataField("walkSpeedMod")]
    public float WalkSpeed = 1f;

    [DataField("sprintSpeedMod")]
    public float SprintSpeed = 1f;

    [DataField("accelerationMod")]
    public float Acceleration = 1f;
}
