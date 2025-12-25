using Content.Shared.Damage;

namespace Content.Server.Damage.Components;

[RegisterComponent]
public sealed partial class StaminaDamageUserOnTriggerComponent : Component
{
    public float Stun = 15f;
}
