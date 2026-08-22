using System;

namespace Content.Shared._Cataclysm14.Zombies.Smoker;

/// <summary>
/// Marks the Smoker tongue action so the UI can give it an arming phase
/// </summary>
[RegisterComponent]
public sealed partial class SmokerTongueActionComponent : Component
{
    [DataField]
    public TimeSpan ArmDelay = TimeSpan.FromSeconds(3);

    // Clientside timestamp used to keep the action selected/locked during arming
    public TimeSpan ClientUnlockAt;
}
