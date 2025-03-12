using Robust.Shared.GameStates;

namespace Content.Shared._Mono.Ships;

/// <summary>
/// A component that enhances a shuttle's FTL range.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class FTLDriveComponent : Component
{
    /// <summary>
    /// Whether the FTL drive is currently powered.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("powered")]
    [AutoNetworkedField]
    public bool Powered;

    /// <summary>
    /// The maximum FTL range this drive can achieve.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("range")]
    [AutoNetworkedField]
    public float Range = 512f;
}
